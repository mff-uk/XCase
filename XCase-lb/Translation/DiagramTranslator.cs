using XCase.Model;
using System.Collections.Generic;

namespace XCase.Translation
{
	/// <summary>
	/// Abstract class from which classes that translate PSM diagram
	/// into string representation are derived. One derived class
	/// is provided - <see cref="XCase.Translation.XmlSchema.XmlSchemaTranslator"/>
	/// which translates diagram into schema in XML Schema language. 
	/// </summary>
	/// <remarks>
	/// Bodies of all function except <see cref="TranslateSubordinateComponent"/>,
	/// <see cref="TranslateAssociation"/>
	/// and <see cref="TranslateSpecializations"/> are empty and up to the 
	/// derived classes to override. 
	/// </remarks>
	/// <typeparam name="Context">The type of the context. This type is up to derived
	/// classes to define, it should encapsulate the information that is 
	/// passed among translation functions. The meaning of the context is up 
	/// to deervied classes to define. </typeparam>
	/// <typeparam name="TypeIdentifier">The type of of identifier used for translated classes.</typeparam>
	public abstract class DiagramTranslator<Context, TypeIdentifier>
	{
		/// <summary>
		/// The translated diagram
		/// </summary>
		public PSMDiagram Diagram { get; protected set; }

		/// <summary>
		/// Log where errors and warnings are written during translation.
		/// </summary>
		public TranslationLog Log { get; private set; }

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="DiagramTranslator{Context, TypeIdentifier}"/> class.
		/// </summary>
		protected DiagramTranslator()
		{
			Log = new TranslationLog();
		}

		/// <summary>
		/// Translates the specified diagram into string. The semantics of the return 
		/// value is up to the derived classes to define. 
		/// </summary>
		/// <remarks>
		/// Body of the function should call <see cref="TranslateClass"/> for each root
		/// of <paramref name="diagram"/> (<see cref="PSMDiagram.Roots"/>). 
		/// </remarks>
		/// <param name="diagram">The translated diagram.</param>
		/// <returns>string representation of the <paramref name="diagram"/></returns>
		public abstract string Translate(PSMDiagram diagram);

		/// <summary>
		/// Translates class. 
		/// </summary>
		/// <remarks>
		/// The body of the function should call <see cref="TranslateSubordinateComponent"/>
		/// for each component in <see cref="PSMSuperordinateComponent.Components"/> of <paramref name="psmClass"/>
		/// and also <see cref="TranslateSpecializations"/> to translate specializations of <paramref name="psmClass"/>
		/// </remarks>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="context">The translation context.</param>
		/// <returns></returns>
		protected virtual TypeIdentifier TranslateClass(PSMClass psmClass, Context context) { return default(TypeIdentifier); }

		/// <summary>
		/// Translates the specializations of <paramref name="generalClass"/>. Calls
		/// <see cref="TranslateSpecialization"/> for each specialization of <paramref name="generalClass"/>.
		/// </summary>
		/// <seealso cref="Class.Specifications"/>
		/// <param name="generalClass">The general class.</param>
		/// <param name="generalTypeName">Name of the general type.</param>
		protected void TranslateSpecializations(PSMClass generalClass, TypeIdentifier generalTypeName)
		{
			foreach (Generalization specialization in generalClass.Specifications)
			{
				TranslateSpecialization(specialization, generalTypeName);
			}
		}

		/// <summary>
		/// Translates the <paramref name="specialization"/>.
		/// </summary>
		/// <remarks>
		/// Could call <see cref="TranslateClass"/> for <paramref name="specialization"/>'s 
		/// <see cref="Generalization.Specific"/> class to translate the specializing class the 
		/// same way general classes are translated. If this is not the desired behaviour, 
		/// all components of the specializing clases should be translated via 
		/// <see cref="TranslateSubordinateComponent"/> call. 
		/// </remarks>
		/// <param name="specialization">The translated specialization.</param>
		/// <param name="generalTypeName">Identifier of the general type.</param>
		protected virtual void TranslateSpecialization(Generalization specialization, TypeIdentifier generalTypeName) { }

		/// <summary>
		/// Translates the attribute container.
		/// </summary>
		/// <param name="attributeContainer">The attribute container.</param>
		/// <param name="context">The translation context.</param>
		protected virtual void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, Context context) { }

		/// <summary>
		/// Translates the content choice.
		/// </summary>
		/// <param name="contentChoice">The content choice.</param>
		/// <param name="context">The translation context.</param>
		protected virtual void TranslateContentChoice(PSMContentChoice contentChoice, Context context) { }

		/// <summary>
		/// Translates the content container.
		/// </summary>
		/// <param name="contentContainer">The content container.</param>
		/// <param name="context">The translation context.</param>
		protected virtual void TranslateContentContainer(PSMContentContainer contentContainer, Context context) { }

		/// <summary>
		/// Translates the association. The default implementation calls
		/// <see cref="TranslateAssociationChild"/> on <see cref="PSMAssociation.Child"/>
		/// of <paramref name="association"/>, but this behaviour could be redefined in 
		/// derived classes. 
		/// </summary>
		/// <param name="association">The association.</param>
		/// <param name="context">The translation context.</param>
		/// <remarks>Should probably call <see cref="TranslateAssociationChild"/> on  
		/// <see cref="PSMAssociation.Child"/>
		/// of <paramref name="association"/> if redefined. 
		/// </remarks>
		protected virtual void TranslateAssociation(PSMAssociation association, Context context)
		{
			TranslateAssociationChild(association.Child, context);
		}

		/// <summary>
		/// Translates the association child. 
		/// </summary>
		/// <remarks>
		/// The <see cref="PSMAssociationChild"/> can be 
		/// either <see cref="PSMClass"/> or <see cref="PSMClassUnion"/>. If the child is 
		/// <see cref="PSMClass"/>, <see cref="TranslateClass"/> could be used. 
		/// </remarks>
		/// <param name="associationChild">The association child.</param>
		/// <param name="context">The translation context.</param>
		protected virtual void TranslateAssociationChild(PSMAssociationChild associationChild, Context context) { }

		/// <summary>
		/// Translates the subordinate component.
		/// </summary>
		/// <remarks>If not redefined, the defult implementation calls one of the following according
		/// to the type of the component: <see cref="TranslateContentContainer"/>, 
		/// <see cref="TranslateContentChoice"/>, <see cref="TranslateAssociation"/> or 
		/// <see cref="TranslateAttributeContainer"/>.
		/// </remarks>
		/// <param name="component">The component.</param>
		/// <param name="context">The translation context.</param>
		protected void TranslateSubordinateComponent(PSMSubordinateComponent component, Context context)
		{
			if (component is PSMContentContainer)
				TranslateContentContainer((PSMContentContainer)component, context);
			else if (component is PSMContentChoice)
				TranslateContentChoice((PSMContentChoice)component, context);
			else if (component is PSMAttributeContainer)
				TranslateAttributeContainer((PSMAttributeContainer)component, context);
			else if (component is PSMAssociation)
				TranslateAssociation((PSMAssociation)component, context);
		}

	}
}