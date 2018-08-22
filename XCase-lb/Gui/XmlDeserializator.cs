using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using XCase.Model;
using XCase.Controller;
using System.Xml.XPath;

namespace XCase.Gui
{
    /// <summary>
    /// Possible types of visual diagrams in XCase GUI
    /// </summary>
    public enum DiagramType
    {
        /// <summary>
        /// PIM Diagram
        /// </summary>
        PIMDiagram,

        /// <summary>
        /// PSM Diagram
        /// </summary>
        PSMDiagram
    }

    /// <summary>
    /// Memo struct for AssociationEnd class
    /// </summary>
    public struct AssociationEndMemo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Aggregation
        /// </summary>
        public string aggregation { get; set; }
        /// <summary>
        /// Deafult
        /// </summary>
        public string def { get; set; }
        /// <summary>
        /// Default value
        /// </summary>
        public string default_value { get; set; }
        /// <summary>
        /// IsComposite
        /// </summary>
        public string is_composite { get; set; }
        public string is_derived { get; set; }
        public string is_ordered { get; set; }
        public string is_readonly { get; set; }
        public string is_unique { get; set; }
        public string lower { get; set; }
        public string upper { get; set; }
        public string cardinality { get; set; }
        public string type { get; set; }
        public string visibility { get; set; }

        /// <summary>
        ///  X coordinate
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        public double Width { get; set; }
    }

    /// <summary>
    ///  Exception caused by bad references during the restoration
    /// </summary>
    public class DeserializationException : SystemException
    { }

    /// <summary>
    /// Class ensures restoration of an XCase project (UML model + visualization) from a XML file
    /// </summary>
    /// <remarks>
    /// <para>
    /// The passed XML file must be valid against the internal schema XCaseSchema.xsd. 
    /// To ensure this, call static method <see cref="ValidateXML(string,ref string)">ValidateXML</see> before starting restoration itself.
    /// </para>
    /// <para>
    /// To restore the file, call RestoreProject method.
    /// </para>
    /// </remarks>
    public class XmlDeserializator
    {
        #region XmlDeserializator attributes
        /// <summary>
        ///  Project being restored
        /// </summary>
        private Project project;


        /// <summary>
        ///  Model being restored
        /// </summary>
        private Model.Model model;

        /// <summary>
        /// Window where to place restored visualization diagrams
        /// </summary>
        private MainWindow mainwindow;

        /// <summary>
        ///  XML document with XCase project to restore
        /// </summary>
        private XPathDocument doc;

        /// <summary>
        /// Global namespace manager used for restoration
        /// </summary>
        private XmlNamespaceManager ns;

        /// <summary>
        /// XML Schema version
        /// </summary>
        private string schemaVersion = "1.0";

        /// <summary>
        /// Table of all restored elements that have @id in XCase XML file
        /// <list type="">
        /// <item>Key = ID</item>
        /// <item>Value = Element with this ID</item>
        /// </list>
        /// </summary>
        private Hashtable idTable = new Hashtable();

        /// <summary>
        /// Table of diagrams and its root PSM classes
        /// </summary>
        /// <list type="">
        /// <item>Key = PIM Class</item>
        /// <item>Value = XPathNodeIterator pointing to the list of its derived PSM classes</item>
        /// </list>
        private Hashtable rootTable = new Hashtable();

        /// <summary>
        /// Table of PIM classes and related derived PSM classes
        /// </summary>
        /// <list type="">
        /// <item>Key = PIM Class</item>
        /// <item>Value = List of its roots (IDs of root psm classes)</item>
        /// </list>
        private Hashtable derivedClasses = new Hashtable();

        /// <summary>
        /// Determines if the input XML is the valid one [against internal schema XCaseSchema.xsd]
        /// </summary>
        private static bool isPassedXmlValid = true;

        /// <summary>
        /// Error message in the case of input XML invalidity
        /// </summary>
        private static String xmlValidationErrorMessage;

        #endregion

        /// <summary>
        /// Called in the case of any problem during the restoration
        /// </summary>
        private void DeserializationError()
        {
            // temporary, just for development
            throw new DeserializationException();
        }

        /// <summary>
        /// Initialization of deserializator
        /// </summary>
        public XmlDeserializator()
        { }

        /// <summary>
        /// XCase project is loaded from the XML file and displayed in the editor. </para>
        /// </summary>
        /// <param name="file">XML file containing serialized XCase project</param>
        /// <param name="window">MainWindow where to display restored PIM and PSM diagrams</param>
        public bool RestoreProject(string file, MainWindow window)
        {
            return Restore(file, window);
        }

        /// <summary>
        /// XCase project is loaded from the stream and displayed in the editor.
        /// </summary>
        /// <param name="input">Stream containing serialized XCase project</param>
        /// <param name="window">MainWindow where to display restored PIM and PSM diagrams</param>
        public bool RestoreProject(System.IO.Stream input, MainWindow window)
        {
            return Restore(input, window);
        }

        /// <summary>
        /// Recreates all PIM and PSM diagrams
        /// </summary>
        /// <param name="input"></param> 
        private bool Restore(object input, MainWindow window)
        {
			//try
			//{
                mainwindow = window;
                project = mainwindow.CurrentProject;
                model = project.Schema.Model;

                if (input is string)
                    doc = new XPathDocument((string)input);
                else
                    if (input is System.IO.Stream)
                        doc = new XPathDocument((System.IO.Stream)input);
                    else
                    {
                        return false;
                    }

                XPathNavigator nav = doc.CreateNavigator();
                ns = new XmlNamespaceManager(nav.NameTable);

                ns.AddNamespace(XmlVoc.defaultPrefix, XmlVoc.defaultNamespace);

                // Version check
                XPathExpression expr = nav.Compile(XmlVoc.xmlRootElement);
                expr.SetContext(ns);
                XPathNavigator version = nav.SelectSingleNode(expr);
                string v = version.GetAttribute(XmlVoc.xmlAttVersion, "");
                if (!v.Equals(schemaVersion))
                    return false;

                // PIM diagrams
                expr = nav.Compile(XmlVoc.allPIMdiagrams);
                expr.SetContext(ns);
                XPathNodeIterator iterator = nav.Select(expr);

                mainwindow.propertiesWindow.BindDiagram(ref mainwindow.dockManager);

                // For each stored PIM diagram create new PIM diagram
                while (iterator.MoveNext())
                {
                    Diagram diag = new PIMDiagram(iterator.Current.GetAttribute(XmlVoc.xmlAttName, ""));
                    project.AddDiagram(diag);

                    // mainwindow.propertiesWindow.BindDiagram(ref mainwindow.dockManager);
                }

                // PSM diagrams
                expr = nav.Compile(XmlVoc.allPSMdiagrams);
                expr.SetContext(ns);
                iterator = nav.Select(expr);



                // For each stored PSM diagram create new PSM diagram
                while (iterator.MoveNext())
                {
                    Diagram diag = new PSMDiagram(iterator.Current.GetAttribute(XmlVoc.xmlAttName, ""));
                    project.AddDiagram(diag);
                    // mainwindow.propertiesWindow.BindDiagram(ref mainwindow.dockManager);

                    // Roots
                    expr = iterator.Current.Compile(XmlVoc.xmlRoots + "/" + XmlVoc.xmlRoot);
                    expr.SetContext(ns);
                    XPathNodeIterator rit = iterator.Current.Select(expr);
                    List<string> roots = new List<string>();
                    while (rit.MoveNext())
                    {
                        string id = rit.Current.GetAttribute(XmlVoc.xmlAttRef, "");
                        roots.Add(id);
                    }
                    rootTable.Add(diag, roots);
                }


                RestorePrimitiveTypes();

                RestoreProfiles(XmlVoc.allProfiles);

                // All elements in model are restored together with their visualizations
                RestoreModel();
			//}
			//catch (Exception)
			//{
			//    return false;
			//}

            return true;
        }

        #region Primitive types

        /// <summary>
        /// Restores basic primitive types
        /// </summary>
        private void RestorePrimitiveTypes()
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathExpression ptypes = nav.Compile(XmlVoc.selectPrimitiveTypes);
            ptypes.SetContext(ns);
            XPathNodeIterator iterator = nav.Select(ptypes);

            while (iterator.MoveNext())
            {
                // @id
                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");

                SimpleDataType d = model.Schema.AddPrimitiveType();
                idTable.Add(id, d);

                // @name
                d.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, "");

                // @description
                d.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, "");
            }
        }

        #endregion

        #region Profiles

        /// <summary>
        /// Restores XCase profiles
        /// </summary>
        /// <param name="uri">XPath query selecting profiles element in XML document</param>
        private void RestoreProfiles(string uri)
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathExpression profiles = nav.Compile(uri);
            profiles.SetContext(ns);
            XPathNodeIterator iterator = nav.Select(profiles);

            // for each profile found
            while (iterator.MoveNext())
            {
                Profile p = model.Schema.AddProfile();

                // @name
                p.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, "");

                // stereotypes
                XPathExpression expr = iterator.Current.Compile(XmlVoc.relativeStereotypes);
                expr.SetContext(ns);
                XPathNodeIterator it = iterator.Current.Select(expr);
                while (it.MoveNext())
                {
                    Stereotype s = p.AddStereotype();
                    // @id
                    string id = it.Current.GetAttribute(XmlVoc.xmlAttID, "");
                    idTable.Add(id, s);

                    // @name
                    s.Name = it.Current.GetAttribute(XmlVoc.xmlAttName, "");

                    // receivers
                    expr = it.Current.Compile(XmlVoc.relativeReceivers);
                    expr.SetContext(ns);
                    XPathNodeIterator it2 = it.Current.Select(expr);
                    while (it2.MoveNext())
                        s.AppliesTo.Add(it2.Current.GetAttribute(XmlVoc.xmlAttType, ""));

                    // properties
                    expr = it.Current.Compile(XmlVoc.relativeProperties);
                    expr.SetContext(ns);
                    it2 = it.Current.Select(expr);
                    while (it2.MoveNext())
                    {
                        Property property = s.AddAttribute();
                        RestoreProperty(property, it2.Current);
                    }

                    // TODO: Remove this after the deserializator update that reads XSem stereotypes
                    // from Template.xml
                    // Awful hack by Luk on 25/04/2009 that allows use of the new PSMClass.AllowAnyAttribute
                    // property before the deserializator will be updated...
                    if (s.Name.Equals("PSMClass") || s.Name.Equals("PSMStructuralRepresentative"))
                    {
                        if (s.Attributes.Get("AllowAnyAttribute") == null)
                        {
                            Property property = s.AddAttribute();
                            property.Name = "AllowAnyAttribute";
                            property.Type = model.Schema.PrimitiveTypes.Get("boolean");
                            property.DefaultValue = new ValueSpecification(false);
                        }
                    }
                    // END of awful hack

                }
            }
        }

        #endregion

        #region Model

        /// <summary>
        /// Restores XCase UML model
        /// <para>
        /// Comprises restore of: Comments, Packages, Classes, Datatypes, Associations, Generalizations
        /// </para>
        /// </summary>
        private void RestoreModel()
        {
            #region Model attributes

            XPathExpression m = doc.CreateNavigator().Compile(XmlVoc.selectModelElement);
            m.SetContext(ns);
            XPathNavigator xmlmodel = doc.CreateNavigator().SelectSingleNode(m);

            model.ViewPoint = xmlmodel.GetAttribute(XmlVoc.xmlAttViewpoint, "");

            model.Schema.XMLNamespace = xmlmodel.GetAttribute(XmlVoc.xmlAttNamespace, "");

            // project name
            string n = xmlmodel.GetAttribute(XmlVoc.xmlAttName, "");
            if (!n.Equals("User model"))
                project.Caption = n;

            #endregion

            XPathExpression comments = doc.CreateNavigator().Compile(XmlVoc.modelComments);
            comments.SetContext(ns);
            RestoreComments(doc.CreateNavigator().SelectSingleNode(comments), model);

            // ------------------ PIM part restoration
            RestoreDatatypes(doc.CreateNavigator(), XmlVoc.modelDatatypes, model);
            RestorePackages(doc.CreateNavigator(), XmlVoc.modelPackages, model);



            // Derived PSM classes are NOT restored 
            RestorePIMClasses(doc.CreateNavigator(), XmlVoc.modelPimClasses, model);

            RestoreAssociationClasses(doc.CreateNavigator(), XmlVoc.modelAssociationClasses, model);

            // PIM generalizations only
            RestoreGeneralizations(XmlVoc.modelPIMGeneralizations);

            RestorePIMAssociations(XmlVoc.modelAssociations);

            //-------------------- PSM part restorations

            // PSM Classes and their components
            RestorePSMClasses();

            // PSM generalizations only
            RestoreGeneralizations(XmlVoc.modelPSMGeneralizations);

            RestorePSMAssociations(doc.CreateNavigator());

            RestoreRoots();

        }

        #region Comments restore

        /// <summary>
        /// Restores comments connected to given NamedElement
        /// </summary>
        /// <param name="nav">Reference to 'comments' element in XML document</param>
        /// <param name="p">Element, which the restored comments belong to</param>
        private void RestoreComments(XPathNavigator xn, Element p)
        {
            if (xn == null)
                return;

            if (!xn.Name.Equals(XmlVoc.xmlCommentsElement))
                DeserializationError();

            XPathExpression expr = xn.Compile(XmlVoc.xmlCommentElement);
            expr.SetContext(ns);
            XPathNodeIterator iterator = xn.Select(expr);

            // for each comment
            while (iterator.MoveNext())
            {
                // Restores the comment itself
                Comment c = p.AddComment();
                // @id
                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");
                idTable.Add(id, c);

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
                    int diagno = int.Parse(visualization.Current.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, ""));

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

        #endregion

        #region Packages

        /// <summary>
        /// Restores nested packages.
        /// </summary>
        /// <param name="xn">Reference to a XML element</param>
        /// <param name="uri">XPath query that should be applied to xn in order to get the packages location</param>
        /// <param name="p">Package to fill with read XML data</param>
        private void RestorePackages(XPathNavigator xn, string uri, Package p)
        {
            XPathExpression expr = xn.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator iterator = xn.Select(expr);

            // for each package
            while (iterator.MoveNext())
            {
                Package package = p.AddNestedPackage();

                // @name
                package.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, "");

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

        #endregion

        #region Classes

        #region PIM Classes

        /// <summary>
        /// Restores PIM classes
        /// </summary>
        /// <param name="nav">Reference to a XML elemen</param>
        /// <param name="uri">XPath query that should be applied to nav in order to get the classes location</param>
        /// <param name="package">Package where to restore the classes</param>
        private void RestorePIMClasses(XPathNavigator nav, string uri, Package package)
        {
            XPathExpression expr;
            expr = nav.Compile(uri);
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
        /// <param name="xn">Reference to XML 'pim_class' element, which represents the restored class</param>
        /// <param name="package">Parent package for the restored PIM class</param>
        private void RestorePimClass(XPathNavigator xn, Package package)
        {
            // class ID from @ref
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            if (!idTable.Contains(id))
            {
                PIMClass c0 = package.AddClass();
                idTable.Add(id, c0);
            }

            PIMClass c = (PIMClass)idTable[id];

            //  @name
            c.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

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
            derivedClasses.Add((PIMClass)c, psm);

            // --------------------------------------------------------------------

            // Restore all visualizations for the current PIM class
            expr = xn.Compile(XmlVoc.GetVisualizationForPIMClass(id));
            expr.SetContext(ns);
            XPathNodeIterator xit = xn.Select(expr);

            while (xit.MoveNext())
            {
                expr = xit.Current.Compile("..");
                expr.SetContext(ns);
                int diagno = int.Parse(xit.Current.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, ""));

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
        private void RestoreClassOperation(Operation p, XPathNavigator xpn)
        {
            // @id
            string id = xpn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, p);
            // @name
            p.Name = xpn.GetAttribute(XmlVoc.xmlAttName, "");

            // @type
            string type = xpn.GetAttribute(XmlVoc.xmlAttType, "");
            if (type.Equals("null"))
                p.Type = null;
            else
            {
                if (!idTable.Contains(type)) DeserializationError();
                p.Type = (DataType)idTable[type];
            }

            // @lower
            uint i;
            if (UInt32.TryParse(xpn.GetAttribute(XmlVoc.xmlAttLower, ""), out i))
                p.Lower = i;
            else
                p.Lower = null;
            // @upper
            p.Upper = NUml.Uml2.UnlimitedNatural.Parse(xpn.GetAttribute(XmlVoc.xmlAttUpper, ""));
            // @visibility
            p.Visibility = (NUml.Uml2.VisibilityKind)
            System.Enum.Parse(typeof(NUml.Uml2.VisibilityKind), xpn.GetAttribute(XmlVoc.xmlAttVisibility, ""));

        }

        #endregion

        #region PSM Classes

        /// <summary>
        /// Restores all derived PSM classes for all PIM classes in project.
        /// </summary>
        private void RestorePSMClasses()
        {
            IDictionaryEnumerator en = derivedClasses.GetEnumerator();
            while (en.MoveNext())
            {
                XPathNodeIterator xn = (XPathNodeIterator)en.Value;
                while (xn.MoveNext())
                {
                    RestorePsmClass(xn.Current, (PIMClass)en.Key);
                }
            }

        }

        /// <summary>
        /// Restores one PSM Class
        /// </summary>
        /// <param name="nav">Reference to XML 'psm_class' element, which represents the restored class</param>
        /// <param name="parent">Parent PIM class for the restored PSM class</param>
        private void RestorePsmClass(XPathNavigator xn, PIMClass parent)
        {
            PSMClass c;

            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");

            if (idTable.Contains(id))
                c = (PSMClass)idTable[id];
            else
            {
                c = parent.DerivePSMClass();
                idTable.Add(id, c);
            }

            // @name
            string name = xn.GetAttribute(XmlVoc.xmlAttName, "");
            // set elsewhere c.Name = name;

            //@abstract
            string abstr = xn.GetAttribute(XmlVoc.xmlAttAbstract, "");
            if (!abstr.Equals(""))
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
            string sr = xn.GetAttribute(XmlVoc.xmlAttStructuralRepresentative, "");
            PSMClass structural_representative = null;
            if (!sr.Equals(string.Empty))
            {
                if (!idTable.Contains(sr))
                {
                    structural_representative = parent.DerivePSMClass();
                    idTable.Add(sr, structural_representative );
                }

                c.RepresentedPSMClass = (PSMClass)idTable[sr];
            }

            // Applied stereotypes
            //RestoreAppliedStereotypes(nav, "stereotype_instances/stereotype_instance", c);

            // Restores class properties
            expr = xn.Compile(XmlVoc.relativePSMAttributes);
            expr.SetContext(ns);
            XPathNodeIterator it = xn.Select(expr);
            while (it.MoveNext())
            {
                // @ref
                string attref = it.Current.GetAttribute(XmlVoc.xmlAttAttRef, "");

                // free attribute (no partner in PIM class)
                if (attref.Equals(""))
                {
                    Property p = c.AddAttribute();
                    RestoreProperty(p, it.Current);
                }
                else
                    if (idTable.Contains(attref))
                    {
                        PSMAttribute p = c.AddAttribute((Property)idTable[attref]);
                        RestoreProperty(p, it.Current);
                    }
                    else
                        DeserializationError();
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
                int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, ""));

                PSMElementViewHelper h = new PSMElementViewHelper(project.PSMDiagrams[diagno]);
                RestoreClassElementVisualization(visualization, h);
                project.PSMDiagrams[diagno].AddModelElement(c, h);

                c.Diagram = project.PSMDiagrams[diagno];


                // ------------- other connected elements ------------- 
            }
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

        #endregion

        #region Common subroutines for PIM and PSM Classes

        /// <summary>
        /// Restores PIM or PSM Class Property.
        /// <para>
        /// Reads information from passed XML element and
        /// fills Property p with the read data.
        /// </para>
        /// </summary>
        /// <param name="p">Property to restore (to fill with data)</param>
        /// <param name="xpn">Reference to appropriate property element in XML document</param>
        private void RestoreProperty(Property p, XPathNavigator xn)
        {
            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, p);

            // @name
            p.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

            // @aggregation
            p.Aggregation = (NUml.Uml2.AggregationKind)
            System.Enum.Parse(typeof(NUml.Uml2.AggregationKind), xn.GetAttribute(XmlVoc.xmlAttAggregation, ""));

            // Updated by Luk on 25/04/2009.
            // Implementation of Property.Default changed, so that it's now
            // just a syntactic sugar for Property.DefaultValue.ToString() / 
            // Property.DefaultValue.ParseFromString().
            // Thus, the restoration of Property.Default becomes obsolete.
            // @default
            //string defstr = xn.GetAttribute(XmlVoc.xmlAttDefault, "");
            //if (!defstr.Equals(""))
            //    p.Default = defstr;
            // END of Luk update (25/04/2009)

            // @type
            string type = xn.GetAttribute(XmlVoc.xmlAttType, "");
            if (type.Equals("null"))
                p.Type = null;
            else
            {
                if (!idTable.Contains(type)) DeserializationError();
                p.Type = (DataType)idTable[type];
            }

            // @default_value
            // Code added by Luk on 25/04/2009 to allow deserialization of the DefaultValue
            // property without need to change the project XML schema.
            string defval = xn.GetAttribute(XmlVoc.xmlAttDefaultValue, "");
            if (!(defval.Equals("") || defval.Equals("null")))
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

            p.IsComposite = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsComposite, ""));
            p.IsDerived = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsDerived, ""));
            p.IsDerivedUnion = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsDerivedUnion, ""));
            p.IsOrdered = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsOrdered, ""));
            p.IsReadOnly = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsReadOnly, ""));
            p.IsUnique = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttIsUnique, ""));

            // @lower
            uint i;
            if (UInt32.TryParse(xn.GetAttribute(XmlVoc.xmlAttLower, ""), out i))
                p.Lower = i;
            else
                p.Lower = null;

            // @upper
            p.Upper = NUml.Uml2.UnlimitedNatural.Parse(xn.GetAttribute(XmlVoc.xmlAttUpper, ""));

            // @visibility
            p.Visibility = (NUml.Uml2.VisibilityKind)
            System.Enum.Parse(typeof(NUml.Uml2.VisibilityKind), xn.GetAttribute(XmlVoc.xmlAttVisibility, ""));

            // PSM Attributes only
            if (p is PSMAttribute)
            {
                // @class_ref
                string cls = xn.GetAttribute(XmlVoc.xmlAttClassRef, "");
                if (!idTable.Contains(cls) || !(idTable[cls] is PSMClass)) DeserializationError();
                ((PSMAttribute)p).Class = (PSMClass)idTable[cls];

                // @alias
                ((PSMAttribute)p).Alias = xn.GetAttribute(XmlVoc.xmlAttAlias, "");

                // @XsdImplementation
                ((PSMAttribute)p).XSDImplementation = xn.GetAttribute(XmlVoc.xmlAttXSDImplementation, "");

                // Note: @att_ref is restored when PSM Attribute is created
            }
        }


        /// <summary>
        /// Applies stereotypes to a given class
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="uri"></param>
        /// <param name="c">Class, which the stereotypes are applied to</param>
        private void RestoreAppliedStereotypes(XPathNavigator xn, string uri, Class c)
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathExpression expr = nav.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator it = xn.Select(expr);

            while (it.MoveNext())
            {
                string s = it.Current.GetAttribute(XmlVoc.xmlAttRef, "");
                if (idTable[s] is Stereotype)
                    ((Stereotype)idTable[s]).ApplyTo(c);
            }
        }

        /// <summary>
        /// Restores visualization for one PIM or PSM class
        /// </summary>
        /// <param name="xn">Pointer to the 'class' element in XML document</param>
        /// <param name="h">ClassViewHelper to restore (to fill with visualization data)</param>
        private void RestoreClassElementVisualization(XPathNavigator xn, ClassViewHelper h)
        {
            if (!xn.Name.Equals(XmlVoc.xmlClassElement))
                DeserializationError();

            // appearance
            XPathExpression appearance = xn.Compile(XmlVoc.xmlAppearanceElement);
            appearance.SetContext(ns);
            RestoreAppearance(xn.SelectSingleNode(appearance), h);

            // @methods_collapsed
            h.OperationsCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttOperationsCollapsed, ""));

            // @properties_collapsed
            h.AttributesCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttAttributesCollapsed, ""));

            //
            h.ElementNameLabelAlignedRight = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttElementNameLabelAlignedRight, ""));

            h.ElementNameLabelCollapsed = bool.Parse(xn.GetAttribute(XmlVoc.xmlAttElementNameLabelCollapsed, ""));
        }


        #endregion

        #endregion

        #region PSM Elements (content container, content choice, attribute container)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xn">Reference to 'components' element in XML document</param>
        /// <param name="c">PSMSuperordinateComponent to add the restored components to</param>
        private void RestoreComponents(XPathNavigator xn, PSMSuperordinateComponent c)
        {
            // components element empty
            if (xn == null)
                return;

            if (!xn.Name.Equals(XmlVoc.xmlComponentsElement))
                DeserializationError();

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
                                DeserializationError();
            } // while

        }

        /// <summary>
        /// Restoration of a PSMAttributeContainer
        /// </summary>
        /// <param name="xn">Reference to PSMAttributeContainer element in XML document</param>
        /// <param name="c">Component to restore PSMAttributeContainer to</param>
        private void RestorePSMAttContainer(XPathNavigator xn, PSMSuperordinateComponent c)
        {
            if (!xn.Name.Equals(XmlVoc.xmlPSMAttContainer))
                DeserializationError();

            PSMAttributeContainer container = (PSMAttributeContainer)c.AddComponent(PSMAttributeContainerFactory.Instance);

            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, container);

            // @name
            container.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

            // diagram
            container.Diagram = c.Diagram;

            // properties
            XPathExpression expr = xn.Compile(XmlVoc.relativePSMAttributes);
            expr.SetContext(ns);
            XPathNodeIterator it = xn.Select(expr);

            while (it.MoveNext())
            {
                // @att_ref
                string attref = it.Current.GetAttribute(XmlVoc.xmlAttAttRef, "");
                if (idTable.Contains(attref))
                {
                    PSMAttribute p = container.AddAttribute((Property)idTable[attref]);
                    RestoreProperty(p, it.Current);

                }
                else if (attref.Equals("")) 
                {
                    PSMAttribute p = container.AddAttribute();
                    RestoreProperty(p, it.Current);
                }
                else
                    DeserializationError();
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
        /// Restoration of PSMContentContainer
        /// </summary>
        /// <param name="xn">Reference to PSMContentContainer element in XML document</param>
        /// <param name="c">Component to restore PSMContentContainer to</param>
        private void RestoreContentContainer(XPathNavigator xn, PSMSuperordinateComponent c)
        {
            if (!xn.Name.Equals(XmlVoc.xmlPSMContentContainer))
                DeserializationError();

            PSMContentContainer container = (PSMContentContainer)c.AddComponent(PSMContentContainerFactory.Instance);

            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, container);

            // @name
            container.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

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
        /// Restoration of PSMContentChoice
        /// </summary>
        /// <param name="xn">Reference to PSMContentChoice element in XML document</param>
        /// <param name="c">Component to restore PSMContentContainer to</param>
        private void RestoreContentChoice(XPathNavigator xn, PSMSuperordinateComponent c)
        {
            if (!xn.Name.Equals(XmlVoc.xmlPSMContentChoice))
                DeserializationError();

            PSMContentChoice choice = (PSMContentChoice)c.AddComponent(PSMContentChoiceFactory.Instance);

            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, choice);

            // @name
            c.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

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
        /// Restoration of visualization for a PSMElement
        /// </summary>
		/// <remarks>
		/// - PSMAttributeContainer
		/// - PSMContentContainer
		/// - PSMContentChoice
		/// </remarks>
		/// <param name="visualization">Reference to appropriate element in XML document</param>
        /// <param name="id">ID of restored element</param>
        /// <param name="container">Element to restore the visualization for</param>
        private void RestoreVisualizationForPSMElement(XPathNavigator visualization, string id, NamedElement container)
        {
            // No visualization found
            if (visualization == null)
                return;

            XPathExpression expr = visualization.Compile("..");
            expr.SetContext(ns);
            int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, ""));

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

        #endregion

        #region Datatypes

        private void RestoreDatatypes(XPathNavigator nav, string uri, Package p)
        {
            XPathExpression expr = nav.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator iterator = nav.Select(expr);

            //datatypes
            while (iterator.MoveNext())
            {
                if (bool.Parse(iterator.Current.GetAttribute(XmlVoc.xmlAttSimple, "")))
                {
                    string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");
                    SimpleDataType d;
                    string parent = iterator.Current.GetAttribute(XmlVoc.xmlAttParentRef, "");
                    if (!parent.Equals("") && idTable.Contains(parent))
                        d = p.AddSimpleDataType((SimpleDataType)idTable[parent]);
                    else
                        d = p.AddSimpleDataType();

                    idTable.Add(id, d);

                    d.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, "");
                    d.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, "");


                }
                else
                {

                    string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");
                    Class c = p.AddClass();
                    idTable.Add(id, c);

                    c.Name = iterator.Current.GetAttribute(XmlVoc.xmlAttName, "");
                    // c.DefaultXSDImplementation = iterator.Current.GetAttribute(XmlVoc.xmlAttDescription, "");

                }
            }
        }

        #endregion

        #region PIM Associations

        #region Association Classes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="uri"></param>
        /// <param name="p"></param>
        private void RestoreAssociationClasses(XPathNavigator xn, string uri, Package p)
        {
            XPathExpression expr;
            expr = xn.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator iterator = xn.Select(expr);

            // Restore each PIM class in package
            while (iterator.MoveNext())
            {
                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");

                AssociationClass a;

                // Model
                RestoreAssociationClassFromModel(iterator.Current, out a);

                idTable.Add(id, a);

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
        private void RestoreAssociationClassFromModel(XPathNavigator xn, out AssociationClass a)
        {
            // Association ID

            List<Class> classes = new List<Class>();
            List<AssociationEndMemo> ends = new List<AssociationEndMemo>();

            string class_id = "";
            // "association_end"
            XPathExpression expr = xn.Compile(XmlVoc.xmlAssociationEndElement);
            expr.SetContext(ns);
            XPathNodeIterator assocend = xn.Select(expr);

            // Go through all AssociationEnds belonging to the current Association
            while (assocend.MoveNext())
            {
                class_id = assocend.Current.GetAttribute(XmlVoc.xmlAttClass, "");

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

            a.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

            // Sets attributes to all AssociationEnds
            Association c = ((Association)a);
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
        /// <param name="class_id"></param>
        /// <param name="id"></param>
        /// <param name="a"></param>
        private void RestoreVisualizationForAssociationClass(string id, AssociationClass a)
        {
            // Find all visualizations for Association class
            XPathNavigator nav = doc.CreateNavigator();
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
                int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, ""));

                AssociationClassViewHelper h = new AssociationClassViewHelper(project.PIMDiagrams[dno]);

                // ----------- association part -----------------

                h.AssociationViewHelper.UseDiamond = bool.Parse(visualization.Current.GetAttribute(XmlVoc.xmlAttDiamond, ""));
                if (h.AssociationViewHelper.UseDiamond)
                {
                    expr = visualization.Current.Compile(XmlVoc.xmlDiamond);
                    expr.SetContext(ns);
                    XPathNavigator diamond = visualization.Current.SelectSingleNode(expr);
                    if (diamond != null)
                    {
                        PositionableElementViewHelper pev = (PositionableElementViewHelper)h.AssociationViewHelper;
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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        private void RestorePIMAssociations(string uri)
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathExpression expr = nav.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator iterator = nav.Select(expr);

            // For each association found in XML
            while (iterator.MoveNext())
            {
                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");

                Association a;

                // Model
                RestoreAssociationFromModel(iterator.Current, out a);

                idTable.Add(id, a);

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
        private void RestoreAssociationFromModel(XPathNavigator xn, out Association a)
        {
            // Association ID

            List<Class> classes = new List<Class>();
            List<AssociationEndMemo> ends = new List<AssociationEndMemo>();

            string class_id = "";
            // "association_end"
            XPathExpression expr = xn.Compile(XmlVoc.xmlAssociationEndElement);
            expr.SetContext(ns);
            XPathNodeIterator assocend = xn.Select(expr);

            // Go through all AssociationEnds belonging to the current Association
            while (assocend.MoveNext())
            {
                class_id = assocend.Current.GetAttribute(XmlVoc.xmlAttClass, "");

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

            a.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

            // Sets attributes to all AssociationEnds
            RestoreAssociationEndAttributes(ref a, ends);

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="a"></param>
        private void RestoreVisualizationForPIMAssociation(string id, Association a)
        {
            // Finds all visualization for PIM association

            XPathNavigator nav = doc.CreateNavigator();
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
                int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, ""));

                AssociationViewHelper h = new AssociationViewHelper(project.PIMDiagrams[dno]);

                h.UseDiamond = bool.Parse(xit.Current.GetAttribute(XmlVoc.xmlAttDiamond, ""));
                if (h.UseDiamond)
                {
                    expr = xit.Current.Compile(XmlVoc.xmlDiamond);
                    expr.SetContext(ns);
                    XPathNavigator diamond = xit.Current.SelectSingleNode(expr);
                    if (diamond != null)
                    {
                        PositionableElementViewHelper pev = (PositionableElementViewHelper)h;
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
        private void RestoreAssociationEnds(XPathNavigator xn, ref AssociationViewHelper h, Association a, Diagram d)
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
        private void RestoreMultiplicityLabel(XPathNavigator xn, AssociationEndViewHelper ae)
        {
            // coordinate_x
            XPathExpression expr = xn.Compile(XmlVoc.xmlCardinalityLabel + "/" + XmlVoc.xmlCoordXElement);
            expr.SetContext(ns);
            XPathNavigator xpn = xn.SelectSingleNode(expr);
            ae.MultiplicityLabelViewHelper = new AssociationLabelViewHelper(ae.Diagram);

            ae.MultiplicityLabelViewHelper.LabelVisible = true;
            ae.MultiplicityLabelViewHelper.X = Double.Parse(xpn.Value.ToString());

            // coordinate_y
            expr = xn.Compile(XmlVoc.xmlCardinalityLabel + "/" + XmlVoc.xmlCoordYElement);
            expr.SetContext(ns);
            xpn = xn.SelectSingleNode(expr);
            ae.MultiplicityLabelViewHelper.Y = Double.Parse(xpn.Value.ToString());

        }

        /// <summary>
        /// Restores Role Label for Association
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="ae"></param>
        private void RestoreRoleLabel(XPathNavigator xn, AssociationEndViewHelper ae)
        {
            // coordinate_x
            XPathExpression expr = xn.Compile(XmlVoc.xmlRoleLabel + "/" + XmlVoc.xmlCoordXElement);
            expr.SetContext(ns);
            XPathNavigator xpn = xn.SelectSingleNode(expr);
            ae.RoleLabelViewHelper = new AssociationLabelViewHelper(ae.Diagram);

            ae.RoleLabelViewHelper.LabelVisible = true;
            ae.RoleLabelViewHelper.X = Double.Parse(xpn.Value.ToString());

            // coordinate_y
            expr = xn.Compile(XmlVoc.xmlRoleLabel + "/" + XmlVoc.xmlCoordYElement);
            expr.SetContext(ns);
            xpn = xn.SelectSingleNode(expr);
            ae.RoleLabelViewHelper.Y = Double.Parse(xpn.Value.ToString());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="h"></param>
        private void RestoreAssociationLabel(XPathNavigator xn, ref AssociationLabelViewHelper h)
        {
            XPathExpression expr = xn.Compile(XmlVoc.xmlNameLabel);
            expr.SetContext(ns);
            XPathNavigator xpn = xn.SelectSingleNode(expr);
            PositionableElementViewHelper pev = (PositionableElementViewHelper)h;
            RestoreAppearance(xpn, ref pev);

        }


        private void RestoreAppearance(XPathNavigator xpn, ref PositionableElementViewHelper h)
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
            if (!r.ToString().Equals(""))
                h.Height = Double.Parse(r.ToString());

            //width
            expr = xpn.Compile(XmlVoc.xmlWidthElement);
            expr.SetContext(ns);
            r = xpn.SelectSingleNode(expr);
            if (!r.ToString().Equals(""))
                h.Width = Double.Parse(r.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ae"></param>
        /// <param name="xpn"></param>
        private void RestoreAssociationAttributes(ref AssociationEndMemo ae, XPathNavigator xpn)
        {
            ae.id = xpn.GetAttribute(XmlVoc.xmlAttID, "");
            ae.name = xpn.GetAttribute(XmlVoc.xmlAttName, "");

            ae.aggregation = xpn.GetAttribute(XmlVoc.xmlAttAggregation, "");
            ae.def = xpn.GetAttribute(XmlVoc.xmlAttDefault, "");
            ae.default_value = xpn.GetAttribute(XmlVoc.xmlAttDefaultValue, "");

            ae.is_composite = xpn.GetAttribute(XmlVoc.xmlAttIsComposite, "");
            ae.is_derived = xpn.GetAttribute(XmlVoc.xmlAttIsDerived, "");
            ae.is_ordered = xpn.GetAttribute(XmlVoc.xmlAttIsOrdered, "");
            ae.is_readonly = xpn.GetAttribute(XmlVoc.xmlAttIsReadOnly, "");
            ae.is_unique = xpn.GetAttribute(XmlVoc.xmlAttIsUnique, "");

            ae.lower = xpn.GetAttribute(XmlVoc.xmlAttLower, "");

            ae.upper = xpn.GetAttribute(XmlVoc.xmlAttUpper, "");
            ae.visibility = xpn.GetAttribute(XmlVoc.xmlAttVisibility, "");

            ae.type = xpn.GetAttribute(XmlVoc.xmlAttType, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="ends"></param>
        private void RestoreAssociationEndAttributes(ref Association a, List<AssociationEndMemo> ends)
        {
            int i = 0;
            List<AssociationEndMemo>.Enumerator e = ends.GetEnumerator();
            while (e.MoveNext())
            {
                idTable.Add(e.Current.id, a.Ends[i]);

                // @type
                string type = e.Current.type;
                if (type.Equals("null"))
                    a.Ends[i].Type = null;
                else
                {
                    if (!idTable.Contains(type)) DeserializationError();
                    a.Ends[i].Type = (DataType)idTable[type];
                }

                a.Ends[i].Lower = UInt32.Parse(e.Current.lower);
                a.Ends[i].Upper = NUml.Uml2.UnlimitedNatural.Parse(e.Current.upper);
                a.Ends[i].Visibility = (NUml.Uml2.VisibilityKind)
                System.Enum.Parse(typeof(NUml.Uml2.VisibilityKind), e.Current.visibility);
                a.Ends[i].Name = e.Current.name;
                a.Ends[i].Default = e.Current.def;
                a.Ends[i].Aggregation = (NUml.Uml2.AggregationKind)
                System.Enum.Parse(typeof(NUml.Uml2.AggregationKind), e.Current.aggregation);

                a.Ends[i].IsComposite = bool.Parse(e.Current.is_composite);
                a.Ends[i].IsDerived = bool.Parse(e.Current.is_derived);
                a.Ends[i].IsOrdered = bool.Parse(e.Current.is_ordered);
                a.Ends[i].IsReadOnly = bool.Parse(e.Current.is_readonly);
                a.Ends[i].IsUnique = bool.Parse(e.Current.is_unique);

                i++;
            }
        }

        #endregion

        #region PSM Associations

        /// <summary>
        /// Restoration of PSMClassUnion
        /// </summary>
        /// <param name="xn">Reference to PSMClassUnion element in XML document</param>
        /// <param name="c">Component to restore PSMClassUnion to</param>
        /// <returns></returns>
        private PSMClassUnion RestoreClassUnion(XPathNavigator xn, PSMSuperordinateComponent c)
        {
            if (xn == null)
                return null;

            if (!xn.Name.Equals(XmlVoc.xmlPSMClassUnion))
                DeserializationError();

            PSMClassUnion cu = c.CreateClassUnion();

            // @id
            string id = xn.GetAttribute(XmlVoc.xmlAttID, "");
            idTable.Add(id, cu);

            // @name
            cu.Name = xn.GetAttribute(XmlVoc.xmlAttName, "");

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
                    string class_ref = components.Current.GetAttribute(XmlVoc.xmlAttRef, "");
                    if (!idTable.Contains(class_ref))
                        DeserializationError();

                    cu.Components.Add((PSMClass)idTable[class_ref]);
                }
                else
                    // class union
                    if (components.Current.Name.Equals(XmlVoc.xmlPSMClassUnion))
                    {
                        PSMClassUnion cu2 = RestoreClassUnion(components.Current, c);
                        if (cu2 != null)
                            cu.Components.Add(cu2);
                    }
                    else
                        DeserializationError();
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
        private void RestorePSMAssociations(XPathNavigator xn)
        {
            // Select all PSM associations 
            XPathExpression expr = xn.Compile(XmlVoc.allPSMAssociations);
            expr.SetContext(ns);
            XPathNodeIterator psm_associations = xn.Select(expr);

            // Restores all PSM associations
            while (psm_associations.MoveNext())
            {
                XPathNavigator assoc = psm_associations.Current;

                string id = assoc.GetAttribute(XmlVoc.xmlAttID, "");

                // parent
                XPathExpression expr_parent = assoc.Compile(XmlVoc.xmlParentElement);
                expr_parent.SetContext(ns);
                XPathNavigator parent = assoc.SelectSingleNode(expr_parent);
                string parent_id = parent.GetAttribute(XmlVoc.xmlAttRef, "");
                if (!idTable.Contains(parent_id))
                {
                    DeserializationError();
                    //System.Windows.MessageBox.Show("PSM association involving a PSM class/container without visualization");
                    //continue;
                }

                string index = assoc.GetAttribute(XmlVoc.xmlAttIndex, "");
                if (index.Equals(""))
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
                string child_id = child.GetAttribute(XmlVoc.xmlAttRef, "");
                if (!idTable.Contains(child_id)) DeserializationError();
                p.Child = (PSMAssociationChild)idTable[child_id];

                // @min
                if (!child.GetAttribute(XmlVoc.xmlAttMin, "").Equals(""))
                    p.Lower = UInt32.Parse(child.GetAttribute(XmlVoc.xmlAttMin, ""));

                // @max
                p.Upper = NUml.Uml2.UnlimitedNatural.Parse(child.GetAttribute(XmlVoc.xmlAttMax, ""));

                //generalizations
                XPathExpression usedg = assoc.Compile(XmlVoc.xmlUsedGeneralizations + "/" + XmlVoc.xmlUsedGeneralization);
                usedg.SetContext(ns);
                XPathNodeIterator gens = assoc.Select(usedg);
                while (gens.MoveNext())
                {
                    string gref = gens.Current.GetAttribute(XmlVoc.xmlAttRef, "");
                    if (!idTable.Contains(gref))
                        DeserializationError();

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

                RestorePSMAssociationVisualization(p, id);

            } // while
        }

        /// <summary>
        ///  Restoration of one Nesting Join
        /// </summary>
        /// <param name="xpn">Reference to Nesting Join element in XML document</param>
        /// <param name="p">PSM Association to add restored nesting join to</param>
        private void RestoreNestingJoin(XPathNavigator xpn, PSMAssociation p)
        {
            // @core_class_ref
            string cc = xpn.GetAttribute(XmlVoc.xmlAttCoreClassRef, "");
            if (!idTable.Contains(cc)) DeserializationError();

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
        /// <param name="path">PIMPath to restore</param>
        private void RestorePIMPath(XPathNavigator xn, PIMPath path)
        {
            XPathExpression pim_step = xn.Compile(XmlVoc.xmlPimStep);
            pim_step.SetContext(ns);
            XPathNodeIterator steps = xn.Select(pim_step);
            while (steps.MoveNext())
            {
                // @start_ref
                string start = steps.Current.GetAttribute(XmlVoc.xmlAttStartRef, "");
                if (!idTable.Contains(start) || !(idTable[start] is PIMClass))
                    DeserializationError();

                // @end_ref
                string end = steps.Current.GetAttribute(XmlVoc.xmlAttEndRef, "");
                if (!idTable.Contains(end) || !(idTable[end] is PIMClass))
                    DeserializationError();

                // @association_ref
                string assoc = steps.Current.GetAttribute(XmlVoc.xmlAttAssociation, "");
                if (!idTable.Contains(assoc) || !(idTable[assoc] is Association))
                    DeserializationError();

                path.AddStep((PIMClass)idTable[start], (PIMClass)idTable[end], (Association)idTable[assoc]);
            }
        }

        /// <summary>
        /// Restoration of visualization for a PSMAssociation
        /// </summary>
        /// <param name="p"></param>
        /// <param name="id"></param>
        private void RestorePSMAssociationVisualization(PSMAssociation p, string id)
        {
            XPathNavigator xn = doc.CreateNavigator();
            // Restore visualization for PSM Association
            XPathExpression expr = xn.Compile(XmlVoc.GetVisualizationForPSMAssociation(id));
            expr.SetContext(ns);
            XPathNavigator visualization = xn.SelectSingleNode(expr);

            // No visualization
            if (visualization == null)
                return;

            expr = visualization.Compile("..");
            expr.SetContext(ns);
            int diagno = int.Parse(visualization.SelectSingleNode(expr).GetAttribute(XmlVoc.xmlAttNo, ""));

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
            view.MultiplicityLabelViewHelper.X = Double.Parse(xpn.Value.ToString());

            expr = visualization.Compile(XmlVoc.xmlMultiplicityLabel + "/" + XmlVoc.xmlCoordYElement);
            expr.SetContext(ns);
            xpn = visualization.SelectSingleNode(expr);
            view.MultiplicityLabelViewHelper.Y = Double.Parse(xpn.Value.ToString());


            project.PSMDiagrams[diagno].AddModelElement(p, view);
        }

        #endregion

        #region Generalizations restore

        private void RestoreGeneralizations(string uri)
        {
            XPathNavigator nav = doc.CreateNavigator();
            XPathExpression expr = nav.Compile(uri);
            expr.SetContext(ns);
            XPathNodeIterator iterator = nav.Select(expr);

            // generalizations
            while (iterator.MoveNext())
            {
                // Restore generalization itself

                string id = iterator.Current.GetAttribute(XmlVoc.xmlAttID, "");
                string general_id = iterator.Current.GetAttribute(XmlVoc.xmlAttGeneral, "");
                string specific_id = iterator.Current.GetAttribute(XmlVoc.xmlAttSpecific, "");
                bool is_substitable = bool.Parse(iterator.Current.GetAttribute(XmlVoc.xmlAttIsSubstitable, ""));

                Class general = (Class)idTable[general_id];
                Class specific = (Class)idTable[specific_id];

                Generalization g = model.Schema.SetGeneralization(general, specific);

                idTable.Add(id, g);

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
                    int dno = int.Parse(pn.GetAttribute(XmlVoc.xmlAttNo, ""));

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

        #endregion

        private void RestoreRoots()
        {
            foreach (PSMDiagram d in project.PSMDiagrams)
            {
                if (!rootTable.Contains(d))
                    DeserializationError();

                List<string> rootIds = (List<string>)rootTable[d];
                for (int i = 0; i < rootIds.Count; ++i)
                {
                    if (!idTable.Contains(rootIds[i]))
                        DeserializationError();
                    d.Roots.Add((PSMClass)idTable[rootIds[i]]);
                }
            }
        }

        #region Common

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xn">Pointer to the 'points' element in XML</param>
        /// <param name="p">Collection to fill with read points</param>
        private void RestorePoints(XPathNavigator xn, out ObservablePointCollection p)
        {
            p = new ObservablePointCollection();

            if (xn == null)
                return;

            if (!xn.Name.Equals(XmlVoc.xmlPoints))
                DeserializationError();

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
        private void RestoreAppearance(XPathNavigator xn, PositionableElementViewHelper h)
        {
            if (!xn.Name.Equals(XmlVoc.xmlAppearanceElement))
                DeserializationError();

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
            if (!xpn.ToString().Equals(""))
                h.Height = Double.Parse(xpn.ToString());

            // width
            expr = xn.Compile(XmlVoc.xmlWidthElement);
            expr.SetContext(ns);
            xpn = xn.SelectSingleNode(expr);
            if (!xpn.ToString().Equals(""))
                h.Width = Double.Parse(xpn.ToString());

        }

        #endregion

        #endregion

        #region Static XML Validation

        /// <summary>
        /// Validates input XML (describing stored XCase project) against internal XML Schema file
        /// </summary>
        /// <param name="input">Stream with input XML</param>
        /// <param name="message">Error message if the passed XML is invalid</param>
        /// <returns>True if the passed XML is valid, false otherwise</returns>
        public static bool ValidateXML(System.IO.Stream input, ref String message)
        {
            return Validate(input, ref message);
        }

        /// <summary>
        /// Validates input XML (describing stored XCase project) against internal XML Schema file
        /// </summary>
        /// <param name="file">Name of XML file or Stream with input XML</param>
        /// <param name="message">Error message if the passed file is invalid</param>
        /// <returns>True if the passed XML is valid, false otherwise</returns>
        public static bool ValidateXML(string file, ref String message)
        {
            return Validate(file, ref message);
        }

        private static bool Validate(object input, ref String message)
        {
            // Load XML Schema file describing the correct XML file
            byte[] b = System.Text.Encoding.GetEncoding("windows-1250").GetBytes(XCase.Gui.Properties.Resources.XCaseSchema);
            System.IO.MemoryStream m = new System.IO.MemoryStream(b);
            XmlReader schema = new XmlTextReader(m);

            XmlReaderSettings schemaSettings = new XmlReaderSettings();
            schemaSettings.Schemas.Add(XmlVoc.defaultNamespace, schema);
            schemaSettings.ValidationType = ValidationType.Schema;

            // Event handler called when an error occurs while validating
            schemaSettings.ValidationEventHandler += new ValidationEventHandler(schemaSettings_ValidationEventHandler);

            XmlReader xmlfile;
            if (input is string)
                xmlfile = XmlReader.Create((string)input, schemaSettings);
            else
                if (input is System.IO.Stream)
                    xmlfile = XmlReader.Create((System.IO.Stream)input, schemaSettings);
                else
                    return false;

            try
            {
                while (xmlfile.Read()) ;
            }
            catch
            {
                isPassedXmlValid = false;
                xmlValidationErrorMessage = "Not a valid XML file";
            }
            finally
            {
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            isPassedXmlValid = false;
            xmlValidationErrorMessage = e.Message;
        }

        #endregion

    }

}
