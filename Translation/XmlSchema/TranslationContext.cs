using System.Collections.Generic;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	public partial class XmlSchemaTranslator
	{
		/// <summary>
		/// Translation context class is a set of three <see cref="XmlSchemaWriter"/>s
		/// where the declarations are written during translation. 
		/// </summary>
		public class TranslationContext
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="TranslationContext"/> class.
			/// </summary>
			/// <param name="treeDeclarations">The tree declarations writer.</param>
			/// <param name="composedContent">The composed content writer.</param>
			/// <param name="composedAttributes">The composed attributes writer.</param>
			public TranslationContext(XmlSchemaWriter treeDeclarations, XmlSchemaWriter composedContent, XmlSchemaWriter composedAttributes)
			{
				this.TreeDeclarations = treeDeclarations;
				this.ComposedContent = composedContent;
				this.ComposedAttributes = composedAttributes;
			}

			/// <summary>
			/// This is the writer to which references to 
			/// attribute groups are propagated from classes without
			/// element labels (that are translated to groups)
			/// </summary>
			/// <value><see cref="XmlSchemaWriter"/></value>
			public XmlSchemaWriter ComposedAttributes { get; private set; }

			/// <summary>
			/// This is the main writer of the translation to which 
			/// declarations are being writen.
			/// </summary>
			/// <value><see cref="XmlSchemaWriter"/></value>
			public XmlSchemaWriter TreeDeclarations { get; private set; }

			/// <summary>
			/// This is the writer to which references to 
			/// model groups are propagated from classes without
			/// element labels (that are translated to groups)
			/// </summary>
			/// <value><see cref="XmlSchemaWriter"/></value>
			public XmlSchemaWriter ComposedContent { get; private set; }

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
		}
	}
}