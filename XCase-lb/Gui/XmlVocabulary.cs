using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using XCase.Model;

namespace XCase.Gui
{
    /// <summary>
    /// XML Vocabulary for XCase XML file.
    /// <para>
    /// Used in XMLSerializator and XMLDeserializator classes.</para>
    /// </summary>
    public class XmlVoc
    {
        public static string xmlRootElement     { get { return "xc:project"; } }
        public static string defaultNamespace   { get { return "http://kocour.ms.mff.cuni.cz/~necasky/xcase"; } }
        public static string defaultPrefix      { get { return "xc"; } }

        #region XML Attributes

        public static string xmlAttXmlns       { get { return "xmlns" + ":" + defaultPrefix; } }
        public static string xmlAttID          { get { return "id"; } }
        public static string xmlAttName        { get { return "name"; } }
        public static string xmlAttNamespace   { get { return "namespace"; } }
        public static string xmlAttIndex       { get { return "index"; } }
        public static string xmlAttDescription { get { return "description"; } }
        public static string xmlAttClass       { get { return "class"; } }
        public static string xmlAttClassRef    { get { return "class_ref"; } }
        public static string xmlAttViewpoint   { get { return "viewpoint"; } }
        public static string xmlAttRef         { get { return "ref"; } }
        public static string xmlAttStartRef    { get { return "start_ref"; } }
        public static string xmlAttEndRef      { get { return "end_ref"; } }
        public static string xmlAttAssociation { get { return "association_ref"; } }
        public static string xmlAttType        { get { return "type"; } }
        public static string xmlAttDefault     { get { return "default"; } }
        public static string xmlAttDefaultValue { get { return "default_value"; } }
        public static string xmlAttDirection   { get { return "direction"; } }
        public static string xmlAttLower       { get { return "lower"; } }
        public static string xmlAttUpper       { get { return "upper"; } }
        public static string xmlAttAggregation { get { return "aggregation"; } }
        public static string xmlAttCardinality { get { return "cardinality"; } }
        public static string xmlAttVisibility  { get { return "visibility"; } }
        public static string xmlAttStereotype  { get { return "stereotype"; } }
        public static string xmlAttSimple      { get { return "simple"; } }
        public static string xmlAttVersion     { get { return "version"; } }

        public static string xmlAttAbstract { get { return "abstract"; } }

        public static string xmlAttGeneral     { get { return "general"; } }
        public static string xmlAttSpecific    { get { return "specific"; } }
        
        public static string xmlAttNo          { get { return "no"; } }
        public static string xmlAttMin         { get { return "min"; } }        
        public static string xmlAttMax         { get { return "max"; } }

        public static string xmlAttCoreClassRef { get { return "core_class_ref"; } }
        public static string xmlAttParentRef    { get { return "parent_ref"; } }
        public static string xmlAttParentUnion  { get { return "parent_union"; } }

        public static string xmlAttDiamond      { get { return "diamond"; } }
        
        public static string xmlAttAttributesCollapsed          { get { return "properties_collapsed"; } }
        public static string xmlAttOperationsCollapsed          { get { return "methods_collapsed"; } }
        public static string xmlAttElementNameLabelCollapsed    { get { return "element_label_collapsed"; } }
        public static string xmlAttElementNameLabelAlignedRight { get { return "element_label_aligned_right"; } }


        #region PSM Attribute
        public static string xmlAttAlias               { get { return "alias"; } }
        public static string xmlAttAttRef              { get { return "att_ref"; } }
        public static string xmlAttXSDImplementation   { get { return "att_implementation"; } }
        #endregion

        public static string xmlAttIsComposite      { get { return "is_composite"; } }
        public static string xmlAttIsOrdered        { get { return "is_ordered"; } }
        public static string xmlAttIsUnique         { get { return "is_unique"; } }
        public static string xmlAttIsDerived        { get { return "is_derived"; } }
        public static string xmlAttIsDerivedUnion   { get { return "is_derived_union"; } }
        public static string xmlAttIsReadOnly       { get { return "is_readonly"; } }

        public static string xmlAttIsSubstitable    { get { return "is_substitable"; } }

        public static string xmlAttRepresentedClass { get { return "represented_class"; } }
        public static string xmlAttRepresentedClassName { get { return "represented_class_name"; } }
        public static string xmlAttStructuralRepresentative { get { return "structural_representative"; } }

        #endregion

        #region XML elements vocabulary for UML model

        public static string xmlUmlElement         { get {  return "xc:uml"; } }
        public static string xmlModelElement       { get {  return "xc:model"; } }

        public static string xmlProfilesElement    { get {  return "xc:profiles"; } }
        public static string xmlProfileElement     { get {  return "xc:profile"; } }

        public static string xmlPackagesElement    { get {  return "xc:packages"; } }
        public static string xmlPackageElement     { get {  return "xc:package"; } }

        public static string xmlStereotypesElement { get {  return "xc:stereotypes"; } }
        public static string xmlStereotypeElement  { get {  return "xc:stereotype"; } }

        public static string xmlStereotypeInstances    { get {  return "xc:stereotype_instances"; } }
        public static string xmlStereotypeInstance     { get {  return "xc:stereotype_instance"; } }

        public static string xmlValueSpecificationElement { get {  return "xc:value_specification"; } }
        public static string xmlValueElement        { get {  return "xc:value"; } }

        public static string xmlReceiversElement   { get {  return "xc:receivers"; } }
        public static string xmlReceiverElement    { get {  return "xc:receiver"; } }

        public static string xmlPrimitiveTypes { get { return "xc:primitive_types"; } }

        public static string xmlDatatypesElement   { get {  return "xc:datatypes"; } }
        public static string xmlDatatypeElement    { get {  return "xc:datatype"; } }

        public static string xmlPimClassesElement  { get {  return "xc:pim_classes"; } }
        public static string xmlPimClassElement    { get {  return "xc:pim_class"; } }

        public static string xmlPsmClassesElement  { get {  return "xc:psm_classes"; } }
        public static string xmlPsmClassElement    { get {  return "xc:psm_class"; } }

        public static string xmlComponentsElement  { get {  return "xc:components"; } }
        public static string xmlPSMAssociation     { get {  return "xc:psm_association"; } }
        public static string xmlPSMAttContainer    { get {  return "xc:psm_att_container"; } }
        public static string xmlPSMContentContainer   { get {  return "xc:content_container"; } }
        public static string xmlPSMContentChoice      { get {  return "xc:content_choice"; } }
        public static string xmlPSMClassUnion         { get { return "xc:class_union"; } }

        public static string xmlPSMAttributes      { get {  return "xc:psm_attributes"; } }
        public static string xmlPSMAttribute       { get {  return "xc:psm_attribute"; } }

        public static string xmlElementName        { get {  return "xc:element_name"; } }
        public static string xmlElementLabel       { get {  return "xc:element_label"; } }
		public static string xmlAllowAnyAttribute { get { return "xc:allow_any_attribute"; } }

        public static string xmlChildElement       { get {  return "xc:child"; } }
        public static string xmlParentElement      { get {  return "xc:parent"; } }

        public static string xmlUsedGeneralizations { get { return "xc:used_generalizations"; } }
        public static string xmlUsedGeneralization  { get { return "xc:used_generalization"; } }

        public static string xmlNestingJoins       { get {  return "xc:nesting_joins"; } }
        public static string xmlNestingJoin        { get {  return "xc:nesting_join"; } }
        public static string xmlCoreClass          { get {  return "xc:core_class"; } }
        public static string xmlPimPath            { get {  return "xc:pim_path"; } }
        public static string xmlPimStep            { get {  return "xc:pim_step"; } }
        public static string xmlContext            { get {  return "xc:context"; } }
        public static string xmlMultiplicityLabel  { get { return "xc:multiplicity_label"; } }

        public static string xmlPropertiesElement      { get {  return "xc:properties"; } }
        public static string xmlPropertyElement        { get {  return "xc:property"; } }

        public static string xmlOperationsElement      { get {  return "xc:operations"; } }
        public static string xmlOperationElement       { get {  return "xc:operation"; } }
        public static string xmlParameterElement       { get {  return "xc:parameter"; } }

        public static string xmlAssociationsElement    { get {  return "xc:associations"; } }
        public static string xmlAssociationElement     { get {  return "xc:association"; } }
        public static string xmlAssociationClassElement { get { return "xc:association_class"; } }
        public static string xmlAssociationEndsElement { get { return "xc:association_ends"; } }
        public static string xmlAssociationEndElement  { get {  return "xc:association_end"; } }

        public static string xmlGeneralizationsElement { get {  return "xc:generalizations"; } }
        public static string xmlGeneralizationElement  { get {  return "xc:generalization"; } }
        public static string xmlPIMGeneralizationElement { get { return "xc:pim_generalization"; } }
        public static string xmlPSMGeneralizationElement { get { return "xc:psm_generalization"; } }

        public static string xmlCommentsElement        { get {  return "xc:comments"; } }
        public static string xmlCommentElement         { get {  return "xc:comment"; } }
        public static string xmlAttributeElement       { get {  return "xc:attribute"; } }

        public static string xmlRoots { get { return "xc:roots"; } }
        public static string xmlRoot { get { return "xc:root"; } }

        #endregion

        #region  XML elements vocabulary for visualization

        public static string xmlDiagramsElement         { get {  return "xc:diagrams"; } }
        public static string xmlAbstractDiagramsElement { get {  return "xc:PIM_diagrams"; } }
        public static string xmlSpecificDiagramsElement { get {  return "xc:PSM_diagrams"; } }
        public static string xmlDiagramElement          { get {  return "xc:diagram"; } }
        public static string xmlClassElement            { get {  return "xc:class"; } }
        public static string xmlAppearanceElement       { get {  return "xc:appearance"; } }
        public static string xmlWidthElement            { get {  return "xc:width"; } }
        public static string xmlHeightElement           { get {  return "xc:height"; } }
        public static string xmlCoordXElement           { get {  return "xc:coordinate_x"; } }
        public static string xmlCoordYElement           { get {  return "xc:coordinate_y"; } }
        public static string xmlPoints                  { get {  return "xc:points"; } }
        public static string xmlPoint                   { get {  return "xc:point"; } }

        public static string xmlDiamond                 { get {  return "xc:diamond"; } }
        public static string xmlNameLabel               { get {  return "xc:namelabel"; } }
        public static string xmlCardinalityLabel        { get {  return "xc:cardinality_label"; } }
        public static string xmlRoleLabel               { get {  return "xc:role_label"; } }
        #endregion

        #region XPath queries

        public static string relativeProfiles       { get { return xmlProfilesElement + "/" + xmlProfileElement; } }
        public static string relativeStereotypes    { get { return  xmlStereotypesElement + "/" + xmlStereotypeElement;} }
        public static string relativeReceivers      { get { return  xmlReceiversElement + "/" + xmlReceiverElement;} }
        public static string relativeProperties     { get { return  xmlPropertiesElement + "/" + xmlPropertyElement;} }
        public static string relativePSMAttributes  { get { return xmlPSMAttributes + "/" + xmlPSMAttribute; } }
              
        public static string relativeOperations     { get { return xmlOperationsElement + "/" + xmlOperationElement; } }
        public static string relativeComments       { get { return xmlCommentsElement;  } }
        public static string relativePimClasses     { get { return xmlPimClassesElement + "/" + xmlPimClassElement;} }
        public static string relativePsmClasses     { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement;} }

        public static string relativePsmClassesNoRepresentants { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement + "[not(@structural_representative)]"; } }
        public static string relativePsmClassesRepresentants { get { return xmlPsmClassesElement + "/" + xmlPsmClassElement + "[@structural_representative]"; } }

        public static string relativeDatatypes      { get { return xmlDatatypesElement + "/" + xmlDatatypeElement;} }
        public static string relativePackages       { get { return xmlPackagesElement + "/" + xmlPackageElement;} }
        public static string relativeStereotypeInstances { get { return xmlStereotypeInstances + "/" + xmlStereotypeInstance;} }
        public static string relativePoints         { get { return xmlPoints + "/" + xmlPoint; } }
        public static string relativeAssociations   { get { return xmlAssociationsElement + "/" + xmlAssociationElement;} }
        public static string relativeAssociationClasses { get { return xmlAssociationsElement + "/" + xmlAssociationClassElement; } }
        public static string relativePSMGeneralizations { get { return xmlGeneralizationsElement + "/" + xmlPSMGeneralizationElement; } }

        public static string relativePIMGeneralizations { get { return xmlGeneralizationsElement + "/" + xmlPIMGeneralizationElement; } }
        public static string relativeNestingJoins   { get { return xmlNestingJoins + "/"+ xmlNestingJoin; } }
        public static string relativePIMSteps       { get { return xmlChildElement + "/" + xmlPimStep ; } }

        public static string relativeAssociationEnds { get { return XmlVoc.xmlAssociationEndsElement + "/" + XmlVoc.xmlAssociationEndElement; } }

        public static string relativeAllComponents  { get { return XmlVoc.xmlComponentsElement + "/*"; } }

        public static string selectUml              { get { return "/" + xmlRootElement + "/" + xmlUmlElement + "/"; } }
        public static string selectModel            { get { return selectUml + xmlModelElement + "/"; } }
        public static string selectModelElement { get { return selectUml + xmlModelElement; } }
        public static string selectPrimitiveTypes   { get { return selectUml + xmlPrimitiveTypes + "/" + xmlDatatypeElement; } }

        public static string modelComments          { get { return selectModel + relativeComments; } }
        public static string modelClasses           { get { return selectModel + relativePimClasses; } }
        public static string modelPackages          { get { return selectModel + relativePackages; } }
        public static string modelPimClasses        { get { return selectModel + relativePimClasses; } }
        public static string modelAssociationClasses { get { return selectModel  + relativeAssociationClasses; }}
        public static string modelDatatypes         { get { return selectModel + relativeDatatypes; } }
        public static string modelAssociations      { get { return selectModel + relativeAssociations; } }
        public static string modelPIMGeneralizations   { get { return selectModel + relativePIMGeneralizations; } }

        public static string modelPSMGeneralizations { get { return selectModel + relativePSMGeneralizations; } }
  
        public static string allProfiles    { get { return selectUml + relativeProfiles; } }
        public static string allDiagrams    { get { return "/" + xmlRootElement + "/" + xmlDiagramsElement ; } }
        public static string allPIMdiagrams { get { return allDiagrams + "/" + xmlAbstractDiagramsElement + "/" + xmlDiagramElement; } }
        public static string allPSMdiagrams { get { return allDiagrams + "/" + xmlSpecificDiagramsElement + "/" + xmlDiagramElement; } }
        public static string allPSMAssociations { get { return ".//"+ xmlPSMAssociation + "[@" + xmlAttID + "]"; } }

        /// <summary>
        /// Returns XPath query describing the PIM class visualization
        /// </summary>
        /// <param name="class_id">ID of the PIM class</param>
        /// <returns></returns>
        public static string GetVisualizationForPIMClass(string class_id)
        {
            return allPIMdiagrams + "/" + xmlClassElement + "[@" + xmlAttRef + "=\"" + class_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing the PSM class visualization
        /// </summary>
        /// <param name="class_id">ID of the PSM class</param>
        /// <returns>XPath query describing the PSM class visualization</returns>
        public static string GetVisualizationForPSMClass(string class_id)
        {
            return allPSMdiagrams + "/" + xmlClassElement + "[@" + xmlAttRef + "=\"" + class_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing visualization for appropriate PIM Association
        /// </summary>
        /// <param name="class_id">ID of PIM Association</param>
        /// <returns>XPath query describing the PIM Association visualization</returns>
        public static string GetVisualizationForPIMAssociation(string association_id)
        {
            return allPIMdiagrams + "/" + xmlAssociationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing visualization for appropriate Association Class
        /// </summary>
        /// <param name="class_id">ID of Association Class</param>
        /// <returns>XPath query describing the Association Class visualization</returns>
        public static string GetVisualizationForClassAssociation(string associationclass_id)
        {
            return allPIMdiagrams + "/" + xmlAssociationClassElement + "[@" + xmlAttRef + "=\"" + associationclass_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing visualization for appropriate PSM Association
        /// </summary>
        /// <param name="class_id">ID of PSM Association</param>
        /// <returns>XPath query describing the PSM Association visualization</returns>
        public static string GetVisualizationForPSMAssociation(string association_id)
        {
            return allPSMdiagrams + "/" + xmlPSMAssociation + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing visualization for appropriate PIM Generalization
        /// </summary>
        /// <param name="class_id">ID of PIM Generalization</param>
        /// <returns>XPath query describing the PIM Generalization visualization</returns>
        public static string GetVisualizationForPIMGeneralization(string association_id)
        {
            return allPIMdiagrams + "/" + xmlGeneralizationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing visualization for appropriate PSM Generalization
        /// </summary>
        /// <param name="class_id">ID of PSM Generalization</param>
        /// <returns>XPath query describing the PSM Generalization visualization</returns>
        public static string GetVisualizationForPSMGeneralization(string association_id)
        {
            return allPSMdiagrams + "/" + xmlGeneralizationElement + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing the PIM Comment visualization
        /// </summary>
        /// <param name="class_id">ID of PIM Comment</param>
        /// <returns>XPath query describing the PIM Comment visualization</returns>
        public static string GetVisualizationForPIMComment(string comment_id)
        {
            return allPIMdiagrams + "/" + xmlCommentElement + "[@" + xmlAttRef + "=\"" + comment_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing PSM Comment visualization
        /// </summary>
        /// <param name="class_id">ID of PSM Comment</param>
        /// <returns>XPath query describing the PSM Comment visualization</returns>
        public static string GetVisualizationForPSMComment(string comment_id)
        {
            return allPSMdiagrams + "/" + xmlCommentElement + "[@" + xmlAttRef + "=\"" + comment_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing PSM Attribute Container visualization
        /// </summary>
        /// <param name="class_id">ID of PSM Attribute Container </param>
        /// <returns>XPath query describing the PSM Attribute Container visualization</returns>
        public static string GetVisualizationForPSMAttContainer(string association_id)
        {
            return allPSMdiagrams + "/" + xmlPSMAttContainer + "[@" + xmlAttRef + "=\"" + association_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing PSM Content Container visualization
        /// </summary>
        /// <param name="class_id">ID of PSM Content Container </param>
        /// <returns>XPath query describing the PSM Content Container visualization</returns>
        public static string GetVisualizationForPSMContentContainer(string container_id)
        {
            return allPSMdiagrams + "/" + xmlPSMContentContainer + "[@" + xmlAttRef + "=\"" + container_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing PSM Content Choice visualization
        /// </summary>
        /// <param name="class_id">ID of PSM Content Choice </param>
        /// <returns>XPath query describing the PSM Content Choice visualization</returns>
        public static string GetVisualizationForPSMContentChoice(string choice_id)
        {
            return allPSMdiagrams + "/" + xmlPSMContentChoice + "[@" + xmlAttRef + "=\"" + choice_id + "\"]";
        }

        /// <summary>
        /// Returns XPath query describing PSM Class Union visualization
        /// </summary>
        /// <param name="class_id">ID of PSM Class Union</param>
        /// <returns>XPath query describing the PSM Class Union visualization</returns>
        public static string GetVisualizationForPSMClassUnion(string union_id)
        {
            return allPSMdiagrams + "/" + xmlPSMClassUnion + "[@" + xmlAttRef + "=\"" + union_id + "\"]";
        }

        #endregion

    };
}