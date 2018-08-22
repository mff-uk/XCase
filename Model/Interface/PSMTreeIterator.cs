using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUml.Uml2;

namespace XCase.Model
{
    public class PSMTreeIterator : ICloneable
    {
        public PSMDiagram Diagram { get; private set; }

        public PSMElement CurrentNode { get; set; }

        public PSMTreeIterator(PSMDiagram diagram)
        {
            Diagram = diagram;
        }

        public PSMTreeIterator(PSMElement currentNode)
            : this(currentNode.Diagram)
        {
            CurrentNode = currentNode;
        }

        public object Clone()
        {
            PSMTreeIterator psmTreeIterator = new PSMTreeIterator(Diagram) { CurrentNode = this.CurrentNode };
            return psmTreeIterator;
        }

        public void SetToRoot(int i)
        {
            if (Diagram.Roots.Count == 0)
            {
                throw new PSMDiagramException("Diagram has no roots", Diagram);
            }
            else if (Diagram.Roots.Count - 1 < i)
            {
                throw new PSMDiagramException("Index out of range", Diagram);
            }
            CurrentNode = Diagram.Roots[i];
        }

        public bool CanGoToParent()
        {
            PSMClass cp = CurrentNode as PSMClass;

            return (cp == null || !Diagram.RootsWithSpecifications.Contains(cp));
        }

        public PSMElement GetParent()
        {
            PSMElement parent = null;
            {
                PSMClass cl = CurrentNode as PSMClass;
                if (cl != null)
                {
                    if (cl.ParentAssociation != null)
                    {
                        parent = cl.ParentAssociation.Parent;
                        return parent;
                    }
                    if (cl.ParentUnion != null)
                    {
                        parent = cl.ParentUnion;
                        return parent;
                    }
                    if (Diagram.Roots.Contains(cl))
                    {
                        return null;
                    }
                    if (cl.Generalizations.Count > 0)
                    {
                        PSMTreeIterator tmp = new PSMTreeIterator((PSMClass)cl.Generalizations[0].General);
                        return tmp.GetParent();
                    }
                }
            }

            {
                PSMSubordinateComponent cs = CurrentNode as PSMSubordinateComponent;
                if (cs != null)
                {
                    parent = cs.Parent;
                    return parent;
                }
            }

            {
                PSMClassUnion cu = CurrentNode as PSMClassUnion;
                if (cu != null)
                {
                    parent = cu.ParentAssociation.Parent;
                    return parent;
                }
            }

            {
                PSMAttribute ca = CurrentNode as PSMAttribute;
                if (ca != null)
                {
                    if (ca.AttributeContainer != null)
                        return ca.AttributeContainer;
                    else
                        return ca.Class;
                }
            }

            return parent;
        }

        public void GoToParent()
        {
            CurrentNode = GetParent();
        }

        public IEnumerable<PSMElement> GetChildNodes()
        {
            {
                PSMSuperordinateComponent cs = CurrentNode as PSMSuperordinateComponent;
                if (cs != null)
                {
                    //return cs.Components.Select(c => (c is PSMAssociation) ? (PSMElement)((PSMAssociation)c).Child : (PSMElement)c);
                    return cs.Components.Cast<PSMElement>();
                }
            }

            {
                PSMClassUnion cu = CurrentNode as PSMClassUnion;
                if (cu != null)
                {
                    return cu.Components.Cast<PSMElement>();
                }
            }

            {
                PSMAssociation a = CurrentNode as PSMAssociation;
                if (a != null)
                {
                    return new PSMElement[] { a.Child };
                }
            }

            {
                PSMAttributeContainer ac = CurrentNode as PSMAttributeContainer;
                if (ac != null)
                {
                    return ac.PSMAttributes.Cast<PSMElement>();
                }
            }

            return new PSMElement[0];
        }

        public static PSMElement GetSignificantAncestorOrSelf(PSMElement element)
        {
            return GetSignificantAncestorOrSelf(element, null);
        }

        public static IEnumerable<PSMElement> SignificantAncestors(PSMElement element, NodeMarker nodeMarker)
        {
            List<PSMElement> result = new List<PSMElement>();
            PSMTreeIterator it = new PSMTreeIterator(element);

            while (it.CanGoToParent())
            {
                it.GoToParent();
                PSMElement significantAncestorOrSelf = it.GetSignificantAncestorOrSelf(nodeMarker);
                result.AddIfNotContained(significantAncestorOrSelf);
            } 

            return result;
        }

        public delegate bool NodeMarker(PSMElement element);

        public static PSMElement GetSignificantAncestorOrSelf(PSMElement element, NodeMarker additionalSignificantNodeMarker)
        {
            PSMTreeIterator helper = new PSMTreeIterator(element);

            while (!((helper.CurrentNode is PSMClass && ((PSMClass)helper.CurrentNode).HasElementLabel) 
                     || helper.CurrentNode is PSMContentContainer
                     || (additionalSignificantNodeMarker != null && additionalSignificantNodeMarker(helper.CurrentNode)))
                   && helper.CanGoToParent())
            {
                helper.GoToParent();
            }

            if ((helper.CurrentNode is PSMClass && ((PSMClass)helper.CurrentNode).HasElementLabel)
                || helper.CurrentNode is PSMContentContainer
                || (additionalSignificantNodeMarker != null && additionalSignificantNodeMarker(helper.CurrentNode)))
                return helper.CurrentNode;
            //else if (!helper.CanGoToParent())
            //    return helper.CurrentNode;
            else
                return null;
        }

        public PSMElement GetSignificantAncestorOrSelf()
        {
            return GetSignificantAncestorOrSelf(CurrentNode);   
        }

        public PSMElement GetSignificantAncestorOrSelf(NodeMarker additionalSignificantNodeMarker)
        {
            return GetSignificantAncestorOrSelf(CurrentNode, additionalSignificantNodeMarker);
        }

        public bool CurrentNodeModelsElement()
        {
            return (CurrentNode is PSMContentContainer) || (CurrentNode is PSMClass && ((PSMClass)CurrentNode).HasElementLabel) || (CurrentNode is PSMAttribute) && ((PSMAttribute)CurrentNode).AttributeContainer != null;
        }

        public static bool NodeIsUnderRepresentedClass(PSMElement redNode)
        {
            PSMTreeIterator it = new PSMTreeIterator(redNode);
            while (it.CanGoToParent())
            {
                it.GoToParent();
                PSMClass psmClass = it.CurrentNode as PSMClass;
                if (psmClass != null && psmClass.IsReferencedFromStructuralRepresentative())
                {
                    return true;
                }
            }
            return false; 
        }

        public override string ToString()
        {
            return string.Format("Iterator, CN: {0}, [{1}]", CurrentNode, CurrentNode.GetType().Name);
        }
        
        #region reading multiplicity 

        public static uint GetLowerMultiplicityOfContentElement(PSMElement node)
        {
            {
                PSMAssociationChild c = node as PSMAssociationChild;
                if (c != null)
                {
                    if (c.ParentUnion != null)
                    {
                        return 1;
                    }
                    if (c.ParentAssociation != null)
                    {
                        Debug.Assert(c.ParentAssociation.Lower != null);
                        return c.ParentAssociation.Lower.Value;
                    }
                    else return 1; // root class
                }
            }
            if (node is PSMContentContainer)
            {
                return 1;
            }
            if (node is PSMAttribute)
            {
                Debug.Assert(((PSMAttribute)node).Lower != null);
                return ((PSMAttribute)node).Lower.Value;
            }
            // should never get here...
            throw new ArgumentException();
        }

        public static UnlimitedNatural GetUpperMultiplicityOfContentElement(PSMElement node)
        {
            {
                PSMClass c = node as PSMClass;
                if (c != null)
                {
                    if (c.ParentUnion != null)
                    {
                        throw new NotImplementedException("Can't handle class union yet. ");
                    }
                    if (c.ParentAssociation != null)
                    {
                        return c.ParentAssociation.Upper;//.Value;
                    }
                    else return 1; // root class
                }
            }
            if (node is PSMContentContainer)
            {
                return 1;
            }
            if (node is PSMAttribute)
            {
                return ((PSMAttribute)node).Upper.Value;
            }
            // should never get here...
            throw new ArgumentException("Node is not correct significant node.");
        }

        #endregion

        /// <summary>
        /// Returns PSM associations between an two nodes (they must be part of a path in the PSM tree).
        /// (assocations are returned in root to leafes order.
        /// </summary>
        /// <param name="ancestor">ancestor node</param>
        /// <param name="descendant">descendant node</param>
        public static IEnumerable<PSMAssociation> GetAssociationsBetweenNodes(PSMElement ancestor, PSMElement descendant)
        {
            List<PSMAssociation> associations = new List<PSMAssociation>();
            PSMTreeIterator it = new PSMTreeIterator(descendant);

            while (it.CurrentNode != ancestor)
            {
                {
                    PSMAssociationChild associationChild = it.CurrentNode as PSMAssociationChild;
                    if (associationChild != null && associationChild.ParentAssociation != null)
                    {
                        associations.Insert(0, associationChild.ParentAssociation);
                    }
                }
                if (!it.CanGoToParent())
                {
                    throw new ArgumentException("Nodes are not valid ancestor-descendant pair");
                }
                it.GoToParent();
            }

            return associations;
        }

        private static bool CanBeDocumentRoot(PSMElement node)
        {
            return (node is PSMContentContainer) || (node is PSMClass && ((PSMClass) node).HasElementLabel);
        }

        public static bool IsInSignificantSubtree(PSMElement node)
        {
            PSMTreeIterator it = new PSMTreeIterator(node);
            PSMElement rootCandidate = null;
            if (CanBeDocumentRoot(node))
                rootCandidate = node;
            while (it.CanGoToParent())
            {
                it.GoToParent();
                if (CanBeDocumentRoot(it.CurrentNode))
                    rootCandidate = it.CurrentNode;
                else if (!(node is PSMClassUnion))
                    rootCandidate = null; 
            }
            return rootCandidate != null; 
        }
    }

    
}