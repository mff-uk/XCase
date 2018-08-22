using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Contains information about which view element represents each model type 
	/// (see <see cref="RegisterRepresentation(RepresentantRegistration)"/> and  <see cref="CanRepresentElement"/>)
	/// and also actual mapping of model objects to view objects (see <see cref="IsElementPresent"/>, and the default indexer)
	/// </summary>
	public class RepresentationCollection
	{
		/// <summary>
		/// Creates new instance of RepresentationCollection
		/// </summary>
		/// <param name="registrationSet">initial set of <see cref="RepresentantRegistration">RepresentantRegistrations</see>
		/// more registrations can be added by <see cref="AddElement"/> calls. </param>
		public RepresentationCollection(IEnumerable<RepresentantRegistration> registrationSet)
		{
			foreach (RepresentantRegistration registration in registrationSet)
			{
				this.RegisterRepresentation(registration);
			}

			ElementTypeNotRegisteredBehavior = EErrorBehavior.ThrowException;
			DeletingNotPresentBehavior = EErrorBehavior.ThrowException;
		}

		/// <summary>
		/// Action performed when an error occurs
		/// </summary>
		public enum EErrorBehavior
		{
			/// <summary>
			/// Error is ignored
			/// </summary>
			Ignore,
			/// <summary>
			/// Exception is thrown when error occurs
			/// </summary>
			ThrowException
		}

		/// <summary>
		/// Action performed when registration lookup fails - i.e. registration is 
		/// requested for an element type that was not yet registered
		/// </summary>
		public EErrorBehavior ElementTypeNotRegisteredBehavior { get; set; }

		/// <summary>
		/// Action performed when trying to delete an element that is not represented.
		/// </summary>
		public EErrorBehavior DeletingNotPresentBehavior { get; set; }

		/// <summary>
		/// Collection of added registrations
		/// </summary>
		private readonly Dictionary<Type, RepresentantRegistration> registeredRepresentations
			= new Dictionary<Type, RepresentantRegistration>();

		/// <summary>
		/// Collection of elements represented on the diagram
		/// </summary>
		/// <seealso cref="AddElement"/>
		/// <seealso cref="RemoveElement"/>
		private readonly Dictionary<Element, IModelElementRepresentant> representedElements
			= new Dictionary<Element, IModelElementRepresentant>();

		/// <summary>
		/// Finds representation of an element
		/// </summary>
		/// <param name="key">model element</param>
		/// <returns><see cref="IModelElementRepresentant"/>, element representing the <paramref name="key"/>
		/// on the diagram</returns>
		public IModelElementRepresentant this[Element key]
		{
			get { return representedElements[key]; }
		}

		/// <summary>
		/// Elements that are represented by the diagram
		/// </summary>
		public ReadOnlyCollection<Element> PresentElements
		{
			get
			{
				return representedElements.Keys.ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Add record of an <paramref name="element"/> and its <paramref name="representant">registration</paramref>
		/// </summary>
		/// <param name="element">model element</param>
		/// <param name="representant">view representation</param>
		public void AddElement(Element element, IModelElementRepresentant representant)
		{
			representedElements.Add(element, representant);
		}

		/// <summary>
		/// Remove record of an <paramref name="element"/> and its registration
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns>true if succesfully removed, false if element was not found in the dictionary</returns>
		public bool RemoveElement(Element element)
		{
			return representedElements.Remove(element);
		}

		/// <summary>
		/// Registers representation in the colleciton of representations. 
		/// Model elements of type <see cref="RepresentantRegistration.ModelElementType"/> will be 
		/// represented by type <see cref="RepresentantRegistration.RepresentantType"/>. 
		/// </summary>
		/// <param name="registration">Registration record</param>
		public void RegisterRepresentation(RepresentantRegistration registration)
		{
			registeredRepresentations[registration.ModelElementType] = registration;
		}

		/// <summary>
		/// Registers representation in the colleciton of representations. Model elements
		/// of type <paramref name="modelElementType"/> will be represented by type 
		/// <paramref name="representantType"/>.
		/// </summary>
		/// <param name="modelElementType">type of the model element</param>
		/// <param name="representantType">type that will represent elements of 
		/// <paramref name="modelElementType"/></param>
		/// <param name="controllerType">type of the <paramref name="modelElementType">
		/// modelElementType's</paramref> controller</param>
		/// <param name="viewHelperType">viewHelperType must be a subclass of ViewHelper
		/// </param>
		public void RegisterRepresentation(Type modelElementType, Type representantType, Type controllerType, Type viewHelperType)
		{
			RegisterRepresentation(new RepresentantRegistration(modelElementType, representantType, controllerType, viewHelperType));
		}

		/// <summary>
		/// Finds <see cref="RepresentantRegistration"/> when <paramref name="element">element's</paramref> type
		/// is registred. 
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns><see cref="RepresentantRegistration"/> record when <paramref name="element"/> is registred, 
		/// false otherwise</returns>
		private RepresentantRegistration? TrySearchRepresentantType(Element element)
		{
			foreach (KeyValuePair<Type, RepresentantRegistration> pair in registeredRepresentations.OrderBy(registration => registration.Value.RepresentantPriority))
			{
				if (pair.Key.IsInstanceOfType(element))
					return pair.Value;
			}
			return null;
		}

		/// <summary>
		/// Tests whether element can be represented (<see cref="RepresentantRegistration"/> 
		/// for the element was <see cref="registeredRepresentations">registered</see> in the <see cref="RepresentationCollection"/>)
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns>true if element can be represented. Action when element can not be represented 
		/// depends upon <see cref="ElementTypeNotRegisteredBehavior"/>.</returns>
		public bool CanRepresentElement(Element element)
		{
			if (TrySearchRepresentantType(element) != null)
				return true;
			if (ElementTypeNotRegisteredBehavior == EErrorBehavior.Ignore)
				return false;
			else
				throw new InvalidOperationException(string.Format(@"Element of type {0} can not be represented on the diagram, 
                    register element first via XCaseCanvas.RepresentationCollection.RegisterRepresentation 
                    or set XCaseCanvas.RepresentationCollection.ElementTypeNotRegisteredBehavior to Ignore", element.GetType().Name));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xCaseCanvas"></param>
		/// <param name="element"></param>
		/// <param name="controller"></param>
		/// <returns></returns>
		public IModelElementRepresentant CreateRepresentant(XCaseCanvas xCaseCanvas, Element element, out ElementController controller)
		{
			RepresentantRegistration? registration = TrySearchRepresentantType(element);
			if (registration == null)
				throw new InvalidOperationException(string.Format("Cannot represent elements of type {0}", element));

			controller = (ElementController)Activator.CreateInstance(registration.Value.ControllerType, element, xCaseCanvas.Controller);
			return (IModelElementRepresentant)Activator.CreateInstance(registration.Value.RepresentantType, xCaseCanvas);
		}

		/// <summary>
		/// Tests whether element representation is present in the <see cref="RepresentationCollection"/> 
		/// and so it can be deleted.
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns>true if element can be deleted. Action when element can not be deleted 
		/// depends upon <see cref="DeletingNotPresentBehavior"/>.</returns>
		public bool CanDeleteElement(Element element)
		{
			if (IsElementPresent(element))
				return true;
			if (DeletingNotPresentBehavior == EErrorBehavior.Ignore)
				return false;
			else
				throw new InvalidOperationException(@"Element of type {0} can not be deleted from the diagram, 
                    because it is not present in the diagram. If you wish to ingore these situations, set 
                    XCaseCanvas.RepresentationCollection.DeletingNotPresentBehavior to Ignore");
		}

		/// <summary>
		/// Returns true if element is present in the collection (is represented by its <see cref="IModelElementRepresentant"/>.)
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns>true if element was found in the dictionary, false otherwise</returns>
		public bool IsElementPresent(Element element)
		{
			return representedElements.ContainsKey(element);
		}

		/// <summary>
		/// Creates new view helper for <paramref name="element"/> according to the registration.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="diagram">The diagram.</param>
		/// <returns></returns>
		public ViewHelper CreateNewViewHelper(Element element, Diagram diagram)
		{
			RepresentantRegistration? representantRegistration = TrySearchRepresentantType(element);
			if (representantRegistration != null)
			{
				return (ViewHelper)Activator.CreateInstance(representantRegistration.Value.ViewHelperType, diagram);
			}
			else
				throw new ArgumentException(string.Format("Can not represent element of type {0}", element.GetType().Name), "element");
		}

		/// <summary>
		/// Returns model element that is represented by <paramref name="representant"/>
		/// </summary>
		/// <param name="representant">The representant.</param>
		/// <returns>model element for representant</returns>
		public Element GetElementRepresentedBy(IModelElementRepresentant representant)
		{
			if (representedElements.ContainsValue(representant))
			{
				return representedElements.Keys.Where(key => representedElements[key] == representant).FirstOrDefault();
			}
			return null;
		}

		/// <summary>
		/// Gets the element representations ordered by their <see cref="RepresentantRegistration.LoadPriority">load priority</see>.
		/// </summary>
		/// <value>The element representation order.</value>
		public IEnumerable<Type> ElementRepresentationOrder
		{
			get
			{
				return registeredRepresentations.Values.OrderBy(registration => registration.LoadPriority)
					.Select(registration => registration.ModelElementType);
			}
		}
	}
}