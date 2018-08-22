using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using XCase.Evolution.Xslt;
using XCase.Model;
using XCase.Translation;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public interface IFromStructuralRepresentative
    {
        PSMClass RepresentedPSMClass { get; }
        PSMClass StructuralRepresentative { get; }
    }

    public abstract class NodeElementWrapper
    {
        public abstract bool IsOptional { get; }

        public abstract bool IsOptionalIn(PSMElement node);

        /// <summary>
        /// Returns true if content was not changed
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public abstract bool IsGreen(EvolutionChangeSet changeSet);

        /// <summary>
        /// Returns true if presence of this element wrapper invalidates any choice in which this
        /// wrapper is contained. 
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public abstract bool InvalidatesChoice(EvolutionChangeSet changeSet);

        internal abstract void Inline(ref List<PSMElement> result);

        public abstract void InlineButLeaveSRContent(ref List<NodeElementWrapper> result);

        public abstract void InlineButLeaveChoices(ref List<NodeElementWrapper> result);

        public IFromStructuralRepresentative IFromStructuralRepresentative
        {
            get; set;
        }
    }

    public abstract class NodeAttributeWrapper
    {
        public abstract void Inline(ref List<PSMAttribute> result);

        /// <summary>
        /// Returns true if attribute was not changed
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public abstract bool IsGreen(EvolutionChangeSet changeSet);

        public abstract bool IsOptional { get; }

        /// <summary>
        /// Returns true if presence of this attribute wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public abstract bool InvalidatesChoice(EvolutionChangeSet changeSet);

        public abstract bool IsOptionalIn(PSMElement node);
    }

    public class SimpleNodeAttribute : NodeAttributeWrapper
    {
        public PSMAttribute Attribute { get; private set; }

        public SimpleNodeAttribute(PSMAttribute attribute)
        {
            Attribute = attribute;
            if (attribute.AttributeContainer != null)
                throw new ArgumentException("Attributes in PSM classes expected");
        }

        public override string ToString()
        {
            return String.Format("SNA: {0}", Attribute);
        }

        public override void Inline(ref List<PSMAttribute> result)
        {
            result.Add(Attribute);
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return !changeSet.changesByTarget.ContainsKey(Attribute) ||
                    !changeSet.changesByTarget[Attribute].Any(c => c.InvalidatesAttributes);
        }

        public override bool IsOptional
        {
            get { return Attribute.Lower == 0; }
        }

        /// <summary>
        /// Returns true if presence of this attribute wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return !IsGreen(changeSet) || !Attribute.ExistsInVersion(changeSet.OldVersion) || changeSet.MultiplicityChanged(Attribute);
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            if (IsOptional)
            {
                return true;
            }

            bool pathOptional = false;
            PSMElement comesFrom = Attribute.Class;

            PSMTreeIterator it = new PSMTreeIterator(comesFrom);
            while (it.CurrentNode != node)
            {
                {
                    PSMAssociationChild associationChild = it.CurrentNode as PSMAssociationChild;
                    if (associationChild != null)
                    {
                        if (associationChild.ParentAssociation != null && associationChild.ParentAssociation.Lower == 0)
                        {
                            pathOptional = true;
                            break;
                        }
                    }
                    if (!it.CanGoToParent())
                    {
                        throw new Exception("should never get here");
                    }
                    it.GoToParent();
                }
            }
            return pathOptional;
        }
    }

    public class SimpleNodeElement : NodeElementWrapper
    {
        private PSMElement element;
        public PSMElement Element
        {
            get { return element; }
            private set
            {
                if (!(value is PSMAttribute || value is PSMContentContainer || value is PSMClass))
                {
                    throw new ArgumentException("class, content container or attribute expected");
                }
                if (value is PSMAttribute && ((PSMAttribute)value).AttributeContainer == null)
                {
                    throw new ArgumentException("Attribute in attribute container expected");
                }
                element = value;
            }
        }

        public SimpleNodeElement(PSMElement element)
        {
            Element = element;
        }

        public override string ToString()
        {
            return String.Format("SNC: {0}", Element);
        }


        public override bool IsOptional
        {
            get
            {
                return PSMTreeIterator.GetLowerMultiplicityOfContentElement(Element) == 0;
            }
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            if (IsOptional)
            {
                return true;
            }

            bool pathOptional = false;
            PSMElement comesFrom;
            if (Element is PSMClass || Element is PSMContentContainer)
                comesFrom = Element;
            else if (Element is PSMAttribute)
                comesFrom = ((PSMAttribute)Element).AttributeContainer;
            else
                throw new ArgumentException();

            PSMTreeIterator it = new PSMTreeIterator(comesFrom);
            while (it.CurrentNode != node)
            {
                {
                    PSMAssociationChild associationChild = it.CurrentNode as PSMAssociationChild;
                    if (associationChild != null)
                    {
                        if (associationChild.ParentAssociation != null && associationChild.ParentAssociation.Lower == 0)
                        {
                            pathOptional = true;
                            break;
                        }
                    }
                    if (!it.CanGoToParent())
                    {
                        throw new Exception("should never get here");
                    }
                    it.GoToParent();
                }
            }
            return pathOptional;
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return changeSet.greenNodes.Contains(Element);
        }

        /// <summary>
        /// Returns true if presence of this element wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return !Element.ExistsInVersion(changeSet.OldVersion) || changeSet.MultiplicityChanged(element);
        }

        internal override void Inline(ref List<PSMElement> result)
        {
            result.Add(Element);
        }

        public override void InlineButLeaveSRContent(ref List<NodeElementWrapper> result)
        {
            result.Add(this);
        }

        public override void InlineButLeaveChoices(ref List<NodeElementWrapper> result)
        {
            result.Add(this);
        }
    }

    public class StructuralRepresentativeAttributes : NodeAttributeWrapper, IFromStructuralRepresentative
    {
        public PSMClass RepresentedPSMClass { get; private set; }

        public PSMClass StructuralRepresentative { get; private set; }

        public StructuralRepresentativeAttributes(PSMClass structuralRepresentative, PSMClass representedPsmClass)
        {
            Debug.Assert(structuralRepresentative.RepresentedPSMClass == representedPsmClass);
            StructuralRepresentative = structuralRepresentative;
            RepresentedPSMClass = representedPsmClass;
        }

        public override string ToString()
        {
            return String.Format("SRA: Attributes ref. from {0}", RepresentedPSMClass);
        }

        public override void Inline(ref List<PSMAttribute> result)
        {
            result.AddRange(RepresentedPSMClass.GetAttributesUnderNode().Inline());
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return RepresentedPSMClass.GetAttributesUnderNode().AreAttributesGreen(changeSet);
        }

        public override bool IsOptional
        {
            get
            {
                return StructuralRepresentative.GetAttributesUnderNode().All(i => i.IsOptional)
                  && StructuralRepresentative.GetRepresentedAttributes().All(i => i.IsOptional);
            }
        }

        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return RepresentedPSMClass.GetAttributesUnderNode().Any(a => a.InvalidatesChoice(changeSet));
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return StructuralRepresentative.GetAttributesUnderNode().All(i => i.IsOptionalIn(node))
                  && StructuralRepresentative.GetRepresentedAttributes().All(i => i.IsOptionalIn(node));
        }
    }

    public class StructuralRepresentativeElements : NodeElementWrapper, IFromStructuralRepresentative
    {
        public PSMClass RepresentedPSMClass { get; private set; }

        public PSMClass StructuralRepresentative { get; private set; }

        public StructuralRepresentativeElements(PSMClass structuralRepresentative, PSMClass representedPsmClass)
        {
            Debug.Assert(structuralRepresentative.RepresentedPSMClass == representedPsmClass);
            RepresentedPSMClass = representedPsmClass;
            StructuralRepresentative = structuralRepresentative;
        }

        public override string ToString()
        {
            return String.Format("SRC: Content ref. from {0} in {1}", RepresentedPSMClass, StructuralRepresentative);
        }

        public override bool IsOptional
        {
            get
            {
                return StructuralRepresentative.GetSubtreeElements().All(i => i.IsOptional)
                    && StructuralRepresentative.GetRepresentedElements().All(i => i.IsOptional);
            }
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return StructuralRepresentative.GetSubtreeElements().All(i => i.IsOptionalIn(node))
                    && StructuralRepresentative.GetRepresentedElements().All(i => i.IsOptionalIn(node));
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return RepresentedPSMClass.GetSubtreeElements().AreElementsGreen(changeSet);
        }

        /// <summary>
        /// Returns true if presence of this element wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        /// <returns></returns>
        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return RepresentedPSMClass.GetSubtreeElements().Any(i => i.InvalidatesChoice(changeSet));
        }

        internal override void Inline(ref List<PSMElement> result)
        {
            List<NodeElementWrapper> represented = RepresentedPSMClass.GetSubtreeElements();
            foreach (NodeElementWrapper nodeElementWrapper in represented)
            {
                nodeElementWrapper.IFromStructuralRepresentative = this;
            }
            result.AddRange(represented.Inline());
        }

        public override void InlineButLeaveSRContent(ref List<NodeElementWrapper> result)
        {
            result.Add(this);
        }

        public override void InlineButLeaveChoices(ref List<NodeElementWrapper> result)
        {
            List<NodeElementWrapper> represented = RepresentedPSMClass.GetSubtreeElements();
            foreach (NodeElementWrapper nodeElementWrapper in represented)
            {
                nodeElementWrapper.IFromStructuralRepresentative = this;
            }
            result.AddRange(represented.InlineButLeaveChoices());
        }
    }

    public class ChoiceAttributeOption : NodeAttributeWrapper
    {
        public List<NodeAttributeWrapper> Items { get; set; }
        public override void Inline(ref List<PSMAttribute> result)
        {
            result.AddRange(Items.Inline());
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return Items.All(i => i.IsGreen(changeSet));
        }

        public override bool IsOptional
        {
            get { return Items.Any(i => i.IsOptional); }
        }

        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return Items.Any(i => i.InvalidatesChoice(changeSet));
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return Items.Any(i => i.IsOptionalIn(node));
        }
    }

    public class ChoiceElementOption : NodeElementWrapper
    {
        public List<NodeElementWrapper> Items { get; set; }

        public override bool IsOptional
        {
            get { return Items.Any(i => i.IsOptional); }
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return Items.Any(i => i.IsOptionalIn(node));
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return Items.All(i => i.IsGreen(changeSet));
        }

        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return Items.Any(i => i.InvalidatesChoice(changeSet));
        }

        internal override void Inline(ref List<PSMElement> result)
        {
            result.AddRange(Items.Inline());
        }

        public override void InlineButLeaveSRContent(ref List<NodeElementWrapper> result)
        {
            result.AddRange(Items.InlineButLeaveSRContent());
        }

        public override void InlineButLeaveChoices(ref List<NodeElementWrapper> result)
        {
            result.AddRange(Items.InlineButLeaveChoices());
        }

        public PSMElement GetFirstNonOptional(Version oldVersion)
        {
            PSMElement firstNonOptional;
            if (this.Items.Count == 1)
                firstNonOptional = this.Items.Inline().First();
            else
                firstNonOptional = this.Items.Inline().Where(e => e.ExistsInVersion(oldVersion)).Where(e => PSMTreeIterator.GetLowerMultiplicityOfContentElement((PSMElement)e.GetInVersion(oldVersion)) > 0).FirstOrDefault();
            return firstNonOptional;
        }
    }

    public class ChoiceAttributes : ChoiceAttributesBase<PSMContentChoice>
    {
        public PSMContentChoice ContentChoice
        {
            get
            {
                return Node;
            }
        }

        public ChoiceAttributes(PSMContentChoice node, IEnumerable<ChoiceAttributeOption> options)
            : base(node, options)
        {
        }

        public override IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version)
        {
            return ((PSMContentChoice)ContentChoice.GetInVersion(version)).Components.Cast<PSMElement>();
        }
    }

    public interface IChoiceAttributesBase
    {
        IEnumerable<ChoiceAttributeOption> Options { get; }
        IEnumerable<NodeAttributeWrapper> OptionsExpanded { get; }
    }

    public interface IChoiceElementsBase
    {
        IEnumerable<ChoiceElementOption> Options { get; }
        IEnumerable<NodeElementWrapper> OptionsExpanded { get; }
    }

    public abstract class ChoiceElementsBase<NODETYPE> : NodeElementWrapper, IChoiceElementsBase 
        where NODETYPE : PSMElement
    {
        public NODETYPE Node { get; private set; }

        public abstract IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version);

        public IEnumerable<ChoiceElementOption> Options { get; private set; }

        public IEnumerable<NodeElementWrapper> OptionsExpanded
        {
            get
            {
                List<NodeElementWrapper> result = new List<NodeElementWrapper>();
                foreach (ChoiceElementOption choiceElementOption in Options)
                {
                    result.AddRange(choiceElementOption.Items);
                }
                return result;
            }
        }

        private IEnumerable<NodeElementWrapper> _options
        {
            get
            {
                return Options.Cast<NodeElementWrapper>();
            }
        }

        protected ChoiceElementsBase(NODETYPE node, IEnumerable<ChoiceElementOption> options)
        {
            Node = node;
            Options = options;
        }

        public override string ToString()
        {
            return String.Format("CCh: {0}", Node);
        }

        public override bool IsOptional
        {
            get { return Options.Any(i => i.IsOptional); }
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return Options.Any(i => i.IsOptionalIn(node));
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return Options.All(c => c.Items.AreElementsGreen(changeSet));
        }

        /// <summary>
        /// Returns true if presence of this element wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        /// <returns></returns>
        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return Options.Any(c => c.InvalidatesChoice(changeSet));
        }

        internal override void Inline(ref List<PSMElement> result)
        {
            result.AddRange(_options.Inline());
        }

        public override void InlineButLeaveSRContent(ref List<NodeElementWrapper> result)
        {
            result.AddRange(_options.InlineButLeaveSRContent());
        }

        public override void InlineButLeaveChoices(ref List<NodeElementWrapper> result)
        {
            result.Add(this);
        }

        public ChoiceElementOption SuggestChoiceForGenerating()
        {
            return Options.FirstOrDefault(e => !e.IsOptionalIn(this.Node));
        }

        /// <summary>
        /// <para>
        /// Returns true if choice was modified in the new version in such a
        /// way that the full choice template must be created for it
        /// </para>
        /// <para>
        /// ContentChoice is invalidated if one of the components changes it's multiplicity or
        /// any component is removed.
        /// </para>
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        /// <returns></returns>
        public bool Invalidated(EvolutionChangeSet changeSet)
        {
            if (!this.Node.ExistsInVersion(changeSet.OldVersion))
                return true;
            if (this.Options.Any(e => e.InvalidatesChoice(changeSet)))
                return true;
            // if any choice was removed, choice is invalidated
            NODETYPE oldVersion = (NODETYPE)this.Node.GetInVersion(changeSet.OldVersion);
            // ReSharper disable CompareNonConstrainedGenericWithNull
            Debug.Assert(oldVersion != null);
            // ReSharper restore CompareNonConstrainedGenericWithNull
            if (GetNodeComponetsInVersion(changeSet.OldVersion).Any(c => !c.ExistsInVersion(changeSet.NewVersion)))
                return true;

            // otherwise not invalidated 
            return false;
        }

        public bool MayRequireGenerating(EvolutionChangeSet changeSet)
        {
            // whole new choice added
            if (!this.Node.ExistsInVersion(changeSet.OldVersion))
                return true;
            // any of the choices was removed
            if (this.GetNodeComponetsInVersion(changeSet.OldVersion).Any(c => !c.ExistsInVersion(changeSet.NewVersion)))
                return true;

            foreach (PSMElement element in _options.Inline())
            {
                if (element.ExistsInVersion(changeSet.OldVersion) && changeSet.MultiplicityChanged(element))
                {
                    if (changeSet.GetMultiplicityChange(element).CanRequireGenerating())
                        return true;
                }

                IEnumerable<PSMAssociation> associations = PSMTreeIterator.GetAssociationsBetweenNodes(Node, element);
                foreach (PSMAssociation association in associations)
                {
                    if (association.Lower == 0)
                        break;
                    if (changeSet.MultiplicityChanged(association) &&
                        changeSet.GetMultiplicityChange(association).CanRequireGenerating())
                        return true;
                }
            }

            return false;
        }
    }


    public class ChoiceElements : ChoiceElementsBase<PSMContentChoice>
    {
        public PSMContentChoice ContentChoice
        {
            get
            {
                return Node;
            }
        }

        public ChoiceElements(PSMContentChoice node, IEnumerable<ChoiceElementOption> options)
            : base(node, options)
        {
        }

        public override IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version)
        {
            return ((PSMContentChoice) ContentChoice.GetInVersion(version)).Components.Cast<PSMElement>();
        }
    }

    public enum EUnionComplexity
    {
        /// <summary>
        /// Union with leading association having upper multiplicity == 1
        /// </summary>
        SingleChoice,
        MultipleWithoutGroups,
        MultipleWithGroups
    }
    
    public class UnionElements : ChoiceElementsBase<PSMClassUnion>
    {
        public PSMClassUnion ClassUnion
        {
            get
            {
                return Node;
            }
        }

        public override IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version)
        {
            return ((PSMClassUnion)ClassUnion.GetInVersion(version)).Components.Cast<PSMElement>();
        }

        public UnionElements(PSMClassUnion node, IEnumerable<ChoiceElementOption> options)
            : base(node, options)
        {
        }

        public XPathExpr PopulationExpression(XsltGeneratorContext context)
        {
            
            List<XPathExpr> result = new List<XPathExpr>();
            
            PopulationExpression(this, ref result, context);
            result.RemoveAll(e => e.IsEmpty());
            string expr = result.ConcatWithSeparator(XPathExpr.PIPE_OPERATOR);
            return new XPathExpr(expr);
        }

        private static void PopulationExpression(NodeElementWrapper nodeElementWrapper, ref List<XPathExpr> result, XsltGeneratorContext context)
        {
            if (nodeElementWrapper is IChoiceElementsBase)
            {
                foreach (ChoiceElementOption option in ((IChoiceElementsBase)nodeElementWrapper).Options)
                {
                    IEnumerable<NodeElementWrapper> inlinedOptions = option.Items.InlineButLeaveChoices();
                    foreach (NodeElementWrapper elementWrapper in inlinedOptions)
                    {
                        List<XPathExpr> subChoice = new List<XPathExpr>();
                        PopulationExpression(elementWrapper, ref subChoice, context);
                        string expr = subChoice.ConcatWithSeparator(XPathExpr.PIPE_OPERATOR);
                        result.Add(new XPathExpr(inlinedOptions.Count() > 1 ? "({0})": "{0}", expr));
                    }
                }
            }
            else if (nodeElementWrapper is SimpleNodeElement)
            {
                if (((SimpleNodeElement)nodeElementWrapper).Element.ExistsInVersion(context.ChangeSet.OldVersion))
                {
                    if (nodeElementWrapper.IFromStructuralRepresentative != null)
                    {
                        XPathExpr e = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, ((SimpleNodeElement)nodeElementWrapper).Element, nodeElementWrapper.IFromStructuralRepresentative);
                        result.Add(e);
                    }
                    else
                    {
                        XPathExpr e = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, ((SimpleNodeElement)nodeElementWrapper).Element, context.CurrentStructuralRepresentative);
                        result.Add(e);
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public XPathExpr DistributionExpression(XsltGeneratorContext context, XmlDocument xsltDocument, TranslationLog log)
        {
            if (Complexity != EUnionComplexity.MultipleWithGroups)
            {
                return this.PopulationExpression(context);
            }
            else
            {
                List<XPathExpr> result = new List<XPathExpr>();
                foreach (ChoiceElementOption option in Options)
                {
                    Debug.Assert(option.Items.Count == 1);
                    NodeElementWrapper optionItem = option.Items.First();
                    if (optionItem is SimpleNodeElement)
                    {
                        XPathExpr e = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, ((SimpleNodeElement)optionItem).Element, context.CurrentStructuralRepresentative);
                        result.Add(e);
                    }
                    else if (optionItem is ContentGroup)
                    {
                        XPathExpr e = new XPathExpr(((ContentGroup) optionItem).CreateGroupingAttribute(context, xsltDocument, log).Value);
                        result.Add(e);
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                string expr = result.ConcatWithSeparator(XPathExpr.PIPE_OPERATOR);
                return new XPathExpr(expr);
            }
        }

        public EUnionComplexity Complexity
        {
            get
            {
                if (ClassUnion.ParentUnion != null || ClassUnion.ParentAssociation == null || ClassUnion.ParentAssociation.Upper == 1)
                    return EUnionComplexity.SingleChoice;       
                else
                {
                    //TODO: inline
                    if (this.OptionsExpanded.All(o => o is SimpleNodeElement))
                        return EUnionComplexity.MultipleWithoutGroups;
                    else
                        return EUnionComplexity.MultipleWithGroups;
                }
            }
        }
    }

    public abstract class ChoiceAttributesBase<NODETYPE> : NodeAttributeWrapper, IChoiceAttributesBase 
        where NODETYPE : PSMElement
    {
        public NODETYPE Node { get; private set; }

        public abstract IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version);

        public IEnumerable<ChoiceAttributeOption> Options { get; private set; }

        public IEnumerable<NodeAttributeWrapper> OptionsExpanded
        {
            get
            {
                List<NodeAttributeWrapper> result = new List<NodeAttributeWrapper>();
                foreach (ChoiceAttributeOption choiceElementOption in Options)
                {
                    result.AddRange(choiceElementOption.Items);
                }
                return result;
            }
        }

        private IEnumerable<NodeAttributeWrapper> _options
        {
            get
            {
                return Options.Cast<NodeAttributeWrapper>();
            }
        }
    
        protected ChoiceAttributesBase(NODETYPE node, IEnumerable<ChoiceAttributeOption> options)
        {
            Node = node;
            Options = options;
        }

        public override string ToString()
        {
            return String.Format("CCh: {0}", Node);
        }

        public override bool IsOptional
        {
            get { return Options.Any(i => i.IsOptional); }
        }

        /// <summary>
        /// Returns true if presence of this element wrapper invalidates any choice in which this
        /// wrapper is contained.
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        /// <returns></returns>
        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return Options.Any(a => a.InvalidatesChoice(changeSet));
        }

        public override void Inline(ref List<PSMAttribute> result)
        {
            result.AddRange(_options.Inline());
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return Options.All(c => c.Items.AreAttributesGreen(changeSet));
        }


        public ChoiceAttributeOption SuggestChoiceForGenerating()
        {
            return Options.FirstOrDefault(e => !e.IsOptional);
        }

        /// <summary>
        /// <para>
        /// Returns true if choice was modified in the new version in such a
        /// way that the full choice template must be created for it
        /// </para>
        /// <para>
        /// ContentChoice is invalidated if one of the attributes changes it's multiplicity or
        /// any attribute is removed.
        /// </para>
        /// </summary>
        /// <param name="changeSet">Collection of changes between two version.</param>
        /// <returns></returns>
        public bool Invalidated(EvolutionChangeSet changeSet)
        {
            if (!this.Node.ExistsInVersion(changeSet.OldVersion))
                return true;
            if (this.Options.Any(e => e.InvalidatesChoice(changeSet)))
                return true;
            // if any choice was removed, choice is invalidated
            NODETYPE oldVersion = (NODETYPE)Node.GetInVersion(changeSet.OldVersion);
            // ReSharper disable CompareNonConstrainedGenericWithNull
            Debug.Assert(oldVersion != null);
            // ReSharper restore CompareNonConstrainedGenericWithNull
            if (GetNodeComponetsInVersion(changeSet.OldVersion).Any(c => !c.ExistsInVersion(changeSet.NewVersion)))
                return true;

            // otherwise not invalidated 
            return false;
        }

        public virtual bool MayRequireGenerating(EvolutionChangeSet changeSet)
        {
            // whole new choice added
            if (!this.Node.ExistsInVersion(changeSet.OldVersion))
                return true;
            // any of the choices was removed
            if (GetNodeComponetsInVersion(changeSet.OldVersion).Any(c => !c.ExistsInVersion(changeSet.NewVersion)))
                return true;

            foreach (PSMAttribute attribute in _options.Inline())
            {
                if (attribute.ExistsInVersion(changeSet.OldVersion) && changeSet.MultiplicityChanged(attribute))
                {
                    if (changeSet.GetMultiplicityChange(attribute).CanRequireGenerating())
                        return true;
                }

                IEnumerable<PSMAssociation> associations = PSMTreeIterator.GetAssociationsBetweenNodes(Node, attribute);
                foreach (PSMAssociation association in associations)
                {
                    if (association.Lower == 0)
                        break;
                    if (changeSet.MultiplicityChanged(association) &&
                        changeSet.GetMultiplicityChange(association).CanRequireGenerating())
                        return true;
                }
            }
            return false;
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return Options.Any(i => i.IsOptionalIn(node));
        }
    }

    public class UnionAttributes: ChoiceAttributesBase<PSMClassUnion>
    {
        public PSMClassUnion ClassUnion
        {
            get
            {
                return Node;
            }
        }

        public override IEnumerable<PSMElement> GetNodeComponetsInVersion(Version version)
        {
            return ((PSMClassUnion)ClassUnion.GetInVersion(version)).Components.Cast<PSMElement>();
        }

        public override bool MayRequireGenerating(EvolutionChangeSet changeSet)
        {
            return base.MayRequireGenerating(changeSet) ||
                (!this.IsOptional && changeSet.MultiplicityChanged(ClassUnion) && changeSet.GetMultiplicityChange(ClassUnion).CanRequireGenerating());
                
        }

        public UnionAttributes(PSMClassUnion node, IEnumerable<ChoiceAttributeOption> options)
            : base(node, options)
        {
        }

        public EUnionComplexity Complexity
        {
            get
            {
                if (ClassUnion.ParentUnion != null || ClassUnion.ParentAssociation == null || ClassUnion.ParentAssociation.Upper == 1)
                    return EUnionComplexity.SingleChoice;
                else
                {
                    //TODO: inline
                    if (this.OptionsExpanded.All(o => o is SimpleNodeAttribute))
                        return EUnionComplexity.MultipleWithoutGroups;
                    else
                        return EUnionComplexity.MultipleWithGroups;
                }
            }
        }
    }

    public class ContentGroup : NodeElementWrapper
    {
        public List<NodeElementWrapper> ContentComponents { get; private set; }

        private List<PSMAttribute> groupAttributes;

        public PSMClass ContainingClass { get; set; }

        public List<PSMAttribute> GetGroupAttributes(Version oldVersion)
        {
            if (groupAttributes == null)
            {
                groupAttributes = ContainingClass.GetAttributesForGroup(oldVersion);
            }
            return groupAttributes;
        }

        public ContentGroup()
        {
            ContentComponents = new List<NodeElementWrapper>();
        }

        public bool HasElementLabel
        {
            get
            {
                return ContainingClass.HasElementLabel;
            }
        }

        public bool HadElementLabel(Version oldVersion)
        {
            PSMClass classOldVersion = (PSMClass)ContainingClass.GetInVersion(oldVersion);
            return classOldVersion != null && classOldVersion.HasElementLabel;
        }

        public string GetOldElementLabel(Version oldVersion)
        {
            PSMClass classOldVersion = (PSMClass)ContainingClass.GetInVersion(oldVersion);
            if (classOldVersion != null && classOldVersion.HasElementLabel)
                return classOldVersion.ElementName;
            else
                return null;
        }

        public override string ToString()
        {
            return String.Format("CG: Group {0}", ContainingClass);
        }

        public override bool IsOptional
        {
            get
            {
                return ContainingClass.GetSubtreeElements().All(i => i.IsOptional)
                    && ContainingClass.GetRepresentedElements().All(i => i.IsOptional);
            }
        }

        public override bool IsOptionalIn(PSMElement node)
        {
            return ContainingClass.GetSubtreeElements().All(i => i.IsOptionalIn(node))
                && ContainingClass.GetRepresentedElements().All(i => i.IsOptionalIn(node));
        }

        public override bool IsGreen(EvolutionChangeSet changeSet)
        {
            return ContentComponents.AreElementsGreen(changeSet);
        }

        public override bool InvalidatesChoice(EvolutionChangeSet changeSet)
        {
            return
                changeSet.MultiplicityChanged(this.ContainingClass) ||
                (ContentComponents).Any(c => c.InvalidatesChoice(changeSet));
        }

        internal override void Inline(ref List<PSMElement> result)
        {
            if (this.HasElementLabel)
                result.Add(ContainingClass);
            else
                result.AddRange(ContentComponents.Inline());
        }

        public override void InlineButLeaveSRContent(ref List<NodeElementWrapper> result)
        {
            if (this.HasElementLabel)
                result.Add(new SimpleNodeElement(ContainingClass));
            else
                result.AddRange(ContentComponents.InlineButLeaveSRContent());
        }

        public override void InlineButLeaveChoices(ref List<NodeElementWrapper> result)
        {
            if (this.HasElementLabel)
                result.Add(new SimpleNodeElement(ContainingClass));
            else
                result.AddRange(ContentComponents.InlineButLeaveChoices());
        }

        public XPathExpr CreateAttributesForGroupExpr(XsltGeneratorContext context)
        {
            string result = GetGroupAttributes(context.ChangeSet.OldVersion).ConcatWithSeparator(
                attribute => XPathHelper.GroupAwareProjectXPath(context, attribute, context.ChangeSet.OldVersion).ToString(), XPathExpr.PIPE_OPERATOR);
            return new XPathExpr(result);
        }

        public XmlAttribute CreateGroupingAttribute(XsltGeneratorContext context, XmlDocument xsltDocument, TranslationLog log)
        {
            PSMClass containingClassOldVersion = (PSMClass)this.ContainingClass.GetInVersion(context.ChangeSet.OldVersion);

            List<NodeElementWrapper> oldElements = new List<NodeElementWrapper>();
            if (containingClassOldVersion != null)
            {
                if (containingClassOldVersion.HasElementLabel)
                {
                    oldElements.Add(new SimpleNodeElement(containingClassOldVersion));
                }
                else
                {
                    oldElements.AddRange(containingClassOldVersion.GetRepresentedElements().InlineButLeaveChoices());
                    oldElements.AddRange(containingClassOldVersion.GetSubtreeElements().InlineButLeaveChoices());
                    if (oldElements.Count == 0)
                    {
                        log.AddError("Unable to create group distributing attribute for {0}.", this.ContainingClass);
                        return xsltDocument.XslGroupStartingWithAttribute(XPathExpr.INVALID_PATH_EXPRESSION, false);
                    }
                }
            }
            Debug.Assert(oldElements.All(e => e is SimpleNodeElement || e is ChoiceElements));
            bool isMultiple;
            XPathExpr value;
            if (oldElements.First() is SimpleNodeElement)
            {
                isMultiple = PSMTreeIterator.GetUpperMultiplicityOfContentElement(((SimpleNodeElement)oldElements.First()).Element) != 1 ||
                             PSMTreeIterator.GetLowerMultiplicityOfContentElement(((SimpleNodeElement)oldElements.First()).Element) != 1;
                value = new XPathExpr(XsltTemplateNameManager.GetElementNameForSignificantElement(((SimpleNodeElement)oldElements.First()).Element));
            }
            else if (oldElements.First() is ChoiceElements)
            {
                isMultiple = false;
                ChoiceElements choice = ((ChoiceElements)oldElements.First());
                if (choice.IsOptional)
                    log.AddWarning("Group distributing attribute for {0} may be incorrect if choice {1} is omitted in the document.", this.ContainingClass, choice);
                value = new XPathExpr(choice.OptionsExpanded.Inline().ConcatWithSeparator(XsltTemplateNameManager.GetElementNameForSignificantElement, XPathExpr.PIPE_OPERATOR));
            }
            else
            {
                throw new ArgumentException();
            }


            return xsltDocument.XslGroupStartingWithAttribute(value, isMultiple);
            // TODO: unikatnost jmena neosetrena

            //if (NameSuggestor<PSMElement>.IsNameUnique(
            //    oldElements,
            //    XsltTemplateNameManager.GetElementNameForSignificantElement(firstOldElement),
            //    XsltTemplateNameManager.GetElementNameForSignificantElement,
            //    firstOldElement
            //    ))
            //if (result == null)
            //{
            //    Log.AddError("Unable to create group distributing attribute for {0}.",
            //                               contentGroup.ContainingClass);
            //    result = xsltDocument.XslGroupStartingWithAttribute(new XPathExpr("###"), false);
            //}

        }

        public XPathExpr CountGroupsExpression(XsltGeneratorContext context, XmlDocument xsltDocument, TranslationLog log)
        {
            Debug.Assert(context.CurrentContentGroup == this);
            if (!this.HadElementLabel(context.ChangeSet.OldVersion))
            {
                XPathExpr groupPath = new XPathExpr(context.ContentGroupPath + "/" + this.CreateGroupingAttribute(context, xsltDocument, log).Value);
                XPathExpr getExistingNodes = XPathHelper.GroupAwareProjectXPathWithoutAttributeCorrection(context, groupPath);
                return getExistingNodes;
            }
            else
            {
                XPathExpr getExistingNodes = XPathHelper.GroupAwareProjectXPath(context, this.ContainingClass, context.ChangeSet.OldVersion);
                return getExistingNodes;
            }
        }

        public XPathExpr CreateGroupMembersSelectExpression(NodeElementWrapper content, XsltGeneratorContext context, List<PSMAttribute> groupAttributes)
        {
            string orOperator = context.InGroup ? XPathExpr.OR_OPERATOR : XPathExpr.PIPE_OPERATOR;
            if (content is ContentGroup && ((ContentGroup)content).HadElementLabel(context.ChangeSet.OldVersion))
            {
                return new XPathExpr(((ContentGroup)content).GetOldElementLabel(context.ChangeSet.OldVersion));
            }

            if (content is ContentGroup || content is StructuralRepresentativeElements || content is ChoiceElements || content is ChoiceElementOption)
            {
                ContentGroup contentGroup = content as ContentGroup;
                ChoiceElements contentChoice = content as ChoiceElements;
                ChoiceElementOption choiceOption = content as ChoiceElementOption;
                StructuralRepresentativeElements structuralRepresentativeElements = content as StructuralRepresentativeElements;
                IEnumerable<NodeElementWrapper> collection;
                if (contentGroup != null)
                {
                    collection = contentGroup.ContentComponents;
                }
                else if (structuralRepresentativeElements != null)
                {
                    collection = structuralRepresentativeElements.RepresentedPSMClass.GetSubtreeElements();
                }
                else if (contentChoice != null)
                {
                    collection = contentChoice.OptionsExpanded;
                }
                else if (choiceOption != null)
                {
                    collection = choiceOption.Items;
                }
                else
                {
                    throw new ArgumentException();
                }

                return TTT(context, structuralRepresentativeElements, collection, groupAttributes, orOperator);
            }

            throw new ArgumentException("content");
        }

        private XPathExpr TTT(XsltGeneratorContext context, StructuralRepresentativeElements structuralRepresentativeElements, IEnumerable<NodeElementWrapper> collection, List<PSMAttribute> groupAttributes, string orOperator)
        {
            List<string> steps = new List<string>();
            foreach (NodeElementWrapper nodeContent in collection)
            {
                string step;
                if (nodeContent is ContentGroup || nodeContent is StructuralRepresentativeElements)
                {
                    step = CreateGroupMembersSelectExpression(nodeContent, context, groupAttributes);
                }
                else if (nodeContent is ChoiceElements)
                {
                    List<XPathExpr> choices = new List<XPathExpr>();
                    foreach (ChoiceElementOption option in ((ChoiceElements)nodeContent).Options)
                    {
                        XPathExpr optionExpr = CreateGroupMembersSelectExpression(option, context, groupAttributes);
                        choices.Add(optionExpr);
                    }
                    step = string.Format("({0})", choices.ConcatWithSeparator(" | "));
                }
                else
                {
                    if (((SimpleNodeElement)nodeContent).Element.GetInVersion(context.ChangeSet.OldVersion) == null)
                        continue;
                    if (groupAttributes != null && ((SimpleNodeElement)nodeContent).Element is PSMAttribute &&
                        groupAttributes.Contains((PSMAttribute)((SimpleNodeElement)nodeContent).Element))
                    {
                        continue;
                    }
                    if (structuralRepresentativeElements == null)
                        step = XPathHelper.GroupAwareProjectXPath(context, ((SimpleNodeElement)nodeContent).Element, context.ChangeSet.OldVersion);
                    else
                        step = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, ((SimpleNodeElement)nodeContent).Element, structuralRepresentativeElements);
                    
                }

                if (step.StartsWith("$cg[name() = '") && step.EndsWith("']"))
                {
                    step = step.Substring("$cg[name() = '".Length, step.Length - "$cg[name() = '".Length - "']".Length);
                }
                steps.Add(context.InGroup ? string.Format("name() = '{0}'", step) : step);
            }

            string result = steps.ConcatWithSeparator(orOperator);

            if (!string.IsNullOrEmpty(result))
            {
                if (context.InGroup)
                    return new XPathExpr("$cg[{0}]", result);
                else
                    return new XPathExpr(result);
            }
            else
            {
                if (context.InGroup)
                    return new XPathExpr(XPathExpr.CurrentGroupVariableExpr);
                else
                    return new XPathExpr("*");
            }
        }
    }

    public static class NodeContentWrapperCollectionExt
    {
        public static IEnumerable<PSMElement> Inline(this IEnumerable<NodeElementWrapper> nodeContents)
        {
            List<PSMElement> result = new List<PSMElement>();
            foreach (NodeElementWrapper nodeContent in nodeContents)
            {
                nodeContent.Inline(ref result);
            }
            return result;
        }

        public static IEnumerable<NodeElementWrapper> InlineButLeaveChoices(this IEnumerable<NodeElementWrapper> nodeContents)
        {
            List<NodeElementWrapper> result = new List<NodeElementWrapper>();
            foreach (NodeElementWrapper nodeContent in nodeContents)
            {
                nodeContent.InlineButLeaveChoices(ref result);
            }
            return result;
        }

        public static IEnumerable<NodeElementWrapper> InlineButLeaveSRContent(this IEnumerable<NodeElementWrapper> nodeContents)
        {
            List<NodeElementWrapper> result = new List<NodeElementWrapper>();
            foreach (NodeElementWrapper nodeContent in nodeContents)
            {
                nodeContent.InlineButLeaveSRContent(ref result);
            }
            return result;
        }

        public static IEnumerable<PSMAttribute> Inline(this IEnumerable<NodeAttributeWrapper> attributes)
        {
            List<PSMAttribute> result = new List<PSMAttribute>();
            foreach (NodeAttributeWrapper nodeAttributesWrapper in attributes)
            {
                nodeAttributesWrapper.Inline(ref result);
            }
            return result;
        }

        public static bool AreElementsGreen(this IEnumerable<NodeElementWrapper> contents, EvolutionChangeSet changeSet)
        {
            return contents.All(c => c.IsGreen(changeSet));
        }

        public static bool AreAttributesGreen(this IEnumerable<NodeAttributeWrapper> attributes, EvolutionChangeSet changeSet)
        {
            return attributes.All(a => a.IsGreen(changeSet));
        }
    }
}