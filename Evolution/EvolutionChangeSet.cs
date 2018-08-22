using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class EvolutionChangeSet: List<EvolutionChange>
    {
        public Version OldVersion { get; set; }
        public Version NewVersion { get; set; }
        public PSMDiagram Diagram { get; set; }

        public Dictionary<PSMElement, List<EvolutionChange>> changesByTargetSignificantNode;
        public Dictionary<PSMElement, List<EvolutionChange>> changesByTarget;
        public List<PSMElement> significantNodes;

        /// <summary>
        /// Nodes whose content is changed 
        /// </summary>
        public List<PSMElement> redNodes;

        /// <summary>
        /// Nodes that contain red nodes in their subtrees. 
        /// </summary>
        public List<PSMElement> blueNodes;

        /// <summary>
        /// Nodes that do not contain red nodes in their subtrees. 
        /// </summary>
        public List<PSMElement> greenNodes;

        /// <summary>
        /// Nodes that are roots of a group
        /// </summary>
        public List<PSMElement> groupNodes;

        /// <summary>
        /// Insignificant nodes that have a red (significant) node in their subtrees
        /// </summary>
        public List<PSMElement> insignificantBlueNodes;

        public EvolutionChangeSet(PSMDiagram diagram, IEnumerable<EvolutionChange> changes, Version oldVersion, Version newVersion)
        {
            Diagram = diagram;
            this.AddRange(changes);
            OldVersion = oldVersion;
            NewVersion = newVersion;
        }

        private void RemoveChangesNotRequiringRevalidation()
        {
            this.Where(c => !ChangesLookupManager.MayRequireRevalidation(c)).ToList();
            this.RemoveAll(c => !ChangesLookupManager.MayRequireRevalidation(c));
        }

        private void CategorizeNodes()
        {
            significantNodes = new List<PSMElement>();
            insignificantBlueNodes = new List<PSMElement>();
            redNodes = new List<PSMElement>();
            blueNodes = new List<PSMElement>();
            greenNodes = new List<PSMElement>();

            PSMTreeIterator it = new PSMTreeIterator(Diagram);

            // ReSharper disable MoreSpecificForeachVariableTypeAvailable
            foreach (PSMSuperordinateComponent root in Diagram.Roots)
                // ReSharper restore MoreSpecificForeachVariableTypeAvailable
            {
                it.CurrentNode = root;
                CategorizeSubtree(it);
            }
            FixStructuralRepresentatives();
            
            Debug.Assert(redNodes.Intersect(blueNodes).Count() == 0);
            Debug.Assert(blueNodes.Intersect(greenNodes).Count() == 0);
            Debug.Assert(greenNodes.Intersect(redNodes).Count() == 0);
        }

        private void IdentifyGroupNodes()
        {
            groupNodes = new List<PSMElement>();
            foreach (PSMElement element in Diagram.DiagramElements.Keys.OfType<PSMElement>())
            {
                PSMClass psmClass = element as PSMClass;
                if (psmClass == null)
                {
                    continue;
                }
                if (!psmClass.EncompassesContentForParentSignificantNode()
                    && !psmClass.EncompassesAttributesForParentSignificantNodeOrSelf())
                    continue;

                if (!psmClass.HasElementLabel)
                {
                    groupNodes.Add(element);
                    continue;
                }
                if (changesByTarget.ContainsKey(psmClass))
                {
                    ClassElementNameChange elc =
                        (ClassElementNameChange)
                        changesByTarget[psmClass].FirstOrDefault(
                            change => change is ClassElementNameChange && change.Element == psmClass);
                    if (elc != null && elc.ElementLabelAdded)
                    {
                        groupNodes.Add(element);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Marks nodes blue or red if there is a change in the represented
        /// class/subtree.
        /// </summary>
        private void FixStructuralRepresentatives()
        {
            PSMTreeIterator.NodeMarker additionalNodeMarker = IsContentGroupNode; 
            foreach (PSMClass sr in Diagram.DiagramElements.Keys.OfType<PSMClass>().Where(c => c.IsStructuralRepresentative))
            {
                foreach (PSMTreeIterator.NodeMarker nodeMarker in new[] { null, additionalNodeMarker})
                {
                    PSMElement representedClassSignificantAncestor = PSMTreeIterator.GetSignificantAncestorOrSelf(sr.RepresentedPSMClass, nodeMarker);
                    PSMElement structuralRepresentativeSignificantAncestor = PSMTreeIterator.GetSignificantAncestorOrSelf(sr, nodeMarker);
                    if (redNodes.Contains(representedClassSignificantAncestor))
                    {
                        redNodes.AddIfNotContained(structuralRepresentativeSignificantAncestor);
                        blueNodes.Remove(structuralRepresentativeSignificantAncestor);
                        greenNodes.Remove(structuralRepresentativeSignificantAncestor);
                        MakeAncestorsBlue(structuralRepresentativeSignificantAncestor, nodeMarker);
                    }
                    else if (blueNodes.Contains(representedClassSignificantAncestor) && !redNodes.Contains(structuralRepresentativeSignificantAncestor))
                    {
                        blueNodes.AddIfNotContained(structuralRepresentativeSignificantAncestor);
                        greenNodes.Remove(structuralRepresentativeSignificantAncestor);
                        MakeAncestorsBlue(structuralRepresentativeSignificantAncestor, nodeMarker);
                    }
                }
            }
        }

        public IEnumerable<PSMClass> FindNewStructuralRepresentatives()
        {
            List<PSMClass> result = new List<PSMClass>();
            foreach (List<EvolutionChange> evolutionChanges in changesByTarget.Values)
            {
                foreach (EvolutionChange change in evolutionChanges)
                {
                    {
                        ClassIsStructuralRepresentativeChange srChanged = change as ClassIsStructuralRepresentativeChange;
                        if (srChanged != null && (srChanged.InvalidatesAttributes || srChanged.InvalidatesContent))
                        {
                            if (srChanged.NewIsStructuralRepresentative)
                            {
                                result.Add(srChanged.PSMClass);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void MakeAncestorsBlue(PSMElement node, PSMTreeIterator.NodeMarker nodeMarker)
        {
            foreach (PSMElement ancestor in PSMTreeIterator.SignificantAncestors(node, nodeMarker))
            {
                if (!redNodes.Contains(ancestor))
                {
                    blueNodes.AddIfNotContained(ancestor);
                    greenNodes.Remove(ancestor);
                }
            }
        }

        private void CategorizeSubtree(PSMTreeIterator iterator)
        {
            if (iterator.CurrentNodeModelsElement() || IsContentGroupNode(iterator.CurrentNode))
            {
                significantNodes.Add(iterator.CurrentNode);

                bool isRed = false;
                if ((changesByTargetSignificantNode.ContainsKey(iterator.CurrentNode) && !changesByTargetSignificantNode[iterator.CurrentNode].All(c => c.Element == iterator.CurrentNode && c is ICanBeIgnoredOnTarget))
                    || GetState(iterator.CurrentNode) == EContentPlacementState.Added)
                {
                    redNodes.Add(iterator.CurrentNode);
                    isRed = true;
                }

                PSMElement cn = iterator.CurrentNode;
                foreach (PSMElement child in iterator.GetChildNodes())
                {
                    iterator.CurrentNode = child;
                    CategorizeSubtree(iterator);
                }

                iterator.CurrentNode = cn;

                PSMElement firstFoundRedChild = iterator.GetChildNodes().FirstOrDefault(child => (child is PSMAssociation) && redNodes.Contains(((PSMAssociation)child).Child) && IsContentGroupNode(((PSMAssociation)child).Child));
                if (!isRed && firstFoundRedChild != null)
                {
                    isRed = true;
                    int index = redNodes.IndexOf(((PSMAssociation)firstFoundRedChild).Child);
                    redNodes.Insert(index, iterator.CurrentNode);
                }

                if (!isRed)
                {
                    if (iterator.GetChildNodes().Any(child => redNodes.Contains(child) || blueNodes.Contains(child) || insignificantBlueNodes.Contains(child)))
                        blueNodes.Add(iterator.CurrentNode);
                    else
                        greenNodes.Add(iterator.CurrentNode);
                }
            }
            else
            {
                PSMElement cn = iterator.CurrentNode;
                foreach (PSMElement child in iterator.GetChildNodes())
                {
                    iterator.CurrentNode = child;
                    CategorizeSubtree(iterator);
                }
                iterator.CurrentNode = cn;

                if (iterator.GetChildNodes().Any(child => redNodes.Contains(child) || blueNodes.Contains(child) || insignificantBlueNodes.Contains(child)))
                    insignificantBlueNodes.Add(iterator.CurrentNode);
            }
        }

        public void Categorize()
        {
            RemoveChangesNotRequiringRevalidation();
            changesByTarget = GroupByTarget();
            IdentifyGroupNodes();
            changesByTargetSignificantNode = GroupByTargetSignificantNode();
            
            CategorizeNodes();
        }

        #region grouping changes

        private Dictionary<PSMElement, List<EvolutionChange>> GroupByTargetSignificantNode()
        {
            Dictionary<PSMElement, List<EvolutionChange>> groupChanges = GroupChanges(groupingFuncBySignificantNode);
            foreach (EvolutionChange change in this)
            {
                List<PSMElement> secondaryTargets = new List<PSMElement>();
                if (change is IDoubleTargetChange)
                    secondaryTargets.Add(((IDoubleTargetChange)change).SecondaryTarget);
                else if (change is ISubelementChange)
                    secondaryTargets.Add(change.Element);
                

                foreach (PSMElement secondaryTarget in secondaryTargets)
                {
                    if (secondaryTarget == null)
                        continue;

                    PSMElement secondaryTargetNV = (PSMElement)secondaryTarget.GetInVersion(change.NewVersion);

                    if (secondaryTargetNV != null)
                    {
                        if (secondaryTargetNV.ModelsElement())
                        {
                            //secondaryTarget = secondaryTarget;
                        }
                        else
                        {
                            secondaryTargetNV = PSMTreeIterator.GetSignificantAncestorOrSelf(secondaryTargetNV, IsContentGroupNode);
                        }

                        if (secondaryTargetNV != null)
                        {
                            if (!groupChanges.ContainsKey(secondaryTargetNV))
                                groupChanges[secondaryTargetNV] = new List<EvolutionChange>();
                            if (!groupChanges[secondaryTargetNV].Contains(change))
                                groupChanges[secondaryTargetNV].Add(change);
                        }
                    }
                }
            }

            foreach (KeyValuePair<PSMElement, List<EvolutionChange>> kvp in groupChanges.ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                PSMElement target = kvp.Key;
                List<EvolutionChange> changeList = kvp.Value;

                if (IsContentGroupNode(target))
                {
                    PSMElement groupParent = PSMTreeIterator.GetSignificantAncestorOrSelf(target);
                    if (groupParent == null)
                        groupParent = target; 
                    if (!groupChanges.ContainsKey(groupParent))
                    {
                        groupChanges[groupParent] = new List<EvolutionChange>();
                    }
                    foreach (EvolutionChange change in changeList)
                    {
                        if (!groupChanges[groupParent].Contains(change))
                        {
                            groupChanges[groupParent].Add(change);
                        }
                    }
                }
            }

            return groupChanges;
        }

        private Dictionary<PSMElement, List<EvolutionChange>> GroupByTarget()
        {
            Dictionary<PSMElement, List<EvolutionChange>> groupChanges = GroupChanges(groupingFuncByTarget);
            foreach (IDoubleTargetChange doubleTargetChange in this.OfType<IDoubleTargetChange>())
            {
                PSMElement secondaryTarget = doubleTargetChange.SecondaryTarget;
                if (secondaryTarget != null)
                {
                    secondaryTarget = (PSMElement)secondaryTarget.GetInVersion(((EvolutionChange)doubleTargetChange).NewVersion);

                    if (secondaryTarget != null)
                    {
                        if (!groupChanges.ContainsKey(secondaryTarget))
                            groupChanges[secondaryTarget] = new List<EvolutionChange>();
                        groupChanges[secondaryTarget].Add((EvolutionChange)doubleTargetChange);
                    }
                }
            }
            return groupChanges;
        }

        private Dictionary<PSMElement, List<EvolutionChange>> GroupChanges(GroupingHandler groupBy)
        {
            IEnumerable<IGrouping<PSMElement, EvolutionChange>> grouped;
            if (groupBy != null)
            {
                grouped = this.GroupBy(change => groupBy(change));
            }
            else
            {
                grouped = this.GroupBy(change => change.Element);
            }

            Dictionary<PSMElement, List<EvolutionChange>> result = new Dictionary<PSMElement, List<EvolutionChange>>();

            foreach (IGrouping<PSMElement, EvolutionChange> grouping in grouped)
            {
                result[grouping.Key] = grouping.ToList();
            }
            return result;
        }

        private delegate PSMElement GroupingHandler(EvolutionChange change);

        private PSMElement groupingFuncBySignificantNode(EvolutionChange change)
        {
            PSMElement target = (change is ISubelementChange) ? ((ISubelementChange)change).ChangedSubelement : change.Element;

            if (target.ModelsElement())
                return target;
            else
            {
                return PSMTreeIterator.GetSignificantAncestorOrSelf(target, IsContentGroupNode);
            }
        }

        private static PSMElement groupingFuncByTarget(EvolutionChange change)
        {
            PSMElement result; 
            if (change is ISubelementChange)
                result = ((ISubelementChange)change).ChangedSubelement;
            else
                result = change.Element;

            if (result is PSMAssociation)
                result = ((PSMAssociation)result).Child;

            return result;
        }

        #endregion 

        public void Verify()
        {
            this.Categorize();

            foreach (EvolutionChange evolutionChange in this)
            {
                evolutionChange.Verify();
                if (!ChangesLookupManager.MayRequireRevalidation(evolutionChange))
                {
                    Debug.Assert(!evolutionChange.InvalidatesAttributes);
                    Debug.Assert(!evolutionChange.InvalidatesContent);
                }
            }

            foreach (KeyValuePair<PSMElement, List<EvolutionChange>> kvp in this.changesByTarget)
            {
                List<EvolutionChange> changes = kvp.Value;
                int ca = changes.Count(c => c.Element == kvp.Key && c.EditType == EEditType.Addition);
                int cm = changes.Count(c => c.Element == kvp.Key && c.EditType == EEditType.Migratory);
                int cr = changes.Count(c => c.Element == kvp.Key && c.EditType == EEditType.Removal);
                Debug.Assert(ca + cr <= 1);
                Debug.Assert(cm == 0 || ca + cr == 0);
            }
        }

        /// <summary>
        /// Ridi se podle ChagnesByTarget!
        /// </summary>
        /// <param name="contentComponent"></param>
        /// <returns></returns>
        internal EContentPlacementState GetState(PSMElement contentComponent)
        {
            if (!changesByTarget.ContainsKey(contentComponent))
            {
                if (!contentComponent.ExistsInVersion(OldVersion) && contentComponent.ExistsInVersion(NewVersion))
                    return EContentPlacementState.Added;
                return EContentPlacementState.AsItWas;
            }
            Debug.Assert(!changesByTarget[contentComponent].Any(c => c.EditType == EEditType.Removal));
            EContentPlacementState result = EContentPlacementState.AsItWas;

            EvolutionChange ac = changesByTarget[contentComponent]
                //.Where(c => c.Element == contentComponent || (c is ISubelementChange && ((ISubelementChange)c).ChangedSubelement == contentComponent))
                .FirstOrDefault(c => c.EditType == EEditType.Addition);
            if (ac != null)
            {
                result = EContentPlacementState.Added;
            }
            
            EvolutionChange mc = changesByTarget[contentComponent]
                //.Where(c => c.Element == contentComponent || (c is ISubelementChange && ((ISubelementChange)c).ChangedSubelement == contentComponent))
                .FirstOrDefault(c => c.EditType == EEditType.Migratory);
            if (mc != null)
            {
                result = EContentPlacementState.Moved;
            }
            
            EvolutionChange sc = changesByTarget[contentComponent]
                //.Where(c => c.Element == contentComponent || (c is ISubelementChange && ((ISubelementChange)c).ChangedSubelement == contentComponent))
                .FirstOrDefault(c => c.EditType == EEditType.Sedentary);

            if (result == EContentPlacementState.AsItWas && sc != null)
            {
                if (!(sc is IChangeWithEditTypeOverride) || sc.Element != contentComponent)
                    result = EContentPlacementState.AsItWas; 
                else
                {
                    IChangeWithEditTypeOverride changeWithEditTypeOverride = ((IChangeWithEditTypeOverride)sc);
                    switch (changeWithEditTypeOverride.EditTypeOverride)
                    {
                        case EEditType.Addition:
                            result = EContentPlacementState.Added;
                            break;
                        case EEditType.Migratory:
                            result = EContentPlacementState.Moved;
                            break;
                        case EEditType.Sedentary:
                            result = EContentPlacementState.AsItWas;
                            break;
                        case EEditType.Removal:
                            result = EContentPlacementState.AsItWas;
                            break;
                    }
                }
            }
            return result;
        }

        public bool RepresentedContentInvalidated(PSMElement node)
        {
            {
                PSMClass psmClass = node as PSMClass;
                if (psmClass != null)
                {
                    if (psmClass.IsStructuralRepresentative && GetState(psmClass.RepresentedPSMClass) == EContentPlacementState.Added
                        && psmClass.RepresentedPSMClass.EncompassesContentForParentSignificantNode())
                                return true;

                    if (psmClass.IsStructuralRepresentative && ContentInvalidated(psmClass.RepresentedPSMClass))
                        return true;
                }
            }
            return false; 
        }

        public bool RepresentedAttributesInvalidated(PSMElement node)
        {
            {
                PSMClass psmClass = node as PSMClass;
                if (psmClass != null)
                {
                    if (psmClass.IsStructuralRepresentative && GetState(psmClass.RepresentedPSMClass) == EContentPlacementState.Added
                        && psmClass.RepresentedPSMClass.EncompassesAttributesForParentSignificantNode())
                            return true;
                    if (psmClass.IsStructuralRepresentative && AttributesInvalidated(psmClass.RepresentedPSMClass))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ridi se podle changesByTargetSignificantNode
        /// </summary>
        public bool ContentInvalidated(PSMElement significantNode)
        {
            if (GetState(significantNode) == EContentPlacementState.Added
                && significantNode.EncompassesContentForParentSignificantNode())
                return true;

            {
                PSMClass psmClass = significantNode as PSMClass;
                if (psmClass != null)
                {
                    if (psmClass.IsStructuralRepresentative
                        && FindNewStructuralRepresentatives().Contains(psmClass)
                        && (psmClass.RepresentedPSMClass.EncompassesContentForParentSignificantNode()))
                        return true;

                    if (psmClass.IsStructuralRepresentative && ContentInvalidated(psmClass.RepresentedPSMClass))
                        return true; 
                }


            }

            List<NodeElementWrapper> subtreeElements = significantNode.GetSubtreeElements();

            if (subtreeElements.InlineButLeaveSRContent()
                .OfType<StructuralRepresentativeElements>()
                .Any(src => ContentInvalidated(src.RepresentedPSMClass)))
                return true;

            if (subtreeElements.OfType<ContentGroup>().Any(group => ContentInvalidated(group.ContainingClass)))
                return true; 

            return changesByTargetSignificantNode.ContainsKey(significantNode) && 
                changesByTargetSignificantNode[significantNode].Any(c => c.InvalidatesContent);              
        }

        /// <summary>
        /// Ridi se podle changesByTargetSignificantNode
        /// </summary>
        public bool AttributesInvalidated(PSMElement significantNode)
        {
            if (GetState(significantNode) == EContentPlacementState.Added
                && significantNode.EncompassesAttributesForParentSignificantNode())
                return true;

            {
                PSMClass psmClass = significantNode as PSMClass;
                if (psmClass != null)
                {
                    if (psmClass.IsStructuralRepresentative 
                        && FindNewStructuralRepresentatives().Contains(psmClass)
                        && (psmClass.RepresentedPSMClass.EncompassesAttributesForParentSignificantNode() || psmClass.RepresentedPSMClass.Attributes.Count > 0))
                        return true; 

                    if (psmClass.IsStructuralRepresentative && AttributesInvalidated(psmClass.RepresentedPSMClass))
                        return true;
                }
            }

            IEnumerable<NodeAttributeWrapper> attributesUnderNode = significantNode.GetAttributesUnderNode();
            
            if (attributesUnderNode.OfType<StructuralRepresentativeAttributes>()
                .Any(sra => AttributesInvalidated(sra.RepresentedPSMClass)))
                return true;

            return changesByTargetSignificantNode.ContainsKey(significantNode) && 
                changesByTargetSignificantNode[significantNode].Any(c => c.InvalidatesAttributes);
        }

        public IMultiplicityChange GetMultiplicityChange(PSMElement psmElement)
        {
            if (psmElement is PSMAssociation)
            {
                AssociationMultiplicityChange find = this.OfType<AssociationMultiplicityChange>().FirstOrDefault(ec => ec.Association == psmElement);
                return find; 
            }
            else
            {
                Debug.Assert(changesByTarget[psmElement].Count(c => c is IMultiplicityChange) <= 1);
                return changesByTarget[psmElement].OfType<IMultiplicityChange>().FirstOrDefault();
            }
        }

        public bool MultiplicityChanged(PSMElement psmElement)
        {
            if (psmElement is PSMAssociation)
            {
                AssociationMultiplicityChange find = this.OfType<AssociationMultiplicityChange>().FirstOrDefault(ec => ec.Association == psmElement);
                return (find != null);
            }
            else
            {
                return changesByTarget.ContainsKey(psmElement) &&
                changesByTarget[psmElement].Any(change =>
                    (change.Element == psmElement && change is IMultiplicityChange)
                    || (change is AssociationMultiplicityChange && ((PSMAssociation)change.Element).Child == psmElement));
            }
        }

        /// <summary>
        /// Finds PSM classes that need to be treated as groups (Element label was added)
        /// </summary>
        public void FixGroups(ref List<NodeElementWrapper> nodeContents)
        {
            for (int index = 0; index < nodeContents.Count; index++)
            {
                NodeElementWrapper nodeContent = nodeContents[index];
                if (nodeContent is SimpleNodeElement)
                {
                    if (IsContentGroupNode(((SimpleNodeElement)nodeContent).Element))
                    {
                        ConvertToContentGroup(ref nodeContents, (SimpleNodeElement) nodeContent);
                    }
                }
            }
        }

        public bool IsContentGroupNode(PSMElement element)
        {
            return groupNodes.Contains(element);
        }

        public bool ContinueInGroup(PSMElement node)
        {
            PSMElement elementOldVersion = (PSMElement) node.GetInVersion(OldVersion);
            return (elementOldVersion != null && IsUnderContentGroup(elementOldVersion));
        }

        private static void ConvertToContentGroup(ref List<NodeElementWrapper> nodeContents, SimpleNodeElement nodeElement)
        {
            Debug.Assert(nodeElement.Element is PSMClass);
            int index = nodeContents.IndexOf(nodeElement);
            nodeContents.RemoveAt(index);
            ContentGroup cg = ((PSMClass) nodeElement.Element).GetNodeAsContentGroup();    
            nodeContents.Insert(index, cg);
        }

        public bool IsUnderContentGroup(PSMElement element)
        {
            PSMElement dummy;
            return IsUnderContentGroup(element, out dummy);
        }

        public bool IsUnderContentGroup(PSMElement element, out PSMElement groupElement)
        {
            //Debug.Assert(PSMTreeIterator.ModelsElement(element));
            groupElement = null;
            if (element.ModelsElement() /* && !(element is PSMAttribute) */)
                return false;
            PSMTreeIterator it = new PSMTreeIterator(element);
            while (it.CanGoToParent())
            {
                it.GoToParent();
                if (IsContentGroupNode(it.CurrentNode))
                {
                    groupElement = it.CurrentNode;
                    return true;
                } 
                if (it.CurrentNodeModelsElement())
                    return false;   
            }
            return false;
        }

        internal EContentPlacementState GetGroupState(ContentGroup currentContentGroup)
        {
            if (!changesByTarget.ContainsKey(currentContentGroup.ContainingClass))
            {
                return EContentPlacementState.AsItWas;
            }
            Debug.Assert(!changesByTarget[currentContentGroup.ContainingClass].Any(c => c.EditType == EEditType.Removal));
            EContentPlacementState result = EContentPlacementState.AsItWas;

            EvolutionChange mc = changesByTarget[currentContentGroup.ContainingClass].FirstOrDefault(c => c.EditType == EEditType.Migratory);
            if (mc != null)
            {
                result = EContentPlacementState.Moved;
            }
            
            if (currentContentGroup.ContainingClass.GetInVersion(OldVersion) == null)
            {
                return EContentPlacementState.Added;
            }
            
            return result;
        }

       
    }
}