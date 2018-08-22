using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using XCase.Model;

namespace XCase.Controller.Commands.Helpers
{
	/// <summary>
	/// Used for returning references to elements and sharing them among Commands in macrocommands.
	/// </summary>
	public class ElementHolder<ElementType> : HolderBase<ElementType>, IXmlSerializable
		where ElementType : class, Element
	{
		public ElementHolder()
		{
		}

		public ElementHolder(ElementType element) : base(element)
		{
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			throw new System.NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("ElementHolder");

			writer.WriteElementString("HoldedType", Element.GetType().Name);
			if (Element is NamedElement)
				writer.WriteElementString("Element", ((NamedElement)Element).QualifiedName);
			else
				throw new NotImplementedException("Method or operation is not implemented.");

			writer.WriteEndElement();
		}
	}
}
