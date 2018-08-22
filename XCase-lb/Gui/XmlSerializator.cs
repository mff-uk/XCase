using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using XCase.Model;

namespace XCase.Gui
{
    /// <summary>
    /// Exception caused by some model inconsistency during the serialization
    /// </summary>
    public class SerializationException : SystemException
    { }

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

        /// <summary>
        /// UML model to serialize
        /// </summary>
        private Model.Model model;

        /// <summary>
        /// Name of the serialized project
        /// </summary>
        private string projectName = "Project1";

        /// <summary>
        ///  PIM diagrams in XCase project to serialize
        /// </summary>
        private ObservableCollection<PIMDiagram> PIMDiagrams;

        /// <summary>
        ///  PSM diagrams in XCase project to serialize
        /// </summary>
        private ObservableCollection<PSMDiagram> PSMDiagrams;

        /// <summary>
        /// Table of all serialized elements that have unique ID attribute in the output XML file.
        /// <list type="">
        /// <item>Key =  Element</item>
        /// <item>Value = ID</item>
        /// </list>
        /// </summary>
        private Hashtable idTable = new Hashtable();

        /// <summary>
        /// XmlTextWriter used for writing the content of the output XML file
        /// </summary>
        private XmlTextWriter xmlTextWriter;

        #endregion

        /// <summary>
        /// Initialization of serializator
        /// </summary>
        /// <param name="project">XCase project to serialize</param>
        public XmlSerializator(Project project)
        {
            model = project.Schema.Model;
            PIMDiagrams = project.PIMDiagrams;
            PSMDiagrams = project.PSMDiagrams;

            projectName = project.Caption;
        }

        /// <summary>
        /// Serializes project to a XML file
        /// </summary>
        /// <param name="filename">Name of XML file, which the project is serialized to</param>
        public bool SerilizeTo(string filename)
        {
			//try
			//{
                // Start element
                xmlTextWriter = new XmlTextWriter(filename, Encoding.UTF8);
                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement(XmlVoc.xmlRootElement);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttXmlns, XmlVoc.defaultNamespace);
                // @version
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttVersion, "1.0");

                // Serialization of UML part
                SerializeUml();

                // Serialization of visualization part
                SerializeDiagrams();

                // End element
                xmlTextWriter.WriteEndElement(); // XmlVoc.xmlRootElement
                xmlTextWriter.WriteEndDocument();
                xmlTextWriter.Close();
			//}
			//catch
			//{
			//    return false;
			//} 

            return true;
        }
       
        /// <summary>
        /// Called from the serialization in the case of unexpected error (typically model inconsistency)
        /// </summary>
        private void SerializationError()
        { 
            throw new SerializationException();
        }

        #region UML model serialization

        /// <summary>
        /// Serialization of UML part of the project. 
        /// Includes serialization of: primitive types, profiles and model.
        /// </summary>
        private void SerializeUml()
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlUmlElement);

            SerializePrimitiveTypes(model.Schema.PrimitiveTypes);

            SerializeProfiles();
            
            SerializeModel();

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlUmlElement
        }

        #region Datatypes serialization

        /// <summary>
        /// Serialization of primitive types
        /// </summary>
        /// <param name="datatypes">Collection of primitive types to serialize</param>
        private void SerializePrimitiveTypes(ObservableCollection<SimpleDataType> datatypes)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPrimitiveTypes);

            foreach (SimpleDataType d in datatypes)
            {
                SerializeDatatype(d);    
            }

            xmlTextWriter.WriteEndElement();
        }
        
        /// <summary>
        ///  Serialization of datatypes
        /// </summary>
        /// <param name="datatypes">Collection of datatypes to serialize</param>
        private void SerializeDatatypes(ObservableCollection<DataType> datatypes)
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
                if (!idTable.Contains(((SimpleDataType)s).Parent))
                  idTable.Add(((SimpleDataType)s).Parent, idTable.Count);

                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttParentRef, ((int)idTable[((SimpleDataType)s).Parent]).ToString());
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
        private void SerializeProfiles()
        {
            // profiles
            xmlTextWriter.WriteStartElement(XmlVoc.xmlProfilesElement);

            foreach (Profile p in model.Schema.Profiles)
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
        private void SerializeModel()
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlModelElement);

            #region model attributes
            
            // @id
            WriteID(model);

            //@namespace for generated xml schemas
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNamespace, model.Schema.XMLNamespace);

            // @name
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, projectName);

            // @viewpoint
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttViewpoint, model.ViewPoint);

            #endregion

            SerializeComments(model.Comments);
            
            SerializeDatatypes(model.OwnedTypes);
          
            SerializePackages(model.NestedPackages);
            

          
            // Serialization of all PIM and PSM classes.
            // PSM classes are serialized under parent PIM classes.
            SerializeClasses(model.Classes);

            SerializeAssociations();

            SerializeGeneralizations(model.Generalizations);

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlModelElement
        }

        /// <summary>
        ///  Serialization of comments
        /// </summary>
        /// <param name="comments">Collection of comments to serialize</param>
        private void SerializeComments(ObservableCollection<Comment> comments)
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
        private void SerializePackages(ObservableCollection<Package> packages)
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
        private void SerializeClasses(ObservableCollection<PIMClass> classes)
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
        private void SerializeStereotypeInstances(ObservableCollection<StereotypeInstance> instances)
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
            if (!idTable.Contains(s.Stereotype)) SerializationError();
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[s.Stereotype]).ToString());

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
            if (!idTable.Contains(a))
                idTable.Add(a, idTable.Count);

            // Adds all its ends to the table
            foreach (AssociationEnd ae in a.Ends)
            {
                if (!idTable.Contains(ae))
                    idTable.Add(ae, idTable.Count);
            }

            xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationElement);

            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[a]).ToString());

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
            else if (pr is Property)
                xmlTextWriter.WriteStartElement(XmlVoc.xmlPropertyElement);

            SerializeInnerProperty(pr);

            // For PSM Attribute only
            if (pr is PSMAttribute)
            {
                // @class_ref
                if (!idTable.Contains(((PSMAttribute)pr).Class))
                     idTable.Add(((PSMAttribute)pr).Class, idTable.Count);

                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClassRef, idTable[((PSMAttribute)pr).Class].ToString());


                // @alias
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAlias, ((PSMAttribute)pr).Alias);
                
                // @att_implementtion
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttXSDImplementation, ((PSMAttribute)pr).XSDImplementation);
            
                // @att_ref (Null if is it free PSM Attribute)
                if (((PSMAttribute)pr).RepresentedAttribute != null)
                {
                    if (!idTable.Contains(((PSMAttribute)pr).RepresentedAttribute))
                        idTable.Add(((PSMAttribute)pr).RepresentedAttribute, idTable.Count);

                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAttRef,
                            ((int)idTable[((PSMAttribute)pr).RepresentedAttribute]).ToString());
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
            if (pr.Type != null && idTable.Contains(pr.Type))
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, ((int)idTable[pr.Type]).ToString());
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
        private void SerializeOperations(ObservableCollection<Operation> operations)
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
            if (op.Type != null && idTable.Contains(op.Type))
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, ((int)idTable[op.Type]).ToString());
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
                if (p.Type != null && idTable.Contains(p.Type))
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttType, ((int)idTable[p.Type]).ToString());
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
        private void SerializePSMClasses(ObservableCollection<PSMClass> classes)
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
                if (c.RepresentedPSMClass != null)
                {
                    if (!idTable.Contains(c.RepresentedPSMClass)) 
                        idTable.Add(c.RepresentedPSMClass, idTable.Count);
                    
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttStructuralRepresentative, ((int)idTable[c.RepresentedPSMClass]).ToString());
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
        private void SerializeComponents(ObservableCollection<PSMSubordinateComponent> c)
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
        private void SerializePSMSubordinate(PSMSubordinateComponent c, int index )
        {
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttIndex, index.ToString());

            // @id, @name, comments, applied stereotypes
            SerializeNamedElement(c);

            // parent
            if (!idTable.Contains(c.Parent)) SerializationError();
            xmlTextWriter.WriteStartElement(XmlVoc.xmlParentElement);

            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[c.Parent]).ToString());

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
                    if (!idTable.Contains(p))
                        idTable.Add(p, idTable.Count);
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[p]).ToString());

                    xmlTextWriter.WriteEndElement(); // psm_class
                }
                else
                    if (p is PSMClassUnion)
                    {
                        // @ref
                        if (idTable.Contains(p))
                        {
                            // class_union
                            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMClassUnion);
                            // @ref
                            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[p]).ToString());
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
        private void SerializePSMAssociation(PSMAssociation a, int index)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAssociation);

            // @id, @name, comments, applied stereotypes, parent
            SerializePSMSubordinate(a, index);

            // Serializes PSM Class unions first
            if (a.Child is PSMClassUnion)
                SerializeClassUnion((PSMClassUnion)a.Child);

            if (!idTable.Contains(a.Child))
                    idTable.Add(a.Child, idTable.Count);
            
            // child
            xmlTextWriter.WriteStartElement(XmlVoc.xmlChildElement);
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[a.Child]).ToString());
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
                    if (!idTable.Contains(g))
                        idTable.Add(g, idTable.Count);

                    xmlTextWriter.WriteStartElement(XmlVoc.xmlUsedGeneralization);
                    xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[g]).ToString());
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
            if (!idTable.Contains(nj.CoreClass))
                idTable.Add(nj.CoreClass, idTable.Count);
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttCoreClassRef, ((int)idTable[nj.CoreClass]).ToString());

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
            foreach(PIMPath pimp in nj.Context)
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
                if (!idTable.Contains(s.Start))
                    idTable.Add(s.Start, idTable.Count);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttStartRef, ((int)idTable[s.Start]).ToString());

                // @end_ref
                if (!idTable.Contains(s.End))
                    idTable.Add(s.End, idTable.Count);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttEndRef, ((int)idTable[s.End]).ToString());

                // @association_ref
                if (!idTable.Contains(s.Association))
                    idTable.Add(s.Association, idTable.Count);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttAssociation, ((int)idTable[s.Association]).ToString());

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
        private void SerializeAssociations()
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

                if (!idTable.Contains(ae))
                    SerializationError();

                SerializeInnerProperty(ae);

                // @class
                if (!idTable.Contains(ae.Class)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClass, ((int)idTable[ae.Class]).ToString());

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

            // comments
            SerializeComments(a.Comments);

            // stereotype instances
            SerializeStereotypeInstances(a.AppliedStereotypes);
           
            foreach (AssociationEnd ae in a.Ends)
            {
                // association end
                xmlTextWriter.WriteStartElement(XmlVoc.xmlAssociationEndElement);

                if (!idTable.Contains(ae)) 
                    SerializationError();

                SerializeInnerProperty(ae);  
              
                // @class
                if (!idTable.Contains(ae.Class))  SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttClass, ((int)idTable[ae.Class]).ToString());

                xmlTextWriter.WriteEndElement(); // xmlAssociationEndElement
            }

            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAssociationElement
        }
 
        /// <summary>
        ///  Serialization of UML generalizations
        /// </summary>
        /// <param name="generalizations">Collection of generalizations to serialize</param>
        private void SerializeGeneralizations(ObservableCollection<Generalization> generalizations)
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
                if (!idTable.Contains(g.General)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttGeneral, ((int)idTable[g.General]).ToString());
                
                // @specific
                if (!idTable.Contains(g.Specific)) SerializationError();
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttSpecific, ((int)idTable[g.Specific]).ToString());

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
        private void WriteID(object o)
        {
            if (!idTable.Contains(o))
                idTable.Add(o, idTable.Count);

            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttID, ((int)idTable[o]).ToString());
        }

        #endregion

        #region Visual diagrams serialization

        private void SerializeDiagrams()
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlDiagramsElement);

            // abstract diagrams
            xmlTextWriter.WriteStartElement(XmlVoc.xmlAbstractDiagramsElement);
            int n = 0;
            foreach (Diagram d in PIMDiagrams)
            {
                SerializeDiagram(d, n++);
            }
            xmlTextWriter.WriteEndElement(); // XmlVoc.xmlAbstractDiagramsElement

            // specific diagrams
            xmlTextWriter.WriteStartElement(XmlVoc.xmlSpecificDiagramsElement);
            n = 0;
            foreach (Diagram d in PSMDiagrams)
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
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttName, diagram.Caption);

            // @no
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttNo, no.ToString());

            if (diagram is PSMDiagram)
                SerializeRoots((PSMDiagram)diagram);

            foreach (KeyValuePair<Element, ViewHelper> pair in diagram.DiagramElements)
            {
                if (pair.Key.GetType().GetInterface("AssociationClass") != null && pair.Value is AssociationClassViewHelper)
                    SerializeVisualAssociationClass((AssociationClass)pair.Key, (int)idTable[pair.Key], (AssociationClassViewHelper)pair.Value);
                else
                if (pair.Key.GetType().GetInterface("PIMClass") != null && pair.Value is ClassViewHelper)
                    SerializeVisualClass((Class)pair.Key, (int)idTable[pair.Key], (ClassViewHelper)pair.Value);
                else
                    if (pair.Key.GetType().GetInterface("PSMClass") != null && pair.Value is PSMElementViewHelper)
                        SerializeVisualClass((Class)pair.Key, (int)idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                    else
                        if (pair.Key.GetType().GetInterface("Association") != null && pair.Value is AssociationViewHelper)
                            SerializeVisualPIMAssociation((Association)pair.Key, (int)idTable[pair.Key], (AssociationViewHelper)pair.Value);
                        else
                            if (pair.Key.GetType().GetInterface("Comment") != null && pair.Value is CommentViewHelper)
                                SerializeVisualComment((Comment)pair.Key, (int)idTable[pair.Key], (CommentViewHelper)pair.Value);
                            else
                                if (pair.Key.GetType().GetInterface("Generalization") != null && pair.Value is GeneralizationViewHelper)
                                    SerializeVisualGeneralization((Generalization)pair.Key, (int)idTable[pair.Key], (GeneralizationViewHelper)pair.Value);
                                else
                                    if (pair.Key.GetType().GetInterface("PSMAttributeContainer") != null && pair.Value is PSMElementViewHelper)
                                        SerializeVisualPSMAttContainer((PSMAttributeContainer)pair.Key, (int)idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                                    else
                                        if (pair.Key.GetType().GetInterface("PSMAssociation") != null && pair.Value is PSMAssociationViewHelper)
                                            SerializeVisualPSMAssociation((PSMAssociation)pair.Key, (int)idTable[pair.Key], (PSMAssociationViewHelper)pair.Value);
                                        else
                                            if (pair.Key.GetType().GetInterface("PSMContentContainer") != null && pair.Value is PSMElementViewHelper)
                                                SerializeVisualPSMContentContainer((PSMContentContainer)pair.Key, (int)idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                                            else
                                                if (pair.Key.GetType().GetInterface("PSMContentChoice") != null && pair.Value is PSMElementViewHelper)
                                                    SerializeVisualPSMContentChoice((PSMContentChoice)pair.Key, (int)idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                                                else
                                                    if (pair.Key.GetType().GetInterface("PSMClassUnion") != null && pair.Value is PSMElementViewHelper)
                                                        SerializeVisualPSMClassUnion((PSMClassUnion)pair.Key, (int)idTable[pair.Key], (PSMElementViewHelper)pair.Value);
                                                    else
                                                    {
                                                        Console.WriteLine("Another interface found: " + pair.Key.GetType().ToString());
                                                        SerializationError();
                                                    }

                
            }

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeRoots(PSMDiagram d)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlRoots);

            foreach(PSMClass c in d.Roots)
            {
                if (!idTable.Contains(c))
                    SerializationError();
                int id = (int)idTable[c];

                xmlTextWriter.WriteStartElement(XmlVoc.xmlRoot);
                xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, id.ToString());
                xmlTextWriter.WriteEndElement();

            }


            xmlTextWriter.WriteEndElement();
        
        }

        private void SerializeVisualAssociationClass(AssociationClass c, int refid, AssociationClassViewHelper view)
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

        private void SerializeVisualElement(int refid, PositionableElementViewHelper view, ObservablePointCollection points)
        {
            // @ref
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, refid.ToString());

            // appearance
            SerializeAppearance(view);

            // points
            SerializeVisualPoints(points);
        }

        private void SerializeVisualClass(Class c, int refid, ClassViewHelper view)
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
            if (c is PSMClass && view is PSMElementViewHelper  && ((PSMElementViewHelper)view).ConnectorViewHelper != null)
                SerializeVisualPoints((((PSMElementViewHelper)view).ConnectorViewHelper.Points));

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualComment(Comment c, int refid, CommentViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlCommentElement);

            SerializeVisualElement(refid, view, view.LinePoints);

            xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualGeneralization(Generalization g, int refid, GeneralizationViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlGeneralizationElement);

            SerializeVisualElement(refid, view, view.Points);
           
            xmlTextWriter.WriteEndElement();
        }

        #region Visual PSM Elements

        private void SerializeVisualPSMAttContainer(PSMAttributeContainer c, int refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMAttContainer);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }
       
        private void SerializeVisualPSMContentContainer(PSMContentContainer c, int refid, PSMElementViewHelper view)
       {
           xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentContainer);

           SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

           xmlTextWriter.WriteEndElement();
        }

        private void SerializeVisualPSMContentChoice(PSMContentChoice c, int refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMContentChoice);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

            xmlTextWriter.WriteEndElement();
        }                 

        private void SerializeVisualPSMClassUnion(PSMClassUnion c, int refid, PSMElementViewHelper view)
        {
            xmlTextWriter.WriteStartElement(XmlVoc.xmlPSMClassUnion);

            SerializeVisualElement(refid, view, view.ConnectorViewHelper.Points);

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
        private void SerializeVisualPIMAssociation(Association a, int refid, AssociationViewHelper view)
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
            if (!idTable.Contains(aeh.AssociationEnd)) SerializationError();
            xmlTextWriter.WriteAttributeString(XmlVoc.xmlAttRef, ((int)idTable[aeh.AssociationEnd]).ToString());

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

        private void SerializeVisualPSMAssociation(PSMAssociation a, int refid, PSMAssociationViewHelper view)
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
        private void SerializeVisualPoints(ObservablePointCollection points)
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

        #endregion

    }
}
