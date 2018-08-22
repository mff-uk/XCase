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
	public class XmlSchemaWriter
	{
		private string namespacePrefix = "xs";

		/// <summary>
		/// Prefix for elements from XML Schema namespace 
		/// </summary>
		/// <value><see cref="String"/></value>
		public string NamespacePrefix
		{
			get
			{
				return namespacePrefix;
			}
			set
			{
				namespacePrefix = value;
			}
		}

        public bool IsGhostWriter { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether something was written with this writer.
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty { get; protected set; }


		/// <summary>
		/// Underlying <see cref="XmlWriter"/>.
		/// </summary>
		/// <value><see cref="XmlWriter"/></value>
		public XmlWriter Writer
		{
			get; set;
		}

		private StringBuilder _sb { get; set; }

		/// <summary>
		/// Log to write translation errors and warnings
		/// </summary>
		/// <value><see cref="TranslationLog"/></value>
		public TranslationLog Log { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlSchemaWriter"/> class.
		/// </summary>
		/// <param name="writer">The underlying writer</param>
		/// <param name="sb">StringBuilder where result is written</param>
		/// <param name="log">Translation log where errors and warnings are written</param>
		public XmlSchemaWriter(XmlWriter writer, StringBuilder sb, TranslationLog log)
		{
			Writer = writer;
			this._sb = sb;
			Log = log;
			this.IsEmpty = true;
		}

		private void AfterWriteDebug()
		{
			//return;
			Writer.Flush();
			//System.Diagnostics.Debug.WriteLine(_sb.ToString());
		}

		/// <summary>
		/// Writes complexType element
		/// </summary>
		public void ComplexType()
		{
			Writer.WriteStartElement(NamespacePrefix, "complexType", "http://www.w3.org/2001/XMLSchema");
			IsEmpty = false; 
			AfterWriteDebug();
		}
		/// <summary>
		/// Writes complexType element (with specified name)
		/// </summary>
		/// <param name="name"></param>
		public void ComplexType(string name)
		{
			ComplexType();
			Writer.WriteAttributeString("name", name);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes sequence element
		/// </summary>
		public void Sequence()
		{
			Writer.WriteStartElement(NamespacePrefix, "sequence", "http://www.w3.org/2001/XMLSchema");
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes complexContent element
		/// </summary>
		public void ComplexContent()
		{
			Writer.WriteStartElement(NamespacePrefix, "complexContent", "http://www.w3.org/2001/XMLSchema");
			IsEmpty = false;
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes extension element
		/// </summary>
		/// <param name="base"></param>
		public void Extension(string @base)
		{
			Writer.WriteStartElement(NamespacePrefix, "extension", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("base", @base);
			IsEmpty = false;
			AfterWriteDebug();
		}

		/// <summary>
		/// writes "element" element. with specified name
		/// </summary>
		/// <param name="name">name attribute of the element</param>
		public void Element(string name)
		{
			Writer.WriteStartElement(NamespacePrefix, "element", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", name);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes minOccurs and maxOccurs attributes (if parameters are not equal to 1)
		/// </summary>
		/// <param name="lower">value for minOccurs attribute</param>
		/// <param name="upper">value for maxOccurs attribute</param>
		public void MultiplicityAttributes(uint ? lower, UnlimitedNatural ? upper)
		{
			if (!lower.HasValue || lower.Value != 1)
				Writer.WriteAttributeString("minOccurs", lower.HasValue ? lower.Value.ToString() : "0");
			if (upper != null && upper.Value != 1)
			{
				if (upper.Value == UnlimitedNatural.Infinity)
					Writer.WriteAttributeString("maxOccurs", "unbounded");
				else 
					Writer.WriteAttributeString("maxOccurs", upper.ToString());
			}
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes abstract="true" attribute.
		/// </summary>
		public void AbstractAttribute()
		{
			Writer.WriteAttributeString("abstract", "true");
		}

		/// <summary>
		/// Writes type attribute
		/// </summary>
		/// <param name="property">property whose <see cref="Model.TypedElement.Type"/> attribute is being written</param>
		/// <param name="simpleTypeWriter">writer where simple type definition is written if the type was not
		/// yet used</param>
		/// <param name="useOccurs">if set to true, minOccurs and maxOccurs attributes are also written if
		/// <paramref name="property"/> multipicity is non-default</param>
		/// <param name="forceOptional">if set to <c>true</c> multiplicity of the attribute is ignored and 
		/// use="optional" is written.</param>
		public void TypeAttribute(Property property, ref SimpleTypesWriter simpleTypeWriter, bool useOccurs, bool forceOptional)
		{
			DataType type = property.Type;
			if (type == null)
			{
				Writer.WriteAttributeString("type", "xs:string");
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
							simpleTypeWriter.WriteSimpleDataTypeDeclaration(simpleType);
							Writer.WriteAttributeString("type", simpleType.Name);
						}
						else
						{
							Writer.WriteAttributeString("type", NamespacePrefix + ":" + simpleType.DefaultXSDImplementation);
						}
					}
					else
					{
						if (type is SimpleDataType)
						{
							Writer.WriteAttributeString("type", NamespacePrefix + ":" + type.Name);
							Log.AddWarning(string.Format(LogMessages.XS_MISSING_TYPE_XSD, type));
						}
						else
						{
							Writer.WriteAttributeString("type", type.Name);
							Log.AddWarning(string.Format(LogMessages.XS_MISSING_TYPE_XSD, type));
						}
					}
				}
				else
				{
					Writer.WriteAttributeString("type", type.ToString());
				}
			}
			if (!String.IsNullOrEmpty(property.Default))
			{
				Writer.WriteAttributeString("default", property.Default);
			}
			if (forceOptional)
			{
				Writer.WriteAttributeString("use", "optional");
			}
			else
			{
				if (!useOccurs)
				{
					if (property.Lower == 0 || property.Lower == null)
					{
						Writer.WriteAttributeString("use", "optional");
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
							Writer.WriteAttributeString("use", "required");
					}
				}
				else
				{
					MultiplicityAttributes(property.Lower, property.Upper);
				}	
			}
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes type attribute.
		/// </summary>
		/// <param name="type">value of the attribute</param>
		public void TypeAttribute(string type)
		{
			Writer.WriteAttributeString("type", type);
			IsEmpty = false; 
			AfterWriteDebug();
		}


		/// <summary>
		/// Writes attributeGroup element
		/// </summary>
		/// <param name="name">name of the attributeGroup</param>
		public void AttributeGroup(string name)
		{
			Writer.WriteStartElement(NamespacePrefix, "attributeGroup", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", name);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes reference to an attributeGroup. 
		/// </summary>
		/// <param name="ref">name of the referenced group</param>
		public void AttributeGroupRef(string @ref)
		{
			Writer.WriteStartElement(NamespacePrefix, "attributeGroup", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("ref", @ref);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes group element
		/// </summary>
		/// <param name="name">name of the group</param>
		public void Group(string name)
		{
			Writer.WriteStartElement(NamespacePrefix, "group", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", name);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes reference to a group
		/// </summary>
		/// <param name="ref">name of the referenced group\</param>
		public void GroupRef(string @ref)
		{
			Writer.WriteStartElement(NamespacePrefix, "group", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("ref", @ref);
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes <see cref="PSMAttribute"/> as an XML attribute (with name and value and default value)
		/// </summary>
		/// <param name="name">name of the attribute</param>
		/// <param name="attribute">written attribute</param>
		/// <param name="simpleTypeWriter">if a simpleType must be created for the attribute, it is written in this writer</param>
		/// <param name="forceOptional">if set to <c>true</c> multiplicity of the attribute is ignored and 
		/// use="optional" is written.</param>
		public void Attribute(string name, PSMAttribute attribute, ref SimpleTypesWriter simpleTypeWriter, bool forceOptional)
		{
			if (attribute.Lower == 0 && attribute.Upper == 0)
				return;
			Writer.WriteStartElement(NamespacePrefix, "attribute", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", name);
			TypeAttribute(attribute, ref simpleTypeWriter, false, forceOptional);
			Writer.WriteEndElement();
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes <see cref="PSMAttribute"/> as an XML element (with name, value and multiplicity)
		/// </summary>
		/// <param name="name">name of the attribute</param>
		/// <param name="attribute">written attribute</param>
		/// <param name="simpleTypeWriter">if a simpleType must be created for the attribute, it is written in this writer</param>
		public void AttributeAsElement(string name, PSMAttribute attribute, ref SimpleTypesWriter simpleTypeWriter)
		{
			Writer.WriteStartElement(NamespacePrefix, "element", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", name);
			TypeAttribute(attribute, ref simpleTypeWriter, true, false);
			Writer.WriteEndElement();
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Writes choice element
		/// </summary>
		public void Choice()
		{
			Writer.WriteStartElement(NamespacePrefix, "choice", "http://www.w3.org/2001/XMLSchema");
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Closes the last opened element
		/// </summary>
		public void EndElement()
		{
			Writer.WriteEndElement();
			IsEmpty = false; 
			AfterWriteDebug();
		}

		/// <summary>
		/// Appends content of another XmlSchemaWriter. 
		/// </summary>
		/// <param name="writer"></param>
		public void AppendContent(XmlSchemaWriter writer)
		{
			writer.EndElement(); // wrap
			// this is important!
			writer.Writer.Flush();
			Writer.AppendXMLFragment(writer._sb.ToString(), "wrap");
			IsEmpty = false; 
			AfterWriteDebug();
			writer.Writer.Close();
		}

		/// <summary>
		/// Writes schema element.
		/// </summary>
		public void Schema()
		{
			Writer.WriteStartElement(NamespacePrefix, "schema", "http://www.w3.org/2001/XMLSchema");
			IsEmpty = false; 
			AfterWriteDebug();
		}


		public void WriteAllowAnyAttribute()
		{
			Writer.WriteStartElement(NamespacePrefix, "anyAttribute", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteEndElement();
			IsEmpty = false;
			AfterWriteDebug();
		}

	    public void Include(string schemaLocation)
	    {
            Writer.WriteStartElement(NamespacePrefix, "include", "http://www.w3.org/2001/XMLSchema");
            Writer.WriteAttributeString("schemaLocation", schemaLocation);
            Writer.WriteEndElement();
            IsEmpty = false;
            AfterWriteDebug();
	    }

	    public void Import(string schemaLocation, string @namespace)
	    {
            Writer.WriteStartElement(NamespacePrefix, "import", "http://www.w3.org/2001/XMLSchema");
            Writer.WriteAttributeString("schemaLocation", schemaLocation);
            Writer.WriteAttributeString("namespace", @namespace);
            Writer.WriteEndElement();
            IsEmpty = false;
            AfterWriteDebug();
	    }
	}

	/// <summary>
	/// Provides additional functionality to .NET class <see cref="XmlWriter"/>.
	/// </summary>
	public static class XmlWriterExt
	{
		/// <summary>
		/// Appends XML fragment to the writer.
		/// </summary>
		/// <param name="writer">writer, where fragment is written</param>
		/// <param name="fragment">appended fragment</param>
		/// <param name="wrappingElementName">if fragment is wrapped in an element, which should not be included itself, 
		/// pass the name of the element in this parameter</param>
		public static void AppendXMLFragment(this XmlWriter writer, string fragment, string wrappingElementName)
		{
			XmlReaderSettings set = new XmlReaderSettings
			                        	{
			                        		IgnoreWhitespace = true,
			                        		ConformanceLevel = ConformanceLevel.Fragment
			                        	};
			XmlReader reader = XmlTextReader.Create(new StringReader(fragment), set);
			if (!string.IsNullOrEmpty(wrappingElementName))
				reader.ReadToFollowing(wrappingElementName);
			reader.Read();
			while (!reader.EOF 
				&& !(!string.IsNullOrEmpty(wrappingElementName) && reader.Name == wrappingElementName)
				)
			{
				writer.WriteNode(reader, true);
			}
		}
	}

	/// <summary>
	/// <see cref="XmlSchemaWriter"/> with functionality to write <see cref="SimpleDataType"/>'s as
	/// restrictions of default XML Schema types. 
	/// </summary>
	public class SimpleTypesWriter : XmlSchemaWriter
	{
		private readonly List<SimpleDataType> declaredTypes;

		/// <summary>
		/// Already declared types
		/// </summary>
		public List<SimpleDataType> DeclaredTypes
		{
			get
			{
				return declaredTypes;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleTypesWriter"/> class.
		/// </summary>
		/// <param name="writer">The underlying writer</param>
		/// <param name="sb">StringBuilder where result is written</param>
		/// <param name="log">Translation log where errors and warnings are written</param>
		public SimpleTypesWriter(XmlWriter writer, StringBuilder sb, TranslationLog log) : 
			base(writer, sb, log)
		{
			declaredTypes = new List<SimpleDataType>();
		}

		/// <summary>
		/// Writes the simple data type declaration as simpleType and restriction element.
		/// </summary>
		/// <param name="simpleType">written type</param>
		public void WriteSimpleDataTypeDeclaration(SimpleDataType simpleType)
		{
			if (declaredTypes.Contains(simpleType))
				return;
			XmlReaderSettings set = new XmlReaderSettings
			{
				IgnoreWhitespace = true,
				ConformanceLevel = ConformanceLevel.Fragment
			};
			XmlReader reader;
			try
			{
				reader = XmlTextReader.Create(new StringReader(simpleType.DefaultXSDImplementation), set);
				while (!reader.EOF) { reader.Read(); }
				reader.Close();
			}
			catch (Exception)
			{
				Log.AddError(String.Format(LogMessages.XS_INCORRECT_XSD, simpleType));
				return;
			}

			if (!simpleType.DefaultXSDImplementation.Contains("<"))
			{
				Log.AddError(String.Format(LogMessages.XS_INCORRECT_XSD, simpleType));
				return;
			}
				
			Writer.WriteStartElement(NamespacePrefix, "simpleType", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("name", simpleType.Name);

			Writer.WriteStartElement(NamespacePrefix, "restriction", "http://www.w3.org/2001/XMLSchema");
			Writer.WriteAttributeString("base",  NamespacePrefix + ":" + simpleType.Parent.DefaultXSDImplementation);
			IsEmpty = false;

			this.Writer.WriteRaw(simpleType.DefaultXSDImplementation.Replace("</", "{/").Replace("<", "<" + NamespacePrefix + ":").Replace("{/", "</"));

			EndElement(); // restriction
			EndElement(); // simpleType
			declaredTypes.Add(simpleType);
		}
	}
}