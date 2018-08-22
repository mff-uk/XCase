using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XCase.Model
{
	/// <summary>
	/// Provides static function to examine PSM Diagram tree/forest. 
	/// </summary>
    public static class PSMTree
    {
        /// <summary>
        /// Checks if the subtree starting in the given class union contains
        /// the given item.
        /// </summary>
        /// <param name="root">References the class union that is the root of searched subtree</param>
        /// <param name="item">References the item being searched</param>
        /// <returns>True if the item is present, false otherwise</returns>
        private static bool ClassUnionContains(PSMClassUnion root, object item)
        {
            if (root == item)
                return true;

            foreach (PSMAssociationChild child in root.Components)
            {
                if (child == item)
                    return true;
                if (child is PSMClass)
                {
                    if (SubtreeContains(child as PSMClass, item))
                        return true;
                }
                else
                    if (ClassUnionContains(child as PSMClassUnion, item))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the given component is present in a tree with given root.
        /// The search starts in the given root and recursively continues to the 
        /// subordinate components (associations, containers, choices, unions) and
        /// finally it descends through the specializations (inheritance) of the classes.
        /// </summary>
        /// <param name="root">Reference to the root of the searched subtree</param>
        /// <param name="item">Reference to the item being searched</param>
        /// <returns>
        /// True if the item is present in the subtree with the given root, false otherwise.
        /// </returns>
        public static bool SubtreeContains(PSMSuperordinateComponent root, object item)
        {
            if (root == item)
                return true;

            foreach (PSMSubordinateComponent component in root.Components)
            {
                if (component == item)
                    return true;

                if (component is PSMSuperordinateComponent)
                {
                    if (SubtreeContains(component as PSMSuperordinateComponent, item))
                        return true;
                }
                else if (component is PSMAssociation)
                {
                    PSMAssociation assoc = component as PSMAssociation;
                    if (assoc.Child is PSMClass)
                        if (SubtreeContains(assoc.Child as PSMClass, item))
                            return true;
                    if (assoc.Child is PSMClassUnion)
                        if (ClassUnionContains(assoc.Child as PSMClassUnion, item))
                            return true;
                }
            }

            if (root is PSMClass)
            {
                PSMClass rc = root as PSMClass;

                foreach (Generalization gen in rc.Specifications)
                {
                    if (SubtreeContains(gen.Specific as PSMClass, item))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the root of the PSM tree that contains the given item (if any).
        /// </summary>
        /// <param name="item">Reference to the item being searched</param>
        /// <returns>Reference to the root of the tree that contains item or null if no such exists</returns>
        public static PSMSuperordinateComponent GetRootOf(object item)
        {
            if (item is PSMSubordinateComponent)
            {
                PSMSubordinateComponent comp = item as PSMSubordinateComponent;
                if (comp.Parent == null)
                    return item as PSMSuperordinateComponent;
                else
                    return GetRootOf(comp.Parent);
            }

            if (item is PSMAssociationChild)
            {
                PSMAssociationChild child = item as PSMAssociationChild;
                if (child.ParentAssociation != null)
                    return GetRootOf(child.ParentAssociation);
                if (child.ParentUnion != null)
                    return GetRootOf(child.ParentUnion);
                
                return item as PSMSuperordinateComponent;
            }

            if (item is PSMAttribute)
                return GetRootOf((item as PSMAttribute).Class);
            
            
            return null;
        }

		/// <summary>
		/// Returns the elements of subtrees of roots in an order in which they can be
		/// added to a PSM diagram.
		/// </summary>
		/// <param name="roots">The roots.</param>
		/// <param name="ordered">The ordered.</param>
		/// <param name="addMetElements">if set to <c>true</c> method adds any elements met 
		/// during the execution (subelements of the elements in <paramref name="roots"/>).</param>
		/// <returns></returns>
		public static bool ReturnElementsInPSMOrder(IEnumerable<Element> roots, out IList<Element> ordered, bool addMetElements)
		{
			/* PSM diagram elements must be loaded from root to leaves, 
			 * following code is BFS implementation */
			Queue<Element> elementsToDo = new Queue<Element>();
			List<Element> alreadyProcessed = new List<Element>();
			List<Element> delayedElements = new List<Element>();

			List<Element> _roots = roots.ToList();
			_roots.Sort(CompareByRootPositionsDesc);
			foreach (Element root in _roots)
			{
				elementsToDo.Enqueue(root);
			}

			List<Element> needed = new List<Element>();

			while (elementsToDo.Count > 0)
			{
				Element node = elementsToDo.Dequeue();
				if (!alreadyProcessed.Contains(node))
				{
					if (node is PSMSuperordinateComponent)
					{
						foreach (PSMSubordinateComponent component in ((PSMSuperordinateComponent)node).Components.Where(i => !alreadyProcessed.Contains(i) && !elementsToDo.Contains(i)))
						{
							elementsToDo.Enqueue(component);
						}
					}

					if (node is PSMClassUnion)
					{
						foreach (PSMAssociationChild component in ((PSMClassUnion)node).Components.Where(i => !alreadyProcessed.Contains(i) && !elementsToDo.Contains(i)))
						{
							elementsToDo.Enqueue(component);
						}
					}

					PSMClass psmClass = node as PSMClass;

					if (psmClass != null)
					{
						foreach (Generalization specification in psmClass.Specifications.Where(i => !alreadyProcessed.Contains(i) && !elementsToDo.Contains(i)))
						{
							elementsToDo.Enqueue(specification);
						}
					}

					PSMAssociation a = node as PSMAssociation;
					Generalization g = node as Generalization;

					PSMSubordinateComponent sub = node as PSMSubordinateComponent;
					PSMAssociationChild child = node as PSMAssociationChild;

					/* 
					 * association can be loaded only after the child is already loaded too. 
					 * otherwise it is postponed, 
					 * 
					 * generalizations can be loaded after both 
					 * general and specific class are loaded
					 * 
					 * component can be loaded after parent is loaded
					 */
					needed.Clear();
					if (a != null)
					{
						if (elementsToDo.Contains(a.Parent))
							needed.Add(a.Parent);
						needed.Add(a.Child);
					}
					if (g != null)
					{
						if (elementsToDo.Contains(g.General))
							needed.Add(g.General);
						needed.Add(g.Specific);
					}
					if (sub != null)
					{
						if (elementsToDo.Contains(sub.Parent))
							needed.Add(sub.Parent);
						//IList<PSMSubordinateComponent> components = sub.Parent.Components;
						//int index = components.IndexOf(sub);
						//needed.AddRange(components.Where(component => components.IndexOf(component) < index).Cast<Element>());
					}
					if (child != null && child.ParentUnion != null)
					{
						if (elementsToDo.Contains(child.ParentUnion))
							needed.Add(child.ParentUnion);
						//IList<PSMAssociationChild> components = child.ParentUnion.Components;
						//int index = components.IndexOf(child);
						//needed.AddRange(components.Where(component => components.IndexOf(component) < index).Cast<Element>());
					}
					//if (child != null && child.ParentAssociation != null)
					//{
					//    if (elementsToDo.Contains(child.ParentAssociation))
					//        needed.Add(child.ParentAssociation);
					//    //IList<PSMAssociationChild> components = child.ParentAssociation.Components;
					//    //int index = components.IndexOf(child);
					//    //needed.AddRange(components.Where(component => components.IndexOf(component) < index).Cast<Element>());
					//}

					if (needed.All(alreadyProcessed.Contains))
					{
						if (!alreadyProcessed.Contains(node))
							alreadyProcessed.Add(node);
					}
					else
					{
						if (delayedElements.Contains(node))
						{
							/* 
							 * association/generalization was already delayed once, 
							 * now it is clear that it will never be loaded correctly, 
							 * algortithm is stopped to avoid infinite cycle.
							 */
							ordered = null;
							return false;
						}
						else
						{
							/* wait untill the association child is loaded */
							foreach (Element element in needed.Where(i => !alreadyProcessed.Contains(i) && !elementsToDo.Contains(i)))
							{
								elementsToDo.Enqueue(element);
							}
							elementsToDo.Enqueue(node);
							delayedElements.Add(node);
						}
					}
				}
			}

			if (!addMetElements)
			{
				alreadyProcessed.RemoveAll(element => !roots.Contains(element));
			}

			ordered = alreadyProcessed;
			return true;
		}

		private static int CompareByRootPositionsDesc(Element e1, Element e2)
		{
            PSMSuperordinateComponent c1 = e1 as PSMSuperordinateComponent;
            PSMSuperordinateComponent c2 = e2 as PSMSuperordinateComponent;
			if (c1 != null && c2 != null)
			{
				ObservableCollection<PSMSuperordinateComponent> roots = c1.Diagram.Roots;
				if (roots.Contains(c1) && roots.Contains(c2))
				{
					return roots.IndexOf(c1) > roots.IndexOf(c2) ? 0 : 1;
				}
				else if (roots.Contains(c1))
				{
					return 1;
				}
				else if (roots.Contains(c2))
				{
					return 0;
				}
				return (c1.GetHashCode() < c2.GetHashCode()) ? 0 : 1;
			}
			if (c1 != null)
				return 1;
			if (c2 != null)
				return 0;
			return (e1.GetHashCode() < e2.GetHashCode()) ? 0 : 1;
		}

		public static Element GetParentOfElement(Element element)
		{
			if (element is PSMSubordinateComponent)
				return ((PSMSubordinateComponent)element).Parent;
			PSMAssociationChild associationChild = element as PSMAssociationChild;
			if (associationChild != null && associationChild.ParentAssociation != null)
			{
				return associationChild.ParentAssociation;
			}

			if (associationChild != null && associationChild.ParentUnion != null)
			{
				return associationChild.ParentUnion;
			}

			PSMClass psmClass = element as PSMClass;
			if (psmClass != null)
			{
				if (psmClass.ParentUnion != null)
					return psmClass.ParentUnion;
				if (psmClass.Generalizations.Count() > 0)
					return psmClass.Generalizations.First();
			}

			Generalization generalization = element as Generalization;
			if (generalization != null)
			{
				return generalization.General;
			}

			return null;
		}

		public static Element GetLeftSiblingOfElement(Element element)
		{
			PSMSubordinateComponent subordinateComponent = element as PSMSubordinateComponent;
			if (subordinateComponent != null)
			{
				PSMSuperordinateComponent parent = subordinateComponent.Parent;
				if (parent != null && parent.Components.IndexOf(subordinateComponent) >= 1)
				{
					return parent.Components[parent.Components.IndexOf(subordinateComponent) - 1];
				}
			}

			PSMClass psmClass = element as PSMClass;
			if (psmClass != null && psmClass.ParentUnion != null)
			{
				if (psmClass.ParentUnion.Components.IndexOf(psmClass) > 0)
				{
					return psmClass.ParentUnion.Components[psmClass.ParentUnion.Components.IndexOf(psmClass) - 1];
				}
			}

			Generalization generalization = element as Generalization;
			if (generalization != null)
			{
				IList<Generalization> specifications = generalization.General.Specifications;
				if (specifications.IndexOf(generalization) == 0)
				{
					return ((PSMClass)generalization.General).Components.LastOrDefault();
				}
				if (specifications.IndexOf(generalization) > 0)
				{
					return specifications[specifications.IndexOf(generalization) - 1];
				}

			}

			return null;
		}

		public static Element GetRightSiblingOfElement(Element element)
		{
			PSMSubordinateComponent subordinateComponent = element as PSMSubordinateComponent;
			if (subordinateComponent != null)
			{
				PSMSuperordinateComponent parent = subordinateComponent.Parent;
				if (parent != null)
				{
					ObservableCollection<PSMSubordinateComponent> components = parent.Components;
					if (components.IndexOf(subordinateComponent) < components.Count - 1)
					{
						return components[components.IndexOf(subordinateComponent) + 1];
					}
					else if (parent is PSMClass)
					{
						PSMClass parentClass = parent as PSMClass;
						if (parentClass.Specifications.Count > 0)
							return parentClass.Specifications.First();
					}
				}
			}

			Generalization generalization = element as Generalization;
			if (generalization != null)
			{
				IList<Generalization> specifications = generalization.General.Specifications;
				if (specifications.IndexOf(generalization) != specifications.Count - 1)
				{
					return specifications[specifications.IndexOf(generalization) + 1];
				}
			}

			PSMClass psmClass = element as PSMClass;
			if (psmClass != null && psmClass.ParentUnion != null)
			{
				ObservableCollection<PSMAssociationChild> components = psmClass.ParentUnion.Components;
				if (components.IndexOf(psmClass) < components.Count - 1)
				{
					return components[components.IndexOf(psmClass) + 1];
				}
			}

			return null;
		}

        public static IEnumerable<Element> GetChildrenOfElement(Element element)
        {
            PSMSuperordinateComponent superordinateComponent = element as PSMSuperordinateComponent;
            if (superordinateComponent != null && superordinateComponent.Components.Count > 0)
            {
                return superordinateComponent.Components;
            }
            PSMClassUnion union = element as PSMClassUnion;
            if (union != null && union.Components.Count > 0)
            {
                return union.Components;
            }
            PSMAssociation association = element as PSMAssociation;
            if (association != null)
            {
                return new Element[] { association.Child };
            }

            PSMClass psmClass = element as PSMClass;
            if (psmClass != null)
            {
                if (psmClass.Specifications.Count > 0)
                    return psmClass.Specifications;
            }

            Generalization generalization = element as Generalization;
            if (generalization != null)
            {
                return new Element[] { generalization.Specific };
            }
            return new Element[0];
        }

		public static Element GetFirstChildOfElement(Element element)
		{
			PSMSuperordinateComponent superordinateComponent = element as PSMSuperordinateComponent;
			if (superordinateComponent != null && superordinateComponent.Components.Count > 0)
			{
				return superordinateComponent.Components[0];
			}
			PSMClassUnion union = element as PSMClassUnion;
			if (union != null && union.Components.Count > 0)
			{
				return union.Components[0];
			}
			PSMAssociation association = element as PSMAssociation;
			if (association != null)
			{
				return association.Child;
			}

			PSMClass psmClass = element as PSMClass;
			if (psmClass != null)
			{
				if (psmClass.Specifications.Count > 0)
					return psmClass.Specifications.First();
			}

			Generalization generalization = element as Generalization;
			if (generalization != null)
			{
				return generalization.Specific;
			}
			return null;
		}

		public static void CopyRepresentantsRelations(IDictionary<Element, Element> createdCopies)
		{
			foreach (PSMClass sourceClass in createdCopies.Keys.OfType<PSMClass>().Where(psmClass => psmClass.RepresentedPSMClass != null && createdCopies.ContainsKey(psmClass.RepresentedPSMClass)))
			{
				PSMClass copyClass = (PSMClass) createdCopies[sourceClass];
				copyClass.RepresentedPSMClass = (PSMClass) createdCopies[sourceClass.RepresentedPSMClass];
			}
		}

		/// <summary>
		/// Returns a set of roots of subtrees given a set of selected elemets in a diagram.
		/// (only PSM class qualifies as root in this method, so some roots does not neccessarily
		/// have to be among <paramref name="elements"/>). 
		/// </summary>
		/// <param name="elements">elements selected in a PSM tree</param>
		/// <returns>roots of the selected subtrees. </returns>
		public static IList<Element> IdentifySubtreeRoots(ICollection<Element> elements)
		{
			List<Element> roots = new List<Element>();

			foreach (Element element in elements)
			{
				Element subtreeRoot = element;
				bool shouldContinue;
				do
				{
					Element parent = GetParentOfElement(subtreeRoot);
					shouldContinue = parent != null && (elements.Contains(parent) || !(subtreeRoot is PSMClass));
					if (shouldContinue)
					{
						subtreeRoot = parent; 
					}
				} while (shouldContinue);
				if (!roots.Contains(subtreeRoot))
				{
					roots.Add(subtreeRoot);
				}
			}


			return roots; 
		}

        public static bool AreComponentsOfCommonParent<TYPE>(IEnumerable<TYPE> components)
            where TYPE : PSMSubordinateComponent
        {
            if (components.Count() == 0)
            {
                return false;
            }

            PSMSuperordinateComponent parent = components.First().Parent;
            return components.All(c => c.Parent == parent);
        }

	    public static void CheckDiagram(PSMDiagram diagram, out List<string> errors, out List<string> warnings)
	    {
	        errors = new List<string>();
            warnings = new List<string>();

	        #region diagram has roots

	        if (diagram.Roots.Count == 0)
	        {
	            errors.Add(string.Format("Diagram has no roots. "));
	        }

	        #endregion

	        #region diagram has global element

	        if (diagram.Roots.Where(r => (r is PSMClass) && ((PSMClass) r).HasElementLabel).Count() == 0)
	        {
	            bool found = false;
	            foreach (PSMSuperordinateComponent superordinate in diagram.Roots)
	            {
	                if (superordinate is PSMContentContainer)
	                {
	                    found = true;
	                    break;
	                }
	                else
	                {
	                    PSMClass c = superordinate as PSMClass;
	                    if (c != null)
	                    {
	                        if (GetSpecificiationsRecursive(c).Any(s => s.HasElementLabel))
	                        {
	                            found = true; 
	                        }
	                    }
	                }
	            }

	            if (!found)
	                warnings.Add(string.Format("Diagram has no root with an element label, no global element is declared. "));
	        }

	        #endregion

	        #region not zero or only one component in choice

	        foreach (Element element in diagram.DiagramElements.Keys)
	        {
	            {
	                PSMContentChoice cc = element as PSMContentChoice;
	                if (cc != null && cc.Components.Count == 1)
	                {
	                    warnings.Add(string.Format("Only one component in {0}. ", cc));
	                }
                    if (cc != null && cc.Components.Count == 0)
                    {
                        warnings.Add(string.Format("Zero components in {0}. ", cc));
                    }
	            }

	            {
	                PSMClassUnion cu = element as PSMClassUnion;
	                if (cu != null && cu.Components.Count == 1)
	                {
	                    warnings.Add(string.Format("Only one component in {0}. ", cu));
	                }
                    if (cu != null && cu.Components.Count == 0)
                    {
                        warnings.Add(string.Format("Zero components in {0}. ", cu));
                    }
	            }
	        }

	        #endregion
	    }

	    public static IEnumerable<PSMClass> GetSpecificiationsRecursive(PSMClass psmClass)
	    {
	        IEnumerable<PSMClass> result = psmClass.Specifications.Select(g => (PSMClass)g.Specific);

	        foreach (PSMClass s in psmClass.Specifications.Select(g => (PSMClass)g.Specific))
	        {
	            result = result.Union(GetSpecificiationsRecursive(s));
	        }

	        return result;
	    }

        /*
	    public static bool IsUnderDirectChoice(PSMElement element)
	    {
	        PSMTreeIterator iterator = new PSMTreeIterator(element.Diagram);

	        bool inChoice = false;

	        iterator.CurrentNode = element;
	        if (!iterator.CanGoToParent())
	        {
	            return false;
	        }

	        do
	        {
	            iterator.GoToParent();
	            if (iterator.CurrentNode is PSMContentChoice)
	            {
	                if (((PSMContentChoice) iterator.CurrentNode).Components.Count > 0)
	                {
	                    inChoice = true;
	                    break;
	                }
	            }

	            if (iterator.CurrentNode is PSMClassUnion)
	            {
	                if (((PSMClassUnion) iterator.CurrentNode).Components.Count > 0)
	                {
	                    inChoice = true;
	                    break;
	                }
	            }
	        } while (!iterator.CurrentNodeModelsElement() && iterator.CanGoToParent());

	        return inChoice;
	    }

        public static bool IsUnderIndirectChoice(PSMElement element)
        {
            PSMTreeIterator iterator = new PSMTreeIterator(element);

            bool inChoice = false;
            if (!iterator.CanGoToParent())
            {
                return false;
            }

            do
            {
                iterator.GoToParent();
                if (iterator.CurrentNode is PSMContentChoice)
                {
                    if (((PSMContentChoice) iterator.CurrentNode).Components.Count > 0)
                    {
                        inChoice = true;
                        break;
                    }
                }

                if (iterator.CurrentNode is PSMClassUnion)
                {
                    if (((PSMClassUnion) iterator.CurrentNode).Components.Count > 0)
                    {
                        inChoice = true;
                        break;
                    }
                }
            } while (iterator.CanGoToParent());

            return inChoice;
        }
        */ 
    }
}
