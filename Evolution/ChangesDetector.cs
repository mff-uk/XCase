using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using XCase.Translation;

namespace XCase.Evolution
{
    public class ChangesDetector : DiagramTranslator<ChangesDetectorContext, string>
    {
        public override string Translate(PSMDiagram diagram)
        {
            throw new InvalidOperationException("Use other overloads of Translate. ");
        }

        public List<EvolutionChange> Translate(PSMDiagram diagramOldVersion, PSMDiagram diagramNewVersion)
        {
            ChangesDetectorContext context = new ChangesDetectorContext
                                                 {
                                                     NewVersion = diagramNewVersion.Version,
                                                     OldVersion = diagramOldVersion.Version,
                                                     Diagram = diagramNewVersion
                                                 };

            Diagram = diagramNewVersion;

            context.ScopeStack.Push(EChangeScope.Diagram); 
            ChangesLookupManager.DetectLocalChanges(context);

            foreach (PSMClass rootClass in diagramNewVersion.Roots)
            {
                TranslateClass(rootClass, context);
            }

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.Diagram);

            EvolutionChangeSet set = new EvolutionChangeSet(context.Diagram, context.DetectedChanges, diagramOldVersion.Version, diagramNewVersion.Version);
            set.Verify();
            return context.DetectedChanges;
        }

        #region Classes and class unions

        protected override string TranslateClass(PSMClass psmClass, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.Class);
            context.CurrentPSMElement = psmClass;

            ChangesLookupManager.DetectLocalChanges(context);

            TranslateAttributes(psmClass, context);

            TranslateComponents(psmClass, context);

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.Class);

            return string.Empty;
        }

        private void TranslateClassUnion(PSMClassUnion classUnion, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.ClassUnion);
            context.CurrentPSMElement = classUnion;

            ChangesLookupManager.DetectLocalChanges(context);

            foreach (PSMAssociationChild psmAssociationChild in classUnion.Components)
            {
                TranslateAssociationChild(psmAssociationChild, context);
            }
            
            EChangeScope pcu = context.ScopeStack.Pop();
            Debug.Assert(pcu == EChangeScope.ClassUnion);
        }

        #endregion 

        #region Attributes

        private static void TranslateAttribute(PSMAttribute psmAttribute, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.Attribute);
            context.CurrentPSMElement = psmAttribute;

            ChangesLookupManager.DetectLocalChanges(context);            

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.Attribute);
        }

        #endregion

        #region Associations

        protected override void TranslateAssociation(PSMAssociation association, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.Association);
            context.CurrentPSMElement = association;

            ChangesLookupManager.DetectLocalChanges(context);
            TranslateAssociationChild(association.Child, context);

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.Association);
        }

        #endregion

        #region Subordinate components

        protected override void TranslateContentContainer(PSMContentContainer contentContainer, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.ContentContainer);
            context.CurrentPSMElement = contentContainer;

            ChangesLookupManager.DetectLocalChanges(context);
            TranslateComponents(contentContainer, context);

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.ContentContainer);
        }

        protected override void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.AttributeContainer);
            context.CurrentPSMElement = attributeContainer;

            ChangesLookupManager.DetectLocalChanges(context);
            TranslateAttributes(attributeContainer, context);

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.AttributeContainer);
        }

        protected override void TranslateContentChoice(PSMContentChoice contentChoice, ChangesDetectorContext context)
        {
            context.ScopeStack.Push(EChangeScope.ContentChoice);
            context.CurrentPSMElement = contentChoice;

            ChangesLookupManager.DetectLocalChanges(context);
            TranslateComponents(contentChoice, context);

            EChangeScope pop = context.ScopeStack.Pop();
            Debug.Assert(pop == EChangeScope.ContentChoice);
        }

        #endregion

        #region methods that don't detect local changes

        private static void TranslateAttributes(IHasPSMAttributes hasPsmAttributes, ChangesDetectorContext context)
        {
            foreach (PSMAttribute psmAttribute in hasPsmAttributes.PSMAttributes)
            {
                TranslateAttribute(psmAttribute, context);
            }
        }

        private void TranslateComponents(PSMSuperordinateComponent psmSuperordinateComponent, ChangesDetectorContext context)
        {
            foreach (PSMSubordinateComponent psmSubordinateComponent in psmSuperordinateComponent.Components)
            {
                TranslateSubordinateComponent(psmSubordinateComponent, context);
            }
        }

        protected override void TranslateAssociationChild(PSMAssociationChild associationChild, ChangesDetectorContext context)
        {
            if (associationChild is PSMClass)
            {
                TranslateClass((PSMClass)associationChild, context);
            }
            else
            {
                TranslateClassUnion((PSMClassUnion)associationChild, context);
            }
        }

        #endregion
    }
}