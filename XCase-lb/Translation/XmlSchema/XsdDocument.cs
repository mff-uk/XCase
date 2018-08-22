using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUml.Uml2;
using XCase.Model;
using DataType=XCase.Model.DataType;
using Property=XCase.Model.Property;

namespace XCase.Translation.XmlSchema
{
	/// <summary>
	/// XmlSchemaWriter is a wrapper around <see cref="XmlWriter"/> that
	/// makes writing elements of XML Schema language more convenient.
	/// Underlying XmlWriter is accessible throu <see cref="Writer"/> property.
	/// </summary>
	public class XsdDocument : XmlDocument
	{
        /// <summary>
        /// Logs where errors and warnings are written.
        /// </summary>
        /// <value><see cref="TranslationLog"/></value>
        public TranslationLog Log { get; set; }
        private readonly Configuration config;
        const string XMLSchemaURI = "http://www.w3.org/2001/XMLSchema";
        private List<string> importedNamespaces;

        public XsdDocument(string targetNamespace, Configuration configuration)
        {
            config = configuration;
            importedNamespaces = new List<string>();
            XmlDeclaration declar = this.CreateXmlDeclaration("1.0", "utf-8", "no");
            this.AppendChild(declar);

            // create <schema> element with its attributes
            XmlElement elSchema = this.CreateElement("xs", "schema", XMLSchemaURI);
            elSchema.SetAttribute("targetNamespace", targetNamespace);
            // and append it into document
            this.AppendChild(elSchema);
        }

        public XsdDocument(Configuration configuration)
        {
            config = configuration;
            importedNamespaces = new List<string>();
            // create <schema> element
            XmlElement elSchema = this.CreateElement("xs", "schema", XMLSchemaURI);
            // and append it into document
            this.AppendChild(elSchema);
        }

        public string getTargetNamespace()
        {
            return getSchemaElement().GetAttribute("targetNamespace");
        }

        public bool isSchemaEmpty()
        {
            XmlNodeList nodes = this.DocumentElement.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                    return false;
            }
            // there is no element node under <schema> element
            return true;
        }

        public void indent()
        {
            foreach (XmlNode child in this.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.XmlDeclaration)
                {
                    XmlWhitespace whiteSpace = this.CreateWhitespace("\r\n");
                    this.InsertAfter(whiteSpace, child);
                    continue;
                }
            }

            XmlElement elSchema = getSchemaElement();
            XmlElement lastChildElement = null;
            foreach (XmlNode child in elSchema.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    indentChildren(child, config.cntIndentSpaces());
                    lastChildElement = (XmlElement)child;
                }
            }

            if (lastChildElement != null)
            {
                string wspaces = "\r\n";
                if (config.emptyLineBeforeGlobal())
                    wspaces = wspaces + "\r\n";
                XmlWhitespace whiteSpace2 = this.CreateWhitespace(wspaces);
                elSchema.InsertAfter(whiteSpace2, lastChildElement);
            }
        }

        public void indentChildren(XmlNode elem, int cntSpaces)
        {
            string spaces = "";
            spaces = spaces.PadRight(cntSpaces);

            if (cntSpaces == config.cntIndentSpaces() && config.emptyLineBeforeGlobal())
            {
                XmlWhitespace whiteSpace5 = this.CreateWhitespace("\r\n");
                elem.ParentNode.InsertBefore(whiteSpace5, elem);
            }
            XmlWhitespace whiteSpace = this.CreateWhitespace("\r\n"+spaces);
            elem.ParentNode.InsertBefore(whiteSpace, elem);

            XmlElement lastChildElement = null;
            foreach (XmlNode child in elem.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;
                indentChildren(child, cntSpaces + config.cntIndentSpaces());
                lastChildElement = (XmlElement)child;
            }

            if (elem.HasChildNodes && lastChildElement != null)
            {
                XmlWhitespace whiteSpace2 = this.CreateWhitespace("\r\n" + spaces);
                elem.InsertAfter(whiteSpace2, lastChildElement);
            }
        }

        public void globalElement(string elementName, string typeName)
        {
            // create element for root class with its attributes
            XmlElement elemElement = this.CreateElement("xs", "element", XMLSchemaURI);
            elemElement.SetAttribute("name", elementName);
            elemElement.SetAttribute("type", typeName);

            // put into XSD document
            this.FirstChild.AppendChild(elemElement);
        }

        public void anyAttribute(XmlElement currentElement)
        {
            XmlElement elemAnyAttribute = this.CreateElement("xs", "anyAttribute", XMLSchemaURI);

            // put into XSD document
            currentElement.AppendChild(elemAnyAttribute);
        }

        public void globalAttributeGroup(string groupName)
        {
            XmlElement elemElement = this.CreateElement("xs", "attributeGroup", XMLSchemaURI);
            elemElement.SetAttribute("name", groupName);

            // put it into XSD document
            this.FirstChild.AppendChild(elemElement);
        }

        public XmlElement choice(XmlElement currentElement)
        {
            XmlElement elemChoice = this.CreateElement("xs", "choice", XMLSchemaURI);
            elemChoice.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemChoice);
            return elemChoice;
        }

        public XmlElement import(string nameSpace, string nameSpacePrefix, string schemaLocation)
        {
            XmlElement elemImport = CreateElement("xs", "import", XMLSchemaURI);
            elemImport.SetAttribute("namespace", nameSpace);
            elemImport.SetAttribute("schemaLocation", schemaLocation);

            if (importedNamespaces.Contains(nameSpace))
                return null;

            importedNamespaces.Add(nameSpace);

            XmlElement elemSchema = getSchemaElement();
            elemSchema.PrependChild(elemImport);
            elemSchema.SetAttribute("xmlns:" + nameSpacePrefix, nameSpace);

            return elemImport;
        }

        public XmlElement attributeAsElement(XmlElement currentElement, string name, PSMAttribute attribute)
        {
            XmlElement elemElement = this.CreateElement("xs", "element", XMLSchemaURI);
            elemElement.SetAttribute("name", name);
            elemElement.IsEmpty = true;

            // add appropriate "type" attribute
            typeAttribute(attribute, elemElement, true, false);

            // put it into XSD document
            currentElement.AppendChild(elemElement);
            return elemElement;
        }

        public XmlElement sequence(XmlElement currentElement)
        {
            XmlElement elemSequence = this.CreateElement("xs", "sequence", XMLSchemaURI);
            elemSequence.IsEmpty = true;

            // put it into XSD documuent
            currentElement.AppendChild(elemSequence);
            return elemSequence;
        }

        public XmlElement groupRef(XmlElement currentElement, string @ref)
        {
            XmlElement elemGroup = this.CreateElement("xs", "group", XMLSchemaURI);
            elemGroup.SetAttribute("ref", @ref);
            elemGroup.IsEmpty = true;
            
            // put it into XSD document
            currentElement.AppendChild(elemGroup);
            return elemGroup;
        }

        public XmlElement attributeGroupRef(XmlElement currentElement, string @ref)
        {
            XmlElement elemAttrGroup = this.CreateElement("xs", "attributeGroup", XMLSchemaURI);
            elemAttrGroup.SetAttribute("ref", @ref);
            elemAttrGroup.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemAttrGroup);
            return elemAttrGroup;
        }

        public XmlElement element(XmlElement currentElement, string name)
        {
            XmlElement elemElement = this.CreateElement("xs", "element", XMLSchemaURI);
            elemElement.SetAttribute("name", name);
            elemElement.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemElement);
            return elemElement;
        }

        public XmlElement elementRef(XmlElement currentElement, string @ref)
        {
            XmlElement elemElementRef = this.CreateElement("xs", "element", XMLSchemaURI);
            elemElementRef.SetAttribute("ref", @ref);
            elemElementRef.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemElementRef);
            return elemElementRef;
        }

        public XmlElement group(XmlElement currentElement, string name)
        {
            XmlElement elemGroup = this.CreateElement("xs", "group", XMLSchemaURI);
            elemGroup.SetAttribute("name", name);
            elemGroup.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemGroup);
            return elemGroup;
        }

        public XmlElement attributeGroup(XmlElement currentElement, string name)
        {
            XmlElement elemAttrGroup = this.CreateElement("xs", "attributeGroup", XMLSchemaURI);
            elemAttrGroup.SetAttribute("name", name);
            elemAttrGroup.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemAttrGroup);
            return elemAttrGroup; 
        }


        public XmlElement complexType(XmlElement currentElement)
        {
            XmlElement elemComplexType = this.CreateElement("xs", "complexType", XMLSchemaURI);
            elemComplexType.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemComplexType);
            return elemComplexType;
        }

        public XmlElement complexType(XmlElement currentElement, string name)
        {
            XmlElement elemComplexType = this.complexType(currentElement);
            elemComplexType.SetAttribute("name", name);
            elemComplexType.IsEmpty = true;

            return elemComplexType; 
        }

        public XmlElement complexContent(XmlElement currentElement)
        {
            XmlElement elemComplexContent = this.CreateElement("xs", "complexContent", XMLSchemaURI);
            elemComplexContent.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemComplexContent);
            return elemComplexContent;
        }

        public XmlElement extension(XmlElement currentElement, string @base)
        {
            XmlElement elemExtension = this.CreateElement("xs", "extension", XMLSchemaURI);
            elemExtension.SetAttribute("base", @base);
            elemExtension.IsEmpty = true;

            // put it into XSD document
            currentElement.AppendChild(elemExtension);
            return elemExtension; 
        }

        public void typeAttribute(XmlElement currentElement, string type)
        {
            currentElement.SetAttribute("type", type);
        }

        public XmlElement attribute(XmlElement currentElement, string name, PSMAttribute attribute, bool forceOptional)
        {
            if (attribute.Lower == 0 && attribute.Upper == 0)
                return null;

            XmlElement elemAttribute = this.CreateElement("xs", "attribute", XMLSchemaURI);
            elemAttribute.SetAttribute("name", name);

            // add appropriate "type" attribute
            typeAttribute(attribute, elemAttribute, false, forceOptional);

            // put it into XSD document
            currentElement.AppendChild(elemAttribute);
            return elemAttribute;
        }

        public void typeAttribute(Property property, XmlElement targetElement, bool useOccurs, bool forceOptional)
        {
            DataType type = property.Type;
            if (type == null)
            {
                targetElement.SetAttribute("type", "xs:string");
                Log.AddWarning(string.Format(LogMessages.XS_TYPE_TRANSLATED_AS_STRING, type, property.Class, property));
            }
            else
            {
                SimpleDataType simpleType = type as SimpleDataType;
                if (simpleType != null)
                {
                    if (!string.IsNullOrEmpty(simpleType.DefaultXSDImplementation))
                    {
                        if (simpleType.Parent != null)
                        {
                            //todo: simpleTypeWriter.WriteSimpleDataTypeDeclaration(simpleType);
                            targetElement.SetAttribute("type", simpleType.Name);
                        }
                        else
                        {
                            targetElement.SetAttribute("type", "xs" + ":" + simpleType.DefaultXSDImplementation);
                        }
                    }
                    else
                    {
                        if (type is SimpleDataType)
                        {
                            targetElement.SetAttribute("type", "xs" + ":" + type.Name);
                            Log.AddWarning(string.Format(LogMessages.XS_MISSING_TYPE_XSD, type));
                        }
                        else
                        {
                            targetElement.SetAttribute("type", type.Name);
                            Log.AddWarning(string.Format(LogMessages.XS_MISSING_TYPE_XSD, type));
                        }
                    }
                }
                else
                {
                    targetElement.SetAttribute("type", type.ToString());
                }
            }
            if (!String.IsNullOrEmpty(property.Default))
            {
                targetElement.SetAttribute("default", property.Default);
            }
            if (forceOptional)
            {
                targetElement.SetAttribute("use", "optional");
            }
            else
            {
                if (!useOccurs)
                {
                    if (property.Lower == 0 || property.Lower == null)
                    {
                        targetElement.SetAttribute("use", "optional");
                        if (property.Upper > 1)
                            Log.AddWarning(string.Format(LogMessages.XS_ATTRIBUTE_MULTIPLICITY_LOST, property.MultiplicityString,
                                                         property.Class, property));
                    }
                    else
                    {
                        if (property.Upper > 1 || property.Lower > 1)
                        {
                            Log.AddWarning(string.Format(LogMessages.XS_ATTRIBUTE_MULTIPLICITY_LOST, property.MultiplicityString,
                                                         property.Class, property));
                        }
                        if (String.IsNullOrEmpty(property.Default))
                            targetElement.SetAttribute("use", "required");
                    }
                }
                else
                {
                    this.multiplicityAttributes(targetElement, property.Lower, property.Upper);
                }
            }
        }

        public void multiplicityAttributes(XmlElement currentElement, uint ? lower, UnlimitedNatural ? upper)
        {
            if (!lower.HasValue || lower.Value != 1)
                currentElement.SetAttribute("minOccures", lower.HasValue ? lower.Value.ToString() : "0");

            if (upper != null && upper.Value != 1)
            {
                if (upper.Value == UnlimitedNatural.Infinity)
                    currentElement.SetAttribute("maxOccurs", "unbounded");
                else
                    currentElement.SetAttribute("maxOccurs", upper.ToString());
            }
        }

        public bool addAttribute(string xpath, string name, string value)
        {
            XmlNodeList nodes = this.SelectNodes(xpath);
            if (nodes.Count!=1)
                return false;

            foreach (XmlNode node in nodes)
            {
                XmlElement element = (XmlElement) node;
                element.SetAttribute(name, value);
            }
            return true;
        }

        public bool addAttribute(int i, string uniqueElemName, string attrName, string attrValue)
        {
            XmlNodeList nodes = this.GetElementsByTagName(uniqueElemName);
            if (nodes.Count != 1)
                return false;

            foreach (XmlNode node in nodes)
            {
                XmlElement element = (XmlElement) node;
                element.SetAttribute(attrName, attrValue);
            }
            return true;
        }

        public XmlElement getElement(string xpath)
        {
            XmlNodeList nodes = this.SelectNodes(xpath);
            if (nodes.Count != 1)
                return null;

            foreach (XmlNode node in nodes)
            {
                return (XmlElement) node;
            }
            return null;
        }

        public XmlElement getSchemaElement()
        {
            return DocumentElement;
        }

        public void setAbstract(XmlElement element)
        {
            element.SetAttribute("abstract", "true");
        }
	}
}