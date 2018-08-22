using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class XsltGeneratorContext
    {
        #region constructor

        /// <summary>
        /// For context outside group
        /// </summary>
        public XsltGeneratorContext(EvolutionChangeSet changeSet, PSMElement bodyNode)
            : this(changeSet, bodyNode, false, null)
        {
        }

        /// <summary>
        /// For context inside group 
        /// </summary>
        public XsltGeneratorContext(EvolutionChangeSet changeSet, PSMElement bodyNode, ContentGroup currentContentGroup)
            : this(changeSet, bodyNode, true, currentContentGroup)
        {
        }

        /// <summary>
        /// For context inside group 
        /// </summary>
        public XsltGeneratorContext(EvolutionChangeSet changeSet, PSMElement bodyNode, bool inGroup, ContentGroup currentContentGroup)
        {
            BodyNode = bodyNode;
            InGroup = inGroup;
            ChangeSet = changeSet;
            CurrentContentGroup = currentContentGroup;
            if (inGroup && currentContentGroup == null)
                throw new ArgumentException();
            if (!inGroup && currentContentGroup != null)
                throw new ArgumentException();
        }

        #endregion
        
        internal PSMElement BodyNode { get; private set; }
        internal XmlElement BodyElement { get; set; }
        internal Modifiers Modifiers { get; set; }
        internal PSMElement CurrentContentNode { get; set; }
        internal XmlElement Template { get; set; }
        internal EvolutionChangeSet ChangeSet { get; private set; }
        internal XPathExpr ProcessedPath { get; set; }
        internal PSMAttribute CurrentAttribute { get; set; }
        internal ContentGroup CurrentContentGroup { get; set; }
        internal bool InGroup { get; set; }
        internal List<PSMAttribute> ContentGroupAttributes { get; set; }
        internal StructuralRepresentativeElements CurrentStructuralRepresentativeElements { get; set; }
        internal StructuralRepresentativeAttributes CurrentStructuralRepresentativeAttributes { get; set; }
        internal ChoiceElements CurrentChoiceElements { get; set; }
        internal UnionElements CurrentUnionElements { get; set; }
        internal UnionAttributes CurrentUnionAttributes { get; set; }
        internal ChoiceAttributes CurrentChoiceAttributes { get; set; }

        internal XsltGeneratorContext CreateCopy()
        {
            XsltGeneratorContext newContext = new XsltGeneratorContext(this.ChangeSet, this.BodyNode)
                                            {
                                                BodyElement = this.BodyElement,
                                                Modifiers = this.Modifiers,
                                                CurrentContentNode = this.CurrentContentNode,
                                                Template = this.Template,
                                                ProcessedPath = this.ProcessedPath,
                                                CurrentAttribute = this.CurrentAttribute,
                                                CurrentContentGroup = this.CurrentContentGroup,
                                                InGroup = this.InGroup,
                                                ContentGroupAttributes = this.ContentGroupAttributes,
                                                CurrentStructuralRepresentativeElements = this.CurrentStructuralRepresentativeElements,
                                                CurrentStructuralRepresentativeAttributes = this.CurrentStructuralRepresentativeAttributes,
                                                CurrentChoiceElements = this.CurrentChoiceElements,
                                                CurrentChoiceAttributes = this.CurrentChoiceAttributes,
                                                CurrentUnionElements = this.CurrentUnionElements,
                                                CurrentUnionAttributes = this.CurrentUnionAttributes
                                            };
            return newContext;
        }

        internal XsltGeneratorContext CreateCopy(Modifiers addedModifiers)
        {
            XsltGeneratorContext newContext = CreateCopy();
            newContext.Modifiers |= addedModifiers;
            return newContext;
        }

        internal XsltGeneratorContext CreateCopy(PSMElement newBodyNode)
        {
            XsltGeneratorContext newContext = CreateCopy();
            newContext.BodyNode = newBodyNode;
            return newContext;
        }

        internal XsltGeneratorContext CreateCopy(XmlElement newBodyElement)
        {
            XsltGeneratorContext newContext = CreateCopy();
            newContext.BodyElement = newBodyElement;
            return newContext;
        }

        internal XsltGeneratorContext CreateCopy(XmlElement newBodyElement, Modifiers addedModifiers)
        {
            XsltGeneratorContext newContext = CreateCopy();
            newContext.Modifiers |= addedModifiers;
            newContext.BodyElement = newBodyElement;
            return newContext;
        }

        internal EContentPlacementState CurrentContentNodeState
        {
            get
            {
                return ChangeSet.GetState(CurrentContentNode);
            }
        }

        internal EContentPlacementState CurrentAtrtibuteState
        {
            get
            {
                return ChangeSet.GetState(CurrentAttribute);
            }
        }

        internal EContentPlacementState CurrentUnionState
        {
            get
            {
                return ChangeSet.GetState(CurrentUnionElements.ClassUnion);
            }
        }
        
        internal EContentPlacementState CurrentContentGroupState()
        {
            return ChangeSet.GetGroupState(CurrentContentGroup);
        }

        internal EContentPlacementState BodyNodeState
        {
            get
            {
                return ChangeSet.GetState(BodyNode);
            }
        }

        internal XPathExpr ContentGroupPath// { get ;set; }
        {
            get
            {
                return XPathHelper.GetXPathForContentGroup(CurrentContentGroup, ChangeSet.OldVersion);
            }
        }

        internal bool GroupWithAttributes
        {
            get
            {
                return CurrentContentGroup != null && CurrentContentGroup.HasElementLabel;
            }
        }

        internal IFromStructuralRepresentative CurrentStructuralRepresentative
        {
            get
            {
                if (CurrentStructuralRepresentativeElements != null)
                {
                    return CurrentStructuralRepresentativeElements;
                }
                else
                    return CurrentStructuralRepresentativeAttributes;
            }
        }

        internal XPathExpr BodyNodeToProcessedPath()
        {
            return NodeToProcessedPath(BodyNode);
        }

        internal XPathExpr NodeToProcessedPath(PSMElement node)
        {
            Version oldVersion = ChangeSet.OldVersion;

            if (!InGroup)
            {
                return XPathHelper.GetXPathForNode(node, oldVersion);
            }
            else
            {
                XPathExpr path = XPathHelper.GetXPathForNode(node, oldVersion).Append("/$cg");
                //if (InGroup/* && ProcessedPath.HasPrefix(ContentGroupPath)*/)
                {
                    Debug.Assert(path.HasPrefix(ContentGroupPath));
                    Debug.Assert(path == (!ContentGroupPath.ToString().EndsWith("/$cg") ? ContentGroupPath.Append("/$cg") : ContentGroupPath));
                    //path = path.InsertAfterPrefix(ContentGroupPath, "/$cg");
                }
                return path;
            }
        }
    }
}