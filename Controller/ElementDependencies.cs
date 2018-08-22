using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// Can be used to find dependent elements in the model or in the diagram.
	/// </summary>
	public class ElementDependencies : Dictionary<Element, List<Element>>
	{
		/// <summary>
		/// Dependencies in PIM diagram
		/// </summary>
		[Flags]
		public enum EDependency
		{
			/// <summary>
			/// No dependencies
			/// </summary>
			None = 0,
			/// <summary>
			/// Association depends on clases which it connects
			/// </summary>
			Class_Association = 1,
			/// <summary>
			/// Generalization depends on general and specific class
			/// </summary>
			Class_Generalization = 2,
			/// <summary>
			/// Comment depends on the element it annotates 
			/// </summary>
			Element_ElementComment = 4,
			/// <summary>
			/// All possible dependencies
			/// </summary>
			All = 7
		}

		private IEnumerable<Element> ignoredElements;

		/// <summary>
		/// Collection of bool flags. Can be used freely for any purpose. 
		/// </summary>
		public Dictionary<Element, bool> Flags = new Dictionary<Element, bool>();

		private Queue<KeyValuePair<Element, Element>> elementsToDo;

		/// <summary>
		/// Returns true if <paramref name="element"/> is present in <paramref name="diagram"/>
		/// </summary>
		/// <param name="diagram">diagram</param>
		/// <param name="element">element</param>
		/// <returns><code>true</code> or <code>false</code></returns>
		private static bool IsInDiagram(Diagram diagram, Element element)
		{
			return diagram.IsElementPresent(element);
		}

		/// <summary>
		/// Finds dependent elements (in model or in diagram)
		/// </summary>
		/// <param name="diagram">reference to a diagram when lookup should be restrected to a diagram.
		/// pass <code>null</code> if lookup should be performed in the whole model</param>
		/// <param name="element">element</param>
		/// <param name="dependency">type of dependency</param>
		/// <param name="dependentElements">collection where to add dependent elements</param>
		/// <param name="transitive">when true, lookup is transitive (dependent elements of dependent elements ...)</param>
		public static void GetDependentElements(Diagram diagram, Element element, EDependency dependency, ref List<Element> dependentElements, bool transitive)
		{
			if (element is Class && (dependency & EDependency.Class_Association) == EDependency.Class_Association)
			{
				foreach (Association association in ((Class)element).Assocations)
				{
					if (!dependentElements.Contains(association))
					{
						if ((diagram == null || IsInDiagram(diagram, association)) && !dependentElements.Contains(association))
						{
							dependentElements.Add(association);
							if (transitive)
								GetDependentElements(diagram, association, dependency, ref dependentElements, true);
						}
					}
				}
			}
			if (element is Class && (dependency & EDependency.Class_Generalization) == EDependency.Class_Generalization)
			{
				foreach (Generalization generalization in ((Class)element).Specifications)
				{
					if (!dependentElements.Contains(generalization))
					{
						if ((diagram == null || IsInDiagram(diagram, generalization)))
						{
							dependentElements.Add(generalization);
							if (transitive)
								GetDependentElements(diagram, generalization, dependency, ref dependentElements, true);
						}
					}
				}
				foreach (Generalization generalization in ((Class)element).Generalizations)
				{
					if (!dependentElements.Contains(generalization))
					{
						if ((diagram == null || IsInDiagram(diagram, generalization)))
						{
							dependentElements.Add(generalization);
							if (transitive)
								GetDependentElements(diagram, generalization, dependency, ref dependentElements, true);
						}	
					}
				}
			}
			if ((dependency & EDependency.Element_ElementComment) == EDependency.Element_ElementComment)
			{
				foreach (Comment comment in element.Comments)
				{
					if (!dependentElements.Contains(comment))
					{
						if ((diagram == null || IsInDiagram(diagram, comment)))
						{
							dependentElements.Add(comment);
							if (transitive)
								GetDependentElements(diagram, comment, dependency, ref dependentElements, true);
						}
					}
				}
			}
			//if ((dependency & EDependency.FullConnections) == EDependency.FullConnections)
			//{
			//    if (element is Association)
			//    {
			//        foreach (AssociationEnd end in ((Association)element).Ends)
			//        {
			//            if (!dependentElements.Contains(end.Class))
			//            {
			//                dependentElements.Add(end.Class);
			//                if (transitive)
			//                    GetDependentElements(diagram, end.Class, dependency, ref dependentElements, true);
			//            }
			//        }
			//    }
			//    if (element is Generalization)
			//    {
			//        Class general = ((Generalization)element).General;
			//        if (!dependentElements.Contains(general))
			//        {
			//            dependentElements.Add(general);
			//            if (transitive)
			//                GetDependentElements(diagram, general, dependency, ref dependentElements, true);
			//        }
			//        Class specific = ((Generalization)element).Specific;
			//        if (!dependentElements.Contains(specific))
			//        {
			//            dependentElements.Add(specific);
			//            if (transitive)
			//                GetDependentElements(diagram, specific, dependency, ref dependentElements, true);
			//        }
			//    }
			//}
		}

		/// <summary>
		/// Finds dependent elements in model for all elements in <paramref name="elements"/>. 
		/// </summary>
		/// <param name="elements">collection of elements for which dependent elements will be
		/// returned</param>
		/// <returns><see cref="ElementDependencies"/></returns>
		public static ElementDependencies FindElementDependenciesInModel(IList<Element> elements)
		{
			return FindElementDependencies(elements, null, EDependency.All);
		}

		/// <summary>
		/// Finds dependent elements in model for all elements in <paramref name="elements"/>. 
		/// </summary>
		/// <param name="elements">collection of elements for which dependent elements will be
		/// returned</param>
		/// <param name="dependency">type of dependency</param>
		/// <returns><see cref="ElementDependencies"/></returns>
		public static ElementDependencies FindElementDependenciesInModel(IList<Element> elements, EDependency dependency)
		{
			return FindElementDependencies(elements, null, dependency);
		}

		/// <summary>
		/// Finds dependent elements in <paramref name="diagram"/> for all elements in <paramref name="elements"/>. 
		/// </summary>
		/// <param name="elements">collection of elements for which dependent elements will be
		/// returned</param>
		/// <param name="diagram">diagram where dependent elements are searched</param>
		/// <returns><see cref="ElementDependencies"/></returns>
		public static ElementDependencies FindElementDependenciesInDiagram(IList<Element> elements, Diagram diagram)
		{
			return FindElementDependenciesInDiagram(elements, diagram, EDependency.All);
		}

		/// <summary>
		/// Finds dependent elements in <paramref name="diagram"/> for all elements in <paramref name="elements"/>. 
		/// </summary>
		/// <param name="elements">collection of elements for which dependent elements will be
		/// returned</param>
		/// <param name="diagram">diagram where dependent elements are searched</param>
		/// <param name="dependency">type of dependencies</param>
		/// <returns><see cref="ElementDependencies"/></returns>
		public static ElementDependencies FindElementDependenciesInDiagram(IList<Element> elements, Diagram diagram, EDependency dependency)
		{
			return FindElementDependencies(elements, diagram, dependency);
		}

		/// <summary>
		/// Finds dependent elements in <paramref name="diagram"/> or in whole model
		/// for all elements in <paramref name="elements"/>. 
		/// </summary>
		/// <param name="elements">collection of elements for which dependent elements will be
		/// returned</param>
		/// <param name="diagram">diagram where dependent elements are searched, when set to <code>null</code>
		/// lookup will be performed in the whole model</param>
		/// <param name="dependency">type of dependencies</param>
		/// <returns><see cref="ElementDependencies"/></returns>
		private static ElementDependencies FindElementDependencies(IList<Element> elements, Diagram diagram, EDependency dependency)
		{
			ElementDependencies dependencies = new ElementDependencies();

			foreach (Element element in elements)
			{
				List<Element> dep = new List<Element>();
				GetDependentElements(diagram, element, dependency, ref dep, true);
				dep.RemoveAll(dependentElement => elements.Contains(dependentElement));
				if (dep.Count > 0)
				{
					dependencies.Flags[element] = false;
					dependencies[element] = dep;
				}
			}
			return dependencies;
		}

		/// <summary>
		/// Adds dependency between <paramref name="element"/> and <paramref name="dependentElement"/>
		/// to the collection
		/// </summary>
		/// <param name="element">element</param>
		/// <param name="dependentElement">dependent element</param>
		public void AddDependency(Element element, Element dependentElement)
		{
			if (element != dependentElement && (ignoredElements == null || !ignoredElements.Contains(dependentElement)))
			{
				if (!ContainsKey(element))
				{
					this[element] = new List<Element>();
					Flags[element] = false;
				}
				if (!this[element].Contains(dependentElement))
					this[element].Add(dependentElement);
			}
		}

		/*
		 * Possible dependencies in PSM diagram: 
		 * 
		 * PSMSuperordinateComponent (PSMClass, PSMContentContainer, PSMContentChoice)
		 *  => Components (PSMAssociation, PSMAttributeContainer, PSMContentContainer, PSMContentChoice)
		 * 
		 * PSMClassUnion
		 *  => Components (PSMClass, PSMClassUnion)
		 *  
		 * PSMAssociation
		 *  => PSMAssociationChild (PSMClass, PSMClassUnion)
		 *  
		 * PSMClass
		 *  => Specifications (Generalization)
		 *  
		 * Generalization 
		 *   => General, Specific (PSMClass)
		 */
		public static ElementDependencies FindPSMDependencies(IEnumerable<Element> deletedElements)
		{
			ElementDependencies result = new ElementDependencies();
			result.elementsToDo = new Queue<KeyValuePair<Element, Element>>();
			foreach (Element element in deletedElements)
			{
				result.elementsToDo.Enqueue(new KeyValuePair<Element, Element>(element, element));
			}
			result.ignoredElements = deletedElements;
			while (result.elementsToDo.Count != 0)
			{
				KeyValuePair<Element, Element> pair = result.elementsToDo.Dequeue();
				if (pair.Value is PSMClass)
				{
					result.AddClassDependencies(pair.Key, (PSMClass)pair.Value);
				}
				else if (pair.Value is PSMClassUnion)
				{
					result.AddClassUnionDependencies(pair.Key, (PSMClassUnion)pair.Value);
				}
				else if (pair.Value is PSMAssociation)
				{
					result.AddAssociationDependencies(pair.Key, (PSMAssociation)pair.Value);
				}
				else if (pair.Value is Generalization)
				{
					result.AddGeneralizationDependencies(pair.Key, (Generalization)pair.Value);
				}
				else if (pair.Value is PSMSuperordinateComponent)
				{
					result.AddSuperordinateComponentDependencies(pair.Key, (PSMSuperordinateComponent)pair.Value);
				}
			}
			return result;
		}
		
		/*
		{
			ElementDependencies result = new ElementDependencies();
			foreach (Element element in deletedElements)
			{
				PSMSuperordinateComponent superordinateComponent = element as PSMSuperordinateComponent;
				if (superordinateComponent != null)
					result.AddPSMChildrenRecursive(superordinateComponent, superordinateComponent);

				PSMAssociation association = element as PSMAssociation;
				if (association != null)
					result.AddPSMAssociationChildRecursive(association, association);

				PSMClassUnion union = element as PSMClassUnion;
				if (union != null)
				{
					result.AddUnionComponentsRecursive(union, union);
				}
				
				PSMClass psmClass = element as PSMClass;
				if (psmClass != null)
				{
					foreach (Generalization specification in psmClass.Specifications)
					{
						result.AddDependency(element, specification);
						result.AddDependency(element, specification.Specific);
						result.AddPSMChildrenRecursive(element, (PSMClass)(specification.Specific));
					}
				}
			}
			return result;
		}

		public void AddPSMChildrenRecursive(Element parent, PSMSuperordinateComponent node)
		{
			foreach (PSMSubordinateComponent subordinateComponent in node.Components)
			{
				AddDependency(parent, subordinateComponent);
				PSMSuperordinateComponent superordinateComponent = subordinateComponent as PSMSuperordinateComponent;
				if (superordinateComponent != null)
					AddPSMChildrenRecursive(parent, superordinateComponent);

				PSMAssociation association = subordinateComponent as PSMAssociation;
				if (association != null)
					AddPSMAssociationChildRecursive(parent, association);
			}
		}

		public void AddPSMAssociationChildRecursive(Element parent, PSMAssociation association)
		{
			AddDependency(parent, association.Child);
			PSMSuperordinateComponent superordinateComponent = association.Child as PSMSuperordinateComponent;
			if (superordinateComponent != null)
				AddPSMChildrenRecursive(parent, superordinateComponent);
			PSMClassUnion childUnion = association.Child as PSMClassUnion;
			if (childUnion != null)
			{
				AddUnionComponentsRecursive(parent, childUnion);
			}
		}

		private void AddUnionComponentsRecursive(Element parent, PSMClassUnion union)
		{
			foreach (PSMAssociationChild child in union.Components)
			{
				AddDependency(parent, child);
				PSMClass psmClass = child as PSMClass;
				if (psmClass != null)
				{
					AddPSMChildrenRecursive(parent, psmClass);
				}
				PSMClassUnion childUnion = child as PSMClassUnion;
				if (childUnion != null)
				{
					AddUnionComponentsRecursive(parent, childUnion);
				}
			}
		}
		*/
		private void AddClassDependencies(Element root, PSMClass psmClass)
		{
			AddSuperordinateComponentDependencies(root, psmClass);
			foreach (Generalization specification in psmClass.Specifications)
			{
				AddDependency(root, specification);
				elementsToDo.Enqueue(new KeyValuePair<Element, Element>(root, specification));
			}
			foreach (Generalization generalization in psmClass.Generalizations)
			{
				if (!(this.ContainsKey(root) && this[root].Contains(generalization)))
				{
					AddDependency(root, generalization);
				}
			}
			if (psmClass.ParentAssociation != null)
				AddDependency(root, psmClass.ParentAssociation);
		}

		private void AddClassUnionDependencies(Element root, PSMClassUnion union)
		{
			foreach (PSMAssociationChild psmAssociationChild in union.Components)
			{
				AddDependency(root, psmAssociationChild);
				elementsToDo.Enqueue(new KeyValuePair<Element, Element>(root, psmAssociationChild));
			}
			if (union.ParentUnion != null)
			{
				AddDependency(root, union.ParentUnion);
				AddClassUnionDependencies(root, union.ParentUnion);
			}
			if (union.ParentAssociation != null)
			{
				AddDependency(root, union.ParentAssociation);
			}
		}

		private void AddAssociationDependencies(Element root, PSMAssociation association)
		{
			AddDependency(root, association.Child);
			elementsToDo.Enqueue(new KeyValuePair<Element, Element>(root, association.Child));
		}

		private void AddGeneralizationDependencies(Element root, Generalization generalization)
		{
			AddDependency(root, generalization.Specific);
			elementsToDo.Enqueue(new KeyValuePair<Element, Element>(root, generalization.Specific));
		}

		private void AddSuperordinateComponentDependencies(Element root, PSMSuperordinateComponent superordinateComponent)
		{
			foreach (PSMSubordinateComponent component in superordinateComponent.Components)
			{
				AddDependency(root, component);
				elementsToDo.Enqueue(new KeyValuePair<Element, Element>(root, component));
			}
		}



		/// <summary>
		/// Finds elements hidden in a PIM diagram.
		/// </summary>
		/// <param name="presentElements">elements that are present in the diagram</param>
		/// <param name="model">The model where to search.</param>
		/// <returns>elements that are hidden in the diagram</returns>
		public static List<Element> FindHiddenElements(IEnumerable<Element> presentElements, Model.Model model)
		{
			
			List<Element> result = new List<Element>();
			foreach (Element element in presentElements)
			{
				Class c = element as Class;
				if (c != null)
				{
					foreach (Association association in c.Assocations)
					{
						if (!presentElements.Contains(association) 
							&& !result.Contains(association) 
							&& association.Ends.All(end => presentElements.Contains(end.Class)))
							result.Add(association);
					}
					foreach (Generalization specification in c.Specifications)
					{
						if (!presentElements.Contains(specification) 
							&& !result.Contains(specification)
							&& presentElements.Contains(specification.Specific))
							result.Add(specification);
					}
					foreach (Generalization generalization in c.Generalizations)
					{
						if (!presentElements.Contains(generalization) 
							&& !result.Contains(generalization)
							&& presentElements.Contains(generalization.General))
							result.Add(generalization);
					}
				}
				foreach (Comment comment in element.Comments)
				{
					if (!presentElements.Contains(comment)
						&& !result.Contains(comment))
						result.Add(comment);
				}
			}

			foreach (Comment comment in model.Comments)
			{
				if (!presentElements.Contains(comment)
					&& !result.Contains(comment))
					result.Add(comment);
			}
			return result;
		}
	}
}