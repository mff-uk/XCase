using System;
using System.Collections.Generic;
using System.Linq;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    /// <summary>
    /// Provides methods for manipulating XPath expressions.
    /// </summary>
    public static class XPathHelper
    {
        //public enum EProjectionResultType
        //{
        //    Ancestor,
        //    Descendant,
        //    Self,
        //    Absolute
        //}

        /// <summary>
        /// Converts <paramref name="pathTo"/>
        /// into a path relative to <paramref name="currentPath"/>.
        /// </summary>
        /// <param name="pathTo">projected path</param>
        /// <param name="currentPath">currentPath</param>
        /// <param name="inGroup">flag if current path can is group path</param>
        /// <returns>relative xpath expression</returns>
        private static XPathExpr ProjectXPath(XPathExpr currentPath, XPathExpr pathTo, bool inGroup)
        {
            //EProjectionResultType projectionType = EProjectionResultType.Absolute;
            if (currentPath == null)
            {
                return pathTo;
            }
            char[] separator = new[] {'/'};
            string[] cn_parts = currentPath.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
            string[] path_parts = pathTo.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);

            int countSame = 0;
            while (cn_parts.Length > countSame && path_parts.Length > countSame && cn_parts[countSame] == path_parts[countSame])
            {   
                countSame++;
            }

            string res = String.Empty;
            bool cgHandled = false;
            for (int j = countSame; j < path_parts.Length; j++)
            {
                // removed and replaced by variable construction 
                 if (inGroup && !cgHandled && cn_parts[j - 1] == XPathExpr.CurrentGroupVariableExpr)
                {
                    string name = path_parts[j];
                    // HACK: for attribute matching in groups @ is stripped from the name
                    name = name.TrimStart('@');
                    res += string.Format("$cg[name() = '{0}']", name);
                    //projectionType = EProjectionResultType.Descendant;
                    if (j < path_parts.Length - 1)
                        res += "/";
                    cgHandled = true;
                }
                else
                {
                    res += path_parts[j];
                    //projectionType = EProjectionResultType.Descendant;
                    if (j < path_parts.Length - 1)
                        res += "/";
                }
            }
            if (countSame > 0 && path_parts.Length == countSame)
            {
                //projectionType = EProjectionResultType.Self;
                // up
                for (int j = countSame; j < cn_parts.Length; j++)
                {
                    res += "../";
                }
                // and down again
                for (int j = countSame; j < cn_parts.Length; j++)
                {
                    res += cn_parts[j];
                    if (j < cn_parts.Length - 1)
                        res += "/";
                }
            }


            
            return new XPathExpr(res);
        }

        /// <summary>
        /// Converts path to <paramref name="psmElementNewVersion"/>
        /// into a path relative to <paramref name="context"/>.<see cref="XsltGeneratorContext.ProcessedPath"/>.
        /// (and also relative to <paramref name="context"/>.<see cref="XsltGeneratorContext.ContentGroupPath"/> if it is used).
        /// If context is in group (<see cref="XsltGeneratorContext"/>.<see cref="XsltGeneratorContext.InGroup"/>) which 
        /// has group attributes and <paramref name="psmElementNewVersion"/> is one of these attributes, the returned path 
        /// is modified to use the '$attributes' variable.  
        /// </summary>
        /// <param name="context">current generator context</param>
        /// <param name="psmElementNewVersion">element for which the path is projected</param>
        /// <param name="oldVersion">old version, used to determine the path to <paramref name="psmElementNewVersion"/> in the 
        /// old version of the diagram</param>
        /// <returns>relative xpath expression</returns>
        public static XPathExpr GroupAwareProjectXPath(XsltGeneratorContext context, PSMElement psmElementNewVersion, Version oldVersion)
        {
            XPathExpr oldPath = ((PSMElement)psmElementNewVersion.GetInVersion(oldVersion)).XPathE();
            if (context.InGroup && oldPath.HasPrefix(context.ContentGroupPath))
                oldPath = oldPath.InsertAfterPrefix(context.ContentGroupPath, "/$cg");
            XPathExpr tmp = ProjectXPath(context.ProcessedPath, oldPath, context.InGroup);
            if (context.InGroup && context.ContentGroupAttributes != null && psmElementNewVersion is PSMAttribute &&
                context.ContentGroupAttributes.Contains((PSMAttribute)psmElementNewVersion))
            {
                if (tmp.ToString().StartsWith(XPathExpr.CurrentGroupVariableExpr))
                {
                    tmp = new XPathExpr(tmp.ToString().Replace(XPathExpr.CurrentGroupVariableExpr, "$attributes"));
                }
            }

            return tmp;
        }

        /// <summary>
        /// Converts path <paramref name="oldPath"/>
        /// into a path relative to <paramref name="context"/>.<see cref="XsltGeneratorContext.ProcessedPath"/>.
        /// (and also relative to <paramref name="context"/>.<see cref="XsltGeneratorContext.ContentGroupPath"/> if it is used).
        /// </summary>
        /// <param name="context">current generator context</param>
        /// <param name="oldPath">projected path</param>
        /// <returns>relative xpath expression</returns>
        public static XPathExpr GroupAwareProjectXPathWithoutAttributeCorrection(XsltGeneratorContext context, XPathExpr oldPath)
        {
            if (context.InGroup && oldPath.HasPrefix(context.ContentGroupPath))
                oldPath = oldPath.InsertAfterPrefix(context.ContentGroupPath, "/$cg");
            XPathExpr tmp = ProjectXPath(context.ProcessedPath, oldPath, context.InGroup);
            return tmp;
        }

        /// <summary>
        /// <para>
        /// Returns expresion where <paramref name="attributes" /> are converted to their relative 
        /// XPath expressions and joined with "," operator. 
        /// </para>
        /// The XPath expressions are modified so that the part belonging to represented class
        /// is replaced with the part belonging to structural representative 
        /// </summary>
        /// <example>
        /// sequence {Item/prod-red/@ProductSet Item/prod-red/@Category}
        /// is returned as  "Item/prod-blue/@ProductSet, Item/prod-blue/@Category"
        /// where prod-blue is a structural representative of prod-red. 
        /// </example>
        public static XPathExpr AttributeListWithSRSubstitution(XsltGeneratorContext context, IEnumerable<NodeAttributeWrapper> attributes, IFromStructuralRepresentative structuralRepresentativeReplacement, bool onlyExisting, string concatOperator)
        {
            List<XPathExpr> modified = new List<XPathExpr>();
            foreach (PSMAttribute attribute in attributes.Inline())
            {
                if (onlyExisting && !attribute.ExistsInVersion(context.ChangeSet.OldVersion))
                    continue;
                XPathExpr expr;
                if (structuralRepresentativeReplacement != null)
                {
                    expr = GroupAwareProjectXPathWithSRSubstitution(context, attribute, structuralRepresentativeReplacement);
                }
                else
                {
                    expr = GroupAwareProjectXPath(context, attribute, context.ChangeSet.OldVersion);
                }

                modified.Add(expr);
            }

            return new XPathExpr(modified.ConcatWithSeparator(concatOperator));
        }

        /// <summary>
        /// <para>
        /// Returns expresion where <paramref name="nodeContents" /> are converted to their relative 
        /// XPath expressions and joined with "," operator. 
        /// </para>
        /// The XPath expressions are modified so that the part belonging to represented class
        /// is replaced with the part belonging to structural representative 
        /// </summary>
        /// <example>
        /// sequence {Item/prod-red/ProductSet Item/prod-red/Category}
        /// is returned as  "Item/prod-blue/ProductSet, Item/prod-blue/Category"
        /// where prod-blue is a structural representative of prod-red. 
        /// </example>
        public static XPathExpr ElementsListWithSRSubstitution(XsltGeneratorContext context, IEnumerable<NodeElementWrapper> nodeContents, IFromStructuralRepresentative structuralRepresentativeReplacement, string concatOperator)
        {
            List<XPathExpr> modified = new List<XPathExpr>();
            foreach (PSMElement element in nodeContents.Inline().Where(c => c.ModelsElement()))
            {
                XPathExpr expr;
                if (structuralRepresentativeReplacement != null)
                {
                    expr = GroupAwareProjectXPathWithSRSubstitution(context, element, structuralRepresentativeReplacement);
                }
                else
                {
                    expr = GroupAwareProjectXPath(context, element, context.ChangeSet.OldVersion);
                }
                modified.Add(expr);
            }

            return new XPathExpr(modified.ConcatWithSeparator(concatOperator));
        }

        /// <summary>
        /// Performs <see cref="GroupAwareProjectXPath"/> and substitutes steps in the result path according
        /// to structural representative information. (So the path is relative to the structural representative)
        /// </summary>
        /// <param name="context">The generator context.</param>
        /// <param name="element">The element.</param>
        /// <param name="structuralRepresentativeReplacement">The structural representative replacement.</param>
        /// <returns></returns>
        public static XPathExpr GroupAwareProjectXPathWithSRSubstitution(XsltGeneratorContext context, PSMElement element, IFromStructuralRepresentative structuralRepresentativeReplacement)
        {
            XPathExpr expr;
            
            if (structuralRepresentativeReplacement != null)
            {
                var cp = context.ProcessedPath;

                //if (PSMTreeIterator.AreInTheSamePSMTree(structuralRepresentativeReplacement.RepresentedPSMClass, structuralRepresentativeReplacement.StructuralRepresentative))


                PSMElement representedOldAnc = PSMTreeIterator.GetSignificantAncestorOrSelf((PSMClass)structuralRepresentativeReplacement.RepresentedPSMClass.GetInVersion(context.ChangeSet.OldVersion));
                PSMElement structuralRepresentativeOldAnc = PSMTreeIterator.GetSignificantAncestorOrSelf((PSMClass)structuralRepresentativeReplacement.StructuralRepresentative.GetInVersion(context.ChangeSet.OldVersion));

                if (representedOldAnc != null)
                    context.ProcessedPath = GetXPathForNode(representedOldAnc, context.ChangeSet.OldVersion);
                else
                {
                    context.ProcessedPath = new XPathExpr("<virt-root>");
                    if (context.InGroup)
                        context.ProcessedPath = context.ProcessedPath.Append("/$cg");
                }
                
                string from = representedOldAnc != null ? XsltTemplateNameManager.GetElementNameForSignificantElement(representedOldAnc) : null;
                string to = structuralRepresentativeOldAnc != null ? XsltTemplateNameManager.GetElementNameForSignificantElement(structuralRepresentativeOldAnc) : null;

                XPathExpr e = null;

                if (from != null && to != null &&
                    !(context.ProcessedPath.ToString().EndsWith(from) && cp.ToString().EndsWith(to)))
                {
                    e = ProjectXPath(cp, context.ProcessedPath, false);
                    if (!XPathExpr.IsNullOrEmpty(e) && !String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(from))
                    {
                        if (e.ToString().Contains(from))
                            e = new XPathExpr(e.ToString().Replace(from, to));    
                    }
                }
                expr = GroupAwareProjectXPath(context, element, context.ChangeSet.OldVersion);

                if (!XPathExpr.IsNullOrEmpty(e))
                    // ReSharper disable PossibleNullReferenceException
                    expr = e.Append("/" + expr);
                    // ReSharper restore PossibleNullReferenceException

                context.ProcessedPath = cp;
            }
            else
            {
                expr = GroupAwareProjectXPath(context, element, context.ChangeSet.OldVersion);
            }
            
            return expr;
        }

        /// <summary>
        /// Gets path to a content grop
        /// </summary>
        /// <param name="contentGroup">The content group.</param>
        /// <param name="oldVersion">The old version.</param>
        /// <returns>path to a content group</returns>
        public static XPathExpr GetXPathForContentGroup(ContentGroup contentGroup, Version oldVersion)
        {
            return GetXPathForContentGroup(contentGroup.ContainingClass, oldVersion);
        }

        /// <summary>
        /// Gets path to a content grop
        /// </summary>
        /// <param name="containingClass">The containing class of the group.</param>
        /// <param name="oldVersion">The old version.</param>
        /// <returns>path to a content group</returns>
        private static XPathExpr GetXPathForContentGroup(PSMElement containingClass, Version oldVersion)
        {
            PSMElement groupElementOldVersion = (PSMElement)containingClass.GetInVersion(oldVersion);
            if (groupElementOldVersion != null)
                return groupElementOldVersion.XPathE();
            else
                return GetXPathForAddedNode(containingClass, oldVersion).Append("/$cg");
        }

        /// <summary>
        /// Gets the X path for a node. If the node existed in the previous version, the old
        /// path is returned. For new nodes it returns the path to the closest ancestor 
        /// that existed in the previous version. 
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="oldVersion">The old version.</param>
        /// <returns>xpath expression</returns>
        public static XPathExpr GetXPathForNode(PSMElement node, Version oldVersion)
        {
            PSMElement nodeOldVersion = (PSMElement)node.GetInVersion(oldVersion);
            if (nodeOldVersion != null)
                return nodeOldVersion.XPathE();
            else
                return GetXPathForAddedNode(node, oldVersion);
        }

        /// <summary>
        /// returns the path to the closest ancestor of <see cref="addedNode"/>
        /// that existed in the previous version. 
        /// </summary>
        /// <param name="addedNode">The added node.</param>
        /// <param name="oldVersion">The old version.</param>
        /// <returns>xpath expression</returns>
        private static XPathExpr GetXPathForAddedNode(PSMElement addedNode, Version oldVersion)
        {
            PSMTreeIterator it = new PSMTreeIterator(addedNode);
            PSMElement ancestorOldVersion = null;
            while (ancestorOldVersion == null)
            {
                if (it.CanGoToParent())
                    it.GoToParent();
                else
                    return null;
                it.CurrentNode = it.GetSignificantAncestorOrSelf();
                if (it.CurrentNode != null)
                {
                    ancestorOldVersion = (PSMElement)it.CurrentNode.GetInVersion(oldVersion);
                }
                else
                {
                    break;
                }
            }
            return ancestorOldVersion != null ? ancestorOldVersion.XPathE() : null;
        }

        #region SR path expansion

        private class FollowedPath: List<PSMElement>
        {
            public XPathExpr ToXPath()
            {
                string _p = String.Empty;
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    PSMElement psmElement = this[i];
                    if (psmElement.ModelsElement())
                        _p += "/" + XsltTemplateNameManager.GetElementNameForSignificantElement(psmElement);
                    else if (psmElement is PSMAttribute)
                    {
                        _p += "/@" + ((PSMAttribute) psmElement).AliasOrName;
                    }
                }
                return new XPathExpr(_p);
            }

            public FollowedPath Copy()
            {
                FollowedPath followedPath = new FollowedPath();
                followedPath.AddRange(this);
                return followedPath;
            }
        }
            
        /// <summary>
        /// Returns all XPath expressions where PSM element can appear in an XML document. Each structural
        /// representative in a diagram can contribute with one expression, where an element can appear.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<XPathExpr> PathsWhereElementAppears(PSMElement element)
        {
            List<FollowedPath> result = new List<FollowedPath>();

            FollowedPath followedPath = new FollowedPath();
            
            result.AddRange(FollowPath(element, followedPath));

            return from p in result 
                   where p.Count > 0 && PSMTreeIterator.IsInSignificantSubtree(p.Last())
                   select p.ToXPath();
        }

        private static IEnumerable<FollowedPath> FollowPath(PSMElement element, FollowedPath followedPath)
        {
            List<FollowedPath> result = new List<FollowedPath>();
            PSMTreeIterator it = new PSMTreeIterator(element);
            followedPath.Add(element);
            while (it.CanGoToParent())
            {
                it.GoToParent();
                PSMClass psmClass = it.CurrentNode as PSMClass;
                if (psmClass != null && psmClass.IsReferencedFromStructuralRepresentative())
                {
                    foreach (PSMClass representative in 
                        psmClass.Diagram.DiagramElements.Keys.OfType<PSMClass>().Where(rClass => rClass.RepresentedPSMClass == psmClass))
                    {
                        result.AddRange(FollowPath(representative, followedPath.Copy()));    
                    }
                }
                followedPath.Add(it.CurrentNode);
            }
            if (followedPath.Last() is PSMClass && ((PSMClass)followedPath.Last()).HasElementLabel)
            {
                result.Add(followedPath);
            }
            return result;
        }

        #endregion

        #region simple XPath manipulations

        /// <summary>
        /// Returns XPath expression without it's last step. 
        /// </summary>
        /// <param name="xpath">xpath</param>
        public static string RemoveLastStep(string xpath)
        {
            return xpath.Remove(xpath.LastIndexOf('/'), xpath.Length - xpath.LastIndexOf('/'));
        }

        /// <summary>
        /// Adds '[1]' predicate to each step (thus the xpath expression only selects the first child in each step).
        /// </summary>
        /// <param name="xpath">xpath</param>
        /// <param name="leaveLastStep">if set to <c>true</c> leaves the last step intact.</param>
        /// <returns></returns>
        public static XPathExpr AllwaysReferenceFirstChild(XPathExpr xpath, bool leaveLastStep)
        {
            string tmp = xpath.ToString();
            System.Text.RegularExpressions.Regex r = 
                new System.Text.RegularExpressions.Regex("/");
            tmp = tmp.Replace("$cg/", "__CG__");
            string tmp2 = r.Replace(tmp, "[1]/", int.MaxValue, 1);
            tmp2 = tmp2.Replace("__CG__", "$cg/");
            int p = tmp2.LastIndexOf("/");
            if (!leaveLastStep && !tmp2.EndsWith("/") && tmp2[p+1] != '@' && tmp2[p+1] != '[')
                tmp2 = tmp2 + "[1]";
            //!tmp2.StartsWith("/") && !tmp2.StartsWith("@") && !tmp2.EndsWith(XPathExpr.CurrentGroupVariableExpr))
                //tmp2 = tmp2.Insert(tmp.Contains("/") ? tmp2.IndexOf("/") : tmp2.Length, "[1]");
            return new XPathExpr(tmp2);
        }

        #endregion 
    }
}