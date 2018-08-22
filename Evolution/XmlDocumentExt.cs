using System.Diagnostics;
using System.Text;
using System.Xml;

namespace XCase.Evolution.Xslt
{
    public static class XmlDocumentExt
    {
        public static XmlElement CreateElement(this XmlElement element, string name)
        {
            XmlElement e = element.OwnerDocument.CreateElement(name);
            element.AppendChild(e);
            return e;
        }

        public static XmlAttribute AddAttributeWithValue(this XmlElement element, string attributeName, string value)
        {
            XmlAttribute xmlAttribute = element.OwnerDocument.CreateAttribute(attributeName);
            xmlAttribute.Value = value;
            element.Attributes.Append(xmlAttribute);
            return xmlAttribute;
        }

        public static XmlComment CreateComment(this XmlElement element, string comment)
        {
            XmlComment xmlComment = element.OwnerDocument.CreateComment(comment);
            element.AppendChild(xmlComment);
            return xmlComment;
        }

        public static XmlElement CreateLeafElement(this XmlElement element, string name, string value)
        {
            XmlElement e = element.OwnerDocument.CreateElement(name);
            XmlText text = element.OwnerDocument.CreateTextNode(value);
            e.AppendChild(text);
            element.AppendChild(e);
            return e;
        }

        public static XmlText CreateTextElement(this XmlElement element, string text)
        {
            XmlText textNode = element.OwnerDocument.CreateTextNode(text);
            element.AppendChild(textNode);
            return textNode;
        }

        public static string PrettyPrintXML(this XmlDocument document)
        {
            StringBuilder _tmp = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(_tmp, new XmlWriterSettings { Indent = true, CheckCharacters = false, NewLineOnAttributes = false});
            Debug.Assert(writer != null);
            document.Save(writer);
            writer.Flush();
            writer.Close();
            _tmp.Replace("utf-16", "utf-8");
            return _tmp.ToString(); 
        }

        public static void SetAttributeValue(this XmlElement attribute, string value)
        {
            attribute.AppendChild(attribute.OwnerDocument.CreateTextNode(value));
        }

        public static void SetAttributeValue(this XmlElement attribute, XmlElement valueElement)
        {
            attribute.AppendChild(valueElement);
        }
    }
}