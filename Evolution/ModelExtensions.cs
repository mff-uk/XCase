using System;
using System.Collections.Generic;
using System.Linq;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public static class PSMElementExt
    {
        public static XPathExpr XPathE(this PSMElement element)
        {
            return new XPathExpr(element.XPath);
        }

        public static bool ModelsElement(this PSMElement node)
        {
            return (node is PSMContentContainer) || (node is PSMClass && ((PSMClass)node).HasElementLabel) || (node is PSMAttribute) && ((PSMAttribute)node).AttributeContainer != null;
        }

        public static bool EncompassesContentForParentSignificantNode(this PSMElement element)
        {
            PSMClass childClass = element as PSMClass;
            if (childClass != null)
            {
                if (childClass.HasElementLabel)
                    return true;
                else if (childClass.IsStructuralRepresentative && EncompassesContentForParentSignificantNode(childClass.RepresentedPSMClass))
                    return true;
                else
                    return childClass.Components.Any(EncompassesContentForParentSignificantNode);
            }

            if (element is PSMContentContainer)
                return true;

            if (element is PSMAttributeContainer)
                return ((PSMAttributeContainer)element).PSMAttributes.Count > 0;

            if (element is PSMAssociation)
            {
                return EncompassesContentForParentSignificantNode(((PSMAssociation)element).Child);
            }

            if (element is PSMClassUnion)
            {
                return ((PSMClassUnion) element).Components.Any(EncompassesContentForParentSignificantNode);
            }
            return false;
        }

        /// <summary>
        /// Returns attributes under a node (when called for a superordinate component, attributes
        /// from all subordinate PSM classes without element labels are included in the result too)
        /// </summary>
        /// <param name="node">node where to search for attributes</param>
        /// <returns></returns>
        public static IEnumerable<NodeAttributeWrapper> GetAttributesUnderNode(this PSMElement node)
        {
            List<NodeAttributeWrapper> result = new List<NodeAttributeWrapper>();
            GetAttributesUnderNode(node, true, ref result);
            return result;
        }

        /// <summary>
        /// Adds attributes under a node into <param name="attributeList"/> (when called for a superordinate component, attributes
        /// from all subordinate PSM classes without element labels are included in the result too)
        /// </summary>
        /// <param name="node">node where to search for attributes</param>
        /// <param name="firstCall">false value indicates recursive call</param>
        /// <param name="attributeList">list in which attributes are added</param>
        private static void GetAttributesUnderNode(PSMElement node, bool firstCall, ref List<NodeAttributeWrapper> attributeList)
        {
            if (!firstCall && node is PSMClass
                && !((PSMClass)node).HasElementLabel
                && ((PSMClass)node).IsStructuralRepresentative)
            {
                PSMClass representedClass = ((PSMClass)node).RepresentedPSMClass;
                attributeList.Add(new StructuralRepresentativeAttributes((PSMClass)node, representedClass));
            }

            {
                PSMClass psmClass = node as PSMClass;
                if (psmClass != null)
                {
                    if (firstCall || (!psmClass.HasElementLabel))
                    {
                        foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
                        {
                            attributeList.Add(new SimpleNodeAttribute(psmAttribute));
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (node is PSMClass || (node is PSMContentContainer && firstCall))
            {
                foreach (PSMSubordinateComponent component in ((PSMSuperordinateComponent)node).Components)
                {
                    GetAttributesUnderNode(component, false, ref attributeList);
                }
            }

            if (node is PSMClassUnion)
            {
                //throw new NotImplementedException("Can't handle unions yet");
            }

            if (node is PSMContentChoice)
            {
                PSMContentChoice contentChoice = (PSMContentChoice)node;
                PSMTreeIterator it = new PSMTreeIterator(contentChoice);
                List<ChoiceAttributeOption> options = new List<ChoiceAttributeOption>();
                foreach (PSMElement childNode in it.GetChildNodes())
                {
                    List<NodeAttributeWrapper> choices = new List<NodeAttributeWrapper>();
                    GetAttributesUnderNode(childNode, false, ref choices);
                    if (choices.Count > 0)
                    {
                        ChoiceAttributeOption option = new ChoiceAttributeOption();
                        option.Items = choices;
                        options.Add(option);
                    }
                }
                if (options.Count > 0)
                {
                    ChoiceAttributes choiceAttributes = new ChoiceAttributes(contentChoice, options);
                    attributeList.Add(choiceAttributes);
                }
            }

            else if (node is PSMClassUnion)
            {
                PSMClassUnion classUnion = (PSMClassUnion)node;
                PSMTreeIterator it = new PSMTreeIterator(classUnion);
                List<ChoiceAttributeOption> options = new List<ChoiceAttributeOption>();
                foreach (PSMElement childNode in it.GetChildNodes())
                {
                    List<NodeAttributeWrapper> choices = new List<NodeAttributeWrapper>();
                    GetAttributesUnderNode(childNode, false, ref choices);
                    if (choices.Count > 0)
                    {
                        ChoiceAttributeOption option = new ChoiceAttributeOption();
                        option.Items = choices;
                        options.Add(option);
                    }
                }
                if (options.Count > 0)
                {
                    UnionAttributes choiceAttributes = new UnionAttributes(classUnion, options);
                    attributeList.Add(choiceAttributes);
                }
            }

            if (node is PSMAssociation)
            {
                GetAttributesUnderNode(((PSMAssociation)node).Child, false, ref attributeList);
            }
            return;
        }

        public static List<PSMAttribute> GetAttributesForGroup(this PSMClass groupNode, Version oldVersion)
        {
            // take elements in the group that used to be attributes 
            List<PSMAttribute> result = new List<PSMAttribute>();
            List<NodeElementWrapper> contentComponents = groupNode.GetSubtreeElements();
            foreach (NodeElementWrapper contentComponent in contentComponents)
            {
                if (contentComponent is SimpleNodeElement)
                {
                    SimpleNodeElement simpleNodeElement = ((SimpleNodeElement)contentComponent);
                    if (simpleNodeElement.Element is PSMAttribute)
                    {
                        PSMAttribute attribute = (PSMAttribute)simpleNodeElement.Element;
                        PSMAttribute attributeOldVersion = (PSMAttribute)attribute.GetInVersion(oldVersion);
                        if (attributeOldVersion != null)
                        {
                            PSMElement parent = (attributeOldVersion).Class;
                            if (parent is PSMClass && ((PSMClass)parent).HasElementLabel)
                                continue;

                            if (attribute.AttributeContainer != null && attributeOldVersion.AttributeContainer == null)
                            {
                                result.Add(attribute);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #region access to elements

        public static List<NodeElementWrapper> GetSubtreeElements(this PSMElement node)
        {
            PSMTreeIterator it = new PSMTreeIterator(node);
            List<NodeElementWrapper> contentList = new List<NodeElementWrapper>();

            //if (((PSMClass)node).IsStructuralRepresentative && node.ModelsElement())
            //{
            //    PSMClass representedClass = ((PSMClass)node).RepresentedPSMClass;
            //    contentList.Add(new StructuralRepresentativeElements((PSMClass)node, representedClass));
            //}

            foreach (PSMElement childNode in it.GetChildNodes())
            {
                GetSubtreeContentComponentsInclRoot(childNode, ref contentList);
            }
            return contentList;
        }

        private static void GetSubtreeContentComponentsInclRoot(PSMElement node, ref List<NodeElementWrapper> e)
        {
            List<NodeElementWrapper> contentList = e;

            bool? group = null;

            if ((node is PSMClass) || (node is PSMContentContainer))
            {
                if (node.ModelsElement())
                {
                    group = false;
                }
                else
                {
                    group = true;
                }
            }

            if (group != null) // i.e. (node is PSMClass) || (node is PSMContentContainer)
            {
                if (group == false) // not in group
                    contentList.Add(new SimpleNodeElement(node));
                else // in group
                {
                    if (EncompassesContentForParentSignificantNode(node))
                    {
                        ContentGroup cg = new ContentGroup();
                        cg.ContainingClass = (PSMClass)node;

                        if (((PSMClass)node).IsStructuralRepresentative && !node.ModelsElement())
                        {
                            PSMClass representedClass = ((PSMClass)node).RepresentedPSMClass;
                            cg.ContentComponents.Add(new StructuralRepresentativeElements((PSMClass)node, representedClass));
                        }

                        PSMTreeIterator it = new PSMTreeIterator(node);
                        foreach (PSMElement component in it.GetChildNodes())
                        {
                            List<NodeElementWrapper> tmp = new List<NodeElementWrapper>();
                            GetSubtreeContentComponentsInclRoot(component, ref tmp);
                            cg.ContentComponents.AddRange(tmp);
                        }
                        contentList.Add(cg);
                    }
                }
            }
            #region choice and union 
            else if (node is PSMAttributeContainer)
            {
                foreach (PSMAttribute psmAttribute in ((PSMAttributeContainer)node).PSMAttributes)
                {
                    contentList.Add(new SimpleNodeElement(psmAttribute));
                }
            }
            else if ((node is PSMContentChoice) || (node is PSMClassUnion))
            {
                PSMTreeIterator it = new PSMTreeIterator(node);
                List<ChoiceElementOption> options = new List<ChoiceElementOption>();
                foreach (PSMElement childNode in it.GetChildNodes())
                {
                    List<NodeElementWrapper> items = new List<NodeElementWrapper>();
                    GetSubtreeContentComponentsInclRoot(childNode, ref items);
                    if (items.Count > 0)
                    {
                        ChoiceElementOption option = new ChoiceElementOption();
                        option.Items = items;
                        options.Add(option);
                    }
                }
                if (options.Count > 0)
                {
                    if (node is PSMContentChoice)
                    {
                        ChoiceElements choiceElements = new ChoiceElements((PSMContentChoice) node, options);
                        contentList.Add(choiceElements);
                    }
                    else
                    {
                        UnionElements unionElements = new UnionElements((PSMClassUnion) node, options);
                        contentList.Add(unionElements);
                    }
                }
            }
            #endregion 
            else if (node is PSMAssociation)
            {
                GetSubtreeContentComponentsInclRoot(((PSMAssociation)node).Child, ref contentList);
            }
            return;
        }


        /// <summary>
        /// If <paramref name="node"/> is a structural representative of some class,
        /// method returns contents of the represented class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static List<NodeElementWrapper> GetRepresentedElements(this PSMElement node)
        {
            PSMClass psmClass = node as PSMClass;
            List<NodeElementWrapper> result = new List<NodeElementWrapper>();
            if (psmClass == null || !psmClass.IsStructuralRepresentative)
            {
                return result;
            }

            List<NodeElementWrapper> representedClassContent = psmClass.RepresentedPSMClass.GetSubtreeElements();
            result.AddRange(representedClassContent);
            return result;
        }

        #endregion

        #region access to attributes





        public static List<NodeAttributeWrapper> GetRepresentedAttributes(this PSMElement node)
        {
            PSMClass psmClass = node as PSMClass;
            List<NodeAttributeWrapper> result = new List<NodeAttributeWrapper>();
            if (psmClass == null || !psmClass.IsStructuralRepresentative)
            {
                return result;
            }

            IEnumerable<NodeAttributeWrapper> attributes = psmClass.RepresentedPSMClass.GetAttributesUnderNode();
            result.AddRange(attributes);
            return result;
        }

        public static bool EncompassesAttributesForParentSignificantNodeOrSelf(this PSMElement node)
        {
            if (node is PSMClass)
            {
                PSMClass c = (PSMClass)node;
                if (c.PSMAttributes.Count > 0)
                {
                    return true;
                }
            }
            return node.EncompassesAttributesForParentSignificantNode();
        }

        public static bool EncompassesAttributesForParentSignificantNode(this PSMElement child)
        {
            PSMClass childClass = child as PSMClass;
            if (childClass != null && !childClass.HasElementLabel)
            {
                if (childClass.PSMAttributes.Count > 0)
                    return true;
                if (childClass.IsStructuralRepresentative && childClass.RepresentedPSMClass.EncompassesAttributesForParentSignificantNode())
                    return true;
                return childClass.Components.Any(EncompassesAttributesForParentSignificantNode);
            }

            if (child is PSMClassUnion)
            {
                return ((PSMClassUnion)child).Components.Any(EncompassesAttributesForParentSignificantNode);
            }

            if (child is PSMAssociation)
            {
                return EncompassesAttributesForParentSignificantNode(((PSMAssociation)child).Child);
            }
            return false;
        }

        #endregion

        public static ContentGroup GetNodeAsContentGroup(this PSMClass node)
        {
            ContentGroup cg = new ContentGroup();
            cg.ContainingClass = node;
            List<NodeElementWrapper> subtreeContentComponents = cg.ContainingClass.GetSubtreeElements();
            List<NodeElementWrapper> representedComponents = cg.ContainingClass.GetRepresentedElements();
            if (representedComponents.Count > 0)
            {
                StructuralRepresentativeElements src = new StructuralRepresentativeElements(cg.ContainingClass, cg.ContainingClass.RepresentedPSMClass);
                cg.ContentComponents.Add(src);
            }
            cg.ContentComponents.AddRange(subtreeContentComponents);
            return cg;
        }
    }

    public static class PSMContentChoiceExt
    {
        public static List<NodeElementWrapper> FindElementChoices(this PSMContentChoice choice)
        {
            List<NodeElementWrapper> choices = choice.GetSubtreeElements();
            return choices;
            
        }
    }
}