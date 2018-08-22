using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	/// <summary>
	/// Factory class for creating <see cref="XmlSchemaWriter"/>s. 
	/// Remembers all created writers and can join all join them 
	/// and return them as one result.
	/// </summary>
	public class WriterFactory
	{
		/// <summary>
		/// Log for writing translation errors and warnings
		/// </summary>
		/// <value><see cref="TranslationLog"/></value>
		public TranslationLog Log { get; set; }

		private readonly XmlWriterSettings writerSettings;
		private readonly XmlWriterSettings resultSettings;
		
		StringBuilder _sbGlobalDeclarations;
		StringBuilder _sbSimpleTypes;
		List<XmlSchemaWriter> createdTreeDeclarations;

		/// <summary>
		/// Writer for writing global elements
		/// </summary>
		public XmlSchemaWriter globalDeclarations;

		/// <summary>
		/// Writer for writing simple types (restriction of XML Schema default types)
		/// </summary>
		public SimpleTypesWriter simpleTypesDeclarations;

		/// <summary>
		/// Initializes a new instance of the <see cref="WriterFactory"/> class.
		/// </summary>
		public WriterFactory()
		{
			writerSettings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				OmitXmlDeclaration = true,
				CloseOutput = true
			};
			resultSettings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "  ",
				OmitXmlDeclaration = false,
				CloseOutput = true
			};
		}

		/// <summary>
		/// Reset's the factory for new translation
		/// </summary>
		public void Initialize()
		{
			if (createdTreeDeclarations == null)
				createdTreeDeclarations = new List<XmlSchemaWriter>();
			else 
				createdTreeDeclarations.Clear();

			_sbGlobalDeclarations = new StringBuilder();
			globalDeclarations = CreateWriter(_sbGlobalDeclarations);

			_sbSimpleTypes = new StringBuilder();
			XmlWriter writer = XmlWriter.Create(_sbSimpleTypes, writerSettings);
			simpleTypesDeclarations = new SimpleTypesWriter(writer, _sbSimpleTypes, Log);
			// ReSharper disable PossibleNullReferenceException
			writer.WriteStartElement("wrap");
			// ReSharper restore PossibleNullReferenceException
			writer.WriteAttributeString("xmlns", "xs", null, "http://www.w3.org/2001/XMLSchema");
		}

		/// <summary>
		/// Gets the result 
		/// (<see cref="globalDeclarations"/> + all writers created with <see cref="CreateGlobalWriter"/> + <see cref="simpleTypesDeclarations"/>)
		/// wrapped in schema element.
		/// </summary>
		/// <returns>resulting xml schema</returns>
		public string GetResult(string projectNamespace, PSMDiagram diagram)
		{
			StringBuilder _sbResult = new StringBuilder();
			XmlWriter wrResult = XmlWriter.Create(_sbResult, resultSettings);
			XmlSchemaWriter w = new XmlSchemaWriter(wrResult, _sbResult, Log);
			wrResult.WriteStartDocument();
			w.Schema();
			if (!projectNamespace.EndsWith("/"))
				projectNamespace = projectNamespace + "/";
			wrResult.WriteAttributeString("xmlns", projectNamespace);
            wrResult.Flush();
            _sbResult.AppendLine();
            wrResult.WriteAttributeString("elementFormDefault", "qualified");
			wrResult.Flush();
			_sbResult.AppendLine();
		    if (!String.IsNullOrEmpty(diagram.TargetNamespace))
		    {
                wrResult.WriteAttributeString("targetNamespace", diagram.TargetNamespace);
		    }
            else
            {
                wrResult.WriteAttributeString("targetNamespace", projectNamespace);
            }
			wrResult.Flush();
			_sbResult.AppendLine();
            foreach (PSMDiagramReference psmDiagramReference in diagram.DiagramReferences)
            {
                if (!String.IsNullOrEmpty(psmDiagramReference.Namespace))
                {
                    // writer.WriteAttributeString("xmlns", "bk", null, "urn:book");
                    // Write the xmlns:bk="urn:book" namespace declaration
                    // chci xmlns:per="http://www.person.org"
                    wrResult.WriteAttributeString("xmlns", psmDiagramReference.NamespacePrefix, null, psmDiagramReference.Namespace);
                    wrResult.Flush();
                    _sbResult.AppendLine();
                }
            }
			
			if (globalDeclarations.IsEmpty)
				Log.AddWarning(LogMessages.XS_NO_ROOT);
			else
			{
				w.AppendContent(globalDeclarations);
				wrResult.Flush();
				_sbResult.AppendLine();
			}
			foreach (XmlSchemaWriter writer in createdTreeDeclarations)
			{
				if (!writer.IsEmpty && !writer.IsGhostWriter)
				{
					w.AppendContent(writer);
					w.Writer.Flush();
					_sbResult.AppendLine();
				}
			}
			if (!simpleTypesDeclarations.IsEmpty)
			{
				w.AppendContent(simpleTypesDeclarations);
			}
			globalDeclarations.Writer.Close();
			wrResult.WriteEndElement(); // schema
			wrResult.Close();
			return _sbResult.ToString();
		}

		/// <summary>
		/// Creates empty XmlSchemaWriter.
		/// </summary>
		/// <returns>Created writer</returns>
		public XmlSchemaWriter CreateWriter()
		{
			StringBuilder _sb = new StringBuilder();
			return CreateWriter(_sb);
		}

		/// <summary>
		/// Creates empty XmlSchemaWriter.
		/// </summary>
		/// <param name="sb">Underlying stringbuilder where writer will write</param>
		public XmlSchemaWriter CreateWriter(StringBuilder sb)
		{
			XmlWriter writer = XmlWriter.Create(sb, writerSettings);
			XmlSchemaWriter xmlSchemaWriter = new XmlSchemaWriter(writer, sb, Log);
			// ReSharper disable PossibleNullReferenceException
			writer.WriteStartElement("wrap");
			// ReSharper restore PossibleNullReferenceException
			writer.WriteAttributeString("xmlns", "xs", null, "http://www.w3.org/2001/XMLSchema");
			return xmlSchemaWriter;
		}

	    /// <summary>
	    /// Creates the writer whose contents will be contained in result returned by <see cref="GetResult"/>.
	    /// </summary>
	    /// <param name="ghostWriter"></param>
	    /// <returns>Created writer</returns>
	    public XmlSchemaWriter CreateGlobalWriter(bool ghostWriter)
		{
			XmlSchemaWriter classWriter = CreateWriter();
	        classWriter.IsGhostWriter = ghostWriter;
			createdTreeDeclarations.Add(classWriter);
			return classWriter;
		}
	}
}