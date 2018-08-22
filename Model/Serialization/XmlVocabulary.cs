using System;

namespace XCase.Model.Serialization
{
	public abstract class XmlVocBase
	{
		public virtual string xmlRootElement     { get { return "xc:project"; } }
		public virtual string defaultNamespace   { get { return "http://kocour.ms.mff.cuni.cz/~necasky/xcase"; } }
		public virtual string defaultPrefix      { get { return "xc"; } }

		#region xml attributes

		public virtual string xmlAttXmlns       { get { return "xmlns" + ":" + defaultPrefix; } }
		public virtual string xmlAttID          { get { return "id"; } }
		public virtual string xmlAttName        { get { return "name"; } }
		public virtual string xmlAttNamespace   { get { return "namespace"; } }
		public virtual string xmlAttTargetNamespace     { get { return "target_namespace"; } }
        public virtual string xmlAttNamespacePrefix     { get { return "namespace_prefix"; } }
        public virtual string xmlAttLocal               { get { return "local"; } }
        public virtual string xmlAttSchemaLocation      { get { return "schema_location"; } }
        public virtual string xmlAttReferencedDiagramId { get { return "referenced_diagram"; } }
        public virtual string xmlAttReferencingDiagramId { get { return "referencing_diagram"; } }

		public virtual string xmlAttIndex       { get { return "index"; } }
		public virtual string xmlAttDescription { get { return "description"; } }
		public virtual string xmlAttClass       { get { return "class"; } }
		public virtual string xmlAttClassRef    { get { return "class_ref"; } }
		public virtual string xmlAttViewpoint   { get { return "viewpoint"; } }
		public virtual string xmlAttRef         { get { return "ref"; } }
		public virtual string xmlAttStartRef    { get { return "start_ref"; } }
		public virtual string xmlAttEndRef      { get { return "end_ref"; } }
		public virtual string xmlAttAssociation { get { return "association_ref"; } }
		public virtual string xmlAttType        { get { return "type"; } }
		public virtual string xmlAttDefault     { get { return "default"; } }
		public virtual string xmlAttDefaultValue { get { return "default_value"; } }
		public virtual string xmlAttDirection   { get { return "direction"; } }
		public virtual string xmlAttLower       { get { return "lower"; } }
		public virtual string xmlAttUpper       { get { return "upper"; } }
		public virtual string xmlAttAggregation { get { return "aggregation"; } }
		public virtual string xmlAttCardinality { get { return "cardinality"; } }
		public virtual string xmlAttVisibility  { get { return "visibility"; } }
		public virtual string xmlAttStereotype  { get { return "stereotype"; } }
		public virtual string xmlAttSimple      { get { return "simple"; } }

        public virtual string xmlAttOntoEquiv { get { return "ontologyEquivalent"; } }
		public virtual string xmlAttAbstract { get { return "abstract"; } }
		public virtual string xmlAttGeneral     { get { return "general"; } }
		public virtual string xmlAttSpecific    { get { return "specific"; } }
		public virtual string xmlAttNo          { get { return "no"; } }
		public virtual string xmlAttDiagramId	{ get { return "diagram_id"; } }
		public virtual string xmlAttMin         { get { return "min"; } }
		public virtual string xmlAttMax         { get { return "max"; } }
		public virtual string xmlAttCoreClassRef { get { return "core_class_ref"; } }
		public virtual string xmlAttParentRef    { get { return "parent_ref"; } }
		public virtual string xmlAttParentUnion  { get { return "parent_union"; } }
		public virtual string xmlAttDiamond      { get { return "diamond"; } }
		public virtual string xmlAttAttributesCollapsed          { get { return "properties_collapsed"; } }
		public virtual string xmlAttOperationsCollapsed          { get { return "methods_collapsed"; } }
		public virtual string xmlAttElementNameLabelCollapsed    { get { return "element_label_collapsed"; } }
		public virtual string xmlAttElementNameLabelAlignedRight { get { return "element_label_aligned_right"; } }
		public virtual string xmlAttAlias               { get { return "alias"; } }
		public virtual string xmlAttAttRef              { get { return "att_ref"; } }
		public virtual string xmlAttXSDImplementation   { get { return "att_implementation"; } }
		public virtual string xmlAttIsComposite      { get { return "is_composite"; } }
		public virtual string xmlAttIsOrdered        { get { return "is_ordered"; } }
		public virtual string xmlAttIsUnique         { get { return "is_unique"; } }
		public virtual string xmlAttIsDerived        { get { return "is_derived"; } }
		public virtual string xmlAttIsDerivedUnion   { get { return "is_derived_union"; } }
		public virtual string xmlAttIsReadOnly       { get { return "is_readonly"; } }
		public virtual string xmlAttIsSubstitable    { get { return "is_substitable"; } }
		public virtual string xmlAttRepresentedClass { get { return "represented_class"; } }
		public virtual string xmlAttRepresentedClassName { get { return "represented_class_name"; } }
		public virtual string xmlAttStructuralRepresentative { get { return "structural_representative"; } }

		public virtual string xmlAttVersion { get { return "version"; } }

		#endregion

		#region xml element names

		public virtual string xmlUmlElement         { get {  return "xc:uml"; } }
		public virtual string xmlModelElement       { get {  return "xc:model"; } }
		public virtual string xmlProfilesElement    { get {  return "xc:profiles"; } }
		public virtual string xmlProfileElement     { get {  return "xc:profile"; } }
		public virtual string xmlPackagesElement    { get {  return "xc:packages"; } }
		public virtual string xmlPackageElement     { get {  return "xc:package"; } }
		public virtual string xmlStereotypesElement { get {  return "xc:stereotypes"; } }
		public virtual string xmlStereotypeElement  { get {  return "xc:stereotype"; } }
		public virtual string xmlStereotypeInstances    { get {  return "xc:stereotype_instances"; } }
		public virtual string xmlStereotypeInstance     { get {  return "xc:stereotype_instance"; } }
		public virtual string xmlValueSpecificationElement { get {  return "xc:value_specification"; } }
		public virtual string xmlValueElement        { get {  return "xc:value"; } }
		public virtual string xmlReceiversElement   { get {  return "xc:receivers"; } }
		public virtual string xmlReceiverElement    { get {  return "xc:receiver"; } }
		public virtual string xmlPrimitiveTypes { get { return "xc:primitive_types"; } }
		public virtual string xmlDatatypesElement   { get {  return "xc:datatypes"; } }
		public virtual string xmlDatatypeElement    { get {  return "xc:datatype"; } }
		public virtual string xmlPimClassesElement  { get {  return "xc:pim_classes"; } }
		public virtual string xmlPimClassElement    { get {  return "xc:pim_class"; } }
		public virtual string xmlPsmClassesElement  { get {  return "xc:psm_classes"; } }
		public virtual string xmlPsmClassElement    { get {  return "xc:psm_class"; } }
		public virtual string xmlComponentsElement  { get {  return "xc:components"; } }
		public virtual string xmlPSMAssociation     { get {  return "xc:psm_association"; } }
		public virtual string xmlPSMAttContainer    { get {  return "xc:psm_att_container"; } }
		public virtual string xmlPSMContentContainer   { get {  return "xc:content_container"; } }
		public virtual string xmlPSMContentChoice      { get {  return "xc:content_choice"; } }
		public virtual string xmlPSMClassUnion         { get { return "xc:class_union"; } }
        public virtual string xmlPSMDiagramReference   { get { return "xc:diagram_reference"; } }
		public virtual string xmlPSMAttributes      { get {  return "xc:psm_attributes"; } }
		public virtual string xmlPSMAttribute       { get {  return "xc:psm_attribute"; } }
		public virtual string xmlElementName        { get {  return "xc:element_name"; } }
		public virtual string xmlElementLabel       { get {  return "xc:element_label"; } }
		public virtual string xmlAllowAnyAttribute { get { return "xc:allow_any_attribute"; } }
		public virtual string xmlChildElement       { get {  return "xc:child"; } }
		public virtual string xmlParentElement      { get {  return "xc:parent"; } }
		public virtual string xmlUsedGeneralizations { get { return "xc:used_generalizations"; } }
		public virtual string xmlUsedGeneralization  { get { return "xc:used_generalization"; } }
		public virtual string xmlNestingJoins       { get {  return "xc:nesting_joins"; } }
		public virtual string xmlNestingJoin        { get {  return "xc:nesting_join"; } }
		public virtual string xmlCoreClass          { get {  return "xc:core_class"; } }
		public virtual string xmlPimPath            { get {  return "xc:pim_path"; } }
		public virtual string xmlPimStep            { get {  return "xc:pim_step"; } }
		public virtual string xmlContext            { get {  return "xc:context"; } }
		public virtual string xmlMultiplicityLabel  { get { return "xc:multiplicity_label"; } }
		public virtual string xmlPropertiesElement      { get {  return "xc:properties"; } }
		public virtual string xmlPropertyElement        { get {  return "xc:property"; } }
		public virtual string xmlOperationsElement      { get {  return "xc:operations"; } }
		public virtual string xmlOperationElement       { get {  return "xc:operation"; } }
		public virtual string xmlParameterElement       { get {  return "xc:parameter"; } }
		public virtual string xmlAssociationsElement    { get {  return "xc:associations"; } }
		public virtual string xmlAssociationElement     { get {  return "xc:association"; } }
		public virtual string xmlAssociationClassElement { get { return "xc:association_class"; } }
		public virtual string xmlAssociationEndsElement { get { return "xc:association_ends"; } }
		public virtual string xmlAssociationEndElement  { get {  return "xc:association_end"; } }
		public virtual string xmlGeneralizationsElement { get {  return "xc:generalizations"; } }
		public virtual string xmlGeneralizationElement  { get {  return "xc:generalization"; } }
		public virtual string xmlPIMGeneralizationElement { get { return "xc:pim_generalization"; } }
		public virtual string xmlPSMGeneralizationElement { get { return "xc:psm_generalization"; } }
		public virtual string xmlCommentsElement        { get {  return "xc:comments"; } }
		public virtual string xmlCommentElement         { get {  return "xc:comment"; } }
		public virtual string xmlAttributeElement       { get {  return "xc:attribute"; } }
		public virtual string xmlRoots { get { return "xc:roots"; } }
		public virtual string xmlRoot { get { return "xc:root"; } }
		public virtual string xmlDiagramsElement         { get {  return "xc:diagrams"; } }
		public virtual string xmlAbstractDiagramsElement { get {  return "xc:PIM_diagrams"; } }
		public virtual string xmlSpecificDiagramsElement { get {  return "xc:PSM_diagrams"; } }
		public virtual string xmlDiagramElement          { get {  return "xc:diagram"; } }
		public virtual string xmlClassElement            { get {  return "xc:class"; } }
		public virtual string xmlAppearanceElement       { get {  return "xc:appearance"; } }
		public virtual string xmlWidthElement            { get {  return "xc:width"; } }
		public virtual string xmlHeightElement           { get {  return "xc:height"; } }
		public virtual string xmlCoordXElement           { get {  return "xc:coordinate_x"; } }
		public virtual string xmlCoordYElement           { get {  return "xc:coordinate_y"; } }
		public virtual string xmlPoints                  { get {  return "xc:points"; } }
		public virtual string xmlPoint                   { get {  return "xc:point"; } }
		public virtual string xmlDiamond                 { get {  return "xc:diamond"; } }
		public virtual string xmlNameLabel               { get {  return "xc:namelabel"; } }
		public virtual string xmlCardinalityLabel        { get {  return "xc:cardinality_label"; } }
		public virtual string xmlRoleLabel               { get {  return "xc:role_label"; } }

		#endregion

		#region relative elements

		public virtual string relativeProfiles       { get { return xmlProfilesElement + "/" + xmlProfileElement; } }
		public virtual string relativeStereotypes    { get { return  xmlStereotypesElement + "/" + xmlStereotypeElement;} }
		public virtual string relativeReceivers      { get { return  xmlReceiversElement + "/" + xmlReceiverElement;} }
		public virtual string relativeProperties     { get { return  xmlPropertiesElement + "/" + xmlPropertyElement;} }
		public virtual string relativePSMAttributes  { get { return xmlPSMAttributes + "/" + xmlPSMAttribute; } }
		public virtual string relativeOperations     { get { return xmlOperationsElement + "/" + xmlOperationElement; } }
		public virtual string relativeComments       { get { return xmlCommentsElement;  } }
		public virtual string relativePimClasses     { get { return xmlPimClassesElement + "/" + xmlPimClassElement;} }
		public virtual string relativePsmClasses     { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement;} }
		public virtual string relativePsmClassesNoRepresentants { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement + "[not(@structural_representative)]"; } }
		public virtual string relativePsmClassesRepresentants { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement + "[@structural_representative]"; } }
		public virtual string relativeDatatypes      { get { return xmlDatatypesElement + "/" + xmlDatatypeElement;} }
		public virtual string relativePackages       { get { return xmlPackagesElement + "/" + xmlPackageElement;} }
		public virtual string relativeStereotypeInstances { get { return xmlStereotypeInstances + "/" + xmlStereotypeInstance;} }
		public virtual string relativePoints         { get { return xmlPoints + "/" + xmlPoint; } }
		public virtual string relativeAssociations   { get { return xmlAssociationsElement + "/" + xmlAssociationElement;} }
		public virtual string relativeAssociationClasses { get { return xmlAssociationsElement + "/" + xmlAssociationClassElement; } }
		public virtual string relativePSMGeneralizations { get { return xmlGeneralizationsElement + "/" + xmlPSMGeneralizationElement; } }
        public virtual string relativePSMDiagramReferences { get { return xmlPSMDiagramReference; } }
		public virtual string relativePIMGeneralizations { get { return xmlGeneralizationsElement + "/" + xmlPIMGeneralizationElement; } }
		public virtual string relativeNestingJoins   { get { return xmlNestingJoins + "/"+ xmlNestingJoin; } }
		public virtual string relativePIMSteps       { get { return xmlChildElement + "/" + xmlPimStep ; } }
		public virtual string relativeAssociationEnds { get { return xmlAssociationEndsElement + "/" + xmlAssociationEndElement; } }
		public virtual string relativeAllComponents  { get { return xmlComponentsElement + "/*"; } }

		#endregion

		#region composed paths

		public abstract string selectModel            { get; }
		public abstract string selectModelElement { get; }
		public virtual string modelComments          { get { return selectModel + relativeComments; } }
		public virtual string modelClasses           { get { return selectModel + relativePimClasses; } }
		public virtual string modelPackages          { get { return selectModel + relativePackages; } }
		public virtual string modelPimClasses        { get { return selectModel + relativePimClasses; } }
		public virtual string modelAssociationClasses { get { return selectModel  + relativeAssociationClasses; }}
		public virtual string modelDatatypes         { get { return selectModel + relativeDatatypes; } }
		public virtual string modelAssociations      { get { return selectModel + relativeAssociations; } }
		public virtual string modelPIMGeneralizations   { get { return selectModel + relativePIMGeneralizations; } }
		public virtual string modelPSMGeneralizations { get { return selectModel + relativePSMGeneralizations; } }
        public virtual string modelPSMDiagramReferences { get { return allPSMdiagrams + "/" + relativePSMDiagramReferences; } }
		public virtual string allUserProfiles { get { return allProfiles + "[@name != 'XSem']"; } }
		public virtual string allPIMdiagrams { get { return allDiagrams + "/" + xmlAbstractDiagramsElement + "/" + xmlDiagramElement; } }
		public virtual string allPSMdiagrams { get { return allDiagrams + "/" + xmlSpecificDiagramsElement + "/" + xmlDiagramElement; } }
		public virtual string allPSMAssociations { get { return selectModel + "/" + xmlPSMAssociation + "[@" + xmlAttID + "]"; } }

		#endregion

		#region get visualization fuctions

		/// <summary>
		/// Returns XPath query describing the PIM class visualization
		/// </summary>
		/// <param name="class_id">ID of the PIM class</param>
		/// <returns></returns>
		public virtual string GetVisualizationForPIMClass(string class_id)
		{
			return allPIMdiagrams + "/" + xmlClassElement + "[@" + xmlAttRef + "=\"" + class_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing the PSM class visualization
		/// </summary>
		/// <param name="class_id">ID of the PSM class</param>
		/// <returns>XPath query describing the PSM class visualization</returns>
		public virtual string GetVisualizationForPSMClass(string class_id)
		{
			return allPSMdiagrams + "/" + xmlClassElement + "[@" + xmlAttRef + "=\"" + class_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing visualization for appropriate PIM Association
		/// </summary>
		/// <param name="association_id">ID of PIM Association</param>
		/// <returns>XPath query describing the PIM Association visualization</returns>
		public virtual string GetVisualizationForPIMAssociation(string association_id)
		{
			return allPIMdiagrams + "/" + xmlAssociationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing visualization for appropriate Association Class
		/// </summary>
		/// <param name="associationclass_id">ID of Association Class</param>
		/// <returns>XPath query describing the Association Class visualization</returns>
		public virtual string GetVisualizationForClassAssociation(string associationclass_id)
		{
			return allPIMdiagrams + "/" + xmlAssociationClassElement + "[@" + xmlAttRef + "=\"" + associationclass_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing visualization for appropriate PSM Association
		/// </summary>
		/// <param name="association_id">ID of PSM Association</param>
		/// <returns>XPath query describing the PSM Association visualization</returns>
		public virtual string GetVisualizationForPSMAssociation(string association_id)
		{
			return allPSMdiagrams + "/" + xmlPSMAssociation + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing visualization for appropriate PIM Generalization
		/// </summary>
		/// <param name="association_id">ID of PIM Generalization</param>
		/// <returns>XPath query describing the PIM Generalization visualization</returns>
		public virtual string GetVisualizationForPIMGeneralization(string association_id)
		{
			return allPIMdiagrams + "/" + xmlGeneralizationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing visualization for appropriate PSM Generalization
		/// </summary>
		/// <param name="association_id">ID of PSM Generalization</param>
		/// <returns>XPath query describing the PSM Generalization visualization</returns>
		public virtual string GetVisualizationForPSMGeneralization(string association_id)
		{
			return allPSMdiagrams + "/" + xmlGeneralizationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing the PIM Comment visualization
		/// </summary>
		/// <param name="comment_id">ID of PIM Comment</param>
		/// <returns>XPath query describing the PIM Comment visualization</returns>
		public virtual string GetVisualizationForPIMComment(string comment_id)
		{
			return allPIMdiagrams + "/" + xmlCommentElement + "[@" + xmlAttRef + "=\"" + comment_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing PSM Comment visualization
		/// </summary>
		/// <param name="comment_id">ID of PSM Comment</param>
		/// <returns>XPath query describing the PSM Comment visualization</returns>
		public virtual string GetVisualizationForPSMComment(string comment_id)
		{
			return allPSMdiagrams + "/" + xmlCommentElement + "[@" + xmlAttRef + "=\"" + comment_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing PSM Attribute Container visualization
		/// </summary>
		/// <param name="container_id">ID of PSM Attribute Container </param>
		/// <returns>XPath query describing the PSM Attribute Container visualization</returns>
		public virtual string GetVisualizationForPSMAttContainer(string container_id)
		{
            return allPSMdiagrams + "/" + xmlPSMAttContainer + "[@" + xmlAttRef + "=\"" + container_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing PSM Content Container visualization
		/// </summary>
		/// <param name="container_id">ID of PSM Content Container </param>
		/// <returns>XPath query describing the PSM Content Container visualization</returns>
		public virtual string GetVisualizationForPSMContentContainer(string container_id)
		{
			return allPSMdiagrams + "/" + xmlPSMContentContainer + "[@" + xmlAttRef + "=\"" + container_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing PSM Content Choice visualization
		/// </summary>
		/// <param name="choice_id">ID of PSM Content Choice </param>
		/// <returns>XPath query describing the PSM Content Choice visualization</returns>
		public virtual string GetVisualizationForPSMContentChoice(string choice_id)
		{
			return allPSMdiagrams + "/" + xmlPSMContentChoice + "[@" + xmlAttRef + "=\"" + choice_id + "\"]";
		}

		/// <summary>
		/// Returns XPath query describing PSM Class Union visualization
		/// </summary>
		/// <param name="union_id">ID of PSM Class Union</param>
		/// <returns>XPath query describing the PSM Class Union visualization</returns>
		public virtual string GetVisualizationForPSMClassUnion(string union_id)
		{
			return allPSMdiagrams + "/" + xmlPSMClassUnion + "[@" + xmlAttRef + "=\"" + union_id + "\"]";
		}

        /// <summary>
        /// Returns XPath query describing PSM diagram reference visualization
        /// </summary>
        /// <param name="reference_id">ID of PSM diagram reference</param>
        /// <returns>XPath query describing the PSM diagram reference visualization</returns>
        public virtual string GetVisualizationForPSMDiagramReference(string reference_id)
        {
            return allPSMdiagrams + "/" + xmlPSMDiagramReference + "[@" + xmlAttRef + "=\"" + reference_id + "\"]";
        }

		#endregion

		public abstract string allProfiles { get; }

		public abstract string allDiagrams { get; }
	}

	/// <summary>
	/// XML Vocabulary for XCase XML file.
	/// <para>
	/// Used in XMLSerializator and XMLDeserializator classes.</para>
	/// </summary>
	public class XmlVocNoVersions : XmlVocBase
	{
		public virtual string selectPrimitiveTypes { get { return selectUml + xmlPrimitiveTypes + "/" + xmlDatatypeElement; } }
		public virtual string selectUml { get { return "/" + xmlRootElement + "/" + xmlUmlElement + "/"; } }
		public override string selectModel { get { return selectUml + xmlModelElement + "/"; } }
		public override string selectModelElement { get { return selectUml + xmlModelElement; } }
		public virtual string xmlAttFirstVersion { get { return "firstVersion"; } }
		public virtual string xmlAttLabel { get { return "label"; } }
		public virtual string xmlAttNumber { get { return "number"; } }
		public virtual string xmlAttCreatedFrom { get { return "createdFrom"; } }
		public override string allProfiles { get { return selectUml + relativeProfiles; } }
		public override string allDiagrams { get { return "/" + xmlRootElement + "/" + xmlDiagramsElement; } }
	};

	public class XmlVocVersions : XmlVocBase
	{
		public virtual string xmlVersionedProjectsElement { get { return "xc:versionedProjects"; } }

		public virtual string xmlVersionsElement { get { return "xc:versions"; } }

		public virtual string xmlVersionElement { get { return "xc:version"; } }

		public virtual string xmlVersionLinksElement { get { return "xc:versionLinks";  } }

		public virtual string xmlVersionLinkElement { get { return "xc:versionLink";  } }
		
		public virtual string xmlVersionLinkDiagramElement { get { return "xc:versionLinkDiagram";  } }

		public virtual string xmlAttLabel { get { return "label"; } }

		public virtual string xmlAttNumber { get { return "number"; } }

		public virtual string xmlAttCreatedFrom { get { return "createdFrom"; } }

		public virtual string xmlAttFirstVersion { get { return "firstVersion"; } }
		
		public virtual string xmlAttFirstVersionRef { get { return "firstVersionRef"; } }

		public virtual string selectVersions { get { return String.Format("/{0}/{1}/{2}", xmlRootElement, xmlVersionsElement, xmlVersionElement); } }
		
		public virtual string selectVersionedProjects { get { return String.Format("/{0}/{1}/{2}", xmlRootElement, xmlVersionedProjectsElement, xmlVersionElement ); } }

		public virtual string selectVersionLinks { get { return String.Format("/{0}/{1}/{2} | /{0}/{1}/{3}", xmlRootElement, xmlVersionLinksElement, xmlVersionLinkElement, xmlVersionLinkDiagramElement); } }

		public int currentVersionNumber { get; set; }

		public string selectCurrentVersiondProject
		{
			get { return String.Format("{0}[@number={1}]", selectVersionedProjects, currentVersionNumber); }
		}

		public override string selectModel
		{
			get { return String.Format("{0}/{1}/", selectCurrentVersiondProject, xmlModelElement); }
		}

		public override string selectModelElement
		{
			get { return String.Format("{0}/{1}", selectCurrentVersiondProject, xmlModelElement); }
		}

		public override string allProfiles
		{
			get { return String.Format("{0}/{1}", selectCurrentVersiondProject, xmlProfilesElement); }
		}

		public override string allDiagrams
		{
			get { return String.Format("{0}/{1}", selectCurrentVersiondProject, xmlDiagramsElement); }
		}
	}
}