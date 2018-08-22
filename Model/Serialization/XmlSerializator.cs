//#define writeStringBuilderFirst
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
#if writeStringBuilderFirst
using System.IO;
#endif

namespace XCase.Model.Serialization
{
    /// <summary>
    /// Class ensures serialization of the entire XCase project to a single XML file [XCase file]
    /// </summary>
    /// <remarks>
    /// <para>
    /// The structure of the output XCase file is described in the internal file XCaseSchema.xsd
    /// </para>
    /// <para>
    /// To restore the serialized project again, use <see cref="XmlDeserializator">XmlDeserializator</see> class
    /// </para>
    /// </remarks>
    public class XmlSerializator
    {
        #region XmlSerializator attributes

        private readonly Project project;

        private readonly VersionManager versionManager;

        /// <summary>
        /// Table of all serialized elements that have unique ID attribute in the output XML file.
        /// <list type="">
        /// <item>Key =  Element</item>
        /// <item>Value = ID</item>
        /// </list>
        /// </summary>
        private SerializatorIdTable idTable = new SerializatorIdTable();

        private readonly Dictionary<Diagram, int> diagramIdTable = new Dictionary<Diagram, int>();

        /// <summary>
        /// XmlTextWriter used for writing the content of the output XML file
        /// </summary>
        private XmlTextWriter xmlTextWriter;

        #endregion

        /// <summary>
        /// Initialization of serializator. Use this overload to 
        /// serialize projects not using versioning. 
        /// </summary>
        /// <param name="project">XCase project to serialize</param>
        /// <seealso cref="XmlSerializator(VersionManager)"/>
        /// <seealso cref="ProjectConverter"/>
        public XmlSerializator(Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Initialization of serializator. Use this overload to 
        /// serialize projects using versioning
        /// </summary>
        /// <param name="versionManager">The version manager.</param>
        /// <seealso cref="XmlSerializator(Project)"/>
        public XmlSerializator(VersionManager versionManager)
        {
            this.versionManager = versionManager;
        }

        /// <summary>
        /// Version of format for the saved project. 
        /// Written in the xml file as version attribute of the xs:project element. 
        /// </summary>
        /// <seealso cref="XmlDeserializator.SchemaVersion"/>
        public const string SchemaVersion = "2.0";

#if writeStringBuilderFirst
		private readonly StringBuilder sb = new StringBuilder();
#endif

        private XmlVocBase XmlVoc;

        /// <summary>
        /// Serializes project to a XML file
        /// </summary>
        /// <param name="filename">Name of XML file, which the project is serialized to</param>
        public void SerilizeTo(string filename)
        {
#if writeStringBuilderFirst
			xmlTextWriter = new XmlTextWriter(new StringWriter(sb));
#else
            xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8);
#endif
#if DEBUG
            xmlTextWriter.Formatting = Formatting.Indented;
#endif
            if (project != null)
            {
                XmlVoc = new XmlVocNoVersions();
            }
            else
            {
                XmlVoc = new XmlVocVersions();
            }

            // Start element
            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteStartElement(XmlVoc.xmlRootElement);
            // @namespace 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttXmlns, XmlVoc.defaultNamespace);
            // @version
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVersion, SchemaVersion);

            if (project != null)
            {
                // Fill the id table from the project stub. 
                idTable.AddFromTable(project.TemplateIdTable);
                // Serialization of UML part
                SerializeUml(project.Schema.Model);

                // Serialization of visualization part
                SerializeDiagrams(project);
            }
            else
            {
                // @name
                // TODO : name attribute
                SerializeVersions();
            }

            // End element
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlRootElement
            xmlTextWriter.WriteEndDocument();
            xmlTextWriter.Close();

#if writeStringBuilderFirst
			xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8);
			xmlTextWriter.WriteString(String.Empty);
			xmlTextWriter.WriteRaw(sb.ToString());
			xmlTextWriter.Close();
#endif
        }

        private void SerializeVersions()
        {
            xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionsElement);
            foreach (Version projectVersion in versionManager.Versions)
            {
                xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionElement);
                xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttNumber, projectVersion.Number.ToString());
                xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttLabel, projectVersion.Label);
                if (projectVersion.CreatedFrom != null)
                    xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttCreatedFrom, projectVersion.CreatedFrom.Number.ToString());
                xmlTextWriter.WriteEndElement(); // xmlVersion
            }
            xmlTextWriter.WriteEndElement(); // xmlVersions

            xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionedProjectsElement);
            foreach (KeyValuePair<Version, Project> kvp in versionManager.VersionedProjects)
            {
                Version version = kvp.Key;
                Project versionedProject = kvp.Value;

                xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionElement);
                xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttNumber, version.Number.ToString());
                xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttLabel, version.Label);

                if (versionedProject.TemplateIdTable != null)
                {
                    versionedProject.TemplateIdTable.Version = version;
                    this.idTable = versionedProject.TemplateIdTable;
                }

                SerializeProfiles(versionedProject.Schema.Model);
                SerializeModel(versionedProject.Schema.Model);
                SerializeDiagrams(versionedProject);
                xmlTextWriter.WriteEndElement(); // xmlVersion
            }
            xmlTextWriter.WriteEndElement(); // xmlVersionedProjects

            //// <versionLink firstVersion="refId" version="refVer" ref="" />	
            xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionLinksElement);

            foreach (System.Collections.DictionaryEntry versionLink in versionManager)
            {
                ComposedKey<IVersionedElement, Version> key = (ComposedKey<IVersionedElement, Version>)versionLink.Key;
                IVersionedElement value = (IVersionedElement)versionLink.Value;
                Element element = key.First as Element; // Diagram, Project
                Diagram diagram = key.First as Diagram;
                Element derivedElement = value as Element;
                Project lproject = key.First as Project;
                if (element != null && derivedElement != null)
                {
                    SerializatorIdTable firstVersionIdTable = versionManager.VersionedProjects[element.Version].TemplateIdTable;
                    SerializatorIdTable derivedVersionIdTable = versionManager.VersionedProjects[value.Version].TemplateIdTable;

                    if (!firstVersionIdTable.ContainsKey(element) || !derivedVersionIdTable.ContainsKey(derivedElement))
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("IdTable not containing {0} / {1}", value, element.FirstVersion));
                        continue;
                    }

                    XCaseGuid firstVersionId = firstVersionIdTable[element];
                    XCaseGuid derivedVersionId = derivedVersionIdTable[derivedElement];

                    xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionLinkElement);
                    xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttFirstVersion, element.FirstVersion.Version.Number.ToString());
                    xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttFirstVersionRef, firstVersionId.ToString());
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVersion, key.Second.Number.ToString());
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, derivedVersionId.ToString());
                    xmlTextWriter.WriteEndElement(); // xmlVersionLinkElement
                }
                else if (diagram != null)
                {
                    xmlTextWriter.WriteStartElement(((XmlVocVersions)XmlVoc).xmlVersionLinkDiagramElement);
                    xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttFirstVersion, diagram.FirstVersion.Version.Number.ToString());
                    xmlTextWriter.WriteAttributeString(((XmlVocVersions)XmlVoc).xmlAttFirstVersionRef, diagramIdTable[(Diagram)diagram.FirstVersion].ToString());
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVersion, key.Second.Number.ToString());
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, diagramIdTable[(Diagram)value].ToString());
                    xmlTextWriter.WriteEndElement(); // xmlVersionLinkDiagramElement
                }
                else if (lproject != null)
                {
                    // ignored
                }
                else
                {
                    throw new NotImplementedException("Method or operation is not implemented.");
                }
            }

            xmlTextWriter.WriteEndElement(); // xmlVersionLinksElement
        }

        /// <summary>
        /// Called from the serialization in the case of unexpected error (typically model inconsistency)
        /// </summary>
        /// <exception cref="SerializationException"><c>SerializationException</c>.</exception>
        private static void SerializationError()
        {
            throw new SerializationException();
        }

        #region UML model serialization

        /// <summary>
        /// Serialization of UML part of the project. 
        /// Includes serialization of: primitive types, profiles and model.
        /// </summary>
        private void SerializeUml(Model model)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlUmlElement);

            /* REM J.M. 23. 6. 09 - primitive types and PSM profiles
             * are no longer stored in each project, they are retrieved from 
             * project template when project is loaded */
            // SerializePrimitiveTypes(model.Schema.PrimitiveTypes);

            SerializeProfiles(model);

            SerializeModel(model);

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlUmlElement
        }

        #region Datatypes serialization

        /// <summary>
        ///  Serialization of datatypes
        /// </summary>
        /// <param name="datatypes">Collection of datatypes to serialize</param>
        private void SerializeDatatypes(IEnumerable<DataType> datatypes)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlDatatypesElement);

            foreach (DataType d in datatypes)
            {
                SerializeDatatype(d);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlDatatypesElement
        }

        /// <summary>
        /// Serialization of one datatypes
        /// </summary>
        /// <param name="s">Datatype to serialize</param>
        private void SerializeDatatype(DataType s)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlDatatypeElement);

            //@id
            WriteID(s);

            //@name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, s.Name);

            // @simple
            if (s is SimpleDataType)
            {
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttSimple, "True");
                // @description
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDescription, ((SimpleDataType)s).DefaultXSDImplementation);

                // parent

                if (((SimpleDataType)s).Parent != null)
                {
                    if (!idTable.ContainsKey(((SimpleDataType)s).Parent))
                        idTable.AddWithNewId(((SimpleDataType)s).Parent);

                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttParentRef, idTable[((SimpleDataType)s).Parent].ToString());
                }
            }
            else
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttSimple, "False");


            xmlTextWriter.WriteEndElement(); // datatypes     
        }

        #endregion

        #region Profiles serialization

        /// <summary>
        ///  Serialization of UML profiles
        /// </summary>
        private void SerializeProfiles(Model model)
        {
            // profiles
            xmlTextWriter.WriteStartElement(XmlVoc.xmlProfilesElement);

            foreach (Profile p in model.Schema.Profiles.Where(profile => profile.Name != "XSem"))
            {
                // profile
                xmlTextWriter.WriteStartElement(XmlVoc.xmlProfileElement);

                //@id
                WriteID(p);

                //@name
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, p.Name);

                // stereotypes
                xmlTextWriter.WriteStartElement(XmlVoc.xmlStereotypesElement);
                foreach (Stereotype s in p.Stereotypes)
                {
                    SerializeStereotype(s);
                }
                xmlTextWriter.WriteEndElement(); // stereotypes

                xmlTextWriter.WriteEndElement(); // profile
            }

            xmlTextWriter.WriteEndElement(); // profiles
        }

        /// <summary>
        ///  Serialization of profile stereotypes
        /// </summary>
        /// <param name="s">Stereotype to serialize</param>
        private void SerializeStereotype(Stereotype s)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlStereotypeElement);

            // @id
            WriteID(s);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, s.Name);

            // receivers
            xmlTextWriter.WriteStartElement(XmlVoc.xmlReceiversElement);
            foreach (String str in s.AppliesTo)
            {
                // receiver
                xmlTextWriter.WriteStartElement(XmlVoc.xmlReceiverElement);
                // @type
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, str);
                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlReceiverElement
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlReceiversElement

            // properties
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPropertiesElement);
            foreach (Property p in s.Attributes)
            {
                SerializeProperty(p);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPropertiesElement

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlStereotypeElement

        }
        #endregion

        #region Model serialization

        /// <summary>
        /// Serialization of UML model
        /// </summary>
        private void SerializeModel(Model model)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlModelElement);

            #region model attributes

            // @id
            WriteID(model);

            if (model.Version != null)
            {
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVersion, model.Version.Number.ToString());
            }

            //@namespace for generated xml schemas
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNamespace, model.Schema.XMLNamespace);

            // @name
            if (project != null)
            {
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, project.Caption);
            }
            else
            {
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, versionManager.VersionedProjects[model.Version].Caption);
            }

            // @viewpoint
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttViewpoint, model.ViewPoint);

            #endregion

            SerializeComments(model.Comments);

            SerializeDatatypes(model.OwnedTypes);

            SerializePackages(model.NestedPackages);



            // Serialization of all PIM and PSM classes.
            // PSM classes are serialized under parent PIM classes.
            SerializeClasses(model.Classes);

            SerializeAssociations(model);

            SerializeGeneralizations(model.Generalizations);

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlModelElement
        }

        /// <summary>
        ///  Serialization of comments
        /// </summary>
        /// <param name="comments">Collection of comments to serialize</param>
        private void SerializeComments(IEnumerable<Comment> comments)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCommentsElement);

            foreach (Comment c in comments)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlCommentElement);

                // @id
                WriteID(c);

                // comment text
                xmlTextWriter.WriteString(c.Body);

                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
        }

        /// <summary>
        ///  Serialization of UML packages
        /// </summary>
        /// <param name="packages">Collection of packages to serialize</param>
        private void SerializePackages(IEnumerable<Package> packages)
        {
            // packages
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPackagesElement);

            foreach (Package p in packages)
            {
                // package
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPackageElement);

                // @id
                WriteID(p);

                // @name
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, p.Name);

                // comments
                SerializeComments(p.Comments);

                // stereotype instances
                SerializeStereotypeInstances(p.AppliedStereotypes);

                // datatypes
                SerializeDatatypes(p.OwnedTypes);

                // packages
                SerializePackages(p.NestedPackages);

                // PIM and PSM classes
                SerializeClasses(p.Classes);

                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPackageElement
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPackagesElement
        }

        #region PIM classes serialization

        /// <summary>
        /// Serialization of PIM classes
        /// </summary>
        /// <remarks>Derived PSM classes are serialized under the appropriate PIM classes</remarks>
        /// <param name="classes">Collection of PIM classes to serialize</param>
        private void SerializeClasses(IEnumerable<PIMClass> classes)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPimClassesElement);

            foreach (PIMClass c in classes)
            {

                if (c is AssociationClass)
                    continue;

                xmlTextWriter.WriteStartElement(XmlVoc.xmlPimClassElement);

                WriteID(c);

                // Serialization of common subelements of both PIM and PSM class
                SerializeClass(c);

                // Serialization of all PSM classes derived from the current one
                SerializePSMClasses(c.DerivedPSMClasses);

                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPimClassElement

            }
            xmlTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Serialization of common attributes of both PIM and PSM class
        /// </summary>
        /// <param name="c">PIM or PSM class to serialize</param>
        private void SerializeClass(Class c)
        {
            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, c.Name);

            // @ontologyEquivalent
            if (!(c is PSMClass)) xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttOntoEquiv, c.OntologyEquivalent);

            // class comments
            SerializeComments(c.Comments);

            // class stereotype instances
            SerializeStereotypeInstances(c.AppliedStereotypes);

            // class associations
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationsElement);
            foreach (Association a in c.Assocations)
            {
                SerializeClassAssociation(a);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationsElement

            // class properties / psm_attributes
            if (c is PSMClass)
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttributes);
            else
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPropertiesElement);

            foreach (Property pr in c.Attributes)
            {
                SerializeProperty(pr);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPropertiesElement

            // class operations
            SerializeOperations(c.Operations);
        }

        /// <summary>
        /// Serialization of applied sterotypes
        /// </summary>
        /// <param name="instances">Collection of applied stereotypes to serialize</param>
        private void SerializeStereotypeInstances(IEnumerable<StereotypeInstance> instances)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlStereotypeInstances);
            foreach (StereotypeInstance s in instances)
            {
                SerializeStereotypeInstance(s);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlStereotypeInstances
        }

        /// <summary>
        /// Serialization of one applied stereotype
        /// </summary>
        /// <param name="s">Stereotype instance to serialize</param>
        private void SerializeStereotypeInstance(StereotypeInstance s)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlStereotypeInstance);

            //@id
            WriteID(s);

            // @ref
            if (!idTable.ContainsKey(s.Stereotype)) SerializationError();
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[s.Stereotype].ToString());

            foreach (InstantiatedProperty ip in s.Attributes)
            {
                // value specification
                xmlTextWriter.WriteStartElement(XmlVoc.xmlValueSpecificationElement);

                //@id
                WriteID(ip);

                //@name
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, ip.Name);

                //value
                xmlTextWriter.WriteElementString(XmlVoc.xmlValueElement, (ip.Value.ToString()));

                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlValueSpecificationElement
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlStereotypeInstanceElement

        }

        /// <summary>
        /// Serialization of one class Association
        /// </summary>
        /// <param name="a">Association to serialize</param>
        private void SerializeClassAssociation(Association a)
        {
            // Adds association to the table
            if (!idTable.ContainsKey(a))
                idTable.AddWithNewId(a);

            // Adds all its ends to the table
            foreach (AssociationEnd ae in a.Ends)
            {
                if (!idTable.ContainsKey(ae))
                    idTable.AddWithNewId(ae);
            }

            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationElement);

            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[a].ToString());

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationElement

        }

        /// <summary>
        /// Serialization of shared attributes of both PIM and PSM property
        /// </summary>
        /// <param name="pr">PIM or PSM property to serialize</param>
        private void SerializeProperty(Property pr)
        {
            if (pr is PSMAttribute)
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttribute);
            else
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPropertyElement);

            SerializeInnerProperty(pr);

            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttOntoEquiv, pr.OntologyEquivalent);

            // For PSM Attribute only
            if (pr is PSMAttribute)
            {
                // @class_ref
                if (!idTable.ContainsKey(((PSMAttribute)pr).Class))
                    idTable.AddWithNewId(((PSMAttribute)pr).Class);

                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClassRef, idTable[((PSMAttribute)pr).Class].ToString());


                // @alias
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAlias, ((PSMAttribute)pr).Alias);

                // @att_implementtion
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttXSDImplementation, ((PSMAttribute)pr).XSDImplementation);

                // @att_ref (Null if is it free PSM Attribute)
                if (((PSMAttribute)pr).RepresentedAttribute != null)
                {
                    if (!idTable.ContainsKey(((PSMAttribute)pr).RepresentedAttribute))
                        idTable.AddWithNewId(((PSMAttribute)pr).RepresentedAttribute);

                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAttRef,
                                                       idTable[((PSMAttribute)pr).RepresentedAttribute].ToString());
                }
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPropertyElement OR XmlVoc.xmlPSMAttribute
        }

        /// <summary>
        /// Serialization of various internal attributes shared among PIM and PSM properties and Association ends.
        /// Called from SerializeProperty and SerializeAssociation.
        /// </summary>
        /// <param name="pr">property to serialize</param>
        private void SerializeInnerProperty(Property pr)
        {
            // @id
            WriteID(pr);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, pr.Name);

            // @type
            if (pr.Type != null && idTable.ContainsKey(pr.Type))
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, idTable[pr.Type].ToString());
            else
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, "null");

            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAggregation, pr.Aggregation.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDefault, pr.Default);
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDefaultValue, pr.DefaultValue == null ? "null" : pr.DefaultValue.ToString());

            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsComposite, pr.IsComposite.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsDerived, pr.IsDerived.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsDerivedUnion, pr.IsDerivedUnion.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsOrdered, pr.IsOrdered.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsReadOnly, pr.IsReadOnly.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsUnique, pr.IsUnique.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttLower, pr.Lower.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttUpper, pr.Upper.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVisibility, pr.Visibility.ToString());
        }

        /// <summary>
        /// Serialization of operations
        /// </summary>
        /// <param name="operations">Collection of operations to serialize</param>
        private void SerializeOperations(IEnumerable<Operation> operations)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlOperationsElement);

            foreach (Operation op in operations)
            {
                SerializeOperation(op);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlOperationsElement
        }

        /// <summary>
        /// Serialization of PIM/PSM class operation
        /// </summary>
        /// <param name="op">Operation to serialize</param>
        private void SerializeOperation(Operation op)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlOperationElement);

            // @id
            WriteID(op);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, op.Name);

            // @lower, @upper, @visibility
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttLower, op.Lower.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttUpper, op.Upper.ToString());
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVisibility, op.Visibility.ToString());

            // @type
            if (op.Type != null && idTable.ContainsKey(op.Type))
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, idTable[op.Type].ToString());
            else
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, "null");

            foreach (Parameter p in op.Parameters)
            {
                // parameter
                xmlTextWriter.WriteStartElement(XmlVoc.xmlParameterElement);

                // @id
                WriteID(p);

                // @name
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, p.Name);

                // @lower, @upper, @visibility
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttLower, p.Lower.ToString());
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttUpper, p.Upper.ToString());
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVisibility, p.Visibility.ToString());

                // @type
                if (p.Type != null && idTable.ContainsKey(p.Type))
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, idTable[p.Type].ToString());
                else
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, "null");

                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDirection, p.Direction.ToString());
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsOrdered, p.IsOrdered.ToString());
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsUnique, p.IsUnique.ToString());


                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlParameterElement
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlOperationElement
        }

        #endregion

        #region PSM classes serialization

        /// <summary>
        /// Serialization of PSM classes
        /// </summary>
        /// <param name="classes">Collection of PSM classes to serialize</param>
        private void SerializePSMClasses(IEnumerable<PSMClass> classes)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPsmClassesElement);

            foreach (PSMClass c in classes)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPsmClassElement);

                // @id
                WriteID(c);

                // @abstract
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAbstract, c.IsAbstract.ToString());

                // @structural_representative
                if (c.IsStructuralRepresentative)
                {
                    if (!idTable.ContainsKey(c.RepresentedPSMClass))
                        idTable.AddWithNewId(c.RepresentedPSMClass);

                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttStructuralRepresentative, idTable[c.RepresentedPSMClass].ToString());
                }

                SerializeClass(c);

                // element name
                xmlTextWriter.WriteElementString(XmlVoc.xmlElementName, c.ElementName);

                // allow_any_atribute
                xmlTextWriter.WriteElementString(XmlVoc.xmlAllowAnyAttribute, c.AllowAnyAttribute.ToString());

                // components
                SerializeComponents(c.Components);

                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPsmClassElement
            }

            xmlTextWriter.WriteEndElement();
        }

        #region PSM components

        /// <summary>
        /// Serialization of PSM class components.
        /// <para>
        /// These components can be: PSMAssociation, PSMAttributeContainer, PSMContentContainer, PSMContentChoice
        /// </para>
        /// </summary>
        /// <param name="c">Collection of components to serialize</param>
        private void SerializeComponents(IEnumerable<PSMSubordinateComponent> c)
        {
            // components
            xmlTextWriter.WriteStartElement(XmlVoc.xmlComponentsElement);

            int index = 0;
            foreach (PSMSubordinateComponent component in c)
            {
                if (component is PSMAssociation)
                    SerializePSMAssociation((PSMAssociation)component, index);
                else
                    if (component is PSMContentContainer)
                        SerializePSMContentContainer((PSMContentContainer)component, index);
                    else
                        if (component is PSMContentChoice)
                            SerializePSMContentChoice((PSMContentChoice)component, index);
                        else
                            if (component is PSMAttributeContainer)
                                SerializePSMAttContainer((PSMAttributeContainer)component, index);
                            else
                                // No other component type allowed
                                SerializationError();
                index++;
            }

            xmlTextWriter.WriteEndElement(); // components
        }

        /// <summary>
        /// Serialization of a class implementing PSMSuperordinateComponent interface
        /// </summary>
        /// <param name="c">Class to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMSuperordinate(PSMSuperordinateComponent c, int index)
        {
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIndex, index.ToString());

            // @id, @name, comments, applied stereotypes
            SerializeNamedElement(c);

            // components
            SerializeComponents(c.Components);
        }

        /// <summary>
        /// Serialization of a class that implements PSMSubordinateComponent interface
        /// </summary>
        /// <param name="c">Class to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMSubordinate(PSMSubordinateComponent c, int index)
        {
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIndex, index.ToString());

            // @id, @name, comments, applied stereotypes
            SerializeNamedElement(c);

            // parent
            if (!idTable.ContainsKey(c.Parent)) SerializationError();
            xmlTextWriter.WriteStartElement(XmlVoc.xmlParentElement);

            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[c.Parent].ToString());

            xmlTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Serialization of a class that implements NamedElement interface
        /// </summary>
        /// <param name="c">Class to serialize</param>
        private void SerializeNamedElement(NamedElement c)
        {
            // @id
            WriteID(c);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, c.Name);

            // comments
            SerializeComments(c.Comments);

            // stereotype instances
            SerializeStereotypeInstances(c.AppliedStereotypes);
        }

        #region PSM Association

        /// <summary>
        /// Serialization of PSM Class Union
        /// </summary>
        /// <remarks>Serializes passed PSM Class Union and all PSM Class Unions nested</remarks>
        /// <param name="c">PSMClassUnion to serialize</param>
        private void SerializeClassUnion(PSMClassUnion c)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMClassUnion);

            // @id, @name, comments, sterotype instances
            SerializeNamedElement(c);

            // components
            xmlTextWriter.WriteStartElement(XmlVoc.xmlComponentsElement);

            foreach (PSMAssociationChild p in c.Components)
            {
                if (p is PSMClass)
                {
                    // psm_class
                    xmlTextWriter.WriteStartElement(XmlVoc.xmlPsmClassElement);

                    // @ref
                    if (!idTable.ContainsKey(p))
                        idTable.AddWithNewId(p);
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[p].ToString());

                    xmlTextWriter.WriteEndElement(); // psm_class
                }
                else
                    if (p is PSMClassUnion)
                    {
                        // @ref
                        if (idTable.ContainsKey(p))
                        {
                            // class_union
                            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMClassUnion);
                            // @ref
                            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[p].ToString());
                            xmlTextWriter.WriteEndElement(); // class_union
                        }
                        else
                        {
                            SerializeClassUnion((PSMClassUnion)p);
                        }
                    }
                    else
                        SerializationError();
            }

            xmlTextWriter.WriteEndElement(); // components

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPSMClassUnion
        }

        /// <summary>
        /// Serialization of PSM Association
        /// </summary>
        /// <param name="a">PSMAssociation to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMAssociation(PSMAssociation a, int index)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAssociation);

            // @id, @name, comments, applied stereotypes, parent
            SerializePSMSubordinate(a, index);

            // Serializes PSM Class unions first
            if (a.Child is PSMClassUnion)
                SerializeClassUnion((PSMClassUnion)a.Child);

            if (!idTable.ContainsKey(a.Child))
                idTable.AddWithNewId(a.Child);

            // child
            xmlTextWriter.WriteStartElement(XmlVoc.xmlChildElement);
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[a.Child].ToString());
            // @lower
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttMin, a.Lower.ToString());
            // @upper
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttMax, a.Upper.ToString());
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlChildElement

            // used generalizations
            if (a.UsedGeneralizations.Count > 0)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlUsedGeneralizations);
                foreach (Generalization g in a.UsedGeneralizations)
                {
                    if (!idTable.ContainsKey(g))
                        idTable.AddWithNewId(g);

                    xmlTextWriter.WriteStartElement(XmlVoc.xmlUsedGeneralization);
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[g].ToString());
                    xmlTextWriter.WriteEndElement();
                }

                xmlTextWriter.WriteEndElement();
            }

            // nesting joins
            xmlTextWriter.WriteStartElement(XmlVoc.xmlNestingJoins);
            foreach (NestingJoin nj in a.NestingJoins)
            {
                SerializeNestingJoin(nj);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlNestingJoins

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPSMAssociation
        }

        /// <summary>
        /// Serialization of Nesting Join
        /// </summary>
        /// <param name="nj">NestingJoin to serialize</param>
        private void SerializeNestingJoin(NestingJoin nj)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlNestingJoin);

            // @core_class_ref
            if (!idTable.ContainsKey(nj.CoreClass))
                idTable.AddWithNewId(nj.CoreClass);
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttCoreClassRef, idTable[nj.CoreClass].ToString());

            // child
            xmlTextWriter.WriteStartElement(XmlVoc.xmlChildElement);
            SerializePimPath(nj.Child);
            xmlTextWriter.WriteEndElement();

            // parent
            xmlTextWriter.WriteStartElement(XmlVoc.xmlParentElement);
            SerializePimPath(nj.Parent);
            xmlTextWriter.WriteEndElement();

            // context
            xmlTextWriter.WriteStartElement(XmlVoc.xmlContext);
            foreach (PIMPath pimp in nj.Context)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPimPath);
                SerializePimPath(pimp);
                xmlTextWriter.WriteEndElement();
            }
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlNestingJoin
        }

        /// <summary>
        /// Serialization of PIM Path
        /// </summary>
        /// <param name="p">PIMPath to serialize</param>
        private void SerializePimPath(PIMPath p)
        {
            //xmlTextWriter.WriteStartElement(XmlVoc.xmlPimPath);

            foreach (PIMStep s in p.Steps)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPimStep);

                // @start_ref
                if (!idTable.ContainsKey(s.Start))
                    idTable.AddWithNewId(s.Start);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttStartRef, idTable[s.Start].ToString());

                // @end_ref
                if (!idTable.ContainsKey(s.End))
                    idTable.AddWithNewId(s.End);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttEndRef, idTable[s.End].ToString());

                // @association_ref
                if (!idTable.ContainsKey(s.Association))
                    idTable.AddWithNewId(s.Association);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAssociation, idTable[s.Association].ToString());

                xmlTextWriter.WriteEndElement();
            }

            //xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPimPath

        }

        #endregion

        #region PSM Attribute Container

        /// <summary>
        /// Serialization of PSM Attribute Container
        /// </summary>
        /// <param name="c">PSM Attribute Container to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMAttContainer(PSMAttributeContainer c, int index)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttContainer);

            SerializePSMSubordinate(c, index);

            // psm attributes
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttributes);
            foreach (PSMAttribute a in c.PSMAttributes)
                SerializeProperty(a);
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPSMAttributes

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPSMAttContainer
        }

        #endregion

        #region PSM Content Choice

        /// <summary>
        /// Serialization of PSM Content Choice
        /// </summary>
        /// <param name="c">PSMContentChoice to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMContentChoice(PSMContentChoice c, int index)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentChoice);

            // @id, @name, comments, applied stereotypes, components
            SerializePSMSuperordinate(c, index);

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlContentChoice  
        }

        #endregion

        #region PSM Content Container

        /// <summary>
        /// Serialization of PSM Content Container
        /// </summary>
        /// <param name="c">PSMContentContainer to serialize</param>
        /// <param name="index">The index of the component</param>
        private void SerializePSMContentContainer(PSMContentContainer c, int index)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentContainer);

            // @id, @name, comments, applied stereotypes, components
            SerializePSMSuperordinate(c, index);

            // element label
            xmlTextWriter.WriteElementString(XmlVoc.xmlElementLabel, c.ElementLabel);

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlContentContainer
        }

        #endregion

        #endregion

        #endregion

        #region Associations serialization

        /// <summary>
        /// Serialization of UML associations
        /// </summary>
        private void SerializeAssociations(Model model)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationsElement);

            foreach (Association a in model.Associations)
            {
                if (a is AssociationClass)
                    SerializeAssociationClass((AssociationClass)a);
                else
                    SerializeAssociation(a);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationsElement
        }

        /// <summary>
        /// Serialization of Association class
        /// </summary>
        /// <param name="a">AssociationClass to serialize</param>
        private void SerializeAssociationClass(AssociationClass a)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationClassElement);

            // @id
            WriteID(a);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, a.Name);

            // comments
            SerializeComments(a.Comments);

            // stereotype instances
            SerializeStereotypeInstances(a.AppliedStereotypes);


            // class associations
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationsElement);
            foreach (Association e in a.Assocations)
            {
                SerializeClassAssociation(e);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationsElement

            // class properties / psm_attributes
            if (a is PSMClass)
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttributes);
            else
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPropertiesElement);

            foreach (Property pr in a.Attributes)
            {
                SerializeProperty(pr);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlPropertiesElement

            // class operations
            SerializeOperations(a.Operations);

            // association ends
            foreach (AssociationEnd ae in a.Ends)
            {
                // association end
                xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndElement);

                if (!idTable.ContainsKey(ae))
                    SerializationError();

                SerializeInnerProperty(ae);

                // @class
                if (!idTable.ContainsKey(ae.Class)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClass, idTable[ae.Class].ToString());

                xmlTextWriter.WriteEndElement(); // xmlAssociationEndElement
            }

            // Serialization of all PSM classes derived from the current one
            SerializePSMClasses(a.DerivedPSMClasses);

            xmlTextWriter.WriteEndElement();
        }

        /// <summary>
        ///  Serialization of one UML Association
        /// </summary>
        /// <param name="a">Association to serialize</param>
        private void SerializeAssociation(Association a)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationElement);

            // @id
            WriteID(a);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, a.Name);

            // @ontologyEquivalent
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttOntoEquiv, a.OntologyEquivalent);

            // comments
            SerializeComments(a.Comments);

            // stereotype instances
            SerializeStereotypeInstances(a.AppliedStereotypes);

            foreach (AssociationEnd ae in a.Ends)
            {
                // association end
                xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndElement);

                if (!idTable.ContainsKey(ae))
                    SerializationError();

                SerializeInnerProperty(ae);

                // @class
                if (!idTable.ContainsKey(ae.Class)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClass, idTable[ae.Class].ToString());

                xmlTextWriter.WriteEndElement(); // xmlAssociationEndElement
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationElement
        }

        /// <summary>
        ///  Serialization of UML generalizations
        /// </summary>
        /// <param name="generalizations">Collection of generalizations to serialize</param>
        private void SerializeGeneralizations(IEnumerable<Generalization> generalizations)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlGeneralizationsElement);

            foreach (Generalization g in generalizations)
            {
                // psm generalization
                if (g.General is PSMClass)
                    xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMGeneralizationElement);
                // pim generalization
                else
                    xmlTextWriter.WriteStartElement(XmlVoc.xmlPIMGeneralizationElement);

                // @id
                WriteID(g);

                // @general
                if (!idTable.ContainsKey(g.General)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttGeneral, idTable[g.General].ToString());

                // @specific
                if (!idTable.ContainsKey(g.Specific)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttSpecific, idTable[g.Specific].ToString());

                // @is_substitable
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIsSubstitable, g.IsSubstituable.ToString());

                // comments
                SerializeComments(g.Comments);

                xmlTextWriter.WriteEndElement(); // xmlGeneralizationElement

            }

            xmlTextWriter.WriteEndElement();

        }

        #endregion

        #endregion



        /// <summary>
        ///  Asigns ID to a passed object and adds both to idTable.
        ///  Writes asigned ID to current XML element stored in xmlTextWriter
        /// </summary>
        /// <param name="o">Object to get the ID</param>
        private void WriteID(Element o)
        {
            if (!idTable.ContainsKey(o))
            {
                idTable.AddWithNewId(o);
            }
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttID, idTable[o].ToString());
        }

        #endregion

        #region Visual diagrams serialization

        private void SerializeDiagrams(Project project)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlDiagramsElement);

            // abstract diagrams
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAbstractDiagramsElement);
            foreach (PIMDiagram diagram in project.PIMDiagrams)
            {
                if (!diagramIdTable.ContainsKey(diagram))
                {
                    diagramIdTable[diagram] = diagramIdTable.Count;
                }
            }

            foreach (PSMDiagram diagram in project.PSMDiagrams)
            {
                if (!diagramIdTable.ContainsKey(diagram))
                {
                    diagramIdTable[diagram] = diagramIdTable.Count;
                }
            }

            int n = 0;
            foreach (PIMDiagram diagram in project.PIMDiagrams)
            {
                SerializeDiagram(diagram, n++);
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAbstractDiagramsElement

            // specific diagrams
            xmlTextWriter.WriteStartElement(XmlVoc.xmlSpecificDiagramsElement);
            n = 0;
            foreach (PSMDiagram d in project.PSMDiagrams)
            {
                SerializeDiagram(d, n++);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlSpecificDiagramsElement

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlDiagramsElement
        }

        private void SerializeDiagram(Diagram diagram, int no)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlDiagramElement);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, diagram.CaptionNoVersion);

            // @no
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNo, no.ToString());

            // @diagram id
            //if (versionManager != null)
            {
                if (!diagramIdTable.ContainsKey(diagram))
                {
                    diagramIdTable[diagram] = diagramIdTable.Count;
                }
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDiagramId, diagramIdTable[diagram].ToString());
            }

            if (diagram is PSMDiagram)
            {
                string targetNamespace = ((PSMDiagram)diagram).TargetNamespace;
                if (!String.IsNullOrEmpty(targetNamespace))
                {
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttTargetNamespace, targetNamespace);
                }
                SerializeRoots((PSMDiagram)diagram);
            }

            foreach (KeyValuePair<Element, ViewHelper> pair in diagram.DiagramElements)
            {
                if (pair.Key.GetType().GetInterface("AssociationClass") != null && pair.Value is AssociationClassViewHelper)
                    SerializeVisualAssociationClass((AssociationClass)pair.Key, idTable[pair.Key], (AssociationClassViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PIMClass") != null && pair.Value is ClassViewHelper)
                    SerializeVisualClass((Class)pair.Key, idTable[pair.Key], (ClassViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMClass") != null && pair.Value is PSMElementViewHelper)
                    SerializeVisualClass((Class)pair.Key, idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("Association") != null && pair.Value is AssociationViewHelper)
                    SerializeVisualPIMAssociation((Association)pair.Key, idTable[pair.Key], (AssociationViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("Comment") != null && pair.Value is CommentViewHelper)
                    SerializeVisualComment((Comment)pair.Key, idTable[pair.Key], (CommentViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("Generalization") != null && pair.Value is GeneralizationViewHelper)
                    SerializeVisualGeneralization((Generalization)pair.Key, idTable[pair.Key], (GeneralizationViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMAttributeContainer") != null && pair.Value is PSMElementViewHelper)
                    SerializeVisualPSMAttContainer((PSMAttributeContainer)pair.Key, idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMAssociation") != null && pair.Value is PSMAssociationViewHelper)
                    SerializeVisualPSMAssociation((PSMAssociation)pair.Key, idTable[pair.Key], (PSMAssociationViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMContentContainer") != null && pair.Value is PSMElementViewHelper)
                    SerializeVisualPSMContentContainer((PSMContentContainer)pair.Key, idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMContentChoice") != null && pair.Value is PSMElementViewHelper)
                    SerializeVisualPSMContentChoice((PSMContentChoice)pair.Key, idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                else if (pair.Key.GetType().GetInterface("PSMClassUnion") != null && pair.Value is PSMElementViewHelper)
                    SerializeVisualPSMClassUnion((PSMClassUnion)pair.Key, idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                else if (typeof(PSMDiagramReference).IsAssignableFrom(pair.Key.GetType()) && pair.Value is PSMElementViewHelper)
                    SerializeVisualPSMDiagramReference((PSMDiagramReference)pair.Key, (PSMElementViewHelper)pair.Value);
                else
                {
                    Console.WriteLine("Another interface found: " + pair.Key.GetType());
                    SerializationError();
                }


            }

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeRoots(PSMDiagram d)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlRoots);

            foreach (PSMClass c in d.Roots)
            {
                if (!idTable.ContainsKey(c))
                    SerializationError();
                XCaseGuid id = idTable[c];

                xmlTextWriter.WriteStartElement(XmlVoc.xmlRoot);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, id.ToString());
                xmlTextWriter.WriteEndElement();

            }


            xmlTextWriter.WriteEndElement();

        }

        // ReSharper disable UnusedParameter.Local

        private void SerializeVisualAssociationClass(AssociationClass c, XCaseGuid refid, AssociationClassViewHelper view)
        {
            // association class
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationClassElement);

            //@ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, refid.ToString());

            // @methods_collapsed 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttOperationsCollapsed, view.OperationsCollapsed.ToString());

            // @properties_collapsed           
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAttributesCollapsed, view.AttributesCollapsed.ToString());

            // @element_label_collapsed
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttElementNameLabelCollapsed, view.ElementNameLabelCollapsed.ToString());

            // @element_label_aligned_right 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttElementNameLabelAlignedRight, view.ElementNameLabelAlignedRight.ToString());

            //@diamond
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDiamond, view.AssociationViewHelper.UseDiamond.ToString());

            // appearance
            SerializeAppearance(view);

            // points
            SerializeVisualPoints(view.Points);

            // diamond
            if (view.AssociationViewHelper.UseDiamond)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlDiamond, null);
                SerializeInnerApperanace(view.AssociationViewHelper);
                xmlTextWriter.WriteEndElement();
            }

            // namelabel
            xmlTextWriter.WriteStartElement(XmlVoc.xmlNameLabel);
            SerializeInnerApperanace(view.AssociationViewHelper.MainLabelViewHelper);
            xmlTextWriter.WriteEndElement(); // namelabel

            // association ends
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndsElement);
            foreach (AssociationEndViewHelper aeh in view.AssociationViewHelper.AssociationEndsViewHelpers)
            {
                SerializeVisualAssociationEnd(aeh);
            }
            xmlTextWriter.WriteEndElement();


            xmlTextWriter.WriteEndElement();

        }

        private void SerializeVisualElement(XCaseGuid refid, PositionableElementViewHelper view, IEnumerable<rPoint> points)
        {
            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, refid.ToString());

            // appearance
            SerializeAppearance(view);

            // points
            SerializeVisualPoints(points);
        }

        private void SerializeVisualClass(Class c, XCaseGuid refid, ClassViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlClassElement);

            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, refid.ToString());

            // @methods_collapsed 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttOperationsCollapsed, view.OperationsCollapsed.ToString());

            // @properties_collapsed           
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAttributesCollapsed, view.AttributesCollapsed.ToString());

            // @element_label_collapsed
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttElementNameLabelCollapsed, view.ElementNameLabelCollapsed.ToString());

            // @element_label_aligned_right 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttElementNameLabelAlignedRight, view.ElementNameLabelAlignedRight.ToString());


            // appearance
            SerializeAppearance(view);

            // points
            if (c is PSMClass && view is PSMElementViewHelper && ((PSMElementViewHelper)view).ConnectorViewHelper != null)
                SerializeVisualPoints((((PSMElementViewHelper)view).ConnectorViewHelper.Points));

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualComment(Comment c, XCaseGuid refid, CommentViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCommentElement);

            SerializeVisualElement(refid, view, view.LinePoints);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualGeneralization(Generalization g, XCaseGuid refid, GeneralizationViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlGeneralizationElement);

            SerializeVisualElement(refid, view, view.Points);

            xmlTextWriter.WriteEndElement();
        }

        #region Visual PSM Elements

        private void SerializeVisualPSMAttContainer(PSMAttributeContainer c, XCaseGuid refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttContainer);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMContentContainer(PSMContentContainer c, XCaseGuid refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentContainer);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMContentChoice(PSMContentChoice c, XCaseGuid refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentChoice);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMClassUnion(PSMClassUnion c, XCaseGuid refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMClassUnion);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMDiagramReference(PSMDiagramReference r, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMDiagramReference);
       
            if (!idTable.ContainsKey(r))
                idTable.AddWithNewId(r);

            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[r].ToString());
            // @referenced_diagram
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttReferencedDiagramId, diagramIdTable[r.ReferencedDiagram].ToString());
            // @referencing_diagram
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttReferencingDiagramId, diagramIdTable[r.ReferencingDiagram].ToString());
            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, r.Name);
            // @schema_location 
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttSchemaLocation, r.SchemaLocation);
            // @local
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttLocal, r.Local.ToString().ToLower());
            // @namespace_prefix
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNamespacePrefix, r.NamespacePrefix);
            // @namespace
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNamespace, r.Namespace);
            // appearance
            SerializeAppearance(view);

            xmlTextWriter.WriteEndElement();
        }

        #endregion

        #region Visual Associations

        /// <summary>
        /// Serialization of visual parameters for one PIM Association
        /// </summary>
        /// <param name="a"></param>
        /// <param name="refid"></param>
        /// <param name="view"></param>
        private void SerializeVisualPIMAssociation(Association a, XCaseGuid refid, AssociationViewHelper view)
        {
            // association
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationElement);

            //@ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, refid.ToString());

            //@diamond
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttDiamond, view.UseDiamond.ToString());

            // diamond
            if (view.UseDiamond)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlDiamond, null);
                SerializeInnerApperanace(view);
                xmlTextWriter.WriteEndElement();
            }

            // namelabel
            xmlTextWriter.WriteStartElement(XmlVoc.xmlNameLabel);
            SerializeInnerApperanace(view.MainLabelViewHelper);
            xmlTextWriter.WriteEndElement(); // namelabel

            // points
            if (view.UseDiamond)
                xmlTextWriter.WriteElementString(XmlVoc.xmlPoints, null);
            else
                SerializeVisualPoints(view.Points);

            // association ends
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndsElement);
            foreach (AssociationEndViewHelper aeh in view.AssociationEndsViewHelpers)
            {
                SerializeVisualAssociationEnd(aeh);
            }
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement(); // association
        }

        private void SerializeVisualAssociationEnd(AssociationEndViewHelper aeh)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndElement);

            //@id
            if (!idTable.ContainsKey(aeh.AssociationEnd)) SerializationError();
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, idTable[aeh.AssociationEnd].ToString());

            //Multiplicity label visualization
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCardinalityLabel);
            SerializeInnerApperanace(aeh.MultiplicityLabelViewHelper);
            xmlTextWriter.WriteEndElement();

            //Role label visualization
            xmlTextWriter.WriteStartElement(XmlVoc.xmlRoleLabel);
            SerializeInnerApperanace(aeh.RoleLabelViewHelper);
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMAssociation(PSMAssociation a, XCaseGuid refid, PSMAssociationViewHelper view)
        {
            // association
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAssociation);

            SerializeVisualElement(refid, view, view.Points);

            // multiplicity label
            xmlTextWriter.WriteStartElement(XmlVoc.xmlMultiplicityLabel);
            SerializeInnerApperanace(view.MultiplicityLabelViewHelper);
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement();
        }

        #endregion

        #region Common visual

        /// <summary>
        /// Serializes basic appearance attributes for a PositionableElement: width, height, X and Y
        /// </summary>
        /// <param name="view">PositionableElementViewHelper to serialize</param>
        private void SerializeAppearance(PositionableElementViewHelper view)
        {
            // appearance
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAppearanceElement);

            SerializeInnerApperanace(view);

            xmlTextWriter.WriteEndElement(); // appearance
        }

        private void SerializeInnerApperanace(PositionableElementViewHelper view)
        {
            // Width
            xmlTextWriter.WriteStartElement(XmlVoc.xmlWidthElement);
            if (view.Width.CompareTo(Double.NaN) != 0)
                xmlTextWriter.WriteString(Math.Round(view.Width).ToString());
            xmlTextWriter.WriteEndElement();

            // Height
            xmlTextWriter.WriteStartElement(XmlVoc.xmlHeightElement);
            if (view.Height.CompareTo(Double.NaN) != 0)
                xmlTextWriter.WriteString(Math.Round(view.Height).ToString());
            xmlTextWriter.WriteEndElement();

            // Coordinate x
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCoordXElement);
            xmlTextWriter.WriteString(Math.Round(view.X).ToString());
            xmlTextWriter.WriteEndElement();

            // Coordinate y
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCoordYElement);
            xmlTextWriter.WriteString(Math.Round(view.Y).ToString());
            xmlTextWriter.WriteEndElement();

        }

        /// <summary>
        /// Serializes all visual points from the collection.
        /// Each visual point is determined by X and Y coordinate
        /// </summary>
        /// <param name="points">Collection of points to serialize</param>
        private void SerializeVisualPoints(IEnumerable<rPoint> points)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPoints);

            foreach (System.Windows.Point point in points)
            {
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPoint);

                // Coordinate x
                xmlTextWriter.WriteStartElement(XmlVoc.xmlCoordXElement);
                xmlTextWriter.WriteString(Math.Round(point.X).ToString());
                xmlTextWriter.WriteEndElement();

                // Coordinate y
                xmlTextWriter.WriteStartElement(XmlVoc.xmlCoordYElement);
                xmlTextWriter.WriteString(Math.Round(point.Y).ToString());
                xmlTextWriter.WriteEndElement();

                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
        }

        #endregion

        // ReSharper restore UnusedParameter.Local

        #endregion

    }
}