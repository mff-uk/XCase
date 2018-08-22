#define SAXON_XSLT
#define REGENERATE_SUPPORTING
#define SIMPLE_ITERATIONS
#define SEPARATE_SUPPORTING_TEMPLATES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using XCase.Evolution.Xslt;
using XCase.Model;
using System.Linq;
using Version = XCase.Model.Version;
using System.Xml;

namespace XCase.Evolution
{
    public class XsltTemplateGenerator
    {
        #region initialization 

        /// <summary>
        /// Gets or sets the evolveddiagram.
        /// </summary>
        public PSMDiagram Diagram { get; set; }

        /// <summary>
        /// Gets or sets the old version of <see cref="Diagram"/>.
        /// </summary>
        /// <value></value>
        public Version OldVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the new version.
        /// </summary>
        /// <value>type: <see cref="Version"/></value>        
        public Version NewVersion { get; set; }

        /// <summary>
        /// Gets or sets the change set between <see cref="OldVersion"/> and <see cref="NewVersion"/>.
        /// </summary>
        /// <value>type: <see cref="EvolutionChangeSet"/></value>
        public EvolutionChangeSet ChangeSet { get; set; }

        /// <summary>
        /// Generates values for new attributes and leaf nodes
        /// </summary>
        public AttributeValueGenerator AttributeValueGenerator { get; private set; }

        /// <summary>
        /// Gets or sets the template manager.
        /// Template manager is used to create names for named templates and 
        /// remember which templates were already generated. 
        /// </summary>
        /// <value>type: <see cref="XsltTemplateNameManager"/></value>
        public XsltTemplateNameManager NameManager { get; private set; }

        public Translation.TranslationLog Log { get; private set; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltTemplateGenerator"/> class.
        /// </summary>
        /// <param name="diagram">The diagram.</param>
        public XsltTemplateGenerator(PSMDiagram diagram)
        {
            Diagram = diagram;
            Log = new Translation.TranslationLog();   
        }

        #endregion 

        /// <summary>
        /// Generates the XSLT stylesheet for evolution from <paramref name="oldVersion"/>
        /// to <paramref name="newVersion"/>. 
        /// </summary>
        /// <param name="changes">set of changes between the versions</param>
        /// <param name="oldVersion">old version of the diagram</param>
        /// <param name="newVersion">bew version of the diagram</param>
        /// <returns>XSLT stylesheet (as text) </returns>
        public string Generate(IEnumerable<EvolutionChange> changes, Version oldVersion, Version newVersion)
        {
            Debug.Assert(changes.All(c => c.NewVersion == newVersion && c.OldVersion == oldVersion));
            
            OldVersion = oldVersion;
            NewVersion = newVersion;
            ChangeSet = new EvolutionChangeSet(Diagram, changes, OldVersion, NewVersion);
            AttributeValueGenerator = new AttributeValueGenerator();
            PrepareXsltWriter();
            Log.Clear();
            NameManager = new XsltTemplateNameManager(xslStylesheetNode);
            ChangeSet.Categorize();
            string redNodesStr = "Red nodes: ";
            foreach (PSMElement redNode in ChangeSet.redNodes)
            {
                redNodesStr += redNode.Name + " ";
            }
            xslStylesheetNode.CreateComment(redNodesStr);

            string BlueNodesStr = "Blue nodes: ";
            foreach (PSMElement blueNode in ChangeSet.blueNodes)
            {
                BlueNodesStr += blueNode.Name + " ";
            }
            xslStylesheetNode.CreateComment(BlueNodesStr);

            Queue<PSMElement> nodesToDo = new Queue<PSMElement>(ChangeSet.redNodes);
            List<KeyValuePair<PSMElement, bool>> nodesDone = new List<KeyValuePair<PSMElement, bool>>();
            while (!nodesToDo.IsEmpty())
            {
                PSMElement nodeToDo = nodesToDo.Dequeue();
                XsltGeneratorContext context;
                
                // Decide between node template and group template
                if (ChangeSet.IsContentGroupNode(nodeToDo))
                {
                    ContentGroup nodeAsContentGroup = ((PSMClass) nodeToDo).GetNodeAsContentGroup();
                    Debug.Assert(nodeToDo == nodeAsContentGroup.ContainingClass);
                    context = new XsltGeneratorContext(ChangeSet, nodeToDo, nodeAsContentGroup);
                    // group template need not to be generated if only force callable template is needed
                    if (!NameManager.RequestedTemplates.Contains(new KeyValuePair<PSMElement, bool>(nodeToDo, false))
                        //&& !nodesDone.Contains(new KeyValuePair<PSMElement, bool>(nodeToDo, false)))
                        )
                        continue;
                    nodesDone.Add(new KeyValuePair<PSMElement, bool>(nodeToDo, false));
                    context.ProcessedPath = context.BodyNodeToProcessedPath();
                    GenerateContentGroupTemplate(context); 
                }
                else
                {
                    PSMElement groupElement;
                    context = new XsltGeneratorContext(ChangeSet, nodeToDo, ChangeSet.IsUnderContentGroup(nodeToDo, out groupElement), groupElement != null ? ((PSMClass)groupElement).GetNodeAsContentGroup() : null);
                    // node template need not to be generated if only force callable template is needed
                    if (context.BodyNodeState == EContentPlacementState.Added && 
                        !NameManager.RequestedTemplates.Contains(new KeyValuePair<PSMElement, bool>(nodeToDo, false)))
                        continue;
                    //if (nodesDone.Contains(new KeyValuePair<PSMElement, bool>(nodeToDo, context.BodyNodeState == EContentPlacementState.Added)))
                    //    continue;
                    nodesDone.Add(new KeyValuePair<PSMElement, bool>(nodeToDo, context.BodyNodeState == EContentPlacementState.Added));
                    GenerateRedNodeTemplate(context);
                }

                foreach (KeyValuePair<PSMElement, bool> requestedTemplate in NameManager.RequestedTemplates)
                {
                    if (!nodesDone.Contains(requestedTemplate) && !nodesDone.Contains(new KeyValuePair<PSMElement, bool>(requestedTemplate.Key, !requestedTemplate.Value)) &&
                        !nodesToDo.Contains(requestedTemplate.Key) 
                        && !ChangeSet.greenNodes.Contains(requestedTemplate.Key)
                        && !ChangeSet.blueNodes.Contains(requestedTemplate.Key))    
                        nodesToDo.Enqueue(requestedTemplate.Key);
                }
            }

            // generate force callable templates for new structural representatives
            foreach (PSMClass newStructuralRepresentative in ChangeSet.FindNewStructuralRepresentatives())
            {
                if (!NameManager.ForceCallableSRTemplateExists(newStructuralRepresentative.RepresentedPSMClass))
                {
                    GenerateForceCallableSRTemplates(newStructuralRepresentative.RepresentedPSMClass);
                }
            }


            GenerateBlueNodesTemplate();
            GenerateGreenNodesTemplate();
            AddOtherTemplates();
            
            FinalizeXsltWriter();

            //return _xsltWriterSb.ToString();
            return xsltDocument.PrettyPrintXML();
        }

        #region templates

        /// <summary>
        /// Generates the red node template, chooses between applied template and named template.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>template name for named templates, match pattern for applied templates</returns>
        private void GenerateRedNodeTemplate(XsltGeneratorContext context)
        {
            if (context.BodyNodeState.OneOf(EContentPlacementState.AsItWas, EContentPlacementState.Moved)
                && !context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!NameManager.AppliedTemplates.ContainsKey(context.BodyNode))
                    GenerateAppliedTemplate(context);
            }
            else
            {
                GenerateCallableTemplate(context);
            }
        }

        /// <summary>
        /// Generates the callable template for a red node (<paramref name="context"/>.<see cref="XsltGeneratorContext.BodyNode"/>)
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void GenerateCallableTemplate(XsltGeneratorContext context)
        {
            string templateName = NameManager.GetNodeCallableTemplate(context.BodyNode, context.Modifiers.Is(Modifiers.ForceCallable));
            context.Template = context.InGroup ? xslStylesheetNode.XslNamedTemplate(templateName, XsltDocumentExt.ParamCurrentGroup) : xslStylesheetNode.XslNamedTemplate(templateName);
            context.ProcessedPath = context.BodyNodeToProcessedPath();
            NameManager.NamedTemplates[context.BodyNode] = context.Template;
            NameManager.RedNodeTemplates[context.BodyNode] = context.Template;
            GenerateRedNodeTemplateContent(context);
        }

        /// <summary>
        /// Generates the applied template.
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void GenerateAppliedTemplate(XsltGeneratorContext context)
        {
            Debug.Assert(ChangeSet.redNodes.Contains(context.BodyNode) && context.BodyNodeState.OneOf(EContentPlacementState.AsItWas, EContentPlacementState.Moved));
            context.ProcessedPath = context.BodyNodeToProcessedPath();
            IEnumerable<XPathExpr> pathsWhereElementAppears = XPathHelper.PathsWhereElementAppears((PSMElement)context.BodyNode.GetInVersion(OldVersion));
            XPathExpr fullMatch = XPathExpr.ConcatWithOrOperator(pathsWhereElementAppears);
            context.Template = context.InGroup ? xslStylesheetNode.XslTemplate(fullMatch, XsltDocumentExt.ParamCurrentGroup) : xslStylesheetNode.XslTemplate(fullMatch);
            NameManager.AppliedTemplates[context.BodyNode] = context.Template;
            NameManager.RedNodeTemplates[context.BodyNode] = context.Template;
            GenerateRedNodeTemplateContent(context);
        }

        /// <summary>
        /// Generates the callable template for factored out elements 
        /// and attributes templates of a represented 
        /// PSM class.
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void GenerateRepresentedContentCallableTemplate(XsltGeneratorContext context)
        {
            RepresentedClassCallableTemplateInfo info = new RepresentedClassCallableTemplateInfo();
            IEnumerable<NodeAttributeWrapper> attributes = context.BodyNode.GetAttributesUnderNode();
            List<NodeAttributeWrapper> representedAttributes = context.BodyNode.GetRepresentedAttributes();
            XmlElement template = context.Template;
            XmlElement bodyElement = context.BodyElement;
            if ((attributes.Count() > 0 || representedAttributes.Count > 0)
                && !context.Modifiers.Is(Modifiers.ForceGroupAwareSRContent))
            {
                // attributes template
                if (RepresentedClassCallableTemplateInfo.FullAttributeTemplateNeeded(context.BodyNode, ChangeSet)
                    || context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    string templateName = NameManager.GetRepresentedContentTemplate((PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes, context.Modifiers.Is(Modifiers.ForceCallable));
                    XmlElement attrTemplate = (context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable)) ? 
                        xslStylesheetNode.XslNamedTemplate(templateName, XsltDocumentExt.ParamCurrentGroup) : xslStylesheetNode.XslNamedTemplate(templateName);
                    context.Template = attrTemplate;
                    context.BodyElement = attrTemplate;
                    DecideRepresentedAttributes(context);
                    ProcessNodeAttributes(context, attributes);
                    info.FullAttributesTemplate = true;
                    info.IsGroupAware = context.InGroup;
                    if (context.Modifiers.Is(Modifiers.ForceCallable))
                        NameManager.RegisterForceCallableSRTemplate((PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes, templateName);
                }
                else
                {
                    info.CopyAttributesTemplate = true;
                    info.IsGroupAware = context.InGroup;
                }
            }

            context.BodyElement = bodyElement;
            context.Template = template;

            List<NodeElementWrapper> nodeElements = context.BodyNode.GetSubtreeElements();
            List<NodeElementWrapper> representedNodeElements = context.BodyNode.GetRepresentedElements();
            if (nodeElements.Count() > 0 || representedNodeElements.Count > 0)
            {
                // elements template
                if (RepresentedClassCallableTemplateInfo.FullElementsTemplateNeeded(context.BodyNode, ChangeSet)
                    || context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    string templateName = NameManager.GetRepresentedContentTemplate(
                        (PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Elements, context.Modifiers.Is(Modifiers.ForceCallable));
                    if (context.Modifiers.Is(Modifiers.ForceGroupAwareSRContent))
                        templateName += "-GA";
                    XmlElement elmTemplate = (context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable)) ? 
                        xslStylesheetNode.XslNamedTemplate(templateName, XsltDocumentExt.ParamCurrentGroup) : xslStylesheetNode.XslNamedTemplate(templateName);
                    context.Template = elmTemplate;
                    context.BodyElement = elmTemplate;
                    DecideRepresentedElements(context);
                    ProcessNodeElements(context, nodeElements);
                    info.FullElementsTemplate = true;
                    info.IsGroupAware = context.InGroup;
                    if (context.Modifiers.Is(Modifiers.ForceCallable))
                        NameManager.RegisterForceCallableSRTemplate((PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Elements, templateName);
                }
                else
                {
                    info.CopyElementsTemplate = true;
                    info.IsGroupAware = context.InGroup;
                }
            }
            NameManager.RepresentedClassesInfo[(PSMClass)context.BodyNode] = info;
            
        }

        #region generating template body

        /// <summary>
        /// Creates template body (for node applied and callable template and also for content group template)
        /// </summary>
        /// <param name="context"></param>
        private void GenerateRedNodeTemplateContent(XsltGeneratorContext context)
        {
            if (context.BodyNode.ModelsElement())
            {
                string xmlElementName = XsltTemplateNameManager.GetElementNameForSignificantElement(context.BodyNode);
                context.BodyElement = context.Template.CreateElement(xmlElementName);
            }
            else
            {
                context.BodyElement = context.Template;
            }

            //factor out content for classes referenced from SR
            if (context.BodyNode is PSMClass && ((PSMClass)context.BodyNode).IsReferencedFromStructuralRepresentative())
            {
                XmlElement template = context.Template;
                XmlElement element = context.BodyElement;
                // choose between force callable or ordinary template
                if (context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    if (!NameManager.ForceCallableSRTemplateExists((PSMClass)context.BodyNode))
                        GenerateForceCallableSRTemplates((PSMClass)context.BodyNode);
                }
                else
                    GenerateRepresentedContentCallableTemplate(context);
                context.Template = template;
                context.BodyElement = element;
                GenerateFactoredOutContentReference(context, NameManager.RepresentedClassesInfo[(PSMClass)context.BodyNode]);
            }
            else
            {
                // for structural representatives reference represented PSM class' attributes
                if (context.BodyNode is PSMClass
                    && ChangeSet.FindNewStructuralRepresentatives().Contains((PSMClass)context.BodyNode))
                {
                    DecideRepresentedAttributes(context.CreateCopy(Modifiers.ForceCallable));
                }
                else 
                {
                    DecideRepresentedAttributes(context);
                }

                // generate attribute template if neccessary
                DecideAttributes(context);

                // for structural representatives reference represented PSM class' content
                if (context.BodyNode is PSMClass
                    && ChangeSet.FindNewStructuralRepresentatives().Contains((PSMClass)context.BodyNode))
                {
                    DecideRepresentedElements(context.CreateCopy(Modifiers.ForceCallable));
                }
                else
                {
                    DecideRepresentedElements(context);
                }

                // generate content template if neccessary 
                DecideElements(context);
            }
        }

        /// <summary>
        /// Using information from <see cref="ChangeSet"/> decides, whether 
        /// code processing attributes of current node must be created or 
        /// whether the attributes can be simple copied (they were not invalidated
        /// in the new version). Calls either <see cref="ProcessNodeAttributes"/> or
        /// <see cref="ProcessNotInvalidatedAttributes"/>. 
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void DecideAttributes(XsltGeneratorContext context)
        {
            if (!context.InGroup || context.GroupWithAttributes)
            {
                IEnumerable<NodeAttributeWrapper> attributes = context.BodyNode.GetAttributesUnderNode();
                XsltGeneratorContext newContext = context.GroupWithAttributes ? context.CreateCopy(Modifiers.ForceCallable) : context;

                if (ChangeSet.AttributesInvalidated(newContext.BodyNode) || context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    ProcessNodeAttributes(newContext, attributes);
                }
                else // attributes not invalidated
                {
                    if (attributes.Count() > 0)
                    {
                        ProcessNotInvalidatedAttributes(newContext, attributes, false, false);
                    }
                }
            }
        }

        /// <summary>
        /// Using information from <see cref="ChangeSet"/> decides, whether 
        /// code processing represented attributes of current node must be created or 
        /// whether the attributes can be simple copied (they were not invalidated
        /// in the new version). If represented attributes must be processed, also
        /// generates callable structural representative template (if it was not 
        /// created already for other structural representative). 
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void DecideRepresentedAttributes(XsltGeneratorContext context)
        {
            if (!context.InGroup || context.GroupWithAttributes)
            {
                List<NodeAttributeWrapper> attributes = context.BodyNode.GetRepresentedAttributes();
                if (attributes.Count() > 0)
                {
                    PSMClass representedClass = ((PSMClass) context.BodyNode).RepresentedPSMClass;
                    if (RepresentedClassCallableTemplateInfo.FullAttributeTemplateNeeded(representedClass, ChangeSet)
                        || context.Modifiers.Is(Modifiers.ForceCallable))
                    {
                        Debug.Assert(ChangeSet.RepresentedAttributesInvalidated(context.BodyNode) || context.Modifiers.Is(Modifiers.ForceCallable));
                        var mf = context.Modifiers;
                        if (context.GroupWithAttributes)
                        {
                            if (!NameManager.ForceCallableSRTemplateExists(representedClass))
                            {
                                GenerateForceCallableSRTemplates(representedClass);
                            }
                            context.Modifiers |= Modifiers.ForceCallable;
                        }
                        GenerateStructuralRepresentativeContentReference(context, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes);
                        context.Modifiers = mf;
                    }
                    else
                    {
                        // attributes not invalidated
                        context.BodyElement.CreateComment("SR attributes copied here");
                        XsltGeneratorContext newContext = context.CreateCopy();
                        newContext.CurrentStructuralRepresentativeAttributes = new StructuralRepresentativeAttributes((PSMClass)context.BodyNode, representedClass);
                        ProcessNotInvalidatedAttributes(newContext, attributes, true, false);
                    }
                }
            }
        }

        /// <summary>
        /// Using information from <see cref="ChangeSet"/> decides, whether 
        /// code processing elements of current node must be created or 
        /// whether the elements can be simple copied (they were not invalidated
        /// in the new version). Calls either <see cref="ProcessNodeElements"/> or
        /// <see cref="ProcessNotInvalidatedElements"/>. 
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void DecideElements(XsltGeneratorContext context)
        {
            if (context.BodyNode is PSMAttribute // for leaf node references
                || ChangeSet.ContentInvalidated(context.BodyNode) 
                || context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!(context.BodyNode is PSMAttribute))
                {
                    List<NodeElementWrapper> nodeContents = context.BodyNode.GetSubtreeElements();
                    if (nodeContents.Count > 0)
                    {
                        context.BodyElement.CreateComment(string.Format("Content of {0}", context.BodyNode));
                        ProcessNodeElements(context, nodeContents);
                    }
                }
                else
                {
                    GenerateLeafNodeReference(context);
                }
            }
            else
            {
                #region content not invalidated => copy or process with apply templates

                IEnumerable<NodeElementWrapper> subtreeContentComponents = context.BodyNode.GetSubtreeElements();
                if (subtreeContentComponents.Count() > 0)
                {
                    // content not invalidated
                    ProcessNotInvalidatedElements(context, subtreeContentComponents, false, false);
                }

                #endregion
            }
        }

        /// <summary>
        /// Using information from <see cref="ChangeSet"/> decides, whether 
        /// code processing represented elements of current node must be created or 
        /// whether the elements can be simple copied (they were not invalidated
        /// in the new version). If represented elements must be processed, also
        /// generates callable structural representative template (if it was not 
        /// created already for other structural representative). 
        /// </summary>
        /// <param name="context">The generator context.</param>
        private void DecideRepresentedElements(XsltGeneratorContext context)
        {
            
            List<NodeElementWrapper> nodeContents = context.BodyNode.GetRepresentedElements();

            if (nodeContents.Count > 0)
            {
                //return;
                PSMClass representedClass = ((PSMClass)context.BodyNode).RepresentedPSMClass;
                if (RepresentedClassCallableTemplateInfo.FullElementsTemplateNeeded(representedClass, ChangeSet)
                    || context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    Debug.Assert(ChangeSet.RepresentedContentInvalidated(context.BodyNode) || context.Modifiers.Is(Modifiers.ForceCallable));
                    GenerateStructuralRepresentativeContentReference(context, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Elements);
                }
                else
                {
                    // content not invalidated
                    context.BodyElement.CreateComment("SR contents copied here");
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentStructuralRepresentativeElements = new StructuralRepresentativeElements((PSMClass)context.BodyNode, representedClass);
                    ProcessNotInvalidatedElements(newContext, nodeContents, true, false);
                }
            }
        }

        /// <summary>
        /// Processes not invalidated attributes.
        /// </summary>
        /// <param name="context">The generator context.</param>
        /// <param name="attributes">The attributes to process</param>
        /// <param name="representedAttributes">set to <c>true</c> if <paramref name="attributes"/> are represented attributes</param>
        /// <param name="forceOnly">if set to <c>true</c>, processing template will be called with attributes to 
        /// proceess explicitly named in <paramref name="attributes"/> collection</param>
        private void ProcessNotInvalidatedAttributes(XsltGeneratorContext context, IEnumerable<NodeAttributeWrapper> attributes, bool representedAttributes, bool forceOnly)
        {
            List<TemplateParameter> callParameters = new List<TemplateParameter>();
            XPathExpr without = null;
            XPathExpr only = null;
            XPathExpr tmp = null;
            
            List<NodeAttributeWrapper> srAttributes = null;
            StructuralRepresentativeAttributes srReplacement = null; 
            if (representedAttributes)
            {
                Debug.Assert(context.CurrentStructuralRepresentativeAttributes != null && context.CurrentStructuralRepresentativeAttributes.StructuralRepresentative.IsStructuralRepresentative);
                srAttributes = context.CurrentStructuralRepresentativeAttributes.StructuralRepresentative.GetRepresentedAttributes();
                srReplacement = context.CurrentStructuralRepresentativeAttributes;
            }
            else
            {
                PSMClass psmClass = context.BodyNode as PSMClass;
                if (psmClass != null && psmClass.IsStructuralRepresentative)
                {
                    srAttributes = psmClass.GetRepresentedAttributes();
                    srReplacement = new StructuralRepresentativeAttributes(psmClass, psmClass.RepresentedPSMClass);
                }
            }

            bool onlyNewAttributes = false; 
            if (srAttributes != null && srAttributes.Count > 0)
            {
                tmp = XPathHelper.AttributeListWithSRSubstitution(context, srAttributes, srReplacement, true, ", ");
                if (XPathExpr.IsNullOrEmpty(tmp))
                {
                    onlyNewAttributes = true;
                }
                else
                {
                    if (context.CurrentStructuralRepresentativeAttributes != null)
                        tmp = XPathHelper.AllwaysReferenceFirstChild(tmp, true);
                }
            }

            if (forceOnly)
            {
                tmp = XPathHelper.AttributeListWithSRSubstitution(context, attributes, srReplacement, true, ", ");
                if (context.CurrentStructuralRepresentativeElements != null)
                    tmp = XPathHelper.AllwaysReferenceFirstChild(tmp, true);
            }
            
            if (!representedAttributes && !forceOnly)
                without = tmp;
            else
                only = tmp;

            // TODO : kombinace groups a without
            if (context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
            if (!XPathExpr.IsNullOrEmpty(without))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamExcept, without));
            if (!XPathExpr.IsNullOrEmpty(only))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamOnly, only));

            if (attributes.AreAttributesGreen(ChangeSet) || onlyNewAttributes)
            {
                context.BodyElement.CreateComment(string.Format("Copy attributes"));
                context.BodyElement.CallCopyAttributes(context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable), callParameters);
            }
            else
            {
                throw new InvalidOperationException("Should never get here");
            }
        }

        /// <summary>
        /// Processes not invalidated elements.
        /// </summary>
        /// <param name="context">The generator context.</param>
        /// <param name="elements">The attributes to process</param>
        /// <param name="representedElements">set to <c>true</c> if <paramref name="elements"/> are represented elements</param>
        /// <param name="forceOnly">if set to <c>true</c>, processing template will be called with attributes to 
        /// proceess explicitly named in <paramref name="elements"/> collection</param>
        private void ProcessNotInvalidatedElements(XsltGeneratorContext context, IEnumerable<NodeElementWrapper> elements, bool representedElements, bool forceOnly)
        {
            List<TemplateParameter> callParameters = new List<TemplateParameter>();
            XPathExpr without = null;
            XPathExpr only = null;
            XPathExpr tmp = null;
            List<NodeElementWrapper> representedComponents = null;
            StructuralRepresentativeElements srReplacement = null; 
            if (representedElements)
            {
                Debug.Assert(context.CurrentStructuralRepresentativeElements != null && context.CurrentStructuralRepresentativeElements.StructuralRepresentative.IsStructuralRepresentative);
                representedComponents = context.CurrentStructuralRepresentativeElements.StructuralRepresentative.GetRepresentedElements();
                srReplacement = context.CurrentStructuralRepresentativeElements;
            }
            else
            {
                PSMClass psmClass = context.BodyNode as PSMClass;
                if (psmClass != null && psmClass.IsStructuralRepresentative)
                {
                    representedComponents = psmClass.GetRepresentedElements();
                    srReplacement = new StructuralRepresentativeElements(psmClass, psmClass.RepresentedPSMClass);
                }
            }
            if (forceOnly)
            {
                tmp = XPathHelper.ElementsListWithSRSubstitution(context, elements, srReplacement, ", ");
                if (context.CurrentStructuralRepresentativeElements != null)
                    tmp = XPathHelper.AllwaysReferenceFirstChild(tmp, true);
            }
            else if (representedComponents != null && representedComponents.Count > 0)
            {
                tmp = XPathHelper.ElementsListWithSRSubstitution(context, representedComponents, srReplacement, ", ");
                if (context.CurrentStructuralRepresentativeElements != null)
                    tmp = XPathHelper.AllwaysReferenceFirstChild(tmp, true);
            }

            if (!representedElements && !forceOnly)
                without = tmp;
            else
                only = tmp;

            // TODO : kombinace groups a without
            if (context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
            if (!XPathExpr.IsNullOrEmpty(without))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamExcept, without));
            if (!XPathExpr.IsNullOrEmpty(only))
                callParameters.Add(new TemplateParameter(XsltDocumentExt.ParamOnly, only));

            if (elements.AreElementsGreen(ChangeSet))
            {
                context.BodyElement.CreateComment(string.Format("Copy content"));
                context.BodyElement.CallCopyContent(context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable), callParameters.ToArray());
            }
            // otherwise let the parser process the child nodes
            else
            {
                context.BodyElement.CreateComment(string.Format("Process content"));
                context.BodyElement.CallProcessContent(context.InGroup && !context.Modifiers.Is(Modifiers.ForceCallable), callParameters.ToArray());
            }
        }

        #endregion

        #endregion

        #region element, group and union common methods

        private void GenerateReference(XsltGeneratorContext context, PSMElement referencedElement, EContentPlacementState state, 
            GenerateSingleReferenceH generateSingleReference, GenerateForceCallable generateForceCallable, XPathExpr getExistingNodes)
        {
            #region hints
            //Node:
            //EContentPlacementState state = context.CurrentContentNodeState;
            //GenerateSingleReference generateSingleReference = GenerateContentGroupSingleReference;
            //GenerateForceCallable generateForceCallable = GenerateForceCallableNodeTemplate;
            //PSMElement referencedElement = context.CurrentContentNode;
            //XPathExpr getExsitingNodes = state != EContentPlacementState.Added ? XPathHelper.GroupAwareProjectXPath(context, referencedElement, OldVersion) : null;
            //hints Group:
            //EContentPlacementState state = context.CurrentContentGroupState();
            //GenerateSingleReference generateSingleReference = GenerateContentGroupSingleReference;
            //GenerateForceCallable generateForceCallable = GenerateForceCallableGroupTemplate;
            //PSMClass referencedElement = context.CurrentContentGroup.ContainingClass;
            //XPathExpr getExistingNodes = state != EContentPlacementState.Added ? context.CurrentContentGroup.CountGroupsExpression(context, xsltDocument, Log) : null;
            #endregion 

            uint mulLower = PSMTreeIterator.GetLowerMultiplicityOfContentElement(referencedElement);

            if (context.Modifiers.Is(Modifiers.ForceCallable) && mulLower == 0)
                return;

            if (((state.OneOf(EContentPlacementState.Moved, EContentPlacementState.AsItWas) 
                && ChangeSet.MultiplicityChanged(referencedElement))
                 || (state == EContentPlacementState.Added && mulLower > 1)
                 || (context.Modifiers.Is(Modifiers.ForceCallable) && mulLower > 1))
                && !(context.Modifiers.Is(Modifiers.ForceCallable) && mulLower == 1))
            {
                // existing with multiplicity change or added with multiplicity.lower > 1
                GenerateMultiplicityReference(context, referencedElement, 
                    state, ChangeSet.GetMultiplicityChange(referencedElement), 
                    generateForceCallable, generateSingleReference, getExistingNodes);
            }
            else
            {
                // existing without multiplicity change or added with multiplicity.lower == 1
                generateSingleReference(context, null, true);
            }
        }

        private delegate void GenerateSingleReferenceH(XsltGeneratorContext context, XPathExpr cond, bool flag);

        private static void GenerateMultiplicityReference(XsltGeneratorContext context, PSMElement referencedElement, 
            EContentPlacementState state, IMultiplicityChange multiplicityChange, 
            GenerateForceCallable generateForceCallable, GenerateSingleReferenceH generateSingleReference, XPathExpr getExistingNodes)
        {
            #region hints
            // for node:
            //PSMElement referencedElement = context.CurrentContentGroup.ContainingClass;
            //IMultiplicityChange multiplicityChange = ChangeSet.GetMultiplicityChange(referencedElement);
            //EContentPlacementState currentState = context.CurrentContentGroupState();
            //GenerateForceCallable generateForceCallable = GenerateForceCallableGroupTemplate;
            //GenerateSingleReference generateSingleReference = GenerateContentGroupSingleReference;
            //XPathExpr getExistingNodes = context.CurrentContentGroup.CountGroupsExpression(context, xsltDocument, Log);
            // for group: 
            //PSMElement referencedElement = context.CurrentContentNode;
            //IMultiplicityChange multiplicityChange = ChangeSet.GetMultiplicityChange(referencedElement);
            //EContentPlacementState currentState = context.CurrentContentNodeState;
            //GenerateForceCallable generateForceCallable = GenerateForceCallableNodeTemplate;
            //GenerateSingleReference generateSingleReference = GenerateElementSingleReference;
            //XPathExpr getExistingNodes = XPathHelper.GroupAwareProjectXPath(context, referencedElement, OldVersion);
            #endregion 
            Debug.Assert(multiplicityChange != null || state == EContentPlacementState.Added);

            #region handle existing nodes

            if (state.OneOf(EContentPlacementState.Moved, EContentPlacementState.AsItWas)
                && !context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!multiplicityChange.CanRequireDeleting())
                {
                    generateSingleReference(context, null, true);
                }
                else
                {
                    Debug.Assert(multiplicityChange != null);
                    XPathExpr cond = new XPathExpr("position() <= {0}", multiplicityChange.NewUpper);
                    generateSingleReference(context, cond, true);
                }
            }

            #endregion

            #region potentionally create new nodes

            if (state == EContentPlacementState.Added || multiplicityChange.CanRequireGenerating()
                || context.Modifiers.Is(Modifiers.ForceCallable))
            {
                string iterationStepTemplate = generateForceCallable(context);
                if (iterationStepTemplate != null)
                {
                    XPathExpr iterationsCountExpr;
                    if (state == EContentPlacementState.Added || context.Modifiers.Is(Modifiers.ForceCallable))
                        iterationsCountExpr = new XPathExpr(PSMTreeIterator.GetLowerMultiplicityOfContentElement(referencedElement).ToString());
                    else
                    {
                        Debug.Assert(multiplicityChange != null && multiplicityChange.CanRequireGenerating());
                        iterationsCountExpr = new XPathExpr("{0} - count({1})", multiplicityChange.NewLower,
                                                            getExistingNodes);
                    }
                    #if SIMPLE_ITERATIONS
                    XmlElement xslForEach =
                        context.BodyElement.XslForEach(new XPathExpr("1 to {0}", iterationsCountExpr));
                    xslForEach.XslCallTemplate(iterationStepTemplate);
                    #else 
                    string iterationTemplate =
                        NameManager.GetOrCreateIterationTemplate(containingClass, iterationStepTemplate);
                    context.BodyElement.XslCallTemplate(iterationTemplate, new TemplateParameter("iterations", iterationsCountExpr));
                    #endif
                }
            }

            #endregion
        }

        #endregion 

        #region unions

        private void GenerateUnionElementsReference(XsltGeneratorContext context)
        {
            PSMClassUnion classUnion = context.CurrentUnionElements.ClassUnion;
            //EContentPlacementState state = context.CurrentUnionState;

            uint mulLower = PSMTreeIterator.GetLowerMultiplicityOfContentElement(classUnion);

            if ((context.Modifiers.Is(Modifiers.ForceCallable)) && mulLower == 0)
                return;

            IMultiplicityChange multiplicityChange = ChangeSet.MultiplicityChanged(classUnion) ? ChangeSet.GetMultiplicityChange(classUnion) : null;
            var be = context.BodyElement;
            XPathExpr distribution = null; 
            // handle existing nodes
            if (/*context.CurrentUnionState != EContentPlacementState.Added && */!context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (context.CurrentUnionElements.Invalidated(ChangeSet))
                {
                    XPathExpr population;
                    switch (context.CurrentUnionElements.Complexity)
                    {
                        case EUnionComplexity.SingleChoice:
                            GenerateUnionElementsInnerChoice(context, false);
                            break;
                        case EUnionComplexity.MultipleWithoutGroups:
                        case EUnionComplexity.MultipleWithGroups:
                            population = context.CurrentUnionElements.PopulationExpression(context);
                            distribution = context.CurrentUnionElements.DistributionExpression(context, xsltDocument, Log);
                            XmlElement nextBodyElement;
                            if (!XPathExpr.IsNullOrEmpty(distribution))
                            {
                                if (multiplicityChange != null && multiplicityChange.CanRequireDeleting())
                                {
                                    XmlElement distributingElement;
                                    if (context.CurrentUnionElements.Complexity == EUnionComplexity.MultipleWithoutGroups)
                                    {
                                        distributingElement = context.BodyElement.XslForEach(population);
                                    }
                                    else //if (EUnionComplexity.MultipleWithoutGroups:)
                                    {
                                        distributingElement = context.BodyElement.XslForEachGroup(population);
                                        XmlAttribute groupStartingWithAttribute = xsltDocument.XslGroupStartingWithAttribute(distribution, false);
                                        distributingElement.Attributes.Append(groupStartingWithAttribute);
                                    }
                                    nextBodyElement = distributingElement.XslIf(new XPathExpr("position() <= {0}", multiplicityChange.NewUpper));
                                }
                                else
                                {
                                    if (context.CurrentUnionElements.Complexity == EUnionComplexity.MultipleWithoutGroups)
                                        nextBodyElement = context.BodyElement.XslForEach(population);
                                    else
                                        nextBodyElement = context.BodyElement.XslForEachGroup(population);
                                }

                                context.BodyElement = nextBodyElement;
                                GenerateUnionElementsInnerChoice(context, true);
                            }
                            break;
                        //case EUnionComplexity.MultipleWithGroups:
                            //population = context.CurrentUnionElements.PopulationExpression(context);
                            //distribution = context.CurrentUnionElements.DistributionExpression(context, xsltDocument, Log);
                            //if (multiplicityChange != null && multiplicityChange.CanRequireDeleting())
                            //{
                            //    XmlElement xslForEachGroup = context.BodyElement.XslForEachGroup(population);
                            //    XmlAttribute groupStartingWithAttribute = xsltDocument.XslGroupStartingWithAttribute(distribution, false);
                            //    xslForEachGroup.Attributes.Append(groupStartingWithAttribute);
                            //    nextBodyElement = xslForEachGroup.XslIf(new XPathExpr("position() <= {0}", multiplicityChange.NewUpper));
                            //}
                            //else
                            //{
                            //    nextBodyElement = context.BodyElement.XslForEachGroup(population);
                            //}
                            //context.BodyElement = nextBodyElement;
                            //GenerateUnionElementsInnerChoice(context, true);
                            //break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else 
                {
                    ProcessNotInvalidatedElements(context, context.CurrentUnionElements.Options.Cast<NodeElementWrapper>(), false, true);
                }
            }
            context.BodyElement = be;
            #region generating/adding
            XsltGeneratorContext newContext = context.CreateCopy();
            if (newContext.CurrentUnionElements.ClassUnion.ParentAssociation.Lower > 0)
            {
                if (context.CurrentUnionElements.Invalidated(ChangeSet))
                /* multiplicityChange != null && multiplicityChange.CanRequireGenerating()
                || newContext.CurrentUnionState == EContentPlacementState.Added 
                    && newContext.CurrentUnionElements.ClassUnion.ParentAssociation != null
                    && newContext.CurrentUnionElements.ClassUnion.ParentAssociation.Lower > 1*/
                {
                    newContext.Modifiers |= Modifiers.ForceCallable;
                    distribution = context.CurrentUnionElements.DistributionExpression(context, xsltDocument, Log);
                    XPathExpr countExisting;
                    if (!XPathExpr.IsNullOrEmpty(distribution))
                    {
                        var min = multiplicityChange != null ? multiplicityChange.NewLower :
                            (context.CurrentUnionElements.ClassUnion.ParentAssociation != null ? context.CurrentUnionElements.ClassUnion.ParentAssociation.Lower : 1);
                        countExisting = new XPathExpr("1 to {0} - count({1})", min, distribution);
                    }
                    else
                    {
                        countExisting = new XPathExpr("1 to {0}", context.CurrentUnionElements.ClassUnion.ParentAssociation.Lower);
                    }
                    XmlElement xslForEach = newContext.BodyElement.XslForEach(countExisting);
                    newContext.BodyElement = xslForEach;
                }

                // added, force callable or multiplicity caused generating
                if (newContext.CurrentUnionState == EContentPlacementState.Added || newContext.Modifiers.Is(Modifiers.ForceCallable))
                {
                    newContext.Modifiers |= Modifiers.ForceCallable;
                    if (!NameManager.ForceCallableUnionTemplateExists(newContext.CurrentUnionElements.ClassUnion))
                        GenerateForceCallableUnionTemplate(newContext);
                    string calledTemplateName = NameManager.GetUnionTemplate(newContext.CurrentUnionElements.ClassUnion, newContext.Modifiers.Is(Modifiers.ForceCallable));

                    if (!(newContext.InGroup && ChangeSet.ContinueInGroup(newContext.CurrentContentNode))
                        || newContext.Modifiers.Is(Modifiers.ForceCallable))
                        newContext.BodyElement.XslCallTemplate(calledTemplateName);
                    else
                        newContext.BodyElement.XslCallTemplate(calledTemplateName, new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
                }
            }
            #endregion 
        }

        public void GenerateUnionElementsInnerChoice(XsltGeneratorContext context, bool useGroups)
        {
            if (useGroups)
            {
                if (context.CurrentUnionElements.Complexity == EUnionComplexity.MultipleWithoutGroups)
                    context.BodyElement.XslVariable("cg", new XPathExpr("current()"));
                else
                    context.BodyElement.XslVariable("cg", new XPathExpr("current-group()"));
            }

            XmlElement xslChoose = context.BodyElement.XslChoose();
           
            foreach (ChoiceElementOption choiceElementOption in context.CurrentUnionElements.Options)
            {
                Debug.Assert(choiceElementOption.Items.Count == 1);
                XPathExpr test;
                NodeElementWrapper nodeWrapper = (choiceElementOption.Items[0]);
                Debug.Assert(nodeWrapper is ContentGroup || nodeWrapper is SimpleNodeElement);
                if (nodeWrapper is ContentGroup)
                {
                    if (!((ContentGroup)nodeWrapper).ContainingClass.ExistsInVersion(context.ChangeSet.OldVersion))
                        continue;
                    XsltGeneratorContext newContext = context.CreateCopy(((ContentGroup)nodeWrapper).ContainingClass);
                    newContext.CurrentContentGroup = (ContentGroup)nodeWrapper;
                    newContext.ProcessedPath = newContext.BodyNodeToProcessedPath();
                    newContext.ProcessedPath = newContext.NodeToProcessedPath(newContext.CurrentUnionElements.ClassUnion).Append("/$cg");
                    if (useGroups)
                    {
                        newContext.InGroup = true;
                    }
                    test = newContext.CurrentContentGroup.CreateGroupMembersSelectExpression(newContext.CurrentContentGroup, newContext, newContext.CurrentContentGroup.GetGroupAttributes(OldVersion));
                    XmlElement xslWhen = xslChoose.XslWhen(test);
                    newContext.BodyElement = xslWhen;
                    GenerateContentGroupSingleReference(newContext, null, false);
                    //ProcessNodeElements(newContext, choiceElementOption.Items);
                }
                else
                {
                    if (!((SimpleNodeElement)nodeWrapper).Element.ExistsInVersion(context.ChangeSet.OldVersion))
                        continue;

                    XsltGeneratorContext newContext = context.CreateCopy(((SimpleNodeElement)nodeWrapper).Element);
                    newContext.ProcessedPath = newContext.NodeToProcessedPath(newContext.CurrentUnionElements.ClassUnion).Append("/$cg");
                    if (useGroups)
                    {
                        newContext.InGroup = true;
                    }
                    newContext.CurrentContentGroup = new ContentGroup { 
                        ContainingClass =(PSMClass)PSMTreeIterator.GetSignificantAncestorOrSelf(newContext.CurrentUnionElements.ClassUnion)};
                    test = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(newContext, ((SimpleNodeElement)nodeWrapper).Element, context.CurrentStructuralRepresentative);
                    XmlElement xslWhen = xslChoose.XslWhen(test);
                    newContext.BodyElement = xslWhen;
                    ProcessNodeElements(newContext, choiceElementOption.Items);
                }
            }
        }

        #endregion

        #region choices

        public void GenerateChoiceElementsReference(XsltGeneratorContext context)
        {
            Debug.Assert(context.CurrentChoiceElements != null);
            if (context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!context.CurrentChoiceElements.IsOptionalIn(context.CurrentChoiceElements.Node))
                {
                    context.BodyElement.CreateComment("One choice selected for generating");
                    ChoiceElementOption generatingChoice = context.CurrentChoiceElements.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    ProcessNodeElements(context, generatingChoice.Items);
                }
            }
            else if (context.CurrentChoiceElements.Invalidated(context.ChangeSet))
            {
                XmlElement xslChoose = context.BodyElement.XslChoose();
                foreach (ChoiceElementOption choiceElementOption in context.CurrentChoiceElements.Options)
                {
                    XPathExpr test;
                    PSMElement firstNonOptional;

                    if (choiceElementOption.Items.Inline().All(i => !i.ExistsInVersion(OldVersion)))
                        continue;

                    if (choiceElementOption.Items.Count == 1)
                        firstNonOptional = choiceElementOption.Items.Inline().First();
                    else
                        firstNonOptional = choiceElementOption.Items.Inline().FirstOrDefault(
                        e => e.ExistsInVersion(OldVersion) && PSMTreeIterator.GetLowerMultiplicityOfContentElement((PSMElement)e.GetInVersion(OldVersion)) > 0); 
                    
                    if (firstNonOptional == null)
                    {
                        Log.AddError("Cannot create distinguishing test for choice {0}", context.CurrentChoiceElements);
                        test = XPathExpr.INVALID_PATH_EXPRESSION;
                    }
                    else
                    {
                        test = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, firstNonOptional, context.CurrentStructuralRepresentative);
                    }
                    XmlElement xslWhen = xslChoose.XslWhen(test);
                    XsltGeneratorContext newContext = context.CreateCopy(xslWhen);
                    ProcessNodeElements(newContext, choiceElementOption.Items );
                }
                // possible generation of new content
                if (!context.CurrentChoiceElements.IsOptionalIn(context.CurrentChoiceElements.Node)
                    && context.CurrentChoiceElements.MayRequireGenerating(ChangeSet))
                {
                    ChoiceElementOption generatingChoice = context.CurrentChoiceElements.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    XmlElement xslOtherwise = xslChoose.XslOtherwise();
                    XsltGeneratorContext newContext = context.CreateCopy(xslOtherwise, Modifiers.ForceCallable);
                    ProcessNodeElements(newContext, generatingChoice.Items);
                }
            }
            else
            {
                ProcessNotInvalidatedElements(context, context.CurrentChoiceElements.Options.Cast<NodeElementWrapper>(), false, true);
            }
        }

        public void GenerateUnionAttributesReference(XsltGeneratorContext context)
        {
            Debug.Assert(context.CurrentUnionAttributes != null);
            if (context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!context.CurrentUnionAttributes.IsOptionalIn(context.CurrentUnionAttributes.Node))
                {
                    context.BodyElement.CreateComment("One choice selected for generating");
                    ChoiceAttributeOption generatingChoice = context.CurrentUnionAttributes.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    ProcessNodeAttributes(context, generatingChoice.Items);
                }
            }
            if (context.CurrentUnionAttributes.Invalidated(context.ChangeSet) || context.CurrentUnionAttributes.MayRequireGenerating(ChangeSet))
            {
                
                Dictionary<ChoiceAttributeOption, XPathExpr> optionTests = new Dictionary<ChoiceAttributeOption, XPathExpr>();
                
                foreach (ChoiceAttributeOption attributeOption in context.CurrentUnionAttributes.Options)
                {
                    PSMAttribute firstNonOptional = attributeOption.Items.Inline().FirstOrDefault(
                        a => a.ExistsInVersion(OldVersion) && ((PSMAttribute)a.GetInVersion(OldVersion)).Lower == 1);
                    XPathExpr test;
                    if (firstNonOptional == null)
                    {
                        Log.AddError("Cannot create distinguishing test for choice {0}", context.CurrentUnionAttributes);
                        test = XPathExpr.INVALID_PATH_EXPRESSION;
                    }
                    else
                    {
                        test = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, firstNonOptional, context.CurrentStructuralRepresentative);
                    }

                    optionTests[attributeOption] = test;
                }

                XmlElement elementWhereGenerate = context.BodyElement;
                if (context.CurrentUnionAttributes.Invalidated(context.ChangeSet))
                {
                    XmlElement xslChoose = context.BodyElement.XslChoose();
                    foreach (ChoiceAttributeOption attributeOption in context.CurrentUnionAttributes.Options)
                    {
                        XmlElement xslWhen = xslChoose.XslWhen(optionTests[attributeOption]);
                        XsltGeneratorContext newContext = context.CreateCopy(xslWhen);
                        ProcessNodeAttributes(newContext, attributeOption.Items);
                    }
                    elementWhereGenerate = xslChoose;
                }
                else
                {
                    ProcessNotInvalidatedAttributes(context, context.CurrentUnionAttributes.Options.Cast<NodeAttributeWrapper>(), false, true);
                }
                // possible generation of new content
                if (!context.CurrentUnionAttributes.IsOptionalIn(context.CurrentUnionAttributes.Node)
                    && context.CurrentUnionAttributes.MayRequireGenerating(ChangeSet))
                {
                    ChoiceAttributeOption generatingChoice = context.CurrentUnionAttributes.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    if (context.CurrentUnionAttributes.Invalidated(context.ChangeSet))
                    {
                        elementWhereGenerate = elementWhereGenerate.XslOtherwise();
                    }
                    else
                    {
                        elementWhereGenerate = elementWhereGenerate.XslIf(new XPathExpr("empty({0})", optionTests.Values.ConcatWithSeparator("|")));
                    }
                    XsltGeneratorContext newContext = context.CreateCopy(elementWhereGenerate, Modifiers.ForceCallable);
                    ProcessNodeAttributes(newContext, generatingChoice.Items);
                }
            }
            else
            {
                ProcessNotInvalidatedAttributes(context, context.CurrentUnionAttributes.Options.Cast<NodeAttributeWrapper>(), false, true);
            }
        }

        public void GenerateChoiceAttributesReference(XsltGeneratorContext context)
        {
            Debug.Assert(context.CurrentChoiceAttributes != null);
            if (context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (!context.CurrentChoiceAttributes.IsOptionalIn(context.CurrentChoiceAttributes.ContentChoice))
                {
                    context.BodyElement.CreateComment("One choice selected for generating");
                    ChoiceAttributeOption generatingChoice = context.CurrentChoiceAttributes.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    ProcessNodeAttributes(context, generatingChoice.Items);
                }
            }
            if (context.CurrentChoiceAttributes.Invalidated(context.ChangeSet))
            {
                XmlElement xslChoose = context.BodyElement.XslChoose();
                foreach (ChoiceAttributeOption attributeOption in context.CurrentChoiceAttributes.Options)
                {
                    PSMAttribute firstNonOptional = attributeOption.Items.Inline().FirstOrDefault(
                        a => a.ExistsInVersion(OldVersion) && ((PSMAttribute)a.GetInVersion(OldVersion)).Lower == 1);
                    XPathExpr test;
                    if (firstNonOptional == null)
                    {
                        Log.AddError("Cannot create distinguishing test for choice {0}", context.CurrentChoiceAttributes);
                        test = XPathExpr.INVALID_PATH_EXPRESSION;
                    }
                    else
                    {
                        test = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, firstNonOptional, context.CurrentStructuralRepresentative);
                    }
                    
                    XmlElement xslWhen = xslChoose.XslWhen(test);
                    XsltGeneratorContext newContext = context.CreateCopy(xslWhen);
                    ProcessNodeAttributes(newContext, attributeOption.Items );
                }
                // possible generation of new content
                if (!context.CurrentChoiceAttributes.IsOptionalIn(context.CurrentChoiceAttributes.ContentChoice)
                    && context.CurrentChoiceAttributes.MayRequireGenerating(ChangeSet))
                {
                    ChoiceAttributeOption generatingChoice = context.CurrentChoiceAttributes.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    XmlElement xslOtherwise = xslChoose.XslOtherwise();
                    XsltGeneratorContext newContext = context.CreateCopy(xslOtherwise, Modifiers.ForceCallable);
                    ProcessNodeAttributes(newContext, generatingChoice.Items );
                }
            }
            else
            {
                ProcessNotInvalidatedAttributes(context, context.CurrentChoiceAttributes.Options.Cast<NodeAttributeWrapper>(), false, true);
            }
        }

        #endregion 

        #region elements

        private void ProcessNodeElements(XsltGeneratorContext context, List<NodeElementWrapper> elements)
        {
            ChangeSet.FixGroups(ref elements);
            foreach (NodeElementWrapper nodeContent in elements)
            {
                if (nodeContent is StructuralRepresentativeElements)
                {
                    var csrc = context.CurrentStructuralRepresentativeElements;
                    context.CurrentStructuralRepresentativeElements = (StructuralRepresentativeElements) nodeContent;
                    PSMClass representedClass = context.CurrentStructuralRepresentativeElements.RepresentedPSMClass;

                    if (ChangeSet.FindNewStructuralRepresentatives().Contains(context.CurrentStructuralRepresentativeElements.StructuralRepresentative)
                        && (representedClass.EncompassesContentForParentSignificantNode()))
                    {
                        XsltGeneratorContext newContext = context.CreateCopy(Modifiers.ForceCallable);
                        GenerateStructuralRepresentativeContentReference(newContext, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Elements);
                    }
                    else if (RepresentedClassCallableTemplateInfo.FullElementsTemplateNeeded(representedClass, ChangeSet))
                    {
                        GenerateStructuralRepresentativeContentReference(context, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Elements);
                    }
                    else
                    {
                        var nc = context.CurrentStructuralRepresentativeElements.RepresentedPSMClass.GetSubtreeElements();
                        ProcessNotInvalidatedElements(context, nc, true, false);
                    }
                    context.CurrentStructuralRepresentativeElements = csrc;
                }
                else if (nodeContent is SimpleNodeElement)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentContentNode = ((SimpleNodeElement)nodeContent).Element;
                    GenerateReference(newContext, newContext.CurrentContentNode,
                        newContext.CurrentContentNodeState, GenerateElementSingleReference, GenerateForceCallableNodeTemplate, 
                        newContext.CurrentContentNodeState != EContentPlacementState.Added && !newContext.Modifiers.Is(Modifiers.ForceCallable) ? XPathHelper.GroupAwareProjectXPath(newContext, newContext.CurrentContentNode, OldVersion) : null);
                }
                else if (nodeContent is ContentGroup)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentContentGroup = ((ContentGroup)nodeContent);
                    GenerateReference(newContext, newContext.CurrentContentGroup.ContainingClass,
                        newContext.CurrentContentGroupState(), GenerateContentGroupSingleReference, GenerateForceCallableGroupTemplate,
                        newContext.CurrentContentGroupState() != EContentPlacementState.Added && !newContext.Modifiers.Is(Modifiers.ForceCallable) ? newContext.CurrentContentGroup.CountGroupsExpression(newContext, xsltDocument, Log) : null);
                }
                else if (nodeContent is ChoiceElements)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentChoiceElements = (ChoiceElements) nodeContent;
                    GenerateChoiceElementsReference(newContext);
                }
                else if (nodeContent is UnionElements)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentUnionElements = (UnionElements)nodeContent;
                    GenerateUnionElementsReference(newContext);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        private void GenerateElementSingleReference(XsltGeneratorContext context, XPathExpr cond, bool dummy)
        {
            if (context.CurrentContentNodeState == EContentPlacementState.Added || context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (PSMTreeIterator.GetLowerMultiplicityOfContentElement(context.CurrentContentNode) > 0)
                {
                    if (context.Modifiers.Is(Modifiers.ForceCallable) && !NameManager.ForceCallableNodeTemplateExists(context.CurrentContentNode))
                        GenerateForceCallableNodeTemplate(context);
                    NameManager.RequestedTemplates.Add(new KeyValuePair<PSMElement, bool>(context.CurrentContentNode, context.Modifiers.Is(Modifiers.ForceCallable)));
                    string calledTemplateName = NameManager.GetNodeCallableTemplate(context.CurrentContentNode, context.Modifiers.Is(Modifiers.ForceCallable));
                    if (!(context.InGroup && ChangeSet.ContinueInGroup(context.CurrentContentNode))
                        || context.Modifiers.Is(Modifiers.ForceCallable))
                        context.BodyElement.XslCallTemplate(calledTemplateName);
                    else
                        context.BodyElement.XslCallTemplate(calledTemplateName, new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
                }
            }
            else
            {
                XPathExpr match = XPathHelper.GroupAwareProjectXPath(context, context.CurrentContentNode, OldVersion);
                if (context.InGroup && ChangeSet.ContinueInGroup(context.CurrentContentNode))
                    context.BodyElement.XslApplyTemplates(XPathExpr.IsNullOrEmpty(cond) ? match : match.AppendPredicate(cond),
                                                          new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, match));
                else
                {
                    if (ChangeSet.greenNodes.Contains(context.CurrentContentNode))
                        context.BodyElement.XslCopyOf(XPathExpr.IsNullOrEmpty(cond) ? match : match.AppendPredicate(cond));
                    else
                        context.BodyElement.XslApplyTemplates(XPathExpr.IsNullOrEmpty(cond) ? match : match.AppendPredicate(cond));
                }
            }
        }

        /// <summary>
        /// Used from represented classes to include factored out content
        /// </summary>
        /// <seealso cref="GenerateStructuralRepresentativeContentReference"/>
        private void GenerateFactoredOutContentReference(XsltGeneratorContext context, RepresentedClassCallableTemplateInfo info)
        {
            List<TemplateParameter> parameters = new List<TemplateParameter>();
            if ((info.FullElementsTemplate || info.CopyElementsTemplate)
                || (!context.InGroup && (info.FullAttributesTemplate || info.CopyAttributesTemplate)))
            {
                context.BodyElement.CreateComment("Content factored out for SR");
                if (info.IsGroupAware && !context.Modifiers.Is(Modifiers.ForceCallable))
                {
                    if (context.InGroup)
                        parameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
                    else
                        parameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, new XPathExpr("*")));
                }
            }
            var mf = context.Modifiers;
            if (context.GroupWithAttributes)
            {
                PSMClass representedClass = (PSMClass)context.BodyNode;
                context.Modifiers |= Modifiers.ForceCallable;
                if (!NameManager.ForceCallableSRTemplateExists(representedClass))
                {
                    GenerateForceCallableSRTemplates(representedClass);
                }
            }
            if (info.FullAttributesTemplate && (!context.InGroup || context.GroupWithAttributes))
            {
                string templateName = NameManager.GetRepresentedContentTemplate(
                    (PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes, context.Modifiers.Is(Modifiers.ForceCallable));

                context.BodyElement.XslCallTemplate(templateName, !context.GroupWithAttributes ? parameters : null);
            }
            if (info.CopyAttributesTemplate && (!context.InGroup || context.GroupWithAttributes))
            {
                ProcessNotInvalidatedAttributes(context, context.BodyNode.GetAttributesUnderNode(), false, false);
            }
            context.Modifiers = mf; 
            if (info.FullElementsTemplate)
            {
                string templateName = NameManager.GetRepresentedContentTemplate(
                    (PSMClass)context.BodyNode, XsltTemplateNameManager.ERepresentedTemplatePart.Elements, context.Modifiers.Is(Modifiers.ForceCallable));
                context.BodyElement.XslCallTemplate(templateName, parameters);
            }
            if (info.CopyElementsTemplate)
            {
                ProcessNotInvalidatedElements(context, context.BodyNode.GetSubtreeElements(), false, false);
            }
        }

        /// <summary>
        /// Used from structural representatives to include represented content
        /// </summary>
        private void GenerateStructuralRepresentativeContentReference(XsltGeneratorContext context, PSMClass representedClass, XsltTemplateNameManager.ERepresentedTemplatePart templatePart)
        {
            if (context.Modifiers.Is(Modifiers.ForceCallable) && !NameManager.ForceCallableSRTemplateExists(representedClass))
                GenerateForceCallableSRTemplates(representedClass);
            string templateName = NameManager.GetRepresentedContentTemplate(representedClass, templatePart, context.Modifiers.Is(Modifiers.ForceCallable));
            context.BodyElement.CreateComment(templatePart == XsltTemplateNameManager.ERepresentedTemplatePart.Attributes ? "Attributes from represented class" : "Elements from represented class");
            List<TemplateParameter> parameters = new List<TemplateParameter>();
            if ((ChangeSet.IsContentGroupNode(representedClass) || ChangeSet.IsUnderContentGroup(representedClass) || context.InGroup)
                && !context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (context.InGroup)
                {
                    parameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, XPathExpr.CurrentGroupVariableExpr));
                }
                else 
                {
                    IFromStructuralRepresentative srReplacement = templatePart == XsltTemplateNameManager.ERepresentedTemplatePart.Elements ? (IFromStructuralRepresentative)context.CurrentStructuralRepresentativeElements : context.CurrentStructuralRepresentativeAttributes;
                    if (srReplacement == null)
                        srReplacement = new StructuralRepresentativeAttributes((PSMClass)context.BodyNode, representedClass);
                    XPathExpr pathExpr = XPathHelper.GroupAwareProjectXPathWithSRSubstitution(context, representedClass, srReplacement);
                    if (!XPathExpr.IsNullOrEmpty(pathExpr))
                    {
                        pathExpr = XPathHelper.AllwaysReferenceFirstChild(pathExpr, false);
                        pathExpr = pathExpr.Append("/(node() | @*)");
                    }
                    else
                        pathExpr = new XPathExpr("node() | @*");
                    parameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, pathExpr));
                }
            }

            if (!context.Modifiers.Is(Modifiers.ForceCallable) &&
                context.InGroup && !(ChangeSet.IsContentGroupNode(representedClass) || ChangeSet.IsUnderContentGroup(representedClass)))
            {
                if (!context.Modifiers.Is(Modifiers.ForceGroupAwareSRContent)
                    && (!NameManager.RepresentedClassesInfo.ContainsKey(representedClass)
                    || !NameManager.RepresentedClassesInfo[representedClass].WasRegeneratedForGroup))
                {
                    ContentGroup currentGroup = representedClass.GetNodeAsContentGroup();
                    XsltGeneratorContext newContext = new XsltGeneratorContext(ChangeSet, representedClass, currentGroup)
                        {
                            Modifiers = Modifiers.ForceGroupAwareSRContent
                        };
                    newContext.ProcessedPath = newContext.BodyNodeToProcessedPath();
                    GenerateRepresentedContentCallableTemplate(newContext);
                    NameManager.RepresentedClassesInfo[representedClass].WasRegeneratedForGroup = true;
                }
                templateName += "-GA";
            }

            NameManager.RequestedTemplates.Add(new KeyValuePair<PSMElement, bool>(representedClass, context.Modifiers.Is(Modifiers.ForceCallable)));
            context.BodyElement.XslCallTemplate(templateName, parameters);
        }

        private void GenerateLeafNodeReference(XsltGeneratorContext context)
        {
            Debug.Assert(context.BodyNode is PSMAttribute && ((PSMAttribute)context.BodyNode).AttributeContainer != null);
            PSMAttribute attribute = (PSMAttribute) context.BodyNode;

            if (context.BodyNodeState.OneOf(EContentPlacementState.Moved, EContentPlacementState.AsItWas)
                && !context.Modifiers.Is(Modifiers.ForceCallable))
            {
                XPathExpr match = XPathHelper.GroupAwareProjectXPath(context, attribute, OldVersion);
                if (match.IsEmpty())
                {
                    if (context.InGroup)
                        match = XPathExpr.CurrentGroupVariableExpr;
                    else
                        match = new XPathExpr(".");
                }

                context.BodyElement.XslValueOf(match);
            }
            else
            {
                context.BodyElement.CreateTextElement(AttributeValueGenerator.GenerateValue(attribute));
            }
        }

        #endregion

        #region attributes

        private void ProcessNodeAttributes(XsltGeneratorContext context, IEnumerable<NodeAttributeWrapper> attributes)
        {
            if (attributes.Count() > 0)
            {
                context.BodyElement.CreateComment(string.Format("Attributes of {0}", context.BodyNode));
            }
            foreach (NodeAttributeWrapper attributeWrapper in attributes)
            {
                if (attributeWrapper is SimpleNodeAttribute)
                {
                    PSMAttribute currentAttribute = context.CurrentAttribute;
                    context.CurrentAttribute = ((SimpleNodeAttribute)attributeWrapper).Attribute;
                    GenerateAttributeReference(context);
                    context.CurrentAttribute = currentAttribute;
                }
                else if (attributeWrapper is StructuralRepresentativeAttributes)
                {
                    var csra = context.CurrentStructuralRepresentativeAttributes;
                    var pp = context.ProcessedPath;
                    var be = context.BodyElement;
                    context.CurrentStructuralRepresentativeAttributes = (StructuralRepresentativeAttributes)attributeWrapper;
                    if (!context.Modifiers.Is(Modifiers.ForceCallable) && 
                        context.ProcessedPath != XPathHelper.GetXPathForNode(context.CurrentStructuralRepresentative.StructuralRepresentative, OldVersion))
                    {
                        XPathExpr contextFix = XPathHelper.GroupAwareProjectXPath(context, context.CurrentStructuralRepresentative.StructuralRepresentative, OldVersion);
                        contextFix = XPathHelper.AllwaysReferenceFirstChild(contextFix, true);
                        context.BodyElement = context.BodyElement.XslForEach(contextFix.AppendPredicate("1"));
                        context.ProcessedPath = contextFix;
                    }
                    PSMClass representedClass = context.CurrentStructuralRepresentativeAttributes.RepresentedPSMClass;
                    if (ChangeSet.FindNewStructuralRepresentatives().Contains(context.CurrentStructuralRepresentativeAttributes.StructuralRepresentative)
                        && (representedClass.EncompassesAttributesForParentSignificantNodeOrSelf()))
                    {
                        var mf = context.Modifiers;
                        context.Modifiers |= Modifiers.ForceCallable;
                        GenerateStructuralRepresentativeContentReference(context, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes);
                        context.Modifiers = mf;
                    }
                    else if (RepresentedClassCallableTemplateInfo.FullAttributeTemplateNeeded(representedClass, ChangeSet))
                    {
                        GenerateStructuralRepresentativeContentReference(context, representedClass, XsltTemplateNameManager.ERepresentedTemplatePart.Attributes);
                    }
                    else
                    {
                        var na = representedClass.GetAttributesUnderNode();
                        ProcessNotInvalidatedAttributes(context, na, true, false);
                    }
                    context.CurrentStructuralRepresentativeAttributes = csra;
                    context.ProcessedPath = pp;
                    context.BodyElement = be;
                }
                else if (attributeWrapper is ChoiceAttributes)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentChoiceAttributes = (ChoiceAttributes) attributeWrapper;
                    GenerateChoiceAttributesReference(newContext);
                }
                else if (attributeWrapper is UnionAttributes)
                {
                    XsltGeneratorContext newContext = context.CreateCopy();
                    newContext.CurrentUnionAttributes = (UnionAttributes)attributeWrapper;
                    GenerateUnionAttributesReference(newContext);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        private void GenerateAttributeReference(XsltGeneratorContext context)
        {
            uint mulLower = PSMTreeIterator.GetLowerMultiplicityOfContentElement(context.CurrentAttribute);

            if (mulLower == 0 && (context.CurrentAtrtibuteState == EContentPlacementState.Added || context.Modifiers.Is(Modifiers.ForceCallable)))
                return;

            if (context.CurrentAtrtibuteState == EContentPlacementState.Added || context.Modifiers.Is(Modifiers.ForceCallable))
            {
                string value = AttributeValueGenerator.GenerateValue(context.CurrentAttribute);
                context.BodyElement.XslAttribute(context.CurrentAttribute.AliasOrName, value);
            }
            else if (!ChangeSet.changesByTarget.ContainsKey(context.CurrentAttribute) || ChangeSet.changesByTarget[context.CurrentAttribute].All(c => !c.InvalidatesAttributes))
            {
                XPathExpr xpath = XPathHelper.GroupAwareProjectXPath(context, context.CurrentAttribute, OldVersion);
                if (ChangeSet.IsUnderContentGroup(context.CurrentAttribute))
                    xpath = XPathHelper.AllwaysReferenceFirstChild(xpath, true);
                context.BodyElement.XslCopyOf(xpath);
            }
            else
            {
                XPathExpr xpath = XPathHelper.GroupAwareProjectXPath(context, context.CurrentAttribute, OldVersion);
                if (ChangeSet.IsUnderContentGroup(context.CurrentAttribute))
                    xpath = XPathHelper.AllwaysReferenceFirstChild(xpath, true);
                if (xpath.IsEmpty()) 
                    throw new ArgumentException();
                uint lowerOldMul = PSMTreeIterator.GetLowerMultiplicityOfContentElement((PSMElement)context.CurrentAttribute.GetInVersion(OldVersion));
                uint lowerNewMul = PSMTreeIterator.GetLowerMultiplicityOfContentElement(context.CurrentAttribute);

                if (lowerOldMul == 0 && lowerNewMul == 1)
                {
                    // attribute must be created
                    XmlElement xslChoose = context.BodyElement.XslChoose();
                    XmlElement xslWhen = xslChoose.XslWhen(xpath);
                    XmlElement xslAttribute = xslWhen.XslAttribute(context.CurrentAttribute.AliasOrName, null);
                    xslAttribute.XslValueOf(xpath);
                    XmlElement xslOtherwise = xslChoose.XslOtherwise();
                    string value = AttributeValueGenerator.GenerateValue(context.CurrentAttribute);
                    xslOtherwise.XslAttribute(context.CurrentAttribute.AliasOrName, value);
                }
                else if (lowerOldMul == 0 && lowerNewMul == 0)
                {
                    // attribute is created only if it exists already
                    XmlElement xslIf = context.BodyElement.XslIf(xpath);
                    XmlElement xslAttribute = xslIf.XslAttribute(context.CurrentAttribute.AliasOrName, null);
                    xslAttribute.XslValueOf(xpath);
                }
                else 
                {
                    XmlElement xslAttribute = context.BodyElement.XslAttribute(context.CurrentAttribute.AliasOrName, null);
                    xslAttribute.XslValueOf(xpath);
                }
            }
        }

        #endregion

        #region groups

        /// <summary>
        /// Generates named template for current content group. 
        /// Template parameters are <code>cg</code> for current group and <code>attributs</code> for group attributes (added
        /// only when group has any). With <see cref="Modifiers.ForceCallable"/> context 
        /// modifier <code>cg</code> attribute is omitted. 
        /// </summary>
        /// <param name="context">The generator context.</param>
        /// <returns>template name</returns>
        private string GenerateContentGroupTemplate(XsltGeneratorContext context)
        {
            string templateName = NameManager.GetGroupCallableTemplate(context.CurrentContentGroup, context.Modifiers.Is(Modifiers.ForceCallable));
            List<string> parameters = new List<string>();
            if (!context.Modifiers.Is(Modifiers.ForceCallable))
            {
                parameters.Add(XsltDocumentExt.ParamCurrentGroup);
            }
            context.ContentGroupAttributes = context.CurrentContentGroup.ContainingClass.GetAttributesForGroup(OldVersion);
            if (context.ContentGroupAttributes.Count > 0)
            {
                parameters.Add(XsltDocumentExt.ParamAttributes);
            }
            XmlElement xslNamedTemplate = xslStylesheetNode.XslNamedTemplate(templateName, parameters.ToArray());
            context.Template = xslNamedTemplate;
            GenerateRedNodeTemplateContent(context);
            return templateName;
        }

        private void GenerateContentGroupSingleReference(XsltGeneratorContext context, XPathExpr cond, bool createDistributingElement) 
        {
            if (context.CurrentContentGroupState() == EContentPlacementState.Added || context.Modifiers.Is(Modifiers.ForceCallable))
            {
                if (context.Modifiers.Is(Modifiers.ForceCallable) && !NameManager.ForceCallableGroupTemplateExists(context.CurrentContentGroup))
                    GenerateForceCallableGroupTemplate(context);
                // new with multiplicity.lower = 1
                NameManager.RequestedTemplates.Add(new KeyValuePair<PSMElement, bool>(context.CurrentContentGroup.ContainingClass, context.Modifiers.Is(Modifiers.ForceCallable)));
                string calledTemplateName = NameManager.GetGroupCallableTemplate(context.CurrentContentGroup, context.Modifiers.Is(Modifiers.ForceCallable));
                context.BodyElement.XslCallTemplate(calledTemplateName);
            }
            else if (!ChangeSet.redNodes.Contains(context.CurrentContentGroup.ContainingClass) && XPathExpr.IsNullOrEmpty(cond))
            {
                // group was not invalidated, just process subtree
                XPathExpr match = context.CurrentContentGroup.CreateGroupMembersSelectExpression(context.CurrentContentGroup, context, context.CurrentContentGroup.GetGroupAttributes(OldVersion));
                if (context.InGroup && ChangeSet.ContinueInGroup(context.CurrentContentGroup.ContainingClass))
                    context.BodyElement.XslApplyTemplates(match, new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, match));
                else
                    context.BodyElement.XslApplyTemplates(match);
            }
            else
            {
                #region when position condition exists, regenerate template without condition
                if (!ChangeSet.redNodes.Contains(context.CurrentContentGroup.ContainingClass) && !XPathExpr.IsNullOrEmpty(cond))
                {
                    XsltGeneratorContext newContext = new XsltGeneratorContext(ChangeSet, context.CurrentContentGroup.ContainingClass, context.CurrentContentGroup);
                    newContext.ProcessedPath = newContext.BodyNodeToProcessedPath();
                    GenerateContentGroupTemplate(newContext);
                }
                #endregion 

                List<TemplateParameter> parameters = new List<TemplateParameter>();
                string calledTemplateName = NameManager.GetGroupCallableTemplate(context.CurrentContentGroup, context.Modifiers.Is(Modifiers.ForceCallable));
                if (context.CurrentContentGroup.GetGroupAttributes(OldVersion).Count > 0)
                {
                    XPathExpr attributesForGroup = context.CurrentContentGroup.CreateAttributesForGroupExpr(context);
                    string attributeXsltVariableName = calledTemplateName + "-ATT";
                    context.BodyElement.XslVariable(attributeXsltVariableName, attributesForGroup);
                    parameters.Add(new TemplateParameter("attributes", new XPathExpr("$"+attributeXsltVariableName)));
                } 

                PSMClass oldVersion = (PSMClass)context.CurrentContentGroup.ContainingClass.GetInVersion(OldVersion);
                XmlElement groupDistributingElement;
                XPathExpr cgVal;
                XPathExpr groupMembersSelectExpression = context.CurrentContentGroup.CreateGroupMembersSelectExpression(context.CurrentContentGroup, context, context.CurrentContentGroup.GetGroupAttributes(OldVersion));
                XmlElement callIn;
                
                if (createDistributingElement)
                {
                    if (!oldVersion.HasElementLabel)
                    {
                        XmlAttribute groupingAttribute = context.CurrentContentGroup.CreateGroupingAttribute(context, xsltDocument, Log);
                        groupDistributingElement = context.BodyElement.XslForEachGroup(groupMembersSelectExpression);
                        groupDistributingElement.Attributes.Append(groupingAttribute);
                        cgVal = new XPathExpr("current-group()");
                    }
                    else
                    {
                        groupDistributingElement = context.BodyElement.XslForEach(groupMembersSelectExpression);
                        cgVal = new XPathExpr("current()/@* | current()/*");
                    }
                    callIn = groupDistributingElement;
                    if (!XPathExpr.IsNullOrEmpty(cond))
                    {
                        callIn = groupDistributingElement.XslIf(cond);
                    }
                }
                else
                {
                    cgVal = new XPathExpr("current-group()");
                    callIn = context.BodyElement;
                }
                parameters.Add(new TemplateParameter(XsltDocumentExt.ParamCurrentGroup, cgVal));
                if (context.Modifiers.Is(Modifiers.ForceCallable) && !NameManager.ForceCallableGroupTemplateExists(context.CurrentContentGroup))
                    GenerateForceCallableGroupTemplate(context);
                NameManager.RequestedTemplates.Add(new KeyValuePair<PSMElement, bool>(context.CurrentContentGroup.ContainingClass, context.Modifiers.Is(Modifiers.ForceCallable)));
                callIn.XslCallTemplate(calledTemplateName, parameters);
            }
        }

        #endregion 

        #region force callable templates

        /// <summary>
        /// Delegate for method generating force callable templates. 
        /// </summary>        
        /// <param name="context">The generator context.</param>
        /// <returns>Returns name for callable template. Result <code>null</code> is used to signal that 
        /// the force callable template is not needed. </returns>
        /// <seealso cref="GenerateForceCallableGroupTemplate"/>
        /// <seealso cref="GenerateForceCallableNodeTemplate"/>
        /// <seealso cref="GenerateForceCallableUnionTemplate"/>
        /// <seealso cref="GenerateForceCallableSRTemplates"/>
        private delegate string GenerateForceCallable(XsltGeneratorContext context);
        private string GenerateForceCallableGroupTemplate(XsltGeneratorContext context)
        {
            if (!NameManager.ForceCallableGroupTemplateExists(context.CurrentContentGroup))
            {
                XsltGeneratorContext newContext = new XsltGeneratorContext(ChangeSet, context.CurrentContentGroup.ContainingClass, context.CurrentContentGroup)
                                                      { 
                                                          Modifiers = Modifiers.ForceCallable
                                                      };
                newContext.ProcessedPath = newContext.BodyNodeToProcessedPath();
                GenerateContentGroupTemplate(newContext);
            }
            return NameManager.GetGroupCallableTemplate(context.CurrentContentGroup, true);
        }

        private string GenerateForceCallableNodeTemplate(XsltGeneratorContext context)
        {
            if (!NameManager.ForceCallableNodeTemplateExists(context.CurrentContentNode))
            {
                XsltGeneratorContext newContext = new XsltGeneratorContext(ChangeSet, context.CurrentContentNode)
                                                      {
                                                          Modifiers = Modifiers.ForceCallable 
                                                      };
                GenerateRedNodeTemplate(newContext);
            }
            return NameManager.GetNodeCallableTemplate(context.CurrentContentNode, true);
        }

        private string GenerateForceCallableUnionTemplate(XsltGeneratorContext context)
        {
            if (!NameManager.ForceCallableUnionTemplateExists(context.CurrentUnionElements.ClassUnion))
            {
                XsltGeneratorContext newContext = context.CreateCopy(Modifiers.ForceCallable);
                if (!newContext.CurrentUnionElements.IsOptionalIn(newContext.CurrentUnionElements.ClassUnion))
                {
                    string templateName = NameManager.GetUnionTemplate(context.CurrentUnionElements.ClassUnion, true);
                    newContext.Template = xslStylesheetNode.XslNamedTemplate(templateName);
                    newContext.BodyElement = newContext.Template;
                    newContext.BodyElement.CreateComment("One component of a union selected for generating");
                    ChoiceElementOption generatingChoice = newContext.CurrentUnionElements.SuggestChoiceForGenerating();
                    Debug.Assert(generatingChoice != null);
                    ProcessNodeElements(newContext, generatingChoice.Items);
                    return templateName;
                }
                else
                {
                    return null; 
                }
            }
            else
            {
                return NameManager.GetUnionTemplate(context.CurrentUnionElements.ClassUnion, true);
            }
        }

        private void GenerateForceCallableSRTemplates(PSMClass representedClass)
        {
            XsltGeneratorContext context = new XsltGeneratorContext(ChangeSet, representedClass);
            context.Modifiers |= Modifiers.ForceCallable;
            GenerateRepresentedContentCallableTemplate(context);
        }

        #endregion 

        #region red and blue nodes

        private void GenerateBlueNodesTemplate()
        {
            if (ChangeSet.blueNodes.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (PSMElement blueNode in ChangeSet.blueNodes)
                {
                    PSMElement nodeOld = (PSMElement)blueNode.GetInVersion(OldVersion);
                    IEnumerable<XPathExpr> pathsWhereElementAppears = XPathHelper.PathsWhereElementAppears(nodeOld);
                    XPathExpr allAppearances = XPathExpr.ConcatWithOrOperator(pathsWhereElementAppears);
                    //string oldPath = nodeOld.XPath;
                    if (!XPathExpr.IsNullOrEmpty(allAppearances))
                    {
                        sb.Append(allAppearances);
                        sb.Append(XPathExpr.PIPE_OPERATOR);
                    }
                }
                sb.Remove(sb.Length - 3, 3);
                xslStylesheetNode.CreateComment("blue nodes template");
                XmlElement templateElement = xslStylesheetNode.XslTemplate(new XPathExpr(sb.ToString()));
                XmlElement copyElement = templateElement.XslCopy();
                copyElement.CallCopyAttributes(false);
                copyElement.CallProcessContent(false);
            }
        }

        private void GenerateGreenNodesTemplate()
        {
            List<XPathExpr> paths = new List<XPathExpr>();
            foreach (PSMElement greenNode in ChangeSet.greenNodes)
            {
                if (ChangeSet.IsContentGroupNode(greenNode))
                    continue; 
                // all green nodes existed in the old version
                PSMElement nodeOld = (PSMElement) greenNode.GetInVersion(OldVersion);
                IEnumerable<XPathExpr> pathsWhereElementAppears = XPathHelper.PathsWhereElementAppears(nodeOld);
                XPathExpr allAppearances = XPathExpr.ConcatWithOrOperator(pathsWhereElementAppears);
                if (!XPathExpr.IsNullOrEmpty(allAppearances))
                    paths.Add(allAppearances);
            }
            if (paths.Count > 0)
            {
                XPathExpr fullExpr = new XPathExpr(paths.ConcatWithSeparator(XPathExpr.PIPE_OPERATOR));
                xslStylesheetNode.CreateComment("green nodes template");
                XmlElement templateElement = xslStylesheetNode.XslTemplate(fullExpr);
                templateElement.XslCopyOf(new XPathExpr("."));
            }    
        }
        
        #endregion 
        
        #region STUPID STUFF

        // ReSharper disable MemberCanBeMadeStatic.Local
        private void AddOtherTemplates()
        // ReSharper restore MemberCanBeMadeStatic.Local
        {
#if SEPARATE_SUPPORTING_TEMPLATES
#if REGENERATE_SUPPORTING
            XmlDocument supportingTemplates = new XmlDocument();
#if SAXON_XSLT
            XmlElement slNode = supportingTemplates.XslStylesheet("2.0");
#else 
            XmlElement slNode = xsltDocument.XslStylesheet("1.0");
#endif
            XmlElement xslOutputNode = slNode.XslElement("output");
            xslOutputNode.AddAttributeWithValue("method", "xml");
            xslOutputNode.AddAttributeWithValue("indent", "yes");

            slNode.CreateComment(" ****************** ");
            slNode.CreateComment(" * copy templates * " + DateTime.Now.ToString());
            slNode.CreateComment(" ****************** ");
            slNode.AddCopyAttributesTemplate();
            slNode.AddCopyAttributesTemplateGroupAware();
            slNode.AddCopyContentTemplate();
            slNode.AddCopyContentTemplateGroupAware();
            slNode.AddProcessContentTemplate();
            slNode.AddProcessContentTemplateGroupAware();
            slNode.AddFallbackRules();
            supportingTemplates.Save(System.AppDomain.CurrentDomain.BaseDirectory + "supporting-templates.xslt");
#endif
#else
            slNode.CreateComment(" ****************** ");
            slNode.CreateComment(" * copy templates * ");
            slNode.CreateComment(" ****************** ");
            slNode.AddCopyAttributesTemplate();
            slNode.AddCopyAttributesTemplateGroupAware();
            slNode.AddCopyContentTemplate();
            slNode.AddCopyContentTemplateGroupAware();
            slNode.AddProcessContentTemplate();
            slNode.AddProcessContentTemplateGroupAware();
            slNode.AddFallbackRules();
#endif
        }

        private void FinalizeXsltWriter()
        {
            xsltWriter.WriteEndElement(); //stylesheet
            xsltWriter.Flush();
            xsltWriter.Close();
        }

        private XmlWriter xsltWriter;
        private StringBuilder _xsltWriterSb;
        
        private XmlDocument xsltDocument;
        private XmlElement xslStylesheetNode;

        private void PrepareXsltWriter()
        {
            _xsltWriterSb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            xsltWriter = XmlWriter.Create(_xsltWriterSb, settings);
            Debug.Assert(xsltWriter != null);
            xsltWriter.WriteStartDocument();
            xsltWriter.WriteStartElement("xsl", "stylesheet", XsltDocumentExt.NS_XSLT);
            
            xsltWriter.WriteStartElement("xsl", "output", XsltDocumentExt.NS_XSLT);
            xsltWriter.WriteAttributeString("method", "xml");
            xsltWriter.WriteAttributeString("indent", "yes");
            xsltWriter.WriteEndElement(); //output

            xsltDocument = new XmlDocument();
            #if SAXON_XSLT
            xslStylesheetNode = xsltDocument.XslStylesheet("2.0");
            #else 
            xslStylesheetNode = xsltDocument.XslStylesheet("1.0");
            #endif
            #if SEPARATE_SUPPORTING_TEMPLATES
            string stpath = System.AppDomain.CurrentDomain.BaseDirectory + "supporting-templates.xslt";
            xslStylesheetNode.XslElement("import").AddAttributeWithValue("href", stpath);
            //xslStylesheetNode.XslElement("import").AddAttributeWithValue("href", @"file:\\D:\Programování\XCase\XCase-ev\Gui\bin\Debug\supporting-templates.xslt");
            #endif
            XmlElement xslOutputNode = xslStylesheetNode.XslElement("output");
            xslOutputNode.AddAttributeWithValue("method", "xml");
            xslOutputNode.AddAttributeWithValue("indent", "yes");
        }
        
        #endregion        
    }
}