using System.Collections.Generic;
using System.Xml;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	public partial class XmlSchemaTranslator
	{
		/// <summary>
		/// Translation context class is a set of three <see cref="XmlSchemaWriter"/>s
		/// where the declarations are written during translation. 
		/// </summary>
		public class Context
		{
            public XsdDocument Document { get; set; }
            public XmlElement Element { get; set; }

            public List<ComposedAttrGroup> ComposedAttributes;
            public List<XmlNode> ComposedContent;

            public bool silence;
            private XmlElement lastCreated;
  //          public BinaryHeap<int> ARHeap;

            /// <summary>
            /// Groups that will be referenced in the 
            /// next call of <see cref="XmlSchemaTranslator.TranslateComponentsIncludingRepresentative"/>
            /// </summary>
            public ClassTranslationData[] ReferencedModelGroups { get; set; }

            /// <summary>
            /// Attribute groups that will be referenced in the 
            /// next call of <see cref="XmlSchemaTranslator.TranslateAttributesIncludingRepresentative"/>
            /// </summary>
            public ClassTranslationData[] ReferencedAttributeGroups { get; set; }

            /// <summary>
            /// Reference to an association that leads to node
            /// that should be translated
            /// </summary>
            public PSMAssociation LeadingAssociation { get; set; }


            public Context()
            {
                ComposedAttributes = new List<ComposedAttrGroup>();
                ComposedContent = new List<XmlNode>();
                silence = false;
//                ARHeap = new BinaryHeap<int>();
            }

            public Context(XsdDocument xsdDocument)
            {
                Document = xsdDocument;
                Element = Document.getSchemaElement();
                ComposedAttributes = new List<ComposedAttrGroup>();
                ComposedContent = new List<XmlNode>();
                silence = false;
//                ARHeap = new BinaryHeap<int>();
            }

            public Context(XsdDocument xsdDocument, XmlElement currentElement)
            {
                Document = xsdDocument;
                Element = currentElement;
                ComposedAttributes = new List<ComposedAttrGroup>();
                ComposedContent = new List<XmlNode>();
                silence = false;
//                ARHeap = new BinaryHeap<int>();
            }

            public Context(Context context)
            {
                Document = context.Document;
                Element = context.Element;
                if (context.ComposedAttributes == null)
                    ComposedAttributes = new List<ComposedAttrGroup>();
                else
                    ComposedAttributes = new List<ComposedAttrGroup>(context.ComposedAttributes);
                if (context.ComposedContent == null)
                    ComposedContent = new List<XmlNode>();
                else
                    ComposedContent = new List<XmlNode>(context.ComposedContent);

                ReferencedModelGroups = context.ReferencedModelGroups;
                ReferencedAttributeGroups = context.ReferencedAttributeGroups;
                LeadingAssociation = context.LeadingAssociation;
                silence = false;
//                ARHeap = context.ARHeap.Copy();
            }

            public void changePosition(XsdDocument xsdDocument, XmlElement currentElement)
            {
                this.Document = xsdDocument;
                this.Element = currentElement;
            }

            public void changePosition(Context context)
            {
                this.Document = context.Document;
                this.Element = context.Element;
            }

            public XmlElement last()
            {
                return lastCreated;
            }

            public void down()
            {
                Element = lastCreated;
            }

            public void up()
            {
                Element = (XmlElement) Element.ParentNode;
            }
            
            #region creating nodes and attributes

            public void element(string name)
            {
                if (silence) return;
                lastCreated = Document.element(Element, name);
            }

            public void elementRef(string @ref)
            {
                if (silence) return;
                lastCreated = Document.elementRef(Element, @ref);
            }

            public void complexType(string name)
            {
                if (silence) return;
                lastCreated = Document.complexType(Element, name);
            }

            public void complexType()
            {
                if (silence) return;
                lastCreated = Document.complexType(Element);
            }

            public void group(string name)
            {
                if (silence) return;
                lastCreated = Document.group(Element, name);
            }

            public void groupRef(string @ref)
            {
                if (silence) return;
                lastCreated = Document.groupRef(Element, @ref);
            }

            public void attributeGroup(string name)
            {
                if (silence) return;
                lastCreated = Document.attributeGroup(Element, name);
            }

            public void attributeGroupRef(string @ref)
            {
                if (silence) return;
                lastCreated = Document.attributeGroupRef(Element, @ref);
            }

            public void sequence()
            {
                if (silence) return;
                lastCreated = Document.sequence(Element);
            }

            public void choice()
            {
                if (silence) return;
                lastCreated = Document.choice(Element);
            }

            public void complexContent()
            {
                if (silence) return;
                lastCreated = Document.complexContent(Element);
            }

            public void extension(string @base)
            {
                if (silence) return;
                lastCreated = Document.extension(Element, @base);
            }

            public void attribute(string name, PSMAttribute attribute, bool forceOptional)
            {
                if (silence) return;
                lastCreated = Document.attribute(Element, name, attribute, forceOptional);
            }

            public void attributeAsElement(string name, PSMAttribute attribute)
            {
                if (silence) return;
                lastCreated = Document.attributeAsElement(Element, name, attribute);
            }

            public void typeAttribute(string type)
            {
                if (silence) return;
                Document.typeAttribute(Element, type);
            }

            public void setAbstract()
            {
                if (silence) return;
                Document.setAbstract(Element);
            }

            public void anyAttribute()
            {
                if (silence) return;
                Document.anyAttribute(Element);
            }

            public void multiplicityAttributes(uint? lower, NUml.Uml2.UnlimitedNatural? upper)
            {
                if (silence) return;
                Document.multiplicityAttributes(Element, lower, upper);
            }
            #endregion
        }
	}
}