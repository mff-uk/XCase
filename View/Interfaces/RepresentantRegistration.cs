using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Record of one registration used by <see cref="RepresentationCollection"/>.
	/// Connects together type of a model element, its representation and controller.
	/// </summary>
	/// <seealso cref="RepresentationCollection"/>
	public struct RepresentantRegistration
	{
		/// <summary>
		/// Model element 
		/// </summary>
		public Type ModelElementType { get; private set; }

		/// <summary>
		/// View element that represents <see cref="ModelElementType"/>
		/// </summary>
		public Type RepresentantType { get; private set; }

		/// <summary>
		/// Controller type of <see cref="ModelElementType"/>
		/// </summary>
		public Type ControllerType { get; private set; }

		/// <summary>
		/// ViewHelper type of <see cref="ModelElementType"/>
		/// </summary>
		public Type ViewHelperType { get; private set; }

		/// <summary>
		/// Load priority of the elements, elements with lower priority
		/// are loaded earlier than elements with higher prioerity when 
		/// a diagram is opened. Default value is 10. 
		/// </summary>
		public uint LoadPriority { get; set; }

		/// <summary>
		/// When more registrations match <see cref="ModelElementType"/>,
		/// Registration with lower RepresentantPriority is prefered. 
		/// Default value is 10.
		/// </summary>
		/// <remarks>
		/// Since model elements are publicly accessed through interfaces, it is possible that some
		/// model element = interface will match more registrations. For example, AssociationClass is an
		/// interface derived from Class therefore each registration for Class will also match for 
		/// AssociationClass because <i>AssociationClass is Class</i>. Assigning lower priority 
		/// to registration for AssociationClass than for Class ensures, that representant for 
		/// AssociationClass is selected instead of representant for Class. 
		/// </remarks>
		public uint RepresentantPriority { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="RepresentantRegistration" />. 
		/// </summary>
		/// <param name="modelElementType">modelElementType must be a type implementing Element interface</param>
		/// <param name="representantType">representantType must be a type implementing IModelElementRepresentant interface</param>
		/// <param name="controllerType">controllerType must be a subclass of ElementController</param>
		/// <param name="viewHelperType">viewHelperType must be a subclass of ViewHelper</param>
		/// <param name="loadPriority">load priority for elements of type <paramref name="modelElementType" /></param>
		/// <param name="representantPriority">representant priority for representants of type <paramref name="representantType"/></param>
		/// <remarks>
		/// <para>Type representantType must declare public constructor with 'representantType(XCaseCanvas)' signature</para>
		/// <para>Type controllerType must declare public constructor with 'controllerType(Element, DiagramController)' signature</para>
		/// </remarks>
		public RepresentantRegistration(Type modelElementType, Type representantType, Type controllerType, Type viewHelperType, uint loadPriority, uint representantPriority) 
			: this()
		{
			if (!typeof(IModelElementRepresentant).IsAssignableFrom(representantType))
			{
				throw new ArgumentException("Parameter representantType must be a type implementing IModelElementRepresentant interface", "representantType");
			}

			if (!typeof(Element).IsAssignableFrom(modelElementType))
			{
				throw new ArgumentException("Parameter modelElementType must be a type implementing Element interface", "modelElementType");
			}

			if (!controllerType.IsSubclassOf(typeof(ElementController)))
			{
				throw new ArgumentException("Parameter controllerType must be a subclass of ElementController", "controllerType");
			}

			if (!viewHelperType.IsSubclassOf(typeof(ViewHelper)))
			{
				throw new ArgumentException("Parameter viewHelperType must be a subclass of ViewHelper", "controllerType");
			}

			if (representantType.GetConstructor(new Type[] {typeof(XCaseCanvas)}) == null)
			{
				throw new ArgumentException("Type representantType must declare public constructor with 'representantType(XCaseCanvas)' signature", "representantType");
			}

			if (controllerType.GetConstructor(new Type[] { typeof(Element), typeof(DiagramController) }) != null)
			{
				throw new ArgumentException("Type controllerType must declare public constructor with 'controllerType(Element, DiagramController)' signature", "controllerType");
			}

			ModelElementType = modelElementType;
			RepresentantType = representantType;
			ControllerType = controllerType;
			ViewHelperType = viewHelperType;
			LoadPriority = loadPriority;
			RepresentantPriority = representantPriority;
		}

		/// <summary>
		/// Creates new instance of <see cref="RepresentantRegistration"/>.
		/// </summary>
		/// <param name="modelElementType">modelElementType must be a type implementing Element interface</param>
		/// <param name="representantType">representantType must be a type implementing IModelElementRepresentant interface</param>
		/// <param name="controllerType">controllerType must be a subclass of ElementController</param>
		/// <param name="viewHelperType">viewHelperType must be a subclass of ViewHelper</param>
		/// <remarks>
		/// 	<para>Type representantType must declare public constructor with 'representantType(XCaseCanvas)' signature</para>
		/// 	<para>Type controllerType must declare public constructor with 'controllerType(Element, DiagramController)' signature</para>
		/// </remarks>
		public RepresentantRegistration(Type modelElementType, Type representantType, Type controllerType, Type viewHelperType) : 
			this(modelElementType, representantType, controllerType, viewHelperType, 10, 10)
		{
			
		}
	}

	/// <summary>
	/// List of <see cref="RepresentantRegistration"/>'s
	/// </summary>
	public class RegistrationSet : List<RepresentantRegistration>
	{
		public IEnumerable<Type> ElementRepresentationOrder
		{
			get
			{
				return this.OrderBy(registration => registration.LoadPriority)
					.Select(registration => registration.ModelElementType);
			}
		}
	}
}