using System;
using System.Collections.Generic;
using XCase.Model;
using System.Linq;

namespace XCase.Evolution
{
    public class RepresentedClassCallableTemplateInfo
    {
        public bool FullAttributesTemplate { get; set; }
        public bool FullElementsTemplate { get; set; }
        public bool CopyAttributesTemplate { get; set; }
        public bool CopyElementsTemplate { get; set; }
        public bool IsGroupAware { get; set; }
        public bool WasRegeneratedForGroup { get; set; }
        public static bool FullAttributeTemplateNeeded(PSMElement element, EvolutionChangeSet changeSet)
        {
            List<NodeAttributeWrapper> allAttributes = new List<NodeAttributeWrapper>();
            List<NodeAttributeWrapper> represenetedAttributes = element.GetRepresentedAttributes();
            IEnumerable<NodeAttributeWrapper> nodeContents = element.GetAttributesUnderNode();
            allAttributes.AddRange(represenetedAttributes);
            allAttributes.AddRange(nodeContents);
            return (allAttributes.Count() > 0 && changeSet.AttributesInvalidated(element)
                /*&& allAttributes.Inline().Any(e => PSMTreeIterator.GetLowerMultiplicityOfContentElement(e) != 0)*/);
        }

        public static bool FullElementsTemplateNeeded(PSMElement element, EvolutionChangeSet changeSet)
        {
            List<NodeElementWrapper> allContent = new List<NodeElementWrapper>();
            List<NodeElementWrapper> represenetedContents = element.GetRepresentedElements();
            List<NodeElementWrapper> nodeContents = element.GetSubtreeElements();
            allContent.AddRange(represenetedContents);
            allContent.AddRange(nodeContents);
            return (allContent.Count() > 0 && changeSet.ContentInvalidated(element)
                /*&& allContent.Inline().Any(e => PSMTreeIterator.GetLowerMultiplicityOfContentElement(e) != 0)*/);
        }
    }
}