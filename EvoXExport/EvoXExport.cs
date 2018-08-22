using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Model.Serialization;
using NUml.Uml2;
using Type = System.Type;

namespace XCase.Model.EvoXExport
{
    public class EvoXExport
    {
        private EvoX.Model.ProjectVersion projectVersion;
        private PIMSchema pimSchema;
        private Project xcaseProject;
        private EvoX.Model.Project evoxProject;

        public EvoX.Model.Project ConvertToEvoXProject(Project project)
        {
            this.xcaseProject = project; 
            translatedElements = new Dictionary<Element, EvoX.Model.EvoXObject>();
            associationOrder = new Dictionary<EvoX.Model.PSM.PSMAssociation, int>();

            if (project.VersionManager != null)
            {
                return ConvertToVersionedEvoXProject(project);
            }
            else
            {
                return ConvertToNonversionedEvoXProject(project);
            }
        }

        private EvoX.Model.Project ConvertToNonversionedEvoXProject(Project project)
        {
            evoxProject = new EvoX.Model.Project();
            evoxProject.InitNewEmptyProject(true);

            projectVersion = evoxProject.SingleVersion;
            pimSchema = projectVersion.PIMSchema;

            ConvertPIMSchema();

            foreach (PSMDiagram psmDiagram in project.PSMDiagrams)
            {
                psmSchema = new PSMSchema(evoxProject);
                PSMSchemaClass psmSchemaClass = new PSMSchemaClass(evoxProject, psmSchema);
                psmSchemaClass.Name = psmDiagram.Caption;
                psmSchema.RegisterPSMSchemaClass(psmSchemaClass);
                projectVersion.AddPSMSchema(psmSchema);
                ConvertPSMSchema(psmDiagram);
            }

            return evoxProject;
        }

        public EvoX.Model.Project ConvertToEvoXProject(string fileName)
        {
            XmlDeserializatorBase deserializator;
            if (XmlDeserializatorVersions.UsesVersions(fileName))
            {
                deserializator = new XmlDeserializatorVersions();
            }
            else
            {
                deserializator = new XmlDeserializator();
            }

            // First, validates if the file is a valid XCase XML file
            // TODO: it would be better to have two separate schemas rather than one choice schema 
            string msg =  null;
            if (!deserializator.ValidateXML(fileName, ref msg))
            {
                throw new XCaseException("File cannot be opened. Not a valid XCase XML file");
            }

            // version check
            string v1;
            string v2;
            if (!XmlDeserializatorBase.VersionsEqual(fileName, out v1, out v2))
            {
                throw new XCaseException("Project is obsolete. Project is obsolete and must be converted to a new version before opening. \r\nDo you want to convert it now? ");
            }
            Project deserializedProject = deserializator.RestoreProject(fileName);

            EvoX.Model.Project convertedProject = ConvertToEvoXProject(deserializedProject);
            return convertedProject;
        }

        #region PSM conversion
        
        private void ConvertPSMSchema(PSMDiagram xcasePSMDiagram)
        {
            this.xcasePSMDiagram = xcasePSMDiagram;
            Type[] psmTypes = new[] { typeof(PSMClass), typeof(PSMContentContainer), typeof(PSMClassUnion), typeof(PSMAttributeContainer), typeof(PSMContentChoice),
                typeof(Association), typeof(Generalization), typeof(Comment) };
            
            foreach (KeyValuePair<Element, ViewHelper> diagramElement in xcasePSMDiagram.DiagramElements.OrderBy(
                e => Array.IndexOf(psmTypes, e.GetType())))
            {
                ConvertPSMElement((PSMElement) diagramElement.Key, diagramElement.Value);
            }

            SetStructuralRepresentatives();
            CreateComponentAssociations();
            ReorderAssociations();
        }

        private void CreateComponentAssociations()
        {
            foreach (PSMElement psmElement in xcasePSMDiagram.DiagramElements.Keys)
            {
                if (psmElement is PSMAssociationChild)
                {
                    if (((PSMAssociationChild)psmElement).ParentUnion != null)
                    {
                        CreateLeadingAssociationForUnionComponents((PSMAssociationChild)psmElement, (PSMAssociationMember)translatedElements[psmElement]);
                    }
                }
                else if (psmElement is PSMSubordinateComponent && !(psmElement is PSMAssociation))
                {
                    if (((PSMSubordinateComponent)psmElement).Parent != null)
                    {
                        CreateLeadingAssociation((PSMSubordinateComponent) psmElement, (PSMAssociationMember) translatedElements[psmElement]);
                    }   
                }
            }
        }

        private void ReorderAssociations()
        {
            foreach (EvoX.Model.PSM.PSMAssociation psmAssociation in psmSchema.PSMAssociations)
            {
                if (psmAssociation.Parent.ChildPSMAssociations.IndexOf(psmAssociation) != associationOrder[psmAssociation])
                {
                    psmAssociation.Parent.RemoveAssociation(psmAssociation);
                    psmAssociation.Parent.InsertAssociation(psmAssociation, associationOrder[psmAssociation]);
                }
            }
        }

        private void SetStructuralRepresentatives()
        {
            foreach (PSMClass psmClass in xcasePSMDiagram.DiagramElements.Keys.OfType<PSMClass>())
            {
                if (psmClass.IsStructuralRepresentative)
                {
                    ((EvoX.Model.PSM.PSMClass)translatedElements[psmClass]).RepresentedClass = (EvoX.Model.PSM.PSMClass)translatedElements[psmClass.RepresentedPSMClass];
                }
            }   
        }

        private void ConvertPSMElement(PSMElement element, ViewHelper viewHelper)
        {
            if (element is PSMClass)
            {
                ConvertPSMClass((PSMClass)element, (PSMElementViewHelper)viewHelper);
            }
            else if (element is PSMAssociation)
            {
                ConvertPSMAssociation((PSMAssociation)element, (PSMAssociationViewHelper)viewHelper);
            }
            else if (element is PSMContentContainer)
            {
                ConvertPSMContentContainer((PSMContentContainer)element, (PSMElementViewHelper)viewHelper);
            }
            else if (element is PSMContentChoice)
            {
                ConvertPSMContentChoice((PSMContentChoice)element, (PSMElementViewHelper)viewHelper);
            }
            else if (element is PSMClassUnion)
            {
                ConvertPSMClassUnion((PSMClassUnion)element, (PSMElementViewHelper)viewHelper);
            }
            else if (element is PSMAttributeContainer)
            {
                ConvertPSMAttributeContainer((PSMAttributeContainer)element, (PSMElementViewHelper)viewHelper);
            }
            else if (element is Generalization)
            {

            }
            else
            {
                throw new NotImplementedException(string.Format("Member EvoXExport.ConvertPSMElement not implemented for type {0}.", element.GetType().Name));
            }
        }

        private void ConvertPSMAttributeContainer(PSMAttributeContainer psmAttributeContainer, PSMElementViewHelper psmElementViewHelper)
        {
            if (psmAttributeContainer.PSMAttributes.Count > 0)
            {
                EvoX.Model.PSM.PSMClass evoxPSMClass;
                if (!TranslatedAlready(psmAttributeContainer, out evoxPSMClass))
                {
                    evoxPSMClass = new EvoX.Model.PSM.PSMClass(evoxProject, psmSchema, false);
                    translatedElements[psmAttributeContainer] = evoxPSMClass;
                }

                evoxPSMClass.Name = psmAttributeContainer.PSMClass.Name;

                if (psmAttributeContainer.PSMClass.RepresentedClass != null)
                {
                    evoxPSMClass.Interpretation = (EvoX.Model.PIM.PIMClass)translatedElements[psmAttributeContainer.PSMClass.RepresentedClass];
                }

                foreach (PSMAttribute attribute in psmAttributeContainer.PSMAttributes)
                {
                    ConvertPSMAttribute(evoxPSMClass, attribute, true);
                }
            }
        }

        private EvoX.Model.PSM.PSMAssociation CreateLeadingAssociation(PSMSubordinateComponent subordinate, PSMAssociationMember childAssociationMember)
        {
            if (!(subordinate.Parent is PSMAssociation))
            {
                PSMAssociationMember parent = (PSMAssociationMember)translatedElements[subordinate.Parent];
                EvoX.Model.PSM.PSMAssociation psmAssociation = new EvoX.Model.PSM.PSMAssociation(
                    evoxProject, parent, childAssociationMember, psmSchema);
                psmAssociation.Name = null;
                associationOrder[psmAssociation] = subordinate.ComponentIndex(); 
                return psmAssociation;
            }

            else return null;
        }

        private void ConvertPSMClassUnion(PSMClassUnion psmClassUnion, PSMElementViewHelper psmElementViewHelper)
        {
            PSMContentModel contentModel;
            if (!TranslatedAlready(psmClassUnion, out contentModel))
            {
                contentModel = new PSMContentModel(evoxProject, psmSchema, false);
                translatedElements[psmClassUnion] = contentModel;
            }

            contentModel.Type = PSMContentModelType.Choice;
        }

        private void ConvertPSMContentChoice(PSMContentChoice psmContentChoice, PSMElementViewHelper psmElementViewHelper)
        {
            PSMContentModel contentModel;
            if (!TranslatedAlready(psmContentChoice, out contentModel))
            {
                contentModel = new PSMContentModel(evoxProject, psmSchema, false);
                translatedElements[psmContentChoice] = contentModel;
            }

            contentModel.Type = PSMContentModelType.Choice;
        }

        private void ConvertPSMContentContainer(PSMContentContainer psmContentContainer, PSMElementViewHelper psmElementViewHelper)
        {
            PSMContentModel contentModel;
            if (!TranslatedAlready(psmContentContainer, out contentModel))
            {
                contentModel = new PSMContentModel(evoxProject, psmSchema, false);
                translatedElements[psmContentContainer] = contentModel;
            }

            contentModel.Type = PSMContentModelType.Sequence;
        }

        private void ConvertPSMClass(PSMClass psmClass, PSMElementViewHelper psmElementViewHelper)
        {
            EvoX.Model.PSM.PSMClass evoxPSMClass;
            if (!TranslatedAlready(psmClass, out evoxPSMClass))
            {
                evoxPSMClass = new EvoX.Model.PSM.PSMClass(evoxProject, psmSchema, false);
                translatedElements[psmClass] = evoxPSMClass;
            }

            evoxPSMClass.Name = psmClass.Name;

            if (xcasePSMDiagram.Roots.Contains(psmClass))
            {
                if (psmClass.HasElementLabel)
                {
                    EvoX.Model.PSM.PSMAssociation rootAssociation = new EvoX.Model.PSM.PSMAssociation(
                        evoxProject, psmSchema.PSMSchemaClass, evoxPSMClass, psmSchema);
                    rootAssociation.Name = psmClass.ElementName;
                    associationOrder[rootAssociation] = xcasePSMDiagram.Roots.Where(r => r is PSMClass && ((PSMClass)r).HasElementLabel).ToList().IndexOf(psmClass);
                }
                else
                {
                    psmSchema.RegisterPSMRoot(evoxPSMClass);        
                }
            }

            if (psmClass.RepresentedClass != null)
            {
                evoxPSMClass.Interpretation = (EvoX.Model.PIM.PIMClass)translatedElements[psmClass.RepresentedClass];
            }

            foreach (PSMAttribute attribute in psmClass.PSMAttributes)
            {
                ConvertPSMAttribute(evoxPSMClass, attribute, false);
            }
        }

        private EvoX.Model.PSM.PSMAssociation CreateLeadingAssociationForUnionComponents(PSMAssociationChild associationChild, PSMAssociationMember childAssociationMember)
        {
            PSMAssociationMember parent = (PSMAssociationMember) translatedElements[associationChild.ParentUnion];
            EvoX.Model.PSM.PSMAssociation psmAssociation = new EvoX.Model.PSM.PSMAssociation(
                evoxProject, parent, childAssociationMember, psmSchema);
            PSMClass psmClass = associationChild as PSMClass;
            if (psmClass != null && psmClass.HasElementLabel)
            {
                psmAssociation.Name = psmClass.ElementName;
            }
            else 
            {
                psmAssociation.Name = null;
            }
            associationOrder[psmAssociation] = associationChild.ComponentIndex();
            return psmAssociation;
        }

        private void ConvertPSMAssociation(PSMAssociation psmAssociation, PSMAssociationViewHelper psmAssociationViewHelper)
        {
            EvoX.Model.PSM.PSMAssociation evoxPSMAssociation;
            if (!TranslatedAlready(psmAssociation, out evoxPSMAssociation))
            {
                PSMAssociationMember parent = (PSMAssociationMember)ElementRef(psmAssociation.Parent);
                PSMAssociationMember child = (PSMAssociationMember)ElementRef(psmAssociation.Child);
                evoxPSMAssociation = new EvoX.Model.PSM.PSMAssociation(evoxProject, parent, child, psmSchema);
                associationOrder[evoxPSMAssociation] = psmAssociation.ComponentIndex();
                translatedElements[psmAssociation] = evoxPSMAssociation;
            }

            PSMClass childPSMClass = psmAssociation.Child as PSMClass;
            if (childPSMClass != null && childPSMClass.HasElementLabel)
            {
                evoxPSMAssociation.Name = childPSMClass.ElementName;
            }
            else
            {
                evoxPSMAssociation.Name = null;
            }
            PSMClassUnion psmClassUnion = psmAssociation.Child as PSMClassUnion;
            if (psmClassUnion != null)
            {
                evoxPSMAssociation.Name = null;
            }

            evoxPSMAssociation.Lower = ConvertToUint(psmAssociation.Lower);
            evoxPSMAssociation.Upper = ConvertToUnlimitedInt(psmAssociation.Upper);
        }

        private void ConvertPSMAttribute(EvoX.Model.PSM.PSMClass evoxPsmClass, PSMAttribute attribute, bool element)
        {
            EvoX.Model.PSM.PSMAttribute psmAttribute = new EvoX.Model.PSM.PSMAttribute(evoxProject, evoxPsmClass, psmSchema);
            translatedElements[attribute] = psmAttribute;
            psmAttribute.Name = attribute.Name;
            psmAttribute.Lower = ConvertToUint(attribute.Lower);
            psmAttribute.Upper = ConvertToUnlimitedInt(attribute.Upper);
            if (attribute.Type != null)
            {
                psmAttribute.AttributeType = (EvoX.Model.AttributeType)ElementRef(attribute.Type);
            }
            psmAttribute.Element = element;

            if (attribute.RepresentedAttribute != null)
            {
                psmAttribute.Interpretation = (PIMAttribute)translatedElements[attribute.RepresentedAttribute];
            }
        }

        #endregion 

        #region PIM conversion

        private void ConvertPIMSchema()
        {
            Type[] pimTypes = new[] { typeof(PIMClass), typeof(AssociationClass), typeof(Association), typeof(Generalization), typeof(Comment)};
            foreach (PIMDiagram pimDiagram in xcaseProject.PIMDiagrams)
            {
                foreach (KeyValuePair<Element, ViewHelper> diagramElement in pimDiagram.DiagramElements.OrderBy(
                    e => Array.IndexOf(pimTypes, e.GetType())))
                {
                    ConvertPIMElement(diagramElement.Key, diagramElement.Value);
                }
            }
        }

        private void ConvertPIMElement(Element element, ViewHelper viewHelper)
        {
            if (element is AssociationClass)
            {
                throw new NotImplementedException(
                    string.Format("Member EvoXExport.ConvertPIMElement not implemented for type {0}.",
                                  element.GetType().Name));
            }
            else if (element is PIMClass)
            {
                ConvertPIMClass((PIMClass) element, (ClassViewHelper) viewHelper);
            }
            else if (element is Association)
            {
                ConvertPIMAssociation((Association) element, (AssociationViewHelper) viewHelper);
            }
            else if (element is Comment)
            {
                ConvertPIMComment((Comment) element, (CommentViewHelper) viewHelper);
            }
            else if (element is Generalization)
            {
                ConvertPIMGeneralization((Generalization) element, (GeneralizationViewHelper) viewHelper);
            }
            else
            {
                throw new NotImplementedException(
                    string.Format("Member EvoXExport.ConvertPIMElement not implemented for type {0}.",
                                  element.GetType().Name));
            }
        }

        private void ConvertPIMGeneralization(Generalization generalization, GeneralizationViewHelper generalizationViewHelper)
        {
            // do nothing
        }

        private void ConvertPIMComment(Comment pimComment, CommentViewHelper commentViewHelper)
        {
            // do nothing
        }

        private void ConvertPIMAssociation(Association pimAssociation, AssociationViewHelper associationViewHelper)
        {
            PIMAssociation association;
            if (!TranslatedAlready(pimAssociation, out association))
            {
                association = new PIMAssociation(evoxProject, pimSchema);
                translatedElements[pimAssociation] = association;
            }
            
            association.Name = pimAssociation.Name;
            foreach (AssociationEnd pimAssociationEnd in pimAssociation.Ends)
            {
                PIMAssociationEnd associationEnd = new PIMAssociationEnd(evoxProject, (EvoX.Model.PIM.PIMClass) ElementRef(pimAssociationEnd.Class), association, pimSchema);
                associationEnd.Lower = ConvertToUint(pimAssociationEnd.Lower);
                associationEnd.Upper = ConvertToUnlimitedInt(pimAssociationEnd.Upper);
                associationEnd.Name = pimAssociationEnd.Name;
            }
        }

        private void ConvertPIMClass(PIMClass pimClass, ClassViewHelper classViewHelper)
        {
            EvoX.Model.PIM.PIMClass evoxPIMClass;
            if (!TranslatedAlready(pimClass, out evoxPIMClass))
            {
                evoxPIMClass = new EvoX.Model.PIM.PIMClass(evoxProject, pimSchema);
                translatedElements[pimClass] = evoxPIMClass;
            }

            evoxPIMClass.Name = pimClass.Name;
            foreach (Property attribute in pimClass.Attributes)
            {
                ConvertPIMAttribute(evoxPIMClass, attribute);
            }
        }

        private void ConvertPIMAttribute(EvoX.Model.PIM.PIMClass evoxPIMClass, Property attribute)
        {
            PIMAttribute pimAttribute = new PIMAttribute(evoxProject, evoxPIMClass, pimSchema);
            translatedElements[attribute] = pimAttribute;
            pimAttribute.Name = attribute.Name;
            pimAttribute.Lower = ConvertToUint(attribute.Lower);
            pimAttribute.Upper = ConvertToUnlimitedInt(attribute.Upper);
            if (attribute.Type != null)
            {
                pimAttribute.AttributeType = (EvoX.Model.AttributeType)ElementRef(attribute.Type);    
            }
        }

        #endregion

        #region common conversion

        private bool TranslatedAlready<T>(Element element, out T translation)
            where T: EvoX.Model.EvoXObject 
        {
            if (translatedElements.ContainsKey(element))
            {
                translation = (T) translatedElements[element];
                return true;
            }
            else
            {
                translation = null;
                return false; 
            }
        }

        private Dictionary<Element, EvoX.Model.EvoXObject> translatedElements;
        private Dictionary<EvoX.Model.PSM.PSMAssociation, int> associationOrder;
        private PSMSchema psmSchema;
        private PSMDiagram xcasePSMDiagram; 

        private EvoX.Model.EvoXObject ElementRef(Element element)
        {
            if (translatedElements.ContainsKey(element))
            {
                return translatedElements[element];
            }
            else
                return null;
        }

        private static EvoX.Model.UnlimitedInt ConvertToUnlimitedInt(UnlimitedNatural upper)
        {
            if (upper.IsInfinity)
            {
                return EvoX.Model.UnlimitedInt.Infinity;
            }
            else
            {
                return upper.Value;
            }
        }

        private static uint ConvertToUint(uint? lower)
        {
            return lower.HasValue ? lower.Value : 1;
        }


        private EvoX.Model.Project ConvertToVersionedEvoXProject(Project project)
        {
            evoxProject = new EvoX.Model.Project();
            evoxProject.InitNewEmptyProject(true);



            return evoxProject;
        }

        #endregion

        public void SaveAsEvoxProject(Project project, string filePath)
        {
            EvoX.Model.Project evoXProject = ConvertToEvoXProject(project);
            EvoX.Model.Serialization.ProjectSerializationManager serializationManager = new ProjectSerializationManager();
            serializationManager.SaveProject(evoxProject, filePath);
        }
    }
}