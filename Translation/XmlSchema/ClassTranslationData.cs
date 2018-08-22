using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	public partial class XmlSchemaTranslator
	{
		/// <summary>
		/// Object containing information about translation of a
		/// PSM class (name of the created complex type or
		/// names of the model and attribute group)
		/// </summary>
		public class ClassTranslationData
		{
			/// <summary>
			/// Name inferred from <see cref="NamedElement.Name"/> that serves as a 
			/// base for names of groups and complex type. 
			/// </summary>
			public string NameBase { get; set; }

			/// <summary>
			/// Name of the complex type (if the class was translated to complex type)
			/// </summary>
			public string ComplexTypeName { get; set; }

			/// <summary>
			/// Gets a value indicating whether the class was translated to a complex type
			/// </summary>
			/// <value>
			/// 	<c>true</c> if this instance class was translated to a complex type; otherwise, <c>false</c>.
			/// </value>
			public bool IsComplexType
			{
				get
				{
					return !String.IsNullOrEmpty(ComplexTypeName);
				}
			}

			private string modelGroupName;

			/// <summary>
			/// Name of the group to which the class was translated
			/// </summary>
			public string ModelGroupName
			{
				get
				{
					if (!String.IsNullOrEmpty(modelGroupName))
						return modelGroupName;
					else
					{
						if (ModelGroupUnknown)
							return ModelGroupNameSuggestion;
						else
							return null;
					}
				}
				set
				{
					modelGroupName = value;
				}
			}

			private string attributeGroupName;

			/// <summary>
			/// Name of the attribute group to which the class was translated
			/// </summary>
			public string AttributeGroupName
			{
				get
				{
					if (!String.IsNullOrEmpty(attributeGroupName))
						return attributeGroupName;
					else
					{
						if (AttributeGroupUnknown)
							return AttributeGroupNameSuggestion;
						else
							return null;
					}
				}
				set
				{
					attributeGroupName = value;
				}
			}

			/// <summary>
			/// Gets the name of the optional attribute group.
			/// </summary>
			/// <value>The name of the optional attribute group.</value>
			public string GetOptionalAttributeGroupName
			{
				get
				{
					if (!AttributeGroupName.EndsWith("-opt"))
						return AttributeGroupName + "-opt";
					else
						return AttributeGroupName;
				}
			}

			/// <summary>
			/// Gets the name of the not-optional attribute group.
			/// </summary>
			/// <value>The name of the not-optional attribute group.</value>
			public string GetNormalAttributeGroupName
			{
				get
				{
					if (AttributeGroupName.EndsWith("-opt"))
						return AttributeGroupName.Substring(0, attributeGroupName.LastIndexOf("-opt"));
					else
						return AttributeGroupName;
				}
			}

			/// <summary>
			/// Name for the attribute group, if it will be created
			/// </summary>
			public string AttributeGroupNameSuggestion { get; set; }

			/// <summary>
			/// Name for the group, if it will be created
			/// </summary>
			public string ModelGroupNameSuggestion { get; set; }

			/// <summary>
			/// Gets a value indicating whether the class was translated to a model group.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if the class was translated to a model group; otherwise, <c>false</c>.
			/// </value>
			public bool IsModelGroup
			{
				get
				{
					if (String.IsNullOrEmpty(ModelGroupName) && ModelGroupUnknown)
					{
						MustCreateModelGroup = true;
					}
					return !String.IsNullOrEmpty(ModelGroupName) || ModelGroupUnknown;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the class was translated to an attribute group.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if the class was translated to an attribute group; otherwise, <c>false</c>.
			/// </value>
			public bool IsAttributeGroup
			{
				get
				{
					if (String.IsNullOrEmpty(AttributeGroupName) && AttributeGroupUnknown)
					{
						MustCreateAttributeGroup = true;
					}
					return !String.IsNullOrEmpty(AttributeGroupName) || AttributeGroupUnknown;
				}
			}
			/// <summary>
			/// Possible declarations of attribute group.
			/// </summary>
			[Flags]
			public enum EAGsage
			{
				/// <summary>
				/// Not declared
				/// </summary>
				None = 0,
				/// <summary>
				/// Declared as optional
				/// </summary>
				Optional = 1,
				/// <summary>
				/// Declared as non-optional
				/// </summary>
				Normal = 2,
				/// <summary>
				/// Declared both ways
				/// </summary>
				Both = 3
			}

			/// <summary>
			/// Stores how attribute group was already defined.
			/// (there are two options to define attribute group -
			/// optional and not optional)
			/// </summary>
			public EAGsage AttributeGroupUsage { get; set; }

			/// <summary>
			/// Gets a value indicating whether the attribute group was declared optional
			/// </summary>
			/// <value>
			/// 	<c>true</c> if the attribute group was declared optional; otherwise, <c>false</c>.
			/// </value>
			/// <seealso cref="AttributeGroupUsage"/>
			public bool IsAttributeGroupDeclaredOptional
			{
				get
				{
					return (AttributeGroupUsage & EAGsage.Optional) == EAGsage.Optional;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the attribute group was declared non-optional
			/// </summary>
			/// <value>
			/// 	<c>true</c> if the attribute group was declared non-optional; otherwise, <c>false</c>.
			/// </value>
			/// <seealso cref="AttributeGroupUsage"/>
			public bool IsAttributeGroupDeclaredNormal
			{
				get
				{
					return (AttributeGroupUsage & EAGsage.Normal) == EAGsage.Normal;
				}
			}

			/// <summary>
			/// Gets or sets a value indicating whether an attribute group must be created 
			/// for this class, because it was  referenced earlier when it was not yet known, 
			/// whether the class will be translated to an attribute group or not. 
			/// </summary>
			/// <value>
			/// 	<c>true</c> if class must be translated to an attribute group; otherwise, <c>false</c>.
			/// </value>
			public bool MustCreateAttributeGroup { get; internal set; }

			/// <summary>
			/// Gets or sets a value indicating whether a group must be created 
			/// for this class, because it was  referenced earlier when it was not yet known, 
			/// whether the class will be translated to a group or not. 
			/// </summary>
			/// <value>
			/// 	<c>true</c> if class must be translated to a  group; otherwise, <c>false</c>.
			/// </value>
			public bool MustCreateModelGroup { get; private set; }

			/// <summary>
			/// Gets or sets a value indicating that it is not known yet, whether 
			/// the class will be translated to an attribute group or not .
			/// </summary>
			/// <value>
			/// 	<c>true</c> if it is not known, whether the class will be translated oa an
			///		attribute group; otherwise, <c>false</c>.
			/// </value>
			public bool AttributeGroupUnknown { get; set; }

			/// <summary>
			/// Gets or sets a value indicating that it is not known yet, whether 
			/// the class will be translated to a group or not .
			/// </summary>
			/// <value>
			/// 	<c>true</c> if it is not known, whether the class will be translated to a
			///		group; otherwise, <c>false</c>.
			/// </value>
			public bool ModelGroupUnknown { get; set; }

		    public bool TranslatingNow { get; set; }
		}
	}

	
}