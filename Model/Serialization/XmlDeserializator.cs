using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using NUml.Uml2;
using XCase.Model.Implementation;
using XCase.Model.Properties;
using XCase.Model.Serialization;

namespace XCase.Model
{
	public abstract class XmlDeserializatorBase
	{
		/// <summary>
		/// Version of format supported by the deserializator.
		/// Written in the xml file as version attribute of the xs:project element. 
		/// </summary>
		/// <seealso cref="XmlSerializator.SchemaVersion"/>
		/// <seealso cref="ProjectConverter"/>
		public const string SchemaVersion = "2.0";

		/// <summary>
		///  Project being restored
		/// </summary>
		protected Project project;

		/// <summary>
		///  Model being restored
		/// </summary>
		protected Model model;

		/// <summary>
		///  XML document with <c>XCase</c> project to restore
		/// </summary>
		protected XPathDocument document;

		/// <summary>
		/// Global namespace manager used for restoration
		/// </summary>
		protected XmlNamespaceManager ns;

		/// <summary>
		/// Table of all restored elements that have @id in <c>XCase</c> XML file
		/// <list type="String.Empty">
		/// <item>Key = ID</item>
		/// <item>Value = Element with this ID</item>
		/// </list>
		/// </summary>
		protected DeserializatorIdTable idTable;

	    protected DeserializatorIdTable nonVersionedElements; 

	    protected Dictionary<Version, DeserializatorIdTable> versionedIdTables;  

		protected readonly Dictionary<string, Diagram> diagramIdTable = new Dictionary<string, Diagram>();

		/// <summary>
		/// Table of diagrams and its root PSM classes
		/// </summary>
		/// <list type="String.Empty">
		/// <item>Key = PIM Class</item>
		/// <item>Value = <see cref="XPathNodeIterator"/> pointing to the list of its derived PSM classes</item>
		/// </list>
		protected readonly Dictionary<Diagram, List<string>> rootTable = new Dictionary<Diagram, List<string>>();

		/// <summary>
		/// Table of PIM classes and related derived PSM classes
		/// </summary>
		/// <list type="String.Empty">
		/// <item>Key = PIM Class</item>
		/// <item>Value = List of its roots (IDs of root psm classes)</item>
		/// </list>
		protected readonly Dictionary<PIMClass, XPathNodeIterator> derivedClasses = new Dictionary<PIMClass, XPathNodeIterator>();

		protected XmlVocBase XmlVoc { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		protected XmlDeserializatorBase(): this(new DeserializatorIdTable())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		protected XmlDeserializatorBase(DeserializatorIdTable idTable)
		{
			this.idTable = idTable;
		}

		#region restore overloads

		/// <summary>
		/// <c>XCase</c> project is loaded from the XML file.
		/// </summary>
		/// <param name="file">XML file containing serialized <c>XCase</c> project</param>
		public Project RestoreProject(string file)
		{
			this.FileName = file;
			XPathDocument d = new XPathDocument(file);

			return Restore(d);
		}

		/// <summary>
		/// <c>XCase</c> project is loaded from the stream.
		/// </summary>
		/// <param name="input">Stream containing serialized <c>XCase</c> project</param>
		public Project RestoreProject(Stream input)
		{
			XPathDocument d = new XPathDocument(input);
			return Restore(d);
		}

		/// <summary>
		/// <c>XCase</c> project is loaded from the document.
		/// </summary>
		/// <param name="doc">Document containing serialized <c>XCase</c> project</param>
		public Project Restore(XPathDocument doc)
		{
			return Restore(doc, false);
		}

		protected abstract Project Restore(XPathDocument doc, bool loadingTemplate);

		#endregion

		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected internal Project CreateProjectStub()
		{
			byte[] templateBytes =
				Encoding.GetEncoding("utf-8").GetBytes(Resources.ProjectStub);
			MemoryStream projectStubStream = new MemoryStream(templateBytes);

			string msg = SerializationErrors.DES_UNKNOWN_ERROR;
			// First, validates if the file is a valid XCase XML file
			if (!ValidateXML(projectStubStream, ref msg))
			{
				throw new DeserializationException(String.Format(SerializationErrors.DES_TEMPLATE_CORRUPTED, msg));
			}

			projectStubStream.Position = 0;

			return Restore(new XPathDocument(projectStubStream), true);
		}

		/// <summary>
		/// Restores <c>XCase</c> profiles
		/// </summary>
		/// <param name="uri"><c>XPath</c> query selecting profiles element in XML document
		/// </param>
		protected void RestoreProfiles(string uri)
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression profiles = nav.Compile(uri);
			profiles.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(profiles);

			// for each profile found
			while (iterator.MoveNext())
			{
				Profile p = model.Schema.AddProfile();

				// @name
				p.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);

				// stereotypes
				XPathExpression expr = iterator.Current.Compile(XmlVoc.relativeStereotypes);
				expr.SetContext(ns);
				XPathNodeIterator it = iterator.Current.Select(expr);
				while (it.MoveNext())
				{
					Stereotype s = p.AddStereotype();
					// @id
					string id = it.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
					idTable.Add(id, s);
				    ((_ImplElement) p).Guid = XCaseGuid.Parse(id);

					// @name
					s.Name = it.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);

					// receivers
					expr = it.Current.Compile(XmlVoc.relativeReceivers);
					expr.SetContext(ns);
					XPathNodeIterator it2 = it.Current.Select(expr);
					while (it2.MoveNext())
						s.AppliesTo.Add(it2.Current.GetAttribute(XmlVoc.xmlAttType, String.Empty));

					// properties
					expr = it.Current.Compile(XmlVoc.relativeProperties);
					expr.SetContext(ns);
					it2 = it.Current.Select(expr);
					while (it2.MoveNext())
					{
						Property property = s.AddAttribute();
						RestoreProperty(property, it2.Current);
					}
				}
			}
		}

		#region restore model

		/// <summary>
		/// Restores <c>XCase</c> UML model
		/// <para>
		/// Comprises restore of: Comments, Packages, Classes, DataTypes, Associations, Generalizations
		/// </para>
		/// </summary>
		protected void RestoreModel()
		{
			#region Model attributes

			XPathExpression m = document.CreateNavigator().Compile(XmlVoc.selectModelElement);
			m.SetContext(ns);
			XPathNavigator xmlmodel = document.CreateNavigator().SelectSingleNode(m);

			model.ViewPoint = xmlmodel.GetAttribute(XmlVoc.xmlAttViewpoint, String.Empty);
			model.Schema.XMLNamespace = xmlmodel.GetAttribute(XmlVoc.xmlAttNamespace, String.Empty);
            string id = xmlmodel.GetAttribute(XmlVoc.xmlAttID, String.Empty);
            idTable[id] = model;
            ((_ImplElement)model).Guid = XCaseGuid.Parse(id);

			// project name
			string n = xmlmodel.GetAttribute(XmlVoc.xmlAttName, String.Empty);
			if (!n.Equals("User model"))
				project.Caption = n;

			#endregion

			XPathExpression comments = document.CreateNavigator().Compile(XmlVoc.modelComments);
			comments.SetContext(ns);
			RestoreComments(document.CreateNavigator().SelectSingleNode(comments), model);

			// ------------------ PIM part restoration
			RestoreDatatypes(document.CreateNavigator(), XmlVoc.modelDatatypes, model);
			RestorePackages(document.CreateNavigator(), XmlVoc.modelPackages, model);


                
			// Derived PSM classes are NOT restored 
			RestorePIMClasses(document.CreateNavigator(), XmlVoc.modelPimClasses, model);

			RestoreAssociationClasses(document.CreateNavigator(), XmlVoc.modelAssociationClasses, model);

			// PIM generalizations only
			RestoreGeneralizations(XmlVoc.modelPIMGeneralizations);

			RestorePIMAssociations(XmlVoc.modelAssociations);

			//-------------------- PSM part restorations

			// PSM Classes and their components
			RestorePSMClasses();

			// PSM generalizations only
			RestoreGeneralizations(XmlVoc.modelPSMGeneralizations);

			RestorePSMAssociations(document.CreateNavigator());

			RestoreRoots();

		    RestorePSMDiagramReferences(XmlVoc.modelPSMDiagramReferences);
		}

	    protected void RestorePSMDiagramReferences(string modelPSMDiagramReferences)
	    //protected void RestorePSMAttContainer(XPathNavigator xn, PSMSuperordinateComponent c)
	    {
            XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(modelPSMDiagramReferences);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			while (iterator.MoveNext())
			{
			    if (iterator.Current != null)
			    {
                    PSMDiagramReference r = new PSMDiagramReference();
                    string val = iterator.Current.GetAttribute(XmlVoc.xmlAttRef, string.Empty);
			        int id = -1;
			        if (!string.IsNullOrEmpty(val))
			        {
			            id = int.Parse(val);
			        }
                    
                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttReferencedDiagramId, string.Empty);
			        if (!string.IsNullOrEmpty(val))
			        {
			            r.ReferencedDiagram = (PSMDiagram) diagramIdTable[int.Parse(val).ToString()];
			        }

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttReferencingDiagramId, string.Empty);
                    if (!string.IsNullOrEmpty(val))
                    {
                        r.ReferencingDiagram = (PSMDiagram)diagramIdTable[int.Parse(val).ToString()];
                    }

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttName, string.Empty);
                    r.Name = val;

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttSchemaLocation, string.Empty);
                    r.SchemaLocation = val;

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttLocal, string.Empty);
                    if (!string.IsNullOrEmpty(val))
                    {
                        r.Local = bool.Parse(val);
                    }

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttNamespacePrefix, string.Empty);
                    r.NamespacePrefix = val;

                    val = iterator.Current.GetAttribute(XmlVoc.xmlAttNamespace, string.Empty);
                    r.Namespace = val;

                    //// ------------- visualization -------------
                    XPathNavigator xn = document.CreateNavigator();
                    expr = xn.Compile(XmlVoc.GetVisualizationForPSMDiagramReference(id.ToString()));
                    expr.SetContext(ns);
                    XPathNavigator visualization = xn.SelectSingleNode(expr);
                    RestoreVisualizationForPSMElement(visualization, id.ToString(), r);
			    }
			}

           

            //// ------------- other connected elements ------------- 

            //// Comments
            //XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
            //comments.SetContext(ns);
            //RestoreComments(xn.SelectSingleNode(comments), container);

		}

	    /// <summary>
		/// Restores comments connected to given <see cref="NamedElement"/>
		/// </summary>
		/// <param name="xn">Reference to 'comments' element in XML document</param>
		/// <param name="p">Element, which the restored comments belong to</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreComments(XPathNavigator xn, Element p)
		{
			if (xn == null)
				return;

			if (!xn.Name.Equals(XmlVoc.xmlCommentsElement))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			XPathExpression expr = xn.Compile(XmlVoc.xmlCommentElement);
			expr.SetContext(ns);
			XPathNodeIterator iterator = xn.Select(expr);

			// for each comment
			while (iterator.MoveNext())
			{
				// Restores the comment itself
				Comment c = p.AddComment();
				// @id
				string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
				idTable.Add(id, c);
                ((_ImplElement)c).Guid = XCaseGuid.Parse(id);

				c.Body = iterator.Current.Value;

				// Restores all its visualizations

				// Find the type of the diagram (PIM or PSM)
				expr = xn.Compile(XmlVoc.GetVisualizationForPIMComment(id));
				expr.SetContext(ns);
				XPathNodeIterator visualization = xn.Select(expr);
				DiagramType dt = DiagramType.PIMDiagram;

				if (visualization.Count == 0)
				{
					expr = xn.Compile(XmlVoc.GetVisualizationForPSMComment(id));
					expr.SetContext(ns);
					visualization = xn.Select(expr);
					dt = DiagramType.PSMDiagram;
				}

				if (visualization.Count == 0)
					continue;

				while (visualization.MoveNext())
				{
					// Find the diagram number
					expr = visualization.Current.Compile("..");
					expr.SetContext(ns);
					int diagno = int.Parse(visualization.Current.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, String.Empty));

					CommentViewHelper h;
					if (dt == DiagramType.PIMDiagram)
						h = new CommentViewHelper(project.PIMDiagrams[diagno]);
					else
						h = new CommentViewHelper(project.PSMDiagrams[diagno]);

					// appearance
					XPathExpression appearance = visualization.Current.Compile(XmlVoc.xmlAppearanceElement);
					appearance.SetContext(ns);
					RestoreAppearance(visualization.Current.SelectSingleNode(appearance), h);

					// points
					ObservablePointCollection points_collection;
					XPathExpression points = visualization.Current.Compile(XmlVoc.xmlPoints);
					points.SetContext(ns);
					RestorePoints(visualization.Current.SelectSingleNode(points), out points_collection);
					foreach (rPoint r in points_collection)
						h.LinePoints.Add(r);

					if (dt == DiagramType.PIMDiagram)
						project.PIMDiagrams[diagno].AddModelElement(c, h);
					else
						project.PSMDiagrams[diagno].AddModelElement(c, h);
				}
			}

		}

		/// <summary>
		/// Restores nested packages.
		/// </summary>
		/// <param name="xn">Reference to a XML element</param>
		/// <param name="uri"><c>XPath</c> query that should be applied to <paramref name="xn"/> in order to get the packages location</param>
		/// <param name="p">Package to fill with read XML data</param>
		protected void RestorePackages(XPathNavigator xn, string uri, Package p)
		{
			XPathExpression expr = xn.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = xn.Select(expr);

			// for each package
			while (iterator.MoveNext())
			{
				Package package = p.AddNestedPackage();

                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
                idTable[id] = package;

				// @name
				package.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);

				// comments
				XPathExpression comments = iterator.Current.Compile(XmlVoc.relativeComments);
				comments.SetContext(ns);
				RestoreComments(iterator.Current.SelectSingleNode(comments), package);

				// datatypes
				RestoreDatatypes(iterator.Current, XmlVoc.relativeDatatypes, package);

				// classes
				RestorePIMClasses(iterator.Current, XmlVoc.relativePimClasses, package);

				// nested packages
				RestorePackages(iterator.Current, XmlVoc.relativePackages, package);
			}
		}

		/// <summary>
		/// Restores PIM classes
		/// </summary>
		/// <param name="nav">Reference to a XML element</param>
		/// <param name="uri"><c>XPath</c> query that should be applied to <paramref name="nav"/>
		/// in order to get the classes location</param>
		/// <param name="package">Package where to restore the classes</param>
		protected void RestorePIMClasses(XPathNavigator nav, string uri, Package package)
		{
			XPathExpression expr = nav.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			// Restore each PIM class in package
			while (iterator.MoveNext())
			{
				RestorePimClass(iterator.Current, package);
			}
		}

		/// <summary>
		/// Restores one PIM class
		/// </summary>
		/// <param name="xn">Reference to XML '<c>pim_class</c>' element, which represents the restored class.</param>
		/// <param name="package">Parent package for the restored PIM class</param>
		protected void RestorePimClass(XPathNavigator xn, Package package)
		{
			// class ID from @ref
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			if (!idTable.ContainsKey(id))
			{
				PIMClass c0 = package.AddClass();
			    idTable.Add(id, c0);
			}

			PIMClass c = (PIMClass)idTable[id];

		    ((_ImplElement) c).Guid = XCaseGuid.Parse(id);

			//  @name
            c.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);


            // @ontologyEquivalent
            c.OntologyEquivalent = xn.GetAttribute(XmlVoc.xmlAttOntoEquiv, String.Empty);

			// class properties
			XPathExpression expr = xn.Compile(XmlVoc.relativeProperties);
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);
			while (it.MoveNext())
			{
				Property p = c.AddAttribute();
				RestoreProperty(p, it.Current);
			}

			// class operations
			expr = xn.Compile(XmlVoc.relativeOperations);
			expr.SetContext(ns);
			it = xn.Select(expr);
			while (it.MoveNext())
			{
				Operation o = c.AddOperation();
				RestoreClassOperation(o, it.Current);
			}

			// applied stereotypes
			RestoreAppliedStereotypes(xn, XmlVoc.relativeStereotypeInstances, c);

			// --------------------------------------------------------------------
			// Remember DERIVED (PSM) CLASSES for future restoration
			expr = xn.Compile(XmlVoc.relativePsmClasses);
			expr.SetContext(ns);
			XPathNodeIterator psm = xn.Select(expr);
			derivedClasses.Add(c, psm);

			// --------------------------------------------------------------------

			// Restore all visualizations for the current PIM class
			expr = xn.Compile(XmlVoc.GetVisualizationForPIMClass(id));
			expr.SetContext(ns);
			XPathNodeIterator xit = xn.Select(expr);

			while (xit.MoveNext())
			{
				expr = xit.Current.Compile("..");
				expr.SetContext(ns);
				int diagno = int.Parse(xit.Current.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, String.Empty));

				ClassViewHelper h = new ClassViewHelper(project.PIMDiagrams[diagno]);

				RestoreClassElementVisualization(xit.Current, h);

				project.PIMDiagrams[diagno].AddModelElement(c, h);

			}

			// comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), c);
		}

		/// <summary>
		/// Restores PIM Class Operation
		/// </summary>
		/// <param name="p">Operation to restore (to fill with data)</param>
		/// <param name="xpn">Reference to appropriate property element in XML document</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreClassOperation(Operation p, XPathNavigator xpn)
		{
			// @id
			string id = xpn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, p);
            ((_ImplElement)p).Guid = XCaseGuid.Parse(id);

			// @name
			p.Name = xpn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// @type
			string type = xpn.GetAttribute(XmlVoc.xmlAttType, String.Empty);
			if (type.Equals("null"))
				p.Type = null;
			else
			{
				if (!idTable.ContainsKey(type))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}
				p.Type = (DataType)idTable[type];
			}

			// @lower
			uint i;
			if (UInt32.TryParse(xpn.GetAttribute(XmlVoc.xmlAttLower, String.Empty), out i))
				p.Lower = i;
			else
				p.Lower = null;
			// @upper
			p.Upper = UnlimitedNatural.Parse(xpn.GetAttribute(XmlVoc.xmlAttUpper, String.Empty));
			// @visibility
			p.Visibility = (VisibilityKind)
						   Enum.Parse(typeof(VisibilityKind), xpn.GetAttribute(XmlVoc.xmlAttVisibility, String.Empty));

		}

		/// <summary>
		/// Restores all derived PSM classes for all PIM classes in project.
		/// </summary>
		protected void RestorePSMClasses()
		{
			foreach (KeyValuePair<PIMClass, XPathNodeIterator> kvp in derivedClasses)
			{
				PIMClass c = kvp.Key;
				XPathNodeIterator xn = kvp.Value;

				while (xn.MoveNext())
				{
					RestorePsmClass(xn.Current, c);
				}
			}
		}

		/// <summary>
		/// Restores one PSM Class
		/// </summary>
		/// <param name="xn">Reference to XML 'psm_class' element, which represents the restored class</param>
		/// <param name="parent">Parent PIM class for the restored PSM class</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestorePsmClass(XPathNavigator xn, PIMClass parent)
		{
			PSMClass c;

			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);

			if (idTable.ContainsKey(id))
				c = (PSMClass)idTable[id];
			else
			{
				c = parent.DerivePSMClass();
				idTable.Add(id, c);
			}

            ((_ImplElement)c).Guid = XCaseGuid.Parse(id);

			// @name
			string name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);
			// set elsewhere c.Name = name;

			//@abstract
			string abstr = xn.GetAttribute(XmlVoc.xmlAttAbstract, String.Empty);
			if (!abstr.Equals(String.Empty))
			{
				c.IsAbstract = bool.Parse(abstr);
			}

			// element label
			XPathExpression expr = xn.Compile(XmlVoc.xmlElementName);
			expr.SetContext(ns);
			c.ElementName = xn.SelectSingleNode(expr).Value;

			// allow any attribute
			expr = xn.Compile(XmlVoc.xmlAllowAnyAttribute);
			expr.SetContext(ns);
			XPathNavigator res = xn.SelectSingleNode(expr);
			if (res != null)
			{
				c.AllowAnyAttribute = bool.Parse(res.Value);
			}

			// @structural_representative
			string sr = xn.GetAttribute(XmlVoc.xmlAttStructuralRepresentative, String.Empty);
			PSMClass structural_representative = null;
			if (!sr.Equals(string.Empty))
			{
				if (!idTable.ContainsKey(sr))
				{
					structural_representative = parent.DerivePSMClass();
					idTable.Add(sr, structural_representative);
				    ((_ImplElement) structural_representative).Guid = XCaseGuid.Parse(sr);
				}
				c.RepresentedPSMClass = (PSMClass)idTable[sr];
			}

			// Applied stereotypes
			//RestoreAppliedStereotypes(nav, "stereotype_instances/stereotype_instance", c);

            // ------------- visualization  -------------


            expr = xn.Compile(XmlVoc.GetVisualizationForPSMClass(id));
            expr.SetContext(ns);
            XPathNavigator visualization = xn.SelectSingleNode(expr);

            // NO VISUALIZATION FOR PSM CLASS!!! THIS SHOULD NEVER HAPPEN
            if (visualization == null)
            {
                // REMOVES PREVIOUSLY ADDED PSM CLASS FROM MODEL
                parent.DerivedPSMClasses.Remove(c);
                idTable.Remove(id);

                if (structural_representative != null)
                {
                    parent.DerivedPSMClasses.Remove(structural_representative);
                    idTable.Remove(sr);
                }
                return; // DeserializationError();
            }
            else
            {

                expr = visualization.Compile("..");
                expr.SetContext(ns);
                int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, String.Empty));

                PSMElementViewHelper h = new PSMElementViewHelper(project.PSMDiagrams[diagno]);
                RestoreClassElementVisualization(visualization, h);
                project.PSMDiagrams[diagno].AddModelElement(c, h);

                c.Diagram = project.PSMDiagrams[diagno];
            }

			// Restores class properties
			expr = xn.Compile(XmlVoc.relativePSMAttributes);
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);
			while (it.MoveNext())
			{
				// @ref
				string attref = it.Current.GetAttribute(XmlVoc.xmlAttAttRef, String.Empty);

				// free attribute (no partner in PIM class)
				if (attref.Equals(String.Empty))
				{
					Property p = c.AddAttribute();
					RestoreProperty(p, it.Current);
				}
				else
					if (idTable.ContainsKey(attref))
					{
						PSMAttribute p = c.AddAttribute((Property)idTable[attref]);
						RestoreProperty(p, it.Current);
					}
					else
					{
						throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
					}
			}

			// class operations
			expr = xn.Compile(XmlVoc.relativeOperations);
			expr.SetContext(ns);
			it = xn.Select(expr);
			while (it.MoveNext())
			{
				Operation o = c.AddOperation();
				RestoreClassOperation(o, it.Current);
			}

            // ------------- other connected elements ------------- 

			// comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), c);

			// components
			XPathExpression components = visualization.Compile(XmlVoc.xmlComponentsElement);
			components.SetContext(ns);
			RestoreComponents(xn.SelectSingleNode(components), c);


			// @name for PSM class
			c.Name = name;
		}

		/// <summary>
		/// Restores PIM or PSM Class Property.
		/// <para>
		/// Reads information from passed XML element and
		/// fills Property p with the read data.
		/// </para>
		/// </summary>
		/// <param name="p">Property to restore (to fill with data)</param>
		/// <param name="xn">Reference to appropriate property element in XML document</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreProperty(Property p, XPathNavigator xn)
		{
			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, p);
            ((_ImplElement)p).Guid = XCaseGuid.Parse(id);

			// @name
			p.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

            // @ontologyEquivalent
            p.OntologyEquivalent = xn.GetAttribute(XmlVoc.xmlAttOntoEquiv, String.Empty);

			// @aggregation
			p.Aggregation = (AggregationKind)
							Enum.Parse(typeof(AggregationKind), xn.GetAttribute(XmlVoc.xmlAttAggregation, String.Empty));

			// Updated by Luk on 25/04/2009.
			// Implementation of Property.Default changed, so that it's now
			// just a syntactic sugar for Property.DefaultValue.ToString() / 
			// Property.DefaultValue.ParseFromString().
			// Thus, the restoration of Property.Default becomes obsolete.
			// @default
			//string defstr = xn.GetAttribute(XmlVoc.xmlAttDefault, String.Empty);
			//if (!defstr.Equals(String.Empty))
			//    p.Default = defstr;
			// END of Luk update (25/04/2009)

			// @type
			string type = xn.GetAttribute(XmlVoc.xmlAttType, String.Empty);
			if (type.Equals("null"))
				p.Type = null;
			else
			{
				if (!idTable.ContainsKey(type))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}
				p.Type = (DataType)idTable[type];
			}

			// @default_value
			// Code added by Luk on 25/04/2009 to allow deserialization of the DefaultValue
			// property without need to change the project XML schema.
			string defval = xn.GetAttribute(XmlVoc.xmlAttDefaultValue, String.Empty);
			if (!(defval.Equals(String.Empty) || defval.Equals("null")))
			{
				try
				{
					p.DefaultValue = new ValueSpecification(defval, p.Type);
				}
				catch (Exception)
				{
					p.DefaultValue = null;
				}
			}
			// End of Luk's code (25/04/2009)

			p.IsComposite = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsComposite, String.Empty));
			p.IsDerived = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsDerived, String.Empty));
			p.IsDerivedUnion = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsDerivedUnion, String.Empty));
			p.IsOrdered = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsOrdered, String.Empty));
			p.IsReadOnly = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsReadOnly, String.Empty));
			p.IsUnique = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsUnique, String.Empty));

			// @lower
			uint i;
			if (UInt32.TryParse(xn.GetAttribute(XmlVoc.xmlAttLower, String.Empty), out i))
				p.Lower = i;
			else
				p.Lower = null;

			// @upper
			p.Upper = UnlimitedNatural.Parse(xn.GetAttribute(XmlVoc.xmlAttUpper, String.Empty));

			// @visibility
			p.Visibility = (VisibilityKind)
						   Enum.Parse(typeof(VisibilityKind), xn.GetAttribute(XmlVoc.xmlAttVisibility, String.Empty));

			// PSM Attributes only
			if (p is PSMAttribute)
			{
				// @class_ref
				string cls = xn.GetAttribute(XmlVoc.xmlAttClassRef, String.Empty);
				if (!idTable.ContainsKey(cls) || !(idTable[cls] is PSMClass))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}
				((PSMAttribute)p).Class = (PSMClass)idTable[cls];

				// @alias
				((PSMAttribute)p).Alias = xn.GetAttribute(XmlVoc.xmlAttAlias, String.Empty);

				// @XsdImplementation
				((PSMAttribute)p).XSDImplementation = xn.GetAttribute(XmlVoc.xmlAttXSDImplementation, String.Empty);

				// @att_ref is restored when PSM Attribute is created
			}
		}

		/// <summary>
		/// Applies stereotypes to a given class
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="uri"></param>
		/// <param name="c">Class, which the stereotypes are applied to</param>
		protected void RestoreAppliedStereotypes(XPathNavigator xn, string uri, Class c)
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);

			while (it.MoveNext())
			{
				string s = it.Current.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
				if (idTable[s] is Stereotype)
					((Stereotype)idTable[s]).ApplyTo(c);
			}
		}

		/// <summary>
		/// Restores visualization for one PIM or PSM class
		/// </summary>
		/// <param name="xn">Pointer to the 'class' element in XML document</param>
		/// <param name="h"><see cref="ClassViewHelper"/> to restore (to fill with visualization data)</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreClassElementVisualization(XPathNavigator xn, ClassViewHelper h)
		{
			if (!xn.Name.Equals(XmlVoc.xmlClassElement))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			// appearance
			XPathExpression appearance = xn.Compile(XmlVoc.xmlAppearanceElement);
			appearance.SetContext(ns);
			RestoreAppearance(xn.SelectSingleNode(appearance), h);

			// @methods_collapsed
			h.OperationsCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttOperationsCollapsed, String.Empty));

			// @properties_collapsed
			h.AttributesCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttAttributesCollapsed, String.Empty));

			//
			h.ElementNameLabelAlignedRight = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttElementNameLabelAlignedRight, String.Empty));

			h.ElementNameLabelCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttElementNameLabelCollapsed, String.Empty));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn">Reference to 'components' element in XML document</param>
		/// <param name="c"><see cref="PSMSuperordinateComponent"/> to add the restored components to</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreComponents(XPathNavigator xn, PSMSuperordinateComponent c)
		{
			// components element empty
			if (xn == null)
				return;

			if (!xn.Name.Equals(XmlVoc.xmlComponentsElement))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			// Selects all components under the components element
			XPathExpression expr = xn.Compile("*");
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);
			while (it.MoveNext())
			{
				if (it.Current.Name.Equals(XmlVoc.xmlPSMAttContainer))
					RestorePSMAttContainer(it.Current, c);
				else
					if (it.Current.Name.Equals(XmlVoc.xmlPSMContentContainer))
						RestoreContentContainer(it.Current, c);
					else
						if (it.Current.Name.Equals(XmlVoc.xmlPSMContentChoice))
							RestoreContentChoice(it.Current, c);
						else
							if (it.Current.Name.Equals(XmlVoc.xmlPSMAssociation))

								// !!! PSMAssociations restored elsewhere after all PSM components have been restored
								continue;
							else
							// No other components allowed to be present here
							{
								throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
							}
			} // while

		}

		/// <summary>
		/// Restoration of a <see cref="PSMAttributeContainer"/>
		/// </summary>
		/// <param name="xn">Reference to <see cref="PSMAttributeContainer"/> element in XML document</param>
		/// <param name="c">Component to restore <see cref="PSMAttributeContainer"/> to</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestorePSMAttContainer(XPathNavigator xn, PSMSuperordinateComponent c)
		{
			if (!xn.Name.Equals(XmlVoc.xmlPSMAttContainer))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			PSMAttributeContainer container = (PSMAttributeContainer)c.AddComponent(PSMAttributeContainerFactory.Instance);

			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, container);
            ((_ImplElement)container).Guid = XCaseGuid.Parse(id);

			// @name
			container.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// diagram
			container.Diagram = c.Diagram;

			// properties
			XPathExpression expr = xn.Compile(XmlVoc.relativePSMAttributes);
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);

			while (it.MoveNext())
			{
				// @att_ref
				string attref = it.Current.GetAttribute(XmlVoc.xmlAttAttRef, String.Empty);
				if (idTable.ContainsKey(attref))
				{
					PSMAttribute p = container.AddAttribute((Property)idTable[attref]);
					RestoreProperty(p, it.Current);

				}
				else if (attref.Equals(String.Empty))
				{
					PSMAttribute p = container.AddAttribute();
					RestoreProperty(p, it.Current);
				}
				else
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}
			}

			// ------------- visualization -------------

			expr = xn.Compile(XmlVoc.GetVisualizationForPSMAttContainer(id));
			expr.SetContext(ns);
			XPathNavigator visualization = xn.SelectSingleNode(expr);
			RestoreVisualizationForPSMElement(visualization, id, container);

			// ------------- other connected elements ------------- 

			// Comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), container);

		}

		/// <summary>
		/// Restoration of <see cref="PSMContentContainer"/>
		/// </summary>
		/// <param name="xn">Reference to <see cref="PSMContentContainer"/> element in XML document</param>
		/// <param name="c">Component to restore <see cref="PSMContentContainer"/> to</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreContentContainer(XPathNavigator xn, PSMSuperordinateComponent c)
		{
			if (!xn.Name.Equals(XmlVoc.xmlPSMContentContainer))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			PSMContentContainer container = (PSMContentContainer)c.AddComponent(PSMContentContainerFactory.Instance);

			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, container);
            ((_ImplElement)container).Guid = XCaseGuid.Parse(id);

			// @name
			container.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// diagram
			container.Diagram = c.Diagram;

			XPathExpression expr = xn.Compile(XmlVoc.xmlElementLabel);
			expr.SetContext(ns);
			XPathNavigator element_label = xn.SelectSingleNode(expr);
			container.ElementLabel = element_label.Value;

			// ------------- visualization -------------

			expr = xn.Compile(XmlVoc.GetVisualizationForPSMContentContainer(id));
			expr.SetContext(ns);
			XPathNavigator visualization = xn.SelectSingleNode(expr);
			RestoreVisualizationForPSMElement(visualization, id, container);

			// ------------- other connected elements ------------- 

			// Comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), container);

			// Components
			expr = xn.Compile(XmlVoc.xmlComponentsElement);
			expr.SetContext(ns);
			XPathNavigator components = xn.SelectSingleNode(expr);
			RestoreComponents(components, container);
		}

		/// <summary>
		/// Restoration of <see cref="PSMContentChoice"/>
		/// </summary>
		/// <param name="xn">Reference to <see cref="PSMContentChoice"/> element in XML
		/// document</param>
		/// <param name="c">Component to restore <see cref="PSMContentContainer"/> to
		/// </param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreContentChoice(XPathNavigator xn, PSMSuperordinateComponent c)
		{
			if (!xn.Name.Equals(XmlVoc.xmlPSMContentChoice))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			PSMContentChoice choice = (PSMContentChoice)c.AddComponent(PSMContentChoiceFactory.Instance);

			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, choice);
            ((_ImplElement)choice).Guid = XCaseGuid.Parse(id);

			// @name
			c.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// diagram
			choice.Diagram = c.Diagram;

			// ------------- visualization -------------

			XPathExpression expr = xn.Compile(XmlVoc.GetVisualizationForPSMContentChoice(id));
			expr.SetContext(ns);
			XPathNavigator visualization = xn.SelectSingleNode(expr);
			RestoreVisualizationForPSMElement(visualization, id, choice);

			// ------------- other connected elements ------------- 

			// Comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), choice);

			// Components
			expr = xn.Compile(XmlVoc.xmlComponentsElement);
			expr.SetContext(ns);
			XPathNavigator components = xn.SelectSingleNode(expr);
			RestoreComponents(components, choice);
		}

		/// <summary>
		/// Restoration of visualization for a <see cref="PSMElement"/>
		/// </summary>
		/// <remarks>
		/// - <see cref="PSMAttributeContainer"/>
		/// - <see cref="PSMContentContainer"/>
		/// - <see cref="PSMContentChoice"/>
		/// </remarks>
		/// <param name="visualization">Reference to appropriate element in XML document</param>
		/// <param name="id">ID of restored element</param>
		/// <param name="container">Element to restore the visualization for</param>
		protected void RestoreVisualizationForPSMElement(XPathNavigator visualization, string id, NamedElement container)
		{
			// No visualization found
			if (visualization == null)
				return;

			XPathExpression expr = visualization.Compile("..");
			expr.SetContext(ns);
			int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, String.Empty));

			PSMElementViewHelper h = new PSMElementViewHelper(project.PSMDiagrams[diagno]);

			// appearance
			XPathExpression appearance = visualization.Compile(XmlVoc.xmlAppearanceElement);
			appearance.SetContext(ns);
			RestoreAppearance(visualization.SelectSingleNode(appearance), h);

			// points
			ObservablePointCollection points_collection;
			XPathExpression points = visualization.Compile(XmlVoc.xmlPoints);
			points.SetContext(ns);
			RestorePoints(visualization.SelectSingleNode(points), out points_collection);
			foreach (rPoint r in points_collection)
				h.ConnectorViewHelper.Points.Add(r);

			project.PSMDiagrams[diagno].AddModelElement(container, h);
		}

		protected void RestoreDatatypes(XPathNavigator nav, string uri, Package p)
		{
			XPathExpression expr = nav.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			//datatypes
			while (iterator.MoveNext())
			{
				if (bool.Parse(iterator.Current.GetAttribute(XmlVoc.xmlAttSimple, String.Empty)))
				{
					string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
					SimpleDataType d;
					string parent = iterator.Current.GetAttribute(XmlVoc.xmlAttParentRef, String.Empty);
					if (!parent.Equals(String.Empty) && idTable.ContainsKey(parent))
						d = p.AddSimpleDataType((SimpleDataType)idTable[parent]);
					else
						d = p.AddSimpleDataType();

					idTable.Add(id, d);
                    ((_ImplElement)d).Guid = XCaseGuid.Parse(id);

					d.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);
					d.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, String.Empty);


				}
				else
				{

					string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
					Class c = p.AddClass();
					idTable.Add(id, c);
                    ((_ImplElement)c).Guid = XCaseGuid.Parse(id);

					c.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);
					// c.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, String.Empty);

				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="uri"></param>
		/// <param name="p"></param>
		protected void RestoreAssociationClasses(XPathNavigator xn, string uri, Package p)
		{
			XPathExpression expr = xn.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = xn.Select(expr);

			// Restore each PIM class in package
			while (iterator.MoveNext())
			{
				string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);

				AssociationClass a;

				// Model
				RestoreAssociationClassFromModel(iterator.Current, out a);

				idTable.Add(id, a);
                ((_ImplElement)a).Guid = XCaseGuid.Parse(id);

				// visualization
				RestoreVisualizationForAssociationClass(id, a);

				// comments
				XPathExpression comments = iterator.Current.Compile(XmlVoc.relativeComments);
				comments.SetContext(ns);
				RestoreComments(iterator.Current.SelectSingleNode(comments), a);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		protected void RestoreAssociationClassFromModel(XPathNavigator xn, out AssociationClass a)
		{
			// Association ID

			List<Class> classes = new List<Class>();
			List<AssociationEndMemo> ends = new List<AssociationEndMemo>();

			// "association_end"
			XPathExpression expr = xn.Compile(XmlVoc.xmlAssociationEndElement);
			expr.SetContext(ns);
			XPathNodeIterator assocend = xn.Select(expr);

			// Go through all AssociationEnds belonging to the current Association
			while (assocend.MoveNext())
			{
				string class_id = assocend.Current.GetAttribute(XmlVoc.xmlAttClass, String.Empty);

				if (idTable[class_id] == null)
				{
					a = null;
					return;
				}

				classes.Add((Class)idTable[class_id]);

				AssociationEndMemo ae = new AssociationEndMemo();
				RestoreAssociationAttributes(ref ae, assocend.Current);
				ends.Add(ae);
			}

			// Creates Association that associates all classes in 'classes'
			a = model.Schema.CreateAssociationClass(classes);

			a.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// Sets attributes to all AssociationEnds
			Association c = a;
			RestoreAssociationEndAttributes(ref c, ends);

			// class properties
			expr = xn.Compile(XmlVoc.relativeProperties);
			expr.SetContext(ns);
			XPathNodeIterator it = xn.Select(expr);
			while (it.MoveNext())
			{
				Property p = a.AddAttribute();
				RestoreProperty(p, it.Current);
			}

			// class operations
			expr = xn.Compile(XmlVoc.relativeOperations);
			expr.SetContext(ns);
			it = xn.Select(expr);
			while (it.MoveNext())
			{
				Operation o = a.AddOperation();
				RestoreClassOperation(o, it.Current);
			}

			// --------------------------------------------------------------------
			// Remember DERIVED (PSM) CLASSES for future restoration
			expr = xn.Compile(XmlVoc.relativePsmClasses);
			expr.SetContext(ns);
			XPathNodeIterator psm = xn.Select(expr);
			derivedClasses.Add((PIMClass)c, psm);
			// --------------------------------------------------------------------

			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="a"></param>
		protected void RestoreVisualizationForAssociationClass(string id, AssociationClass a)
		{
			// Find all visualizations for Association class
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVoc.GetVisualizationForClassAssociation(id));
			expr.SetContext(ns);
			XPathNodeIterator visualization = nav.Select(expr);

			// Association class without visualization
			while (visualization.MoveNext())
			{

				// Find appropriate PIM diagram
				XPathExpression parent = visualization.Current.Compile("..");
				parent.SetContext(ns);
				XPathNavigator pn = visualization.Current.SelectSingleNode(parent);
				int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, String.Empty));

				AssociationClassViewHelper h = new AssociationClassViewHelper(project.PIMDiagrams[dno]);

				// ----------- association part -----------------

				h.AssociationViewHelper.UseDiamond = bool.Parse(visualization.Current.GetAttribute(XmlVoc.xmlAttDiamond, String.Empty));
				if (h.AssociationViewHelper.UseDiamond)
				{
					expr = visualization.Current.Compile(XmlVoc.xmlDiamond);
					expr.SetContext(ns);
					XPathNavigator diamond = visualization.Current.SelectSingleNode(expr);
					if (diamond != null)
					{
						PositionableElementViewHelper pev = h.AssociationViewHelper;
						RestoreAppearance(diamond, ref pev);
					}
				}

				// Restores visualization for main association label
				AssociationLabelViewHelper l = h.AssociationViewHelper.MainLabelViewHelper;
				RestoreAssociationLabel(visualization.Current, ref l);

				// Restores visualization for all association ends belonging to the current assocation
				AssociationViewHelper ah = h.AssociationViewHelper;
				RestoreAssociationEnds(visualization.Current, ref ah, a, project.PIMDiagrams[dno]);

				// Restores points
				ObservablePointCollection points_collection;
				XPathExpression points = visualization.Current.Compile(XmlVoc.xmlPoints);
				points.SetContext(ns);
				RestorePoints(visualization.Current.SelectSingleNode(points), out points_collection);
				foreach (rPoint r in points_collection)
					h.Points.Add(r);

				// ----------- class part -----------------

				XPathExpression appearance = visualization.Current.Compile(XmlVoc.xmlAppearanceElement);
				appearance.SetContext(ns);
				RestoreAppearance(visualization.Current.SelectSingleNode(appearance), h);

				// ----------------------------------------

				// Adds restored Association class along with its visualization to the project
				project.PIMDiagrams[dno].AddModelElement(a, h);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uri"></param>
		protected void RestorePIMAssociations(string uri)
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			// For each association found in XML
			while (iterator.MoveNext())
			{
				string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);

				Association a;

				// Model
				RestoreAssociationFromModel(iterator.Current, out a);

				idTable.Add(id, a);
                ((_ImplElement)a).Guid = XCaseGuid.Parse(id);

				// Visualization
				RestoreVisualizationForPIMAssociation(id, a);

				// comments
				XPathExpression comments = iterator.Current.Compile(XmlVoc.relativeComments);
				comments.SetContext(ns);
				RestoreComments(iterator.Current.SelectSingleNode(comments), a);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		protected void RestoreAssociationFromModel(XPathNavigator xn, out Association a)
		{
			// Association ID

			List<Class> classes = new List<Class>();
			List<AssociationEndMemo> ends = new List<AssociationEndMemo>();

			// "association_end"
			XPathExpression expr = xn.Compile(XmlVoc.xmlAssociationEndElement);
			expr.SetContext(ns);
			XPathNodeIterator assocend = xn.Select(expr);

			// Go through all AssociationEnds belonging to the current Association
			while (assocend.MoveNext())
			{
				string class_id = assocend.Current.GetAttribute(XmlVoc.xmlAttClass, String.Empty);

				if (idTable[class_id] == null)
				{
					a = null;
					return;
				}

				classes.Add((Class)idTable[class_id]);

				AssociationEndMemo ae = new AssociationEndMemo();
				RestoreAssociationAttributes(ref ae, assocend.Current);
				ends.Add(ae);
			}

			// Creates Association that associates all classes in 'classes'
			a = model.Schema.AssociateClasses(classes);

			a.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

            // @ontologyEquivalent
            a.OntologyEquivalent = xn.GetAttribute(XmlVoc.xmlAttOntoEquiv, String.Empty);

			// Sets attributes to all AssociationEnds
			RestoreAssociationEndAttributes(ref a, ends);

			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="a"></param>
		protected void RestoreVisualizationForPIMAssociation(string id, Association a)
		{
			// Finds all visualization for PIM association

			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVoc.GetVisualizationForPIMAssociation(id));
			expr.SetContext(ns);
			XPathNodeIterator xit = nav.Select(expr);

			// PIM Association without visualization
			while (xit.MoveNext())
			{
				// Finds appropriate PIM diagram
				XPathExpression parent = xit.Current.Compile("..");
				parent.SetContext(ns);
				XPathNavigator pn = xit.Current.SelectSingleNode(parent);
				int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, String.Empty));

				AssociationViewHelper h = new AssociationViewHelper(project.PIMDiagrams[dno]);

				h.UseDiamond = bool.Parse(xit.Current.GetAttribute(XmlVoc.xmlAttDiamond, String.Empty));
				if (h.UseDiamond)
				{
					expr = xit.Current.Compile(XmlVoc.xmlDiamond);
					expr.SetContext(ns);
					XPathNavigator diamond = xit.Current.SelectSingleNode(expr);
					if (diamond != null)
					{
						PositionableElementViewHelper pev = h;
						RestoreAppearance(diamond, ref pev);
					}
				}

				// Restores visualization for main association label
				AssociationLabelViewHelper l = h.MainLabelViewHelper;
				RestoreAssociationLabel(xit.Current, ref l);

				// Restores visualization for all association ends belonging to the current assocation
				RestoreAssociationEnds(xit.Current, ref h, a, project.PIMDiagrams[dno]);

				// Restores points
				ObservablePointCollection points_collection;
				XPathExpression points = xit.Current.Compile(XmlVoc.xmlPoints);
				points.SetContext(ns);
				RestorePoints(xit.Current.SelectSingleNode(points), out points_collection);
				foreach (rPoint r in points_collection)
					h.Points.Add(r);

				// Adds restored Association along with its visualization to the project

				project.PIMDiagrams[dno].AddModelElement(a, h);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="h"></param>
		/// <param name="a"></param>
		/// <param name="d"></param>
		protected void RestoreAssociationEnds(XPathNavigator xn, ref AssociationViewHelper h, Association a, Diagram d)
		{
			// "association_end"
			XPathExpression expr = xn.Compile(XmlVoc.relativeAssociationEnds);
			expr.SetContext(ns);
			XPathNodeIterator association_ends = xn.Select(expr);

			int i = 0;
			while (association_ends.MoveNext())
			{
				h.AssociationEndsViewHelpers.Add(new AssociationEndViewHelper(d, h, a.Ends[i]));

				AssociationEndViewHelper ae = h.AssociationEndsViewHelpers[i];

				// Multiplicity label visualization
				RestoreMultiplicityLabel(association_ends.Current, ae);

				// Role label visualization
				RestoreRoleLabel(association_ends.Current, ae);

				i++;
			}
		}

		/// <summary>
		/// Restores Multiplicity Label for Association
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="ae"></param>
		protected void RestoreMultiplicityLabel(XPathNavigator xn, AssociationEndViewHelper ae)
		{
			// coordinate_x
			XPathExpression expr = xn.Compile(XmlVoc.xmlCardinalityLabel + "/" + XmlVoc.xmlCoordXElement);
			expr.SetContext(ns);
			XPathNavigator xpn = xn.SelectSingleNode(expr);
			ae.MultiplicityLabelViewHelper = new AssociationLabelViewHelper(ae.Diagram);

			ae.MultiplicityLabelViewHelper.LabelVisible = true;
			ae.MultiplicityLabelViewHelper.X = Double.Parse(xpn.Value);

			// coordinate_y
			expr = xn.Compile(XmlVoc.xmlCardinalityLabel + "/" + XmlVoc.xmlCoordYElement);
			expr.SetContext(ns);
			xpn = xn.SelectSingleNode(expr);
			ae.MultiplicityLabelViewHelper.Y = Double.Parse(xpn.Value);

		}

		/// <summary>
		/// Restores Role Label for Association
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="ae"></param>
		protected void RestoreRoleLabel(XPathNavigator xn, AssociationEndViewHelper ae)
		{
			// coordinate_x
			XPathExpression expr = xn.Compile(XmlVoc.xmlRoleLabel + "/" + XmlVoc.xmlCoordXElement);
			expr.SetContext(ns);
			XPathNavigator xpn = xn.SelectSingleNode(expr);
			ae.RoleLabelViewHelper = new AssociationLabelViewHelper(ae.Diagram);

			ae.RoleLabelViewHelper.LabelVisible = true;
			ae.RoleLabelViewHelper.X = Double.Parse(xpn.Value);

			// coordinate_y
			expr = xn.Compile(XmlVoc.xmlRoleLabel + "/" + XmlVoc.xmlCoordYElement);
			expr.SetContext(ns);
			xpn = xn.SelectSingleNode(expr);
			ae.RoleLabelViewHelper.Y = Double.Parse(xpn.Value);

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn"></param>
		/// <param name="h"></param>
		protected void RestoreAssociationLabel(XPathNavigator xn, ref AssociationLabelViewHelper h)
		{
			XPathExpression expr = xn.Compile(XmlVoc.xmlNameLabel);
			expr.SetContext(ns);
			XPathNavigator xpn = xn.SelectSingleNode(expr);
			PositionableElementViewHelper pev = h;
			RestoreAppearance(xpn, ref pev);

		}

		protected void RestoreAppearance(XPathNavigator xpn, ref PositionableElementViewHelper h)
		{
			// coordinate_x
			XPathExpression expr = xpn.Compile(XmlVoc.xmlCoordXElement);
			expr.SetContext(ns);
			XPathNavigator r = xpn.SelectSingleNode(expr);
			h.X = Double.Parse(r.ToString());

			// coordinate_y
			expr = xpn.Compile(XmlVoc.xmlCoordYElement);
			expr.SetContext(ns);
			r = xpn.SelectSingleNode(expr);
			h.Y = Double.Parse(r.ToString());

			// height
			expr = xpn.Compile(XmlVoc.xmlHeightElement);
			expr.SetContext(ns);
			r = xpn.SelectSingleNode(expr);
			if (!r.ToString().Equals(String.Empty))
				h.Height = Double.Parse(r.ToString());

			//width
			expr = xpn.Compile(XmlVoc.xmlWidthElement);
			expr.SetContext(ns);
			r = xpn.SelectSingleNode(expr);
			if (!r.ToString().Equals(String.Empty))
				h.Width = Double.Parse(r.ToString());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ae"></param>
		/// <param name="xpn"></param>
		protected void RestoreAssociationAttributes(ref AssociationEndMemo ae, XPathNavigator xpn)
		{
			ae.id = xpn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			ae.name = xpn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			ae.aggregation = xpn.GetAttribute(XmlVoc.xmlAttAggregation, String.Empty);
			ae.def = xpn.GetAttribute(XmlVoc.xmlAttDefault, String.Empty);
			ae.default_value = xpn.GetAttribute(XmlVoc.xmlAttDefaultValue, String.Empty);

			ae.is_composite = xpn.GetAttribute(XmlVoc.xmlAttIsComposite, String.Empty);
			ae.is_derived = xpn.GetAttribute(XmlVoc.xmlAttIsDerived, String.Empty);
			ae.is_ordered = xpn.GetAttribute(XmlVoc.xmlAttIsOrdered, String.Empty);
			ae.is_readonly = xpn.GetAttribute(XmlVoc.xmlAttIsReadOnly, String.Empty);
			ae.is_unique = xpn.GetAttribute(XmlVoc.xmlAttIsUnique, String.Empty);

			ae.lower = xpn.GetAttribute(XmlVoc.xmlAttLower, String.Empty);

			ae.upper = xpn.GetAttribute(XmlVoc.xmlAttUpper, String.Empty);
			ae.visibility = xpn.GetAttribute(XmlVoc.xmlAttVisibility, String.Empty);

			ae.type = xpn.GetAttribute(XmlVoc.xmlAttType, String.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="ends"></param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreAssociationEndAttributes(ref Association a, List<AssociationEndMemo> ends)
		{
			int i = 0;
			List<AssociationEndMemo>.Enumerator e = ends.GetEnumerator();
			while (e.MoveNext())
			{
				idTable.Add(e.Current.id, a.Ends[i]);
                ((_ImplElement)a.Ends[i]).Guid = XCaseGuid.Parse(e.Current.id);

				// @type
				string type = e.Current.type;
				if (type.Equals("null"))
					a.Ends[i].Type = null;
				else
				{
					if (!idTable.ContainsKey(type))
					{
						throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
					}
					a.Ends[i].Type = (DataType)idTable[type];
				}

				a.Ends[i].Lower = UInt32.Parse(e.Current.lower);
				a.Ends[i].Upper = UnlimitedNatural.Parse(e.Current.upper);
				a.Ends[i].Visibility = (VisibilityKind)
									   Enum.Parse(typeof(VisibilityKind), e.Current.visibility);
				a.Ends[i].Name = e.Current.name;
				a.Ends[i].Default = e.Current.def;
				a.Ends[i].Aggregation = (AggregationKind)
										Enum.Parse(typeof(AggregationKind), e.Current.aggregation);

				a.Ends[i].IsComposite = bool.Parse(e.Current.is_composite);
				a.Ends[i].IsDerived = bool.Parse(e.Current.is_derived);
				a.Ends[i].IsOrdered = bool.Parse(e.Current.is_ordered);
				a.Ends[i].IsReadOnly = bool.Parse(e.Current.is_readonly);
				a.Ends[i].IsUnique = bool.Parse(e.Current.is_unique);

				i++;
			}
		}

		/// <summary>
		/// Restoration of <see cref="PSMClassUnion"/>
		/// </summary>
		/// <param name="xn">Reference to <see cref="PSMClassUnion"/> element in XML document</param>
		/// <param name="c">Component to restore <see cref="PSMClassUnion"/> to</param>
		/// <returns></returns>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected PSMClassUnion RestoreClassUnion(XPathNavigator xn, PSMSuperordinateComponent c)
		{
			if (xn == null)
				return null;

			if (!xn.Name.Equals(XmlVoc.xmlPSMClassUnion))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			PSMClassUnion cu = c.CreateClassUnion();

			// @id
			string id = xn.GetAttribute(XmlVoc.xmlAttID, String.Empty);
			idTable.Add(id, cu);
            ((_ImplElement)cu).Guid = XCaseGuid.Parse(id);

			// @name
			cu.Name = xn.GetAttribute(XmlVoc.xmlAttName, String.Empty);

			// diagram
			cu.Diagram = c.Diagram;

			// ------------- visualization  -------------

			XPathExpression expr = xn.Compile(XmlVoc.GetVisualizationForPSMClassUnion(id));
			expr.SetContext(ns);
			XPathNavigator visualization = xn.SelectSingleNode(expr);
			RestoreVisualizationForPSMElement(visualization, id, cu);

			// ------------- other connected elements ------------- 

			// components 
			expr = xn.Compile(XmlVoc.xmlComponentsElement + "/*");
			expr.SetContext(ns);
			XPathNodeIterator components = xn.Select(expr);

			while (components.MoveNext())
			{
				// psm class
                if (components.Current.Name.Equals(XmlVoc.xmlPsmClassElement))
                {
                    string class_ref = components.Current.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
                    if (!idTable.ContainsKey(class_ref))
                    {
                        throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
                    }

                    cu.Components.Add((PSMClass)idTable[class_ref]);
                }
                else
                {
                    // class union
                    if (components.Current.Name.Equals(XmlVoc.xmlPSMClassUnion))
                    {
                        PSMClassUnion cu2 = RestoreClassUnion(components.Current, c);
                        if (cu2 != null)
                        {
                            cu.Components.Add(cu2);
                        }
                    }
                    else
                    {
                        throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
                    }
                }
			}

			// Comments
			XPathExpression comments = xn.Compile(XmlVoc.relativeComments);
			comments.SetContext(ns);
			RestoreComments(xn.SelectSingleNode(comments), cu);

			return cu;
		}

		/// <summary>
		/// Restoration of all PSM Associations
		/// </summary>
		/// <param name="xn"></param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestorePSMAssociations(XPathNavigator xn)
		{
			// Select all PSM associations 
			XPathExpression expr = xn.Compile(XmlVoc.allPSMAssociations);
			expr.SetContext(ns);
			XPathNodeIterator psm_associations = xn.Select(expr);

			// Restores all PSM associations
			while (psm_associations.MoveNext())
			{
				XPathNavigator assoc = psm_associations.Current;

				string id = assoc.GetAttribute(XmlVoc.xmlAttID, String.Empty);

				// parent
				XPathExpression expr_parent = assoc.Compile(XmlVoc.xmlParentElement);
				expr_parent.SetContext(ns);
				XPathNavigator parent = assoc.SelectSingleNode(expr_parent);
				string parent_id = parent.GetAttribute(XmlVoc.xmlAttRef, String.Empty);

				if (!idTable.ContainsKey(parent_id))
				{
					foreach (KeyValuePair<string, Element> kvp in idTable.OrderBy(pair => int.Parse(pair.Key)))
					{
						System.Diagnostics.Debug.WriteLine(String.Format("{0} => {1}", kvp.Key, kvp.Value));
					}

					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
					//System.Windows.MessageBox.Show("PSM association involving a PSM class/container without visualization");
					//continue;
				}

				string index = assoc.GetAttribute(XmlVoc.xmlAttIndex, String.Empty);
				if (index.Equals(String.Empty))
					index = "0";

				PSMSuperordinateComponent c = (PSMSuperordinateComponent)idTable[parent_id];

				// ---------- class unions --------------
				// Restore all nested class unions

				XPathExpression class_union = assoc.Compile(XmlVoc.xmlPSMClassUnion);
				class_union.SetContext(ns);
				XPathNavigator cu = assoc.SelectSingleNode(class_union);
				RestoreClassUnion(cu, c);

				// ---------- class unions --------------

				PSMAssociation p = (PSMAssociation)c.AddComponent(PSMAssociationFactory.Instance, int.Parse(index));

				// diagram
				p.Diagram = c.Diagram;

				// child
				XPathExpression expr_child = assoc.Compile(XmlVoc.xmlChildElement);
				expr_child.SetContext(ns);
				XPathNavigator child = assoc.SelectSingleNode(expr_child);
				string child_id = child.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
				if (!idTable.ContainsKey(child_id))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}
				p.Child = (PSMAssociationChild)idTable[child_id];

				// @min
				if (!child.GetAttribute(XmlVoc.xmlAttMin, String.Empty).Equals(String.Empty))
					p.Lower = UInt32.Parse(child.GetAttribute(XmlVoc.xmlAttMin, String.Empty));

				// @max
				p.Upper = UnlimitedNatural.Parse(child.GetAttribute(XmlVoc.xmlAttMax, String.Empty));

				//generalizations
				XPathExpression usedg = assoc.Compile(XmlVoc.xmlUsedGeneralizations + "/" + XmlVoc.xmlUsedGeneralization);
				usedg.SetContext(ns);
				XPathNodeIterator gens = assoc.Select(usedg);
				while (gens.MoveNext())
				{
					string gref = gens.Current.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
					if (!idTable.ContainsKey(gref))
					{
						throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
					}

					Generalization g = (Generalization)idTable[gref];
					p.UsedGeneralizations.Add(g);
					g.ReferencingPSMAssociations.Add(p);
				}


				// nesting joins
				XPathExpression expr_njs = assoc.Compile(XmlVoc.relativeNestingJoins);
				expr_njs.SetContext(ns);
				XPathNodeIterator njs = assoc.Select(expr_njs);
				while (njs.MoveNext())
				{
					RestoreNestingJoin(njs.Current, p);
				}

			    idTable[id] = p;

				RestorePSMAssociationVisualization(p, id);

			} // while
		}

		/// <summary>
		///  Restoration of one Nesting Join
		/// </summary>
		/// <param name="xpn">Reference to Nesting Join element in XML document</param>
		/// <param name="p">PSM Association to add restored nesting join to</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreNestingJoin(XPathNavigator xpn, PSMAssociation p)
		{
			// @core_class_ref
			string cc = xpn.GetAttribute(XmlVoc.xmlAttCoreClassRef, String.Empty);
			if (!idTable.ContainsKey(cc))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			// For backward compatibility
			if (!(idTable[cc] is PIMClass))
				return;

			NestingJoin nj = p.AddNestingJoin((PIMClass)idTable[cc]);

			// child
			XPathExpression expr_child = xpn.Compile(XmlVoc.xmlChildElement);
			expr_child.SetContext(ns);
			XPathNavigator child = xpn.SelectSingleNode(expr_child);

			RestorePIMPath(child, nj.Child);

			// parent
			XPathExpression expr_parent = xpn.Compile(XmlVoc.xmlParentElement);
			expr_parent.SetContext(ns);
			XPathNavigator parent = xpn.SelectSingleNode(expr_parent);

			RestorePIMPath(parent, nj.Parent);

			// context
			XPathExpression context = xpn.Compile(XmlVoc.xmlContext + "/" + XmlVoc.xmlPimPath);
			context.SetContext(ns);
			XPathNodeIterator pim_paths = xpn.Select(context);
			while (pim_paths.MoveNext())
			{
				PIMPath context_path = nj.AddContextPath();
				RestorePIMPath(pim_paths.Current, context_path);
			}

		}

		/// <summary>
		/// Restoration of one PIM Path
		/// </summary>
		/// <param name="xn">Reference to PIM Path element in XML document</param>
		/// <param name="path"><see cref="PIMPath"/> to restore</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestorePIMPath(XPathNavigator xn, PIMPath path)
		{
			XPathExpression pim_step = xn.Compile(XmlVoc.xmlPimStep);
			pim_step.SetContext(ns);
			XPathNodeIterator steps = xn.Select(pim_step);
			while (steps.MoveNext())
			{
				// @start_ref
				string start = steps.Current.GetAttribute(XmlVoc.xmlAttStartRef, String.Empty);
				if (!idTable.ContainsKey(start) || !(idTable[start] is PIMClass))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}

				// @end_ref
				string end = steps.Current.GetAttribute(XmlVoc.xmlAttEndRef, String.Empty);
				if (!idTable.ContainsKey(end) || !(idTable[end] is PIMClass))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}

				// @association_ref
				string assoc = steps.Current.GetAttribute(XmlVoc.xmlAttAssociation, String.Empty);
				if (!idTable.ContainsKey(assoc) || !(idTable[assoc] is Association))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}

				path.AddStep((PIMClass)idTable[start], (PIMClass)idTable[end], (Association)idTable[assoc]);
			}
		}

		/// <summary>
		/// Restoration of visualization for a <see cref="PSMAssociation"/>
		/// </summary>
		/// <param name="p"></param>
		/// <param name="id"></param>
		protected void RestorePSMAssociationVisualization(PSMAssociation p, string id)
		{
			XPathNavigator xn = document.CreateNavigator();
			// Restore visualization for PSM Association
			XPathExpression expr = xn.Compile(XmlVoc.GetVisualizationForPSMAssociation(id));
			expr.SetContext(ns);
			XPathNavigator visualization = xn.SelectSingleNode(expr);

			// No visualization
			if (visualization == null)
				return;

			expr = visualization.Compile("..");
			expr.SetContext(ns);
			int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, String.Empty));

			PSMAssociationViewHelper view = new PSMAssociationViewHelper(project.PSMDiagrams[diagno]);

			XPathExpression appearance = visualization.Compile(XmlVoc.xmlAppearanceElement);
			appearance.SetContext(ns);
			RestoreAppearance(visualization.SelectSingleNode(appearance), view);

			// Restores points
			ObservablePointCollection points_collection;
			XPathExpression points = visualization.Compile(XmlVoc.xmlPoints);
			points.SetContext(ns);
			RestorePoints(visualization.SelectSingleNode(points), out points_collection);
			foreach (rPoint r in points_collection)
				view.Points.Add(r);

			// multiplicity label
			expr = visualization.Compile(XmlVoc.xmlMultiplicityLabel + "/" + XmlVoc.xmlCoordXElement);
			expr.SetContext(ns);
			XPathNavigator xpn = visualization.SelectSingleNode(expr);
			view.MultiplicityLabelViewHelper = new AssociationLabelViewHelper(view.Diagram);

			view.MultiplicityLabelViewHelper.LabelVisible = true;
			view.MultiplicityLabelViewHelper.X = Double.Parse(xpn.Value);

			expr = visualization.Compile(XmlVoc.xmlMultiplicityLabel + "/" + XmlVoc.xmlCoordYElement);
			expr.SetContext(ns);
			xpn = visualization.SelectSingleNode(expr);
			view.MultiplicityLabelViewHelper.Y = Double.Parse(xpn.Value);


			project.PSMDiagrams[diagno].AddModelElement(p, view);
		}

		protected void RestoreGeneralizations(string uri)
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(uri);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			// generalizations
			while (iterator.MoveNext())
			{
				// Restore generalization itself

				string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);
				string general_id = iterator.Current.GetAttribute(XmlVoc.xmlAttGeneral, String.Empty);
				string specific_id = iterator.Current.GetAttribute(XmlVoc.xmlAttSpecific, String.Empty);
				bool is_substitable = bool.Parse(iterator.Current.GetAttribute(XmlVoc.xmlAttIsSubstitable, String.Empty));

				Class general = (Class)idTable[general_id];
				Class specific = (Class)idTable[specific_id];

				Generalization g = model.Schema.SetGeneralization(general, specific);

				idTable.Add(id, g);
                ((_ImplElement)g).Guid = XCaseGuid.Parse(id);

				g.IsSubstituable = is_substitable;

				// Restore its visualization
				// Finds all visualizations for PIM generalization

				if (general is PIMClass)
					expr = nav.Compile(XmlVoc.GetVisualizationForPIMGeneralization(id));
				else
					expr = nav.Compile(XmlVoc.GetVisualizationForPSMGeneralization(id));

				expr.SetContext(ns);
				XPathNodeIterator visualization = nav.Select(expr);

				while (visualization.MoveNext())
				{

					// Finds appropriate PIM diagram
					XPathExpression parent = visualization.Current.Compile("..");
					parent.SetContext(ns);
					XPathNavigator pn = visualization.Current.SelectSingleNode(parent);
					int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, String.Empty));

					Diagram d;
					if (general is PIMClass)
						d = project.PIMDiagrams[dno];
					else
						d = project.PSMDiagrams[dno];

					GeneralizationViewHelper h = new GeneralizationViewHelper(d);

					// Restores points
					ObservablePointCollection points;
					expr = visualization.Current.Compile(XmlVoc.xmlPoints);
					expr.SetContext(ns);
					RestorePoints(visualization.Current.SelectSingleNode(expr), out points);
					foreach (rPoint r in points)
					{
						h.Points.Add(r);
					}

					if (general is PIMClass)
						project.PIMDiagrams[dno].AddModelElement(g, h);
					else
						project.PSMDiagrams[dno].AddModelElement(g, h);


				}
				// comments
				XPathExpression comments = iterator.Current.Compile(XmlVoc.relativeComments);
				comments.SetContext(ns);
				RestoreComments(iterator.Current.SelectSingleNode(comments), g);
			}

		}

		/// <summary>
		/// Restores the of PSM Diagram (inserts ids into rootTable).
		/// </summary>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreRoots()
		{
			foreach (PSMDiagram d in project.PSMDiagrams)
			{
				if (!rootTable.ContainsKey(d))
				{
					throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				}

				List<string> rootIds = rootTable[d];
				foreach (string id in rootIds)
				{
				    if (!idTable.ContainsKey(id))
				    {
				        throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
				    }
				    d.Roots.Add((PSMClass)idTable[id]);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xn">Pointer to the 'points' element in XML</param>
		/// <param name="p">Collection to fill with read points</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestorePoints(XPathNavigator xn, out ObservablePointCollection p)
		{
			p = new ObservablePointCollection();

			if (xn == null)
				return;

			if (!xn.Name.Equals(XmlVoc.xmlPoints))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			XPathExpression expr = xn.Compile(XmlVoc.xmlPoint);
			expr.SetContext(ns);
			XPathNodeIterator points = xn.Select(expr);
			while (points.MoveNext())
			{
				// coordinate_x
				expr = xn.Compile(XmlVoc.xmlCoordXElement);
				expr.SetContext(ns);
				int x = int.Parse(points.Current.SelectSingleNode(expr).Value);

				// coordinate_y
				expr = xn.Compile(XmlVoc.xmlCoordYElement);
				expr.SetContext(ns);
				int y = int.Parse(points.Current.SelectSingleNode(expr).Value);

				p.Add(new rPoint(x, y));
			}

		}

		/// <summary>
		/// Restores visual attributes for PIM or PSM class. 
		/// The restored attributes are X and Y coordinates, width and height.
		/// </summary>
		/// <param name="xn">Pointer to the 'appearance' element in XML document</param>
		/// <param name="h">ViewHelper to fill with data read from XML document</param>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		protected void RestoreAppearance(XPathNavigator xn, PositionableElementViewHelper h)
		{
			if (!xn.Name.Equals(XmlVoc.xmlAppearanceElement))
			{
				throw new DeserializationException(SerializationErrors.DES_UNKNOWN);
			}

			// coordinate_x
			XPathExpression expr = xn.Compile(XmlVoc.xmlCoordXElement);
			expr.SetContext(ns);
			XPathNavigator xpn = xn.SelectSingleNode(expr);
			h.X = Double.Parse(xpn.ToString());

			// coordinate_y
			expr = xn.Compile(XmlVoc.xmlCoordYElement);
			expr.SetContext(ns);
			xpn = xn.SelectSingleNode(expr);
			h.Y = Double.Parse(xpn.ToString());

			// height
			expr = xn.Compile(XmlVoc.xmlHeightElement);
			expr.SetContext(ns);
			xpn = xn.SelectSingleNode(expr);
			if (!xpn.ToString().Equals(String.Empty))
				h.Height = Double.Parse(xpn.ToString());

			// width
			expr = xn.Compile(XmlVoc.xmlWidthElement);
			expr.SetContext(ns);
			xpn = xn.SelectSingleNode(expr);
			if (!xpn.ToString().Equals(String.Empty))
				h.Width = Double.Parse(xpn.ToString());

		}

		#endregion

		#region validation

		/// <summary>
		/// Error message in the case of input XML invalidity
		/// </summary>
		protected static String xmlValidationErrorMessage;

		protected string FileName { get; set; }

		/// <summary>
		/// Determines if the input XML is the valid one [against internal schema XCaseSchema.xsd]
		/// </summary>
		protected static bool isPassedXmlValid = true;

		/// <summary>
		/// Validates input XML (describing stored project) against internal XML Schema file
		/// </summary>
		/// <param name="input">Stream with input XML</param>
		/// <param name="message">Error message if the passed XML is invalid</param>
		/// <returns>True if the passed XML is valid, false otherwise</returns>
		public bool ValidateXML(Stream input, ref String message)
		{
			return Validate(input, ref message);
		}

		/// <summary>
		/// Validates input XML (describing stored project) against internal XML Schema file
		/// </summary>
		/// <param name="file">Name of XML file or Stream with input XML</param>
		/// <param name="message">Error message if the passed file is invalid</param>
		/// <returns>True if the passed XML is valid, false otherwise</returns>
		public bool ValidateXML(string file, ref String message)
		{
			return Validate(file, ref message);
		}

		protected bool Validate(object input, ref String message)
		{
			// TODO: it would be better to have two separate schemas rather than one choice schema 

			// Load XML Schema file describing the correct XML file
			byte[] b = Encoding.GetEncoding("windows-1250").GetBytes(Resources.XCaseSchema);
			MemoryStream m = new MemoryStream(b);
			XmlReader schema = new XmlTextReader(m);

			XmlReaderSettings schemaSettings = new XmlReaderSettings();
			schemaSettings.Schemas.Add(XmlVoc.defaultNamespace, schema);
			schemaSettings.ValidationType = ValidationType.Schema;

			// Event handler called when an error occurs while validating
			schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;

			XmlReader xmlfile;
			if (input is string)
				xmlfile = XmlReader.Create((string)input, schemaSettings);
			else
				if (input is Stream)
					xmlfile = XmlReader.Create((Stream)input, schemaSettings);
				else
					return false;

			try
			{
				while (xmlfile.Read())
				{
				}
			}
			catch
			{
				isPassedXmlValid = false;
				xmlValidationErrorMessage = "Not a valid XML file";
			}
			finally
			{
				xmlfile.Close();
				schema.Close();
				m.Dispose();
				xmlfile.Close();
			}

			if (isPassedXmlValid)
				return true;
			else
			{
				message += xmlValidationErrorMessage;
				isPassedXmlValid = true;
				return false;
			}
		}

		protected static void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			isPassedXmlValid = false;
			xmlValidationErrorMessage = e.Message;
		}

	    public static bool VersionsEqual(string fileName, out string v1, out string v2)
	    {
            XPathDocument doc = new XPathDocument(fileName);
            XPathNavigator nav = doc.CreateNavigator();
            XmlNamespaceManager ns = new XmlNamespaceManager(nav.NameTable);
            XmlVocVersions xmlvoc = new XmlVocVersions();
            ns.AddNamespace(xmlvoc.defaultPrefix, xmlvoc.defaultNamespace);

            XPathExpression expr = nav.Compile(xmlvoc.xmlRootElement);
            expr.SetContext(ns);
            XPathNavigator version = nav.SelectSingleNode(expr);
            v2 = SchemaVersion;
            v1 = version.GetAttribute(xmlvoc.xmlAttVersion, String.Empty);
            if (String.IsNullOrEmpty(v1))
                v1 = "1.0";
            if (v1 != v2)
            {
                return false;
            }
            else return true; 
	    }

		#endregion

		/// <summary>
		/// Restores basic primitive types
		/// </summary>
		protected void RestorePrimitiveTypes()
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression ptypes = nav.Compile(((XmlVocNoVersions)XmlVoc).selectPrimitiveTypes);
			ptypes.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(ptypes);

			while (iterator.MoveNext())
			{
				// @id
				string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, String.Empty);

				SimpleDataType d = model.Schema.AddPrimitiveType();
				idTable.Add(id, d);
                ((_ImplElement)d).Guid = XCaseGuid.Parse(id);

				// @name
				d.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty);

				// @description
				d.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, String.Empty);
			}
		}

		protected void CreatePSMDiagrams()
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVoc.allPSMdiagrams);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			// For each stored PSM diagram create new PSM diagram
			while (iterator.MoveNext())
			{
				PSMDiagram diag = new PSMDiagram(iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty));
				string diagramId = iterator.Current.GetAttribute(XmlVoc.xmlAttDiagramId, String.Empty);
                diag.Guid = XCaseGuid.Parse(diagramId);
				if (!String.IsNullOrEmpty(diagramId))
				{
					diagramIdTable[diagramId] = diag;
				}

			    string targetNamespace = iterator.Current.GetAttribute(XmlVoc.xmlAttTargetNamespace, string.Empty);
			    if (!String.IsNullOrEmpty(targetNamespace))
			    {
			        diag.TargetNamespace = targetNamespace;
			    }

				project.AddDiagram(diag);
				// mainwindow.propertiesWindow.BindDiagram(ref mainwindow.dockManager);


				// Roots
				expr = iterator.Current.Compile(XmlVoc.xmlRoots + "/" + XmlVoc.xmlRoot);
				expr.SetContext(ns);
				XPathNodeIterator rit = iterator.Current.Select(expr);
				List<string> roots = new List<string>();
				while (rit.MoveNext())
				{
					string id = rit.Current.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
					roots.Add(id);
				}
				rootTable.Add(diag, roots);
			}
		}

		protected void CreatePIMDiagrams()
		{
			XPathNavigator nav = document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVoc.allPIMdiagrams);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			// For each stored PIM diagram create new PIM diagram
			while (iterator.MoveNext())
			{
				Diagram diag = new PIMDiagram(iterator.Current.GetAttribute(XmlVoc.xmlAttName, String.Empty));
				string diagramId = iterator.Current.GetAttribute(XmlVoc.xmlAttDiagramId, String.Empty);
			    diag.Guid = XCaseGuid.Parse(diagramId);
				if (!String.IsNullOrEmpty(diagramId))
				{
					diagramIdTable[diagramId] = diag;
				}
				project.AddDiagram(diag);
			}
		}
	}

	public class XmlDeserializatorVersions : XmlDeserializatorBase
	{
		protected XmlVocVersions XmlVocVersions
		{
			get
			{
				return (XmlVocVersions)XmlVoc;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDeserializatorVersions"/> class.
		/// </summary>
		public XmlDeserializatorVersions()
		{
			XmlVoc = new XmlVocVersions();
		}

		public static bool UsesVersions(string filename)
		{
			FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
			XmlTextReader r = new XmlTextReader(file);
			r.ReadToFollowing("xc:versions");
			bool result = r.EOF;
			r.Close();
			file.Close();
			file.Dispose();
			return !result;
		}

		/// <summary>
		/// <c>XCase</c> project is loaded from the document.
		/// </summary>
		protected override Project Restore(XPathDocument doc, bool loadingTemplate)
		{
			throw new NotImplementedException("Method or operation is not implemented.");
		}

		VersionManager versionManager;
		
		public VersionManager RestoreVersionedProject(string fileName)
		{
			versionManager = new VersionManager();

			using (FileStream f = new FileStream(fileName, FileMode.Open))
			{
				XPathDocument doc = new XPathDocument(f);
				this.document = doc;
				ns = new XmlNamespaceManager(doc.CreateNavigator().NameTable);
				ns.AddNamespace(XmlVoc.defaultPrefix, XmlVoc.defaultNamespace);

				RestoreVersions();

				RestoreProjects();

				//model = project.Schema.Model;

				//// PIM diagrams
				//CreatePIMDiagrams();
				//// PSM diagrams
				//CreatePSMDiagrams();

				//RestoreProfiles(XmlVoc.allUserProfiles);

				//// All elements in model are restored together with their visualizations
				//RestoreModel();
			}

			return versionManager;
		}

		private void RestoreProjects()
		{
			XPathNavigator nav = this.document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVocVersions.selectVersionedProjects);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			bool loadingFirstVersion = true;
			bool loadingFirstBranch = false; 
			Project projectFirstVersion = null;
			Model modelFirstVersion = null;

            versionedIdTables = new Dictionary<Version, DeserializatorIdTable>();
		    nonVersionedElements = null; 

		    while (iterator.MoveNext())
			{
				XmlVocVersions.currentVersionNumber = int.Parse(iterator.Current.GetAttribute(XmlVocVersions.xmlAttNumber, String.Empty));
				Version currentVersion = versionManager.Versions.First(version => version.Number == XmlVocVersions.currentVersionNumber);

                this.idTable = new DeserializatorIdTable();
			    versionedIdTables[currentVersion] = idTable;

                Project versionedProject = new XmlDeserializator(this.idTable).CreateProjectStub();
			    if (nonVersionedElements == null)
			    {
			        nonVersionedElements = new DeserializatorIdTable();
                    nonVersionedElements.AddFromTable(idTable);
			    }
                project = versionedProject;
                model = versionedProject.Schema.Model;

                ((_ImplVersionedElement)versionedProject).Version = currentVersion; 

				if (loadingFirstVersion)
				{
					projectFirstVersion = versionedProject;
				    modelFirstVersion = model;
					loadingFirstVersion = false;
                    loadingFirstBranch = true;
                    versionManager.SetAsFirstVersion(versionedProject, currentVersion);
                    versionManager.SetAsFirstVersion(model, currentVersion);
                    //versionManager.SetAsFirstVersion(versionedProject.Schema.Profiles[0], currentVersion);
				}
				else
				{
                    versionManager.RegisterBranch(projectFirstVersion, versionedProject, currentVersion, loadingFirstBranch, projectFirstVersion.Version);
                    versionManager.RegisterBranch(modelFirstVersion, model, currentVersion, loadingFirstBranch, modelFirstVersion.Version);
                    //versionManager.RegisterBranch(firstVersion, versionedProject.Schema.Profiles[0], currentVersion, loadingFirstBranch, firstVersion.Version);
					loadingFirstBranch = false; 
				}

				((IVersionManagerImpl)versionManager).AddVersionedProject(currentVersion, versionedProject);
				
                RestoreProfiles(XmlVoc.allUserProfiles);
				CreatePIMDiagrams();
				CreatePSMDiagrams();
				RestoreModel();
			}

			RestoreVersionLinks();
		}

		private void RestoreVersions()
		{
			XPathNavigator nav = this.document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVocVersions.selectVersions);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);

			while (iterator.MoveNext())
			{
				Version v = new Version();
				v.Number = int.Parse(iterator.Current.GetAttribute(XmlVocVersions.xmlAttNumber, String.Empty));

				string createdFrom = iterator.Current.GetAttribute(XmlVocVersions.xmlAttCreatedFrom, String.Empty);
				if (!String.IsNullOrEmpty(createdFrom))
				{
					v.CreatedFrom = versionManager.Versions.FirstOrDefault(version => version.Number == int.Parse(createdFrom));
				}	

				((IVersionManagerImpl)versionManager).AddVersion(v);
			}
		}

		private void RestoreVersionLinks()
		{
			XPathNavigator nav = this.document.CreateNavigator();
			XPathExpression expr = nav.Compile(XmlVocVersions.selectVersionLinks);
			expr.SetContext(ns);
			XPathNodeIterator iterator = nav.Select(expr);
			
			while (iterator.MoveNext())
			{
				string _firstVersionId = iterator.Current.GetAttribute(XmlVocVersions.xmlAttFirstVersion, String.Empty);
				string _versionId = iterator.Current.GetAttribute(XmlVoc.xmlAttVersion, String.Empty);
				string _itemId = iterator.Current.GetAttribute(XmlVoc.xmlAttRef, String.Empty);
				string _itemFirstVersionId = iterator.Current.GetAttribute(XmlVocVersions.xmlAttFirstVersionRef, String.Empty);

				Version version = versionManager.Versions.First(v => v.Number == int.Parse(_versionId));
				Version firstVersion = versionManager.Versions.First(v => v.Number == int.Parse(_firstVersionId));
				
				if (iterator.Current.Name == XmlVocVersions.xmlVersionLinkElement)
				{   
					Element elementFirstVersion = versionedIdTables[firstVersion][_itemFirstVersionId];
                    Element element = versionedIdTables[version][_itemId];

					if (element.FirstVersion == null || element.FirstVersion.Version != firstVersion)
						versionManager.SetAsFirstVersion(elementFirstVersion, firstVersion);

					versionManager.RegisterBranch(elementFirstVersion, element, version, false, null);
				}
				else
				{
					Diagram diagramFirstVersion = diagramIdTable[_itemFirstVersionId];
					Diagram diagram = diagramIdTable[_itemId];

					if (diagram.FirstVersion == null || diagram.FirstVersion.Version != firstVersion)
						versionManager.SetAsFirstVersion(diagramFirstVersion, firstVersion);
					versionManager.RegisterBranch(diagramFirstVersion, diagram, version, false, null);
				}
			}			
		}
	}

	/// <summary>
	/// Class ensures restoration of an <c>XCase</c> project (UML model + visualization) from a XML file
	/// </summary>
	/// <remarks>
	/// <para>
	/// The passed XML file must be valid against the internal schema XCaseSchema.xsd. 
	/// To ensure this, call static method <see cref="XmlDeserializatorBase.ValidateXML(string,ref string)">ValidateXML</see> before starting restoration itself.
	/// </para>
	/// <para>
	/// To restore the file, call RestoreProject method.
	/// </para>
	/// </remarks>
	public class XmlDeserializator : XmlDeserializatorBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XmlDeserializator"/> class.
		/// </summary>
		public XmlDeserializator()
		{
			XmlVoc = new XmlVocNoVersions();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public XmlDeserializator(DeserializatorIdTable idTable) : base(idTable)
		{
			XmlVoc = new XmlVocNoVersions();
		}

		/// <summary>
		/// Creates an empty project.
		/// </summary>
		/// <returns>Empty project</returns>
		/// <exception cref="DeserializationException">DeserializationException thrown when deserialization fails.</exception>
		public Project CreateEmptyProject()
		{
			byte[] emptyProjectBytes =
				Encoding.GetEncoding("utf-8").GetBytes(Resources.EmptyProject);
			MemoryStream emptyProjectStream = new MemoryStream(emptyProjectBytes);

			string msg = SerializationErrors.DES_UNKNOWN_ERROR;
			// First, validates if the file is a valid XCase XML file
			if (!ValidateXML(emptyProjectStream, ref msg))
			{
				throw new DeserializationException(String.Format(SerializationErrors.DES_TEMPLATE_CORRUPTED, msg));
			}

			emptyProjectStream.Position = 0;

			return Restore(new XPathDocument(emptyProjectStream), false);
		}

		/// <summary>
		/// <c>XCase</c> project is loaded from the document.
		/// </summary>
		protected override Project Restore(XPathDocument doc, bool loadingTemplate)
		{
			XPathNavigator nav = doc.CreateNavigator();
			ns = new XmlNamespaceManager(nav.NameTable);
			ns.AddNamespace(XmlVoc.defaultPrefix, XmlVoc.defaultNamespace);

			#region version check

			XPathExpression expr = nav.Compile(XmlVoc.xmlRootElement);
			expr.SetContext(ns);
			XPathNavigator version = nav.SelectSingleNode(expr);
			
			#endregion

			this.document = doc;
			project = loadingTemplate ? new Project("Project1") : CreateProjectStub();
			this.document = doc;

			model = project.Schema.Model;

			CreatePIMDiagrams();
			CreatePSMDiagrams();

			if (loadingTemplate)
			{
				RestorePrimitiveTypes();
				RestoreProfiles(XmlVoc.allProfiles);
				project.TemplateIdTable = idTable.CreateSerializatorTable(idTable);
			}
			else
			{
				RestoreProfiles(XmlVoc.allUserProfiles);
			}

			// All elements in model are restored together with their visualizations
			RestoreModel();

			return project;
		}

		public static void FillIdTableFromProjectStub(Model model, Dictionary<Element, XCaseGuid> idTable)
		{
			XmlDeserializator dummyDeserializator = new XmlDeserializator();
			dummyDeserializator.CreateProjectStub();
			foreach (KeyValuePair<string, Element> kvp in dummyDeserializator.idTable)
			{
				SimpleDataType type = kvp.Value as SimpleDataType;
				SimpleDataType primitiveType;
				if (type != null && model.Schema.TryFindPrimitiveTypeByName(type.Name, out primitiveType))
				{
				    XCaseGuid id = XCaseGuid.Parse(kvp.Key);
				    idTable.Add(primitiveType, id);
                    ((_ImplElement)primitiveType).Guid = id;
				}
			}
		}
	}
}