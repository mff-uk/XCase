using System;
using System.Linq;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
	/// <summary>
	/// Translates PSM Diagram into XML schema in XML Schema language. 
	/// The algorithm of translation is covered in detail in the 
	/// documentation to <c>XCase</c>. 
	/// </summary>
	public partial class XmlSchemaTranslator: DiagramTranslator<XmlSchemaTranslator.TranslationContext, string>
	{
		/// <summary>
		/// Data for classes that were or are about to be translated.
		/// </summary>
		private Dictionary<PSMClass, ClassTranslationData> classesTranslationData; 

		/// <summary>
		/// <see cref="NamingSupport"/> class is used to translate type names and attribute 
		/// aliases in distinct names to avoid ambiguous references. 
		/// </summary>
		private readonly NamingSupport namingSupport;

		/// <summary>
		/// <see cref="WriterFactory"/> is used to create <see cref="XmlSchemaWriter"/>s
		/// used during translation.
		/// </summary>
		private readonly WriterFactory writerFactory;

		/// <summary>
		/// Holds amount of currently opened &lt;xs:choice.. declarations. 
		/// </summary>
		/// <seealso cref="inChoice"/>
		private int choiceCounter = 0;

		/// <summary>
		/// Gets a value indicating whether there is currently an opened 
		/// &lt;xs:choice.. declaration. When inChoice is <c>true</c>, translation 
		/// of attribute groups writes "opt groups" instead normal groups. 
		/// This topic is covered in detail in the documentation for <c>XCase</c>
		/// </summary>
		private bool inChoice
		{
			get { return choiceCounter > 0; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlSchemaTranslator"/> class.
		/// </summary>
		public XmlSchemaTranslator()
		{
			writerFactory = new WriterFactory { Log = Log };
			namingSupport = new NamingSupport { Log = Log };
		}

		/// <summary>
		/// Translates the specified diagram into XML Schema
		/// </summary>
		/// <param name="diagram">The translated diagram.</param>
		/// <returns>
		/// XML Schema definition of the <paramref name="diagram"/>
		/// </returns>
		public override string Translate(PSMDiagram diagram)
		{
			Diagram = diagram;
			Log.Clear();
			writerFactory.Initialize();
			namingSupport.Initialize();
		    namingSupport.Diagram = diagram; 

			classesTranslationData = new Dictionary<PSMClass, ClassTranslationData>();

            if (Diagram.DiagramReferences.Count() > 0)
            {
                XmlSchemaWriter referencesWriter = writerFactory.globalDeclarations;
                
                foreach (PSMDiagramReference psmDiagramReference in diagram.DiagramReferences)
                {
                    if (!String.IsNullOrEmpty(psmDiagramReference.Namespace))
                    {
                        if (String.IsNullOrEmpty(psmDiagramReference.NamespacePrefix))
                        {
                            Log.AddWarning(LogMessages.NO_NAMESPACE_PREFIX, psmDiagramReference);
                        }
                        referencesWriter.Import(psmDiagramReference.SchemaLocation, psmDiagramReference.Namespace);
                    }
                    else
                    {
                        referencesWriter.Include(psmDiagramReference.SchemaLocation);
                    }
                }
            }


		    TranslateDiagram(diagram);

			if (string.IsNullOrEmpty(diagram.Project.Schema.XMLNamespace))
			{
				Log.AddWarning(LogMessages.XS_NO_DEFAULT_NAMESPACE);
				return writerFactory.GetResult("http//www.example.org/", diagram);
			}
			else
			{
                return writerFactory.GetResult(diagram.Project.Schema.XMLNamespace, diagram);
			}

            
		}

		/// <summary>
		/// Writes the global element declarations (for classes in <see cref="PSMDiagram.Roots"/>
		/// of <paramref name="diagram"/>.
		/// </summary>
		/// <param name="diagram">translated diagram</param>
		private void TranslateDiagram(PSMDiagram diagram)
		{
			namingSupport.Initialize();
			
			foreach (PSMClass root in diagram.Roots)
			{
				if (root.HasElementLabel && root.CanBeDeclaredAsElement())
				{
					string typeName = namingSupport.TranslateTypeName(root);
					namingSupport.typeNameSuggestions[root] = typeName;

					string elementName = namingSupport.GetNameForGlobalElement(root);

					writerFactory.globalDeclarations.Element(elementName);
					writerFactory.globalDeclarations.TypeAttribute(typeName);
					writerFactory.globalDeclarations.EndElement(); // element
				}
			}

			foreach (PSMClass root in diagram.Roots)
			{
				if (!classesTranslationData.ContainsKey(root))
				{
					XmlSchemaWriter classWriter = writerFactory.CreateGlobalWriter(false);
					TranslateClass(root, new TranslationContext(classWriter, null, null));
				}
			}
		}

		/// <summary>
		/// Translates the class. The class is translated either into  a complex type
		/// definition or into model and attribute group. 
		/// </summary>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="translationContext">The translation context.</param>
		/// <returns>name of the complex type if <paramref name="psmClass"/> was 
		/// translated into complex type definition.</returns>
		protected override string TranslateClass(PSMClass psmClass, TranslationContext translationContext)
		{
			string typeName;
			if (psmClass.HasElementLabel)
				typeName = TranslateClassWithLabel(psmClass, translationContext);
			else
			{
				TranslateClassContentToGroups(psmClass, translationContext);
				typeName = null;
			}

			/* if there are specifications, translate them also */
			TranslateSpecializations(psmClass, typeName);

			return typeName;
		}

		/// <summary>
		/// Translates the content container into complex type definition.
		/// </summary>
		/// <param name="contentContainer">The content container.</param>
		/// <param name="translationContext">The translation context.</param>
		protected override void TranslateContentContainer(PSMContentContainer contentContainer, TranslationContext translationContext)
		{
			if (String.IsNullOrEmpty(contentContainer.Name))
			{
				Log.AddError(string.Format(LogMessages.XS_ELEMENT_NAME_MISSING, contentContainer));
				return;
			}
			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

			translationContext.TreeDeclarations.Element(namingSupport.NormalizeTypeName(contentContainer, c => c.Name));
			translationContext.TreeDeclarations.ComplexType();

			XmlSchemaWriter newComposedContent = writerFactory.CreateWriter();
			XmlSchemaWriter newComposedAttributes = writerFactory.CreateWriter();
			XmlSchemaWriter newTreeDeclarations = writerFactory.CreateWriter();

			TranslateComponents(contentContainer, new TranslationContext(newTreeDeclarations, newComposedContent, newComposedAttributes));

			if (!newTreeDeclarations.IsEmpty || !newComposedContent.IsEmpty)
			{
				translationContext.TreeDeclarations.Sequence();
				translationContext.TreeDeclarations.AppendContent(newTreeDeclarations);
				translationContext.TreeDeclarations.AppendContent(newComposedContent);
				translationContext.TreeDeclarations.EndElement(); // sequence	
			}
			translationContext.TreeDeclarations.AppendContent(newComposedAttributes);

			translationContext.TreeDeclarations.EndElement(); // complexType
			translationContext.TreeDeclarations.EndElement(); // element

			choiceCounter = curChoiceCounter;
		}

		/// <summary>
		/// <para>
		/// Translates the association child. 
		/// </para>
		/// <para>
		/// If <paramref name="associationChild"/> is <see cref="PSMClass"/>, the
		/// association is translated  into element declaration in the parent class
		/// referencing the complex type to which  the <see cref="PSMClass"/> is
		/// translated or into attribute or model group references if the  PSM class
		/// was translated into groups.</para>
		/// <para>
		/// If <paramref name="associationChild"/> is class union, the association is
		/// translated into a choice among the <see cref="PSMClassUnion.Components"/>
		/// of the union. 
		/// </para>
		/// </summary>
		/// <param name="associationChild">The association child.</param>
		/// <param name="translationContext">The translation context.</param>
		protected override void TranslateAssociationChild(PSMAssociationChild associationChild, TranslationContext translationContext)
		{
			PSMClass psmClass = associationChild as PSMClass;
			if (psmClass != null)
			{
				XmlSchemaWriter classWriter = writerFactory.CreateGlobalWriter(false);
				string typeName;

				#region edge going to a specialized node
				if (psmClass.Specifications.Count > 0)
				{
					// if under association, choice is written, otherwise under
					// class union choice was already written in during class union translation
					if (associationChild.ParentAssociation != null)
					{
						translationContext.TreeDeclarations.Choice();
						translationContext.TreeDeclarations.MultiplicityAttributes(associationChild.ParentAssociation.Lower, associationChild.ParentAssociation.Upper);
					}

					TranslationContext context = new TranslationContext(classWriter, psmClass.IsAbstract ? null : translationContext.ComposedContent, psmClass.IsAbstract ? null : translationContext.ComposedAttributes);
					typeName = TranslateClass(psmClass, context);
					if (psmClass.HasElementLabel && psmClass.CanBeDeclaredAsElement())
					{
						translationContext.TreeDeclarations.Element(namingSupport.NormalizeTypeName(psmClass, c => c.ElementName));
						translationContext.TreeDeclarations.TypeAttribute(typeName);
						translationContext.TreeDeclarations.EndElement(); // element
					}
					else
					{
						if (psmClass.HasElementLabel && !psmClass.IsAbstract)
						{
							Log.AddWarning(String.Format(LogMessages.XS_NON_ABSTRACT_CLASS, psmClass));
						}
					}

					foreach (Generalization specialization in psmClass.Specifications)
					{
						PSMClass specialized = (PSMClass)specialization.Specific;
						TranslateSubstitutions(translationContext.TreeDeclarations, specialized);
					}

					if (associationChild.ParentAssociation != null)
					{
						translationContext.TreeDeclarations.EndElement(); // choice
					}
				}
				#endregion 
				#region no specializations
				else
				{
					TranslationContext context = new TranslationContext(classWriter, translationContext.ComposedContent, translationContext.ComposedAttributes);
					if (associationChild.ParentAssociation != null)
					{
						context.LeadingAssociation = associationChild.ParentAssociation;
						if (context.LeadingAssociation.Lower == 0)
							choiceCounter++; 
						
					}
					
					typeName = TranslateClass(psmClass, context);

					if (associationChild.ParentAssociation != null && associationChild.ParentAssociation.Lower == 0)
					{
						choiceCounter--;
					}

					if (psmClass.IsAbstract)
					{
						Log.AddError(string.Format(LogMessages.XS_ABSTRACT_NOT_SPECIALIZED, psmClass));
					}

					if (psmClass.HasElementLabel)
					{
						translationContext.TreeDeclarations.Element(namingSupport.NormalizeTypeName(psmClass, c => c.ElementName));
						translationContext.TreeDeclarations.TypeAttribute(typeName);
						if (associationChild.ParentAssociation != null)
							translationContext.TreeDeclarations.MultiplicityAttributes(associationChild.ParentAssociation.Lower,
																					   associationChild.ParentAssociation.Upper);
						else
						{
							PSMClassUnion union = associationChild.ParentUnion;
                            //if (union != null && union.ParentAssociation != null)
                            //{
                            //    translationContext.TreeDeclarations.MultiplicityAttributes(union.ParentAssociation.Lower,
                            //                                                               union.ParentAssociation.Upper);
                            //}
						}
						translationContext.TreeDeclarations.EndElement(); // element
					}
					else
					{
						if (associationChild.ParentAssociation != null && classesTranslationData[psmClass].IsAttributeGroup
							&& associationChild.ParentAssociation.Upper > 1)
							Log.AddWarning(string.Format(LogMessages.XS_ASSOCIATION_MULTIPLICITY_LOST, associationChild.ParentAssociation, associationChild.ParentAssociation.MultiplicityString, psmClass));
					}
				}
				#endregion 
			}

			#region class union
			PSMClassUnion classUnion = associationChild as PSMClassUnion;
			if (classUnion != null)
			{
				choiceCounter++;

				XmlSchemaWriter composedAttributes = writerFactory.CreateWriter();
				XmlSchemaWriter contents = writerFactory.CreateWriter();
				foreach (PSMAssociationChild psmAssociationChild in classUnion.Components)
				{
					TranslateAssociationChild(psmAssociationChild, new TranslationContext(contents, contents, composedAttributes));
				}

				if (!contents.IsEmpty)
				{
					translationContext.TreeDeclarations.Choice();
					if (associationChild.ParentAssociation != null)
						translationContext.TreeDeclarations.MultiplicityAttributes(associationChild.ParentAssociation.Lower,
						                                                           associationChild.ParentAssociation.Upper);
					translationContext.TreeDeclarations.AppendContent(contents);
					translationContext.TreeDeclarations.EndElement(); // choice
				}
				
				if (!composedAttributes.IsEmpty && translationContext.ComposedAttributes != null)
				{
					translationContext.ComposedAttributes.AppendContent(composedAttributes);
					Log.AddWarning(string.Format(LogMessages.XS_ATTRIBUTES_IN_CLASS_UNION, classUnion));
				}
				choiceCounter--;
			}
			#endregion 
		}

		/// <summary>
		/// Translates the substitutions. Writes declarations how <paramref name="specialization"/>
		/// class can substitute the parent class. 
		/// </summary>
		/// <param name="writer">The writer where substitutions are written.</param>
		/// <param name="specialization">The specialization of a class.</param>
		private void TranslateSubstitutions(XmlSchemaWriter writer, PSMClass specialization)
		{
			if (!classesTranslationData.ContainsKey(specialization)) // this should never happen, but to be sure
				return;

			ClassTranslationData specializedData = classesTranslationData[specialization];

			if (specializedData.IsModelGroup)
			{
				writer.GroupRef(specializedData.ModelGroupName);
				writer.EndElement(); // groupRef
			}
			if (specializedData.IsAttributeGroup)
			{
				Log.AddWarning(String.Format(LogMessages.XS_SPECIALIZED_ATTRIBUTE_GROUP, specialization, specializedData.AttributeGroupName));
			}
			if (specialization.HasElementLabel) 
			{
				if (!specialization.IsAbstract)
				{
					writer.Element(namingSupport.NormalizeTypeName(specialization, c => c.ElementName));
					writer.TypeAttribute(specializedData.ComplexTypeName);
					writer.EndElement(); // element
				}
			}
			foreach (Generalization specification in specialization.Specifications)
			{
				TranslateSubstitutions(writer, (PSMClass)specification.Specific);
			}
		}

		/// <summary>
		/// Translates the attribute container. The attributes in the <paramref name="attributeContainer"/> 
		/// are translated into element declarations.
		/// </summary>
		/// <param name="attributeContainer">The attribute container.</param>
		/// <param name="translationContext">The translation context.</param>
		protected override void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, TranslationContext translationContext)
		{
			foreach (PSMAttribute psmAttribute in attributeContainer.PSMAttributes)
			{
				translationContext.TreeDeclarations.AttributeAsElement(namingSupport.NormalizeTypeName(psmAttribute, a => a.AliasOrName), psmAttribute, ref writerFactory.simpleTypesDeclarations);
			}
		}

		/// <summary>
		/// Translates the content choice into xs:choice declaration. 
		/// </summary>
		/// <param name="contentChoice">The content choice.</param>
		/// <param name="translationContext">The translation context.</param>
		protected override void TranslateContentChoice(PSMContentChoice contentChoice, TranslationContext translationContext)
		{
			choiceCounter++;
			XmlSchemaWriter newTreeDeclarations = writerFactory.CreateWriter();
			TranslationContext context = new TranslationContext(newTreeDeclarations, newTreeDeclarations, translationContext.ComposedAttributes);
			foreach (PSMSubordinateComponent component in contentChoice.Components)
			{
				TranslateSubordinateComponent(component, context);
			}
			if (!newTreeDeclarations.IsEmpty)
			{
				translationContext.TreeDeclarations.Choice();
				translationContext.TreeDeclarations.AppendContent(newTreeDeclarations);
				translationContext.TreeDeclarations.EndElement(); // choice
			}
			choiceCounter--;
		}

		/// <summary>
		/// Translates the components of <paramref name="superordinateComponent"/>.
		/// <see cref="DiagramTranslator{Context,TypeIdentifier}.TranslateSubordinateComponent"/>
		/// is called for each components in <see cref="PSMSuperordinateComponent.Components"/>.
		/// </summary>
		/// <param name="superordinateComponent">The superordinate component.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateComponents(PSMSuperordinateComponent superordinateComponent, TranslationContext translationContext)
		{
			foreach (PSMSubordinateComponent c in superordinateComponent.Components)
			{
				TranslateSubordinateComponent(c, translationContext);
			}
		}

		/// <summary>
		/// Looks up <see cref="ClassTranslationData"/> for <paramref name="psmClass"/>' structural representative.
		/// If the representative was not yet translated, it is translated immediately. If class is
		/// not structural representative, <c>false</c> is return.
		/// </summary>
		/// <param name="psmClass">The PSM class for which structural representative is searched</param>
		/// <param name="representativeTranslationData">The structural representative's translation data.</param>
		/// <returns><code>true</code> if class is structural representative, <code>false</code> otherwise</returns>
		private bool PrepareRepresentative(PSMClass psmClass, out ClassTranslationData representativeTranslationData)
		{
			representativeTranslationData = null;
			bool isRepresentative = psmClass.IsStructuralRepresentative;

			/* find (or translate, if it was not already translated) represented class */
			if (isRepresentative 
                //&& !psmClass.IsStructuralRepresentativeExternal
                )
				representativeTranslationData = TranslateClassNow(psmClass.RepresentedPSMClass);
			return isRepresentative;
		}

		/// <summary>
		/// Attributes and content of the class is translated into model and attribute group. 
		/// These groups are then referenced in the complexType element. This approach is used 
		/// for those types that are referenced from structural representatives. 
		/// </summary>
		/// <param name="psmClass">translated class</param>
		/// <param name="translationContext">translation context, complex type is created
		/// int <paramref name="translationContext"/>'s <see cref="TranslationContext.TreeDeclarations"/>writer</param>
		/// <returns>name of the created complex type</returns>
		private string TranslateClassWithLabelAsGroups(PSMClass psmClass, TranslationContext translationContext)
		{
			if (!classesTranslationData.ContainsKey(psmClass))
				classesTranslationData[psmClass] = new ClassTranslationData();
			classesTranslationData[psmClass] = classesTranslationData[psmClass];
			ClassTranslationData myTranslationData = classesTranslationData[psmClass];
			if (myTranslationData.ComplexTypeName != null) //already translated
				return myTranslationData.ComplexTypeName;
			if (myTranslationData.NameBase == null)
				myTranslationData.NameBase = namingSupport.GetNameForComplexType(psmClass);
			myTranslationData.ComplexTypeName = myTranslationData.NameBase;
			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

			XmlSchemaWriter classWriter = writerFactory.CreateGlobalWriter(false);
			/* translate class to model and attribute group */
			TranslateClassContentToGroups(psmClass, new TranslationContext(classWriter, null, null));

			/* create complex type with references to the groups */
			translationContext.TreeDeclarations.ComplexType(myTranslationData.ComplexTypeName);
			if (psmClass.IsAbstract)
				translationContext.TreeDeclarations.AbstractAttribute();
			
			if (myTranslationData.IsModelGroup)
			{
				translationContext.TreeDeclarations.Sequence();
				translationContext.TreeDeclarations.GroupRef(myTranslationData.ModelGroupName);
				translationContext.TreeDeclarations.EndElement(); // group ref
				translationContext.TreeDeclarations.EndElement(); // sequence
			}
			if (myTranslationData.IsAttributeGroup)
			{
				translationContext.TreeDeclarations.AttributeGroupRef(myTranslationData.AttributeGroupName);
				translationContext.TreeDeclarations.EndElement(); // attribute group ref
			}
			if (psmClass.AllowAnyAttribute)
				translationContext.TreeDeclarations.WriteAllowAnyAttribute();
			translationContext.TreeDeclarations.EndElement(); // complex type

			choiceCounter = curChoiceCounter;
			return myTranslationData.NameBase;
		}

		/// <summary>
		/// Translates the class content into complex type declaration.
		/// </summary>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateClassContentWithoutGroups(PSMClass psmClass, TranslationContext translationContext)
		{
			TranslationContext context = new TranslationContext(translationContext.TreeDeclarations, null, writerFactory.CreateWriter());
			TranslateComponentsIncludingRepresentative(psmClass, context);
			TranslateAttributesIncludingRepresentative(psmClass, context);
		}

		/// <summary>
		/// Translates the components of <paramref name="psmClass"/>. If <paramref name="psmClass"/> is 
		/// also a structural representative, the reference to the inherited components is
		/// also written. 
		/// </summary>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateComponentsIncludingRepresentative(PSMClass psmClass, TranslationContext translationContext)
		{
			XmlSchemaWriter newTreeDeclarations = writerFactory.CreateWriter();

			ClassTranslationData myTranslationData;
			bool isRepresentative = PrepareRepresentative(psmClass, out myTranslationData);

			/* translate all components of psmClass to a separate writer - newTreeDeclarations
			 * composed content is put into the same writer (since this is a complex type) and 
			 * composed attributes into another separte writer that will be appended into this
			 * complex type */
			TranslateComponents(psmClass, new TranslationContext(newTreeDeclarations, newTreeDeclarations, translationContext.ComposedAttributes));

			/* if there is some content (either from this class or represented class, create 
			 * a sequence and put the content inside the sequence */
			if (!newTreeDeclarations.IsEmpty || (isRepresentative 
                //&& !psmClass.IsStructuralRepresentativeExternal 
                && myTranslationData.IsModelGroup) ||
				(translationContext.ReferencedModelGroups != null && translationContext.ReferencedModelGroups.Any(g => g.IsModelGroup)))
			{
				translationContext.TreeDeclarations.Sequence();

				if (translationContext.ReferencedModelGroups != null)
				{
					foreach (ClassTranslationData referencedData in translationContext.ReferencedModelGroups)
					{
						translationContext.TreeDeclarations.GroupRef(referencedData.ModelGroupName);
						translationContext.TreeDeclarations.EndElement();
					}
					translationContext.ReferencedModelGroups = null;
				}

				/* reference represented class' model group */
                if (isRepresentative && myTranslationData.IsModelGroup)
				{
					translationContext.TreeDeclarations.GroupRef(myTranslationData.ModelGroupName);
					translationContext.TreeDeclarations.EndElement();
				}
				/* append content of the class itself */
				translationContext.TreeDeclarations.AppendContent(newTreeDeclarations);
				translationContext.TreeDeclarations.EndElement(); // sequence	
			}
		}

		/// <summary>
		/// Translates the class with label. The class is translated into 
		/// a complex type (if the class is referenced from a structural representative, 
		/// <see cref="TranslateClassWithLabelAsGroups"/> is called, otherwise 
		/// class is translated into a complex type where the content and attributes 
		/// are put directly). 
		/// </summary>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="translationContext">The translation context.</param>
		/// <returns>name of the complex type to which <paramref name="psmClass"/> was translated to </returns>
		private string TranslateClassWithLabel(PSMClass psmClass, TranslationContext translationContext)
		{
			/* if this is used as a representedPSMClass from another class, content of the class is translated 
			 * to model group and attribute group and these are then included in a complex type */
			if (Diagram.DiagramElements.Keys.OfType<PSMClass>().Any(rClass => rClass.RepresentedPSMClass == psmClass))
				return TranslateClassWithLabelAsGroups(psmClass, translationContext);

			/* class with label is translated to a named complex type*/
			if (!classesTranslationData.ContainsKey(psmClass))
				classesTranslationData[psmClass] = new ClassTranslationData();
			ClassTranslationData myTranslationData = classesTranslationData[psmClass];
			if (myTranslationData.ComplexTypeName != null) //already translated
				return myTranslationData.ComplexTypeName;
			if (myTranslationData.NameBase == null)
				myTranslationData.NameBase = namingSupport.GetNameForComplexType(psmClass);
			myTranslationData.ComplexTypeName = myTranslationData.NameBase;
			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

			translationContext.TreeDeclarations.ComplexType(myTranslationData.ComplexTypeName);
            if (psmClass.IsStructuralRepresentative && psmClass.RepresentedPSMClass.Generalizations.Count > 0 
                && classesTranslationData.ContainsKey(psmClass.RepresentedPSMClass) && classesTranslationData[psmClass.RepresentedPSMClass].IsComplexType )
            {
                translationContext.TreeDeclarations.ComplexContent();
                translationContext.TreeDeclarations.Extension(classesTranslationData[psmClass.RepresentedPSMClass].ComplexTypeName);
            }
			if (psmClass.IsAbstract)
				translationContext.TreeDeclarations.AbstractAttribute();
			TranslateClassContentWithoutGroups(psmClass, translationContext);
			if (psmClass.AllowAnyAttribute)
				translationContext.TreeDeclarations.WriteAllowAnyAttribute();

            if (psmClass.IsStructuralRepresentative && psmClass.RepresentedPSMClass.Generalizations.Count > 0 
                && classesTranslationData.ContainsKey(psmClass.RepresentedPSMClass) && classesTranslationData[psmClass.RepresentedPSMClass].IsComplexType )
            {
                translationContext.TreeDeclarations.EndElement(); //extension
                translationContext.TreeDeclarations.EndElement(); //complexcontent
            }

			translationContext.TreeDeclarations.EndElement(); // complex type

			choiceCounter = curChoiceCounter;
			return myTranslationData.NameBase;
		}

		/// <summary>
		/// Translates the class even when it is in another subtree. This call is
		/// used when a referenced class must be translated before the translation 
		/// can continue. 
		/// </summary>
		/// <param name="translatedClass">The translated class.</param>
		/// <returns>result of the translation of <paramref name="translatedClass"/></returns>
		private ClassTranslationData TranslateClassNow(PSMClass translatedClass)
		{
			ClassTranslationData myRepresentativeTranslationData;

			if (classesTranslationData.ContainsKey(translatedClass))
			{
				myRepresentativeTranslationData = classesTranslationData[translatedClass];
			}
			else
			{
				string representativeTypeName = namingSupport.GetNameForComplexType(translatedClass);
				myRepresentativeTranslationData = new ClassTranslationData { NameBase = representativeTypeName };
			    myRepresentativeTranslationData.TranslatingNow = true; 
				classesTranslationData.Add(translatedClass, myRepresentativeTranslationData);
				XmlSchemaWriter classWriter = writerFactory.CreateGlobalWriter(false);
				TranslateClass(translatedClass, new TranslationContext(classWriter, null, null));
			    myRepresentativeTranslationData.TranslatingNow = false; 
			}
			return myRepresentativeTranslationData;
		}

		#region specializations

		/// <summary>
		/// Translates the <paramref name="specialization"/>. One of the methods 
		/// <see cref="TranslateSpecializationGeneralIsComplexType"/>,
		/// <see cref="TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel"/> or
		/// <see cref="TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel"/>
		/// is chosen according to whether the general class was translated into 
		/// a complex type and whether the specific class has or has not 
		/// the element label 
		/// </summary>
		/// <param name="specialization">The translated specialization.</param>
		/// <param name="generalTypeName">Identifier of the general type.</param>
		protected override void TranslateSpecialization(Generalization specialization, string generalTypeName)
		{
			PSMClass general = (PSMClass)specialization.General;
			PSMClass specific = (PSMClass)specialization.Specific;

			XmlSchemaWriter classWriter = writerFactory.CreateGlobalWriter(false);

			TranslationContext context = new TranslationContext(classWriter, null, null);
			ClassTranslationData generalData = classesTranslationData[general];

			if (general.HasElementLabel || generalData.IsComplexType)
			{
				TranslateSpecializationGeneralIsComplexType(specific, general, generalTypeName, context);
			}
			else
			{
				if (specific.HasElementLabel)
					TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel(specific, general, context);
				else
					TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel(specific, general, context);
			}
		}

		/// <summary>
		/// Translates specific class into a complex type that is 
		/// an extension of the type to which <paramref name="general"/>
		/// was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="generalTypeName">Name of the complex type to which general class was translated to.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralIsComplexType(PSMClass specific, PSMClass general, string generalTypeName, TranslationContext translationContext)
		{
			string specificTypeName = namingSupport.GetNameForComplexType(specific);
			ClassTranslationData myRepresentativeTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myRepresentativeTranslationData;
			myRepresentativeTranslationData.ComplexTypeName = specificTypeName;
			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

			translationContext.TreeDeclarations.ComplexType(specificTypeName);
			if (specific.IsAbstract)
				translationContext.TreeDeclarations.AbstractAttribute();
			translationContext.TreeDeclarations.ComplexContent();
			translationContext.TreeDeclarations.Extension(generalTypeName);

			TranslateClassContentWithoutGroups(specific, translationContext);
			if (specific.AllowAnyAttribute)
				translationContext.TreeDeclarations.WriteAllowAnyAttribute();
			translationContext.TreeDeclarations.EndElement(); // extension
			translationContext.TreeDeclarations.EndElement(); // complexContent
			translationContext.TreeDeclarations.EndElement(); // complexType	

			PSMClass root;
			if (specific.HasElementLabel 
				&& specific.IsClassSpecializedRoot(Diagram, false, out root)
				&& (!specific.IsAbstract || specific.NonAbstractWithoutLabelRecursive())
				&& specific.ElementName != root.ElementName)
			{
				string elementName = namingSupport.GetNameForGlobalElement(specific);
				writerFactory.globalDeclarations.Element(elementName);
				writerFactory.globalDeclarations.TypeAttribute(namingSupport.NormalizeTypeName(specific, c => c.Name));
				writerFactory.globalDeclarations.EndElement(); // element
			}

			/* if there are specifications, translate them also */
			TranslateSpecializations(specific, specificTypeName);

			choiceCounter = curChoiceCounter;
		}

		/// <summary>
		/// Translates the specific class into a complex type referencing 
		/// model and attribute group to which the general class was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralNotComplexTypeSpecificWithLabel(PSMClass specific, PSMClass general, TranslationContext translationContext)
		{
			ClassTranslationData generalData = classesTranslationData[general];
			string specificTypeName = namingSupport.GetNameForComplexType(specific);
			ClassTranslationData myRepresentativeTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myRepresentativeTranslationData;
			myRepresentativeTranslationData.ComplexTypeName = specificTypeName;
			int curChoiceCounter = choiceCounter;
			choiceCounter = 0;

			translationContext.TreeDeclarations.ComplexType(specificTypeName);
			if (specific.IsAbstract)
				translationContext.TreeDeclarations.AbstractAttribute();

			TranslationContext context = new TranslationContext(translationContext.TreeDeclarations, null, writerFactory.CreateWriter());
			
			if (generalData.IsModelGroup)
				context.ReferencedModelGroups = new [] { generalData };
			if (generalData.IsAttributeGroup)
				context.ReferencedAttributeGroups = new[] { generalData };

			TranslateComponentsIncludingRepresentative(specific, context);
			TranslateAttributesIncludingRepresentative(specific, context);
			if (specific.AllowAnyAttribute)
				translationContext.TreeDeclarations.WriteAllowAnyAttribute();
			translationContext.TreeDeclarations.EndElement(); // complexType	

			PSMClass root;
			if (specific.HasElementLabel
				&& specific.IsClassSpecializedRoot(Diagram, false, out root)
				&& (!specific.IsAbstract || specific.NonAbstractWithoutLabelRecursive())
				&& specific.ElementName != root.ElementName)
			{
				string elementName = namingSupport.GetNameForGlobalElement(specific);
				writerFactory.globalDeclarations.Element(elementName);
				writerFactory.globalDeclarations.TypeAttribute(namingSupport.NormalizeTypeName(specific, c => c.Name));
				writerFactory.globalDeclarations.EndElement(); // element
			}

			/* if there are specifications, translate them also */
			TranslateSpecializations(specific, specificTypeName);

			choiceCounter = curChoiceCounter;
		}

		/// <summary>
		/// Translates the specific class into model and attribute groups that 
		/// reference the groups to which the general class was translated to. 
		/// </summary>
		/// <param name="specific">The specific class.</param>
		/// <param name="general">The general class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateSpecializationGeneralNotComplexTypeSpecificWithoutLabel(PSMClass specific, PSMClass general, TranslationContext translationContext)
		{
			string specificTypeName = namingSupport.TranslateTypeName(specific);
			//XmlSchemaWriter newTreeDeclarations = writerFactory.CreateWriter();
			//XmlSchemaWriter composedAttributes = writerFactory.CreateWriter();
			//TranslationContext context = new TranslationContext(newTreeDeclarations, null, composedAttributes);
			ClassTranslationData generalData = classesTranslationData[general];
			ClassTranslationData myTranslationData = new ClassTranslationData { NameBase = specificTypeName };
			classesTranslationData[specific] = myTranslationData;

			if (generalData.IsModelGroup)
				translationContext.ReferencedModelGroups = new[] { generalData };
			if (generalData.IsAttributeGroup)
				translationContext.ReferencedAttributeGroups = new[] { generalData };

			TranslateClassContentToGroups(specific, translationContext);
			
			/* if there are specifications, translate them also */
			TranslateSpecializations(specific, specificTypeName);
		}

		#endregion 

		/// <summary>
		/// Writes model and attribute groups with the contents of <see cref="PSMClass"/>. 
		/// Called from methods that are translating classes.
		/// </summary>
		/// <param name="psmClass">class whose contents are translated</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateClassContentToGroups(PSMClass psmClass, TranslationContext translationContext)
		{
			ClassTranslationData myTranslationData;
			bool isRepresentative = PrepareRepresentative(psmClass, out myTranslationData);

			/* type name will be used as a name for model and attribute groups */
			string typeName;
			ClassTranslationData meTranslationData;
			bool contentGroupAlreadyTranslated = false;
			bool attributeGroupAlreadyTranslated = false;
			bool needsOptGroup = inChoice;
			if (classesTranslationData.ContainsKey(psmClass) && !classesTranslationData[psmClass].TranslatingNow)
			{
				/* the class was already translated (it was requested from a structural 
				 * representative), typeName is known */
				meTranslationData = classesTranslationData[psmClass];
				typeName = meTranslationData.NameBase;
				if (meTranslationData.IsModelGroup)
				{
					contentGroupAlreadyTranslated = true; 
				}
				if ((needsOptGroup && meTranslationData.IsAttributeGroupDeclaredOptional) ||
					(!needsOptGroup && meTranslationData.IsAttributeGroupDeclaredNormal))
				{
					attributeGroupAlreadyTranslated = true;
				}
			}
			else
			{
				/* the class was not already translated, get the typeName */
                if (!classesTranslationData.ContainsKey(psmClass))
                {
                    typeName = namingSupport.TranslateTypeName(psmClass);
                    meTranslationData = new ClassTranslationData();
                    classesTranslationData[psmClass] = meTranslationData;
                }
                else
                {
                    meTranslationData = classesTranslationData[psmClass];
                    typeName = meTranslationData.NameBase;
                }
			}
			XmlSchemaWriter newComposedAttributes = writerFactory.CreateWriter();
			XmlSchemaWriter newTreeDeclaration = writerFactory.CreateWriter();

			meTranslationData.AttributeGroupUnknown = true;
			meTranslationData.ModelGroupUnknown = true;
			if (meTranslationData.AttributeGroupNameSuggestion == null)
				meTranslationData.AttributeGroupNameSuggestion = typeName + "-a";
			if (needsOptGroup)
			{
				meTranslationData.AttributeGroupNameSuggestion += "-opt";
				if (inChoice) Log.AddWarning(LogMessages.XS_ATTRIBUTES_IN_CHOICE);
			}
			if (meTranslationData.ModelGroupNameSuggestion == null)
				meTranslationData.ModelGroupNameSuggestion = typeName + "-c";

			TranslationContext context = new TranslationContext(newTreeDeclaration, null, newComposedAttributes);
			context.ReferencedModelGroups = translationContext.ReferencedModelGroups;
		
			if (!contentGroupAlreadyTranslated || !attributeGroupAlreadyTranslated)
				TranslateComponentsIncludingRepresentative(psmClass, context);
			
			#region translate content

			/* components are translated as a model group */
			if (!newTreeDeclaration.IsEmpty || meTranslationData.MustCreateModelGroup || contentGroupAlreadyTranslated)
			{
				#region write the model group
				if (!contentGroupAlreadyTranslated)
				{
					string groupName = meTranslationData.ModelGroupNameSuggestion ?? typeName + "-c";
					meTranslationData.ModelGroupName = groupName;
					translationContext.TreeDeclarations.Group(groupName);
					translationContext.TreeDeclarations.AppendContent(newTreeDeclaration);
					translationContext.TreeDeclarations.EndElement(); // group
				}
				#endregion

				/* if ComposedContent writer is passed, put a reference to the created
				 * model group in the ComposedContent writer */
				if (translationContext.ComposedContent != null)
				{
					translationContext.ComposedContent.GroupRef(meTranslationData.ModelGroupName);
					if (translationContext.LeadingAssociation != null)
						translationContext.ComposedContent.MultiplicityAttributes(
							translationContext.LeadingAssociation.Lower, translationContext.LeadingAssociation.Upper);
					translationContext.ComposedContent.EndElement(); // group ref
				}
			}
			
			#endregion 

			#region translate attributes
			
			if (psmClass.Attributes.Count > 0
				|| (isRepresentative && myTranslationData.IsAttributeGroup)
				|| !newComposedAttributes.IsEmpty
				|| translationContext.ReferencedAttributeGroups != null
				|| meTranslationData.MustCreateAttributeGroup
				|| attributeGroupAlreadyTranslated)
			{
				#region write the attribute group
				
				if (!attributeGroupAlreadyTranslated)
				{
					string attributeGroupName = meTranslationData.AttributeGroupNameSuggestion ?? typeName + "-a"; 
					meTranslationData.AttributeGroupName = attributeGroupName;
					if (needsOptGroup)
						meTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
					else
						meTranslationData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;
					
					translationContext.TreeDeclarations.AttributeGroup(attributeGroupName);
					context = new TranslationContext(translationContext.TreeDeclarations, null, newComposedAttributes);
					context.ReferencedAttributeGroups = translationContext.ReferencedAttributeGroups;
					TranslateAttributesIncludingRepresentative(psmClass, context);
					translationContext.TreeDeclarations.EndElement(); // attributeGroup
				}

				#endregion

				/* if ComposedAttributes writer is passed, put a reference to the created
				 * attribute group in the ComposedAttributes writer */
				if (translationContext.ComposedAttributes != null)
				{
					translationContext.ComposedAttributes.AttributeGroupRef(meTranslationData.AttributeGroupName);
					translationContext.ComposedAttributes.EndElement(); // attributeGroup ref
				}
			}

			#endregion

			meTranslationData.AttributeGroupUnknown = false;
			meTranslationData.ModelGroupUnknown = false;
		}

		/// <summary>
		/// Translates the attribute groups again. This method is called when both 
		/// "opt group" and normal attribute groups need to be included in the 
		/// schema. See documentation to <c>XCase</c> for explanation of opt groups. 
		/// </summary>
		/// <param name="psmClass">The PSM class translated.</param>
		/// <param name="attributeGroupName">Name to which the attribute group was
		/// already translated.</param>
		private void TranslateAttributeGroupsAgain(PSMClass psmClass, string attributeGroupName)
		{
			XmlSchemaWriter writer = writerFactory.CreateGlobalWriter(false);

			if (inChoice && !attributeGroupName.EndsWith("-opt"))
			{
				attributeGroupName += "-opt";
			}
			else if (!inChoice && attributeGroupName.EndsWith("-opt"))
			{
				attributeGroupName = attributeGroupName.Substring(0, attributeGroupName.LastIndexOf("-opt"));
			}
			writer.AttributeGroup(attributeGroupName);
			TranslationContext context = new TranslationContext(writer, null, null);
			TranslateAttributesIncludingRepresentative(psmClass, context);
			writer.EndElement(); // attribute group
		}

		/// <summary>
		/// Translates the attributes of a class. If the class is a structural 
		/// representative, the inherited attribute group reference is also included.
		/// </summary>
		/// <param name="psmClass">The translated PSM class.</param>
		/// <param name="translationContext">The translation context.</param>
		private void TranslateAttributesIncludingRepresentative(PSMClass psmClass, TranslationContext translationContext)
		{
			ClassTranslationData myRepresentativeData;
			bool isRepresentative = PrepareRepresentative(psmClass, out myRepresentativeData);

			#region write reference to represented class group
			
			/* append attributes of the represented class as an attribute group */
			if (isRepresentative 
                //&& !psmClass.IsStructuralRepresentativeExternal 
                && myRepresentativeData.IsAttributeGroup)
			{
				if (inChoice)
				{
					if (!myRepresentativeData.IsAttributeGroupDeclaredOptional
						&& !(myRepresentativeData.AttributeGroupUnknown && myRepresentativeData.AttributeGroupUsage == ClassTranslationData.EAGsage.None))
					{
						TranslateAttributeGroupsAgain(psmClass.RepresentedPSMClass, myRepresentativeData.AttributeGroupName);
						myRepresentativeData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
					}
                    if (myRepresentativeData.IsAttributeGroupDeclaredOptional || myRepresentativeData.AttributeGroupUnknown)
                    {
                        translationContext.TreeDeclarations.AttributeGroupRef(myRepresentativeData.GetOptionalAttributeGroupName);
                        myRepresentativeData.MustCreateAttributeGroup = true;
                        myRepresentativeData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Optional;
                        translationContext.TreeDeclarations.EndElement();
                    }
				}
				else
				{
					if (!myRepresentativeData.IsAttributeGroupDeclaredNormal
						&& !(myRepresentativeData.AttributeGroupUnknown && myRepresentativeData.AttributeGroupUsage == ClassTranslationData.EAGsage.None))
					{
						TranslateAttributeGroupsAgain(psmClass.RepresentedPSMClass, myRepresentativeData.AttributeGroupName);
						myRepresentativeData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;
					}
                    if (myRepresentativeData.IsAttributeGroupDeclaredNormal || myRepresentativeData.AttributeGroupUnknown)
                    {
                        translationContext.TreeDeclarations.AttributeGroupRef(myRepresentativeData.GetNormalAttributeGroupName);
                        myRepresentativeData.MustCreateAttributeGroup = true;
                        myRepresentativeData.AttributeGroupUsage |= ClassTranslationData.EAGsage.Normal;
                        translationContext.TreeDeclarations.EndElement();
                    }
				}
			}
			
			#endregion 

			#region write references to referenced groups

			if (translationContext.ReferencedAttributeGroups != null)
			{
				foreach (ClassTranslationData referencedGroup in translationContext.ReferencedAttributeGroups.Where(g => g.IsAttributeGroup))
				{
					translationContext.TreeDeclarations.AttributeGroupRef(referencedGroup.AttributeGroupName);
					translationContext.TreeDeclarations.EndElement();
				}
				translationContext.ReferencedAttributeGroups = null;
			}

			#endregion 

			#region append attributes of the class itself 
			
			/* append attributes of the class itself */
			foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
			{
				translationContext.TreeDeclarations.Attribute(namingSupport.NormalizeTypeName(psmAttribute, a => a.AliasOrName), psmAttribute, ref writerFactory.simpleTypesDeclarations, inChoice);
			}
			if (translationContext.ComposedAttributes != null && !translationContext.ComposedAttributes.IsEmpty)
			{
				translationContext.TreeDeclarations.AppendContent(translationContext.ComposedAttributes);
			}
			
			#endregion 
		}
	}


}