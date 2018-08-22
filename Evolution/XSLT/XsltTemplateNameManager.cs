using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using XCase.Evolution.Xslt;
using XCase.Model;

namespace XCase.Evolution
{
    public class XsltTemplateNameManager
    {
        public XsltTemplateNameManager(XmlElement xslStylesheetNode)
        {
            XslStylesheetNode = xslStylesheetNode;
            AppliedTemplates = new Dictionary<PSMElement, XmlElement>();
            NamedTemplates = new Dictionary<PSMElement, XmlElement>();
            RedNodeTemplates = new Dictionary<PSMElement, XmlElement>();
            Reset();
        }

        public void Reset()
        {
            callableNodeTemplates.Clear();
            forceCallableNodeTemplates.Clear();
            callableGroupTemplates.Clear();
            forceCallableGroupTemplates.Clear();
            RequestedTemplates.Clear();
            representedContentTemplatesElmFC.Clear();
            representedContentTemplatesElm.Clear();
            representedContentTemplatesAttFC.Clear();
            representedContentTemplatesAtt.Clear();
            RepresentedClassesInfo.Clear();
            iterationsTemplates.Clear();
            RedNodeTemplates.Clear();
            AppliedTemplates.Clear();
            NamedTemplates.Clear();
            forceCallableUnionTemplates.Clear();
            unionTemplates.Clear();
        }

        private static string suggestNameFromXPath(PSMElement node)
        {
            return node.XPath.Replace("<virt-root>", "virt-root/").Replace("/", "-").Trim('-');
        }

        public static string GetElementNameForSignificantElement(PSMElement node)
        {
            PSMTreeIterator it = new PSMTreeIterator(node);
            // TODO: prozkoumat
            //Debug.Assert(it.CurrentNodeModelsElement(), "Node is not correct significant node.");
            if (node is PSMClass)
            {
                Debug.Assert(((PSMClass)node).HasElementLabel);
                return ((PSMClass) node).ElementName;
            }
            if (node is PSMContentContainer)
            {
                return node.Name;
            }
            if (node is PSMAttribute)
            {
                Debug.Assert(((PSMAttribute)node).AttributeContainer != null);
                return ((PSMAttribute) node).AliasOrName;
            }
            // should never get here...
            throw new ArgumentException("Node is not correct significant node.");
        }

        public readonly Dictionary<PSMClass, RepresentedClassCallableTemplateInfo> RepresentedClassesInfo
            = new Dictionary<PSMClass, RepresentedClassCallableTemplateInfo>();

        public readonly List<KeyValuePair<PSMElement, bool>> RequestedTemplates = new List<KeyValuePair<PSMElement, bool>>();

        private readonly Dictionary<PSMElement, string> callableGroupTemplates = new Dictionary<PSMElement, string>();
        private readonly Dictionary<PSMElement, string> forceCallableGroupTemplates = new Dictionary<PSMElement, string>();

        private readonly Dictionary<PSMElement, string> forceCallableUnionTemplates = new Dictionary<PSMElement, string>();
        private readonly Dictionary<PSMElement, string> unionTemplates = new Dictionary<PSMElement, string>();

        const string UNION_SUFFIX = "-UNION";
        const string FORCE_CALLABLE_SUFFIX = "-FC";
        const string REPRESENTED_CONTENT_SUFFIX = "-SR";
        const string ATTRIBUTES_SUFFIX = "-ATT";
        const string ELEMENTS_SUFFIX = "-ELM";

        private readonly Dictionary<PSMElement, string> callableNodeTemplates = new Dictionary<PSMElement, string>();

        private readonly Dictionary<PSMElement, string> forceCallableNodeTemplates = new Dictionary<PSMElement, string>();

        public bool ForceCallableNodeTemplateExists(PSMElement node)
        {
            return forceCallableNodeTemplates.ContainsKey(node);
        }

        public string GetNodeCallableTemplate(PSMElement node, bool forceCallable)
        {
            Dictionary<PSMElement, string> templateCollection = forceCallable ? forceCallableNodeTemplates : callableNodeTemplates;
            if (templateCollection.ContainsKey(node))
                return templateCollection[node];
            else
            {
                string name = suggestNameFromXPath(node);
                if (forceCallable)
                    name += FORCE_CALLABLE_SUFFIX;
                templateCollection[node] = name;
                return name;
            }
        }

        public string GetGroupCallableTemplate(ContentGroup contentGroup, bool forceCallable)
        {
            Dictionary<PSMElement, string> templateCollection = forceCallable ? forceCallableGroupTemplates : callableGroupTemplates;
            if (templateCollection.ContainsKey(contentGroup.ContainingClass))
            {
                return templateCollection[contentGroup.ContainingClass];
            }
            else
            {
                string name = suggestNameFromXPath(contentGroup.ContainingClass);
                name += "-GROUP-" + contentGroup.ContainingClass.Name;
                if (forceCallable)
                    name += FORCE_CALLABLE_SUFFIX;
                templateCollection[contentGroup.ContainingClass] = name;
                return name;
            }
        }

        public enum ERepresentedTemplatePart { Attributes, Elements }

        private readonly Dictionary<PSMElement, string> representedContentTemplatesAttFC = new Dictionary<PSMElement, string>();
        private readonly Dictionary<PSMElement, string> representedContentTemplatesAtt = new Dictionary<PSMElement, string>();
        private readonly Dictionary<PSMElement, string> representedContentTemplatesElmFC = new Dictionary<PSMElement, string>();
        private readonly Dictionary<PSMElement, string> representedContentTemplatesElm = new Dictionary<PSMElement, string>();

        public bool ForceCallableGroupTemplateExists(ContentGroup node)
        {
            return forceCallableGroupTemplates.ContainsKey(node.ContainingClass);
        }
        
        public string GetRepresentedContentTemplate(PSMClass containingClass, ERepresentedTemplatePart part, bool forceCallable)
        {
            Dictionary<PSMElement, string> templateCollection;
            if (forceCallable)
            {
                templateCollection = part == ERepresentedTemplatePart.Attributes ? representedContentTemplatesAttFC : representedContentTemplatesElmFC;
            }
            else
            {
                templateCollection = part == ERepresentedTemplatePart.Attributes ? representedContentTemplatesAtt : representedContentTemplatesElm;
            }
            string name;
            if (templateCollection.ContainsKey(containingClass))
                name = templateCollection[containingClass];
            else
            {
                name = suggestNameFromXPath(containingClass);
                if (!containingClass.HasElementLabel)
                    name += "/" + containingClass.Name.ToUpper();
                name = name.Replace("/", "-").Trim('-');
                name += REPRESENTED_CONTENT_SUFFIX;
                if (part == ERepresentedTemplatePart.Attributes)
                {
                    name += ATTRIBUTES_SUFFIX;
                }
                else
                {
                    name += ELEMENTS_SUFFIX;
                }
                if (forceCallable)
                {
                    name += FORCE_CALLABLE_SUFFIX;
                }
                templateCollection[containingClass] = name;
            }
            
            return name;
        }

        public string GetRepresentedContentTemplate(StructuralRepresentativeElements structuralRepresentativeElements, ERepresentedTemplatePart part, bool forceCallable)
        {
            return GetRepresentedContentTemplate(structuralRepresentativeElements.RepresentedPSMClass, part, forceCallable);
        }

        public bool ForceCallableSRTemplateExists(PSMClass representedClass)
        {
            foreach (ERepresentedTemplatePart part in new[] {ERepresentedTemplatePart.Attributes, ERepresentedTemplatePart.Elements})
            {
                Dictionary<PSMElement, string> templateCollection;
                if (part == ERepresentedTemplatePart.Attributes)
                    templateCollection = representedContentTemplatesAttFC;
                else
                    templateCollection = representedContentTemplatesElmFC;
                
                if (templateCollection.ContainsKey(representedClass))
                    return true; 
            }
            return false; 
        }

        public bool ForceCallableUnionTemplateExists(PSMClassUnion classUnion)
        {
            return forceCallableUnionTemplates.ContainsKey(classUnion);
        }

        public string GetUnionTemplate(PSMClassUnion classUnion, bool forceCallable)
        {
            Dictionary<PSMElement, string> templateCollection = forceCallable ? forceCallableUnionTemplates : unionTemplates;
            if (templateCollection.ContainsKey(classUnion))
                return templateCollection[classUnion];
            else
            {
                string name = suggestNameFromXPath(classUnion) + UNION_SUFFIX;
                if (forceCallable)
                    name += FORCE_CALLABLE_SUFFIX;
                templateCollection[classUnion] = name;
                return name;
            }
        }

        public void RegisterForceCallableSRTemplate(PSMClass representedClass, ERepresentedTemplatePart part, string templateName)
        {
            Dictionary<PSMElement, string> templateCollection;
            if (part == ERepresentedTemplatePart.Attributes)
                templateCollection = representedContentTemplatesAttFC;
            else
                templateCollection = representedContentTemplatesElmFC;
            templateCollection[representedClass] = templateName;
        }

        #region 

        XmlElement XslStylesheetNode { get; set; }

        public static string SuggestTemplateName(PSMElement node)
        {
            PSMTreeIterator t = new PSMTreeIterator(node);
            string name = string.Empty;

            while (t.CanGoToParent())
            {
                string step = t.CurrentNodeModelsElement() ? GetElementNameForSignificantElement(t.CurrentNode) : t.CurrentNode.Name;
                name = !string.IsNullOrEmpty(step) ? step + "-" + name : name;
                t.GoToParent();
            }

            if (t.CurrentNode != null)
            {
                string step = t.CurrentNodeModelsElement() ? GetElementNameForSignificantElement(t.CurrentNode) : t.CurrentNode.Name;
                name = step + "-" + name;
            }

            return name.Trim('-') + "-iteration";
        }

        public string GetOrCreateIterationTemplate(PSMElement node, string calledTemplate)
        {
            return GetOrCreateIterationTemplate(SuggestTemplateName(node), calledTemplate);
        }

        public string GetOrCreateIterationTemplate(string name, string calledTemplate)
        {
            if (!iterationsTemplates.ContainsKey(name))
            {
                iterationsTemplates[name] = name;
                XslStylesheetNode.CreateComment(string.Format("iteration template {0}", name));
                XmlElement templateElement = XslStylesheetNode.XslNamedTemplate(name, "iterations");
                XmlElement ifElement = templateElement.XslIf(new XPathExpr("$iterations > 0"));
                ifElement.XslCallTemplate(calledTemplate);
                ifElement.XslCallTemplate(name, new TemplateParameter("iterations", new XPathExpr("$iterations - 1")));
            }
            return iterationsTemplates[name];
        }

        private readonly Dictionary<string, string> iterationsTemplates = new Dictionary<string, string>();

        public Dictionary<PSMElement, XmlElement> AppliedTemplates { get; private set; }

        public Dictionary<PSMElement, XmlElement> NamedTemplates { get; private set; }

        public Dictionary<PSMElement, XmlElement> RedNodeTemplates { get; private set; }

        #endregion
    }
}