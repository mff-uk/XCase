using System;
using System.Collections.Generic;

namespace XCase.Controller
{
	/// <summary>
	/// Declaring <see cref="IBindable"/> interface on a class allows 
	/// using <see cref="ModelPropertyMappingAttribute"/> and <see cref="ViewHelperPropertyMappingAttribute"/>
	/// on the class itself or its properties. Thanks to these attributes, binding between 
	/// properties of the object and properties of a model element or a view helper can
	/// be declaratively defined. 
	/// </summary>
	/// <remarks>
	/// Binding starts by calling <code>StartBindings</code> is suspended by calling 
	/// <code>StopBindigs</code>. Both these methods are extension methods for the IBindable interface.
	/// </remarks>
	public interface IBindable
	{
		
	}

	/// <summary>
	/// Contains declarations of extension methods 
	/// for <see cref="IBindable"/> interface. 
	/// </summary>
	public static class BindableSupport
	{
		/// <summary>
		/// Stores <see cref="TypeBindingData"/> for already examined types.
		/// </summary>
		private static readonly Dictionary<Type, TypeBindingData> _typeBindingData = new Dictionary<Type, TypeBindingData>();

		/// <summary>
		/// Gets <see cref="TypeBindingData"/> for type of <paramref name="bindable"/>.
		/// New instance is created <see cref="TypeBindingData"/> and type is examined 
		/// if this type was not used before.
		/// </summary>
		/// <param name="bindable">The bindable.</param>
		/// <returns><see cref="TypeBindingData"/> for type of <paramref name="bindable"/></returns>
		private static TypeBindingData GetTypeBindingData(this IBindable bindable)
		{
			Type t = bindable.GetType();
			if (!_typeBindingData.ContainsKey(t))
			{
				_typeBindingData[t] = new TypeBindingData(bindable.GetType());
			}
			return _typeBindingData[t];
		}

		/// <summary>
		/// Starts bindings to properties of model element 
		/// and view helper of the object.
		/// </summary>
		/// <param name="bindable">The bound object</param>
		/// <seealso cref="ModelElementAttribute" />
		/// <seealso cref="ViewHelperElementAttribute"/>
		public static void StartBindings(this IBindable bindable)
		{
			GetTypeBindingData(bindable).StartBindings(bindable);
		}

		/// <summary>
		/// Starts bindings to properties of model element
		/// or view helper of the object.
		/// </summary>
		/// <param name="bindable">The bound object</param>
		/// <param name="bindingType">Type of binding (model/view helper).</param>
		/// <seealso cref="ModelElementAttribute"/>
		/// <seealso cref="ViewHelperElementAttribute"/>
		public static void StartBindings(this IBindable bindable, TypeBindingData.EBindingSourceType bindingType)
		{
			GetTypeBindingData(bindable).StartBindings(bindable, bindingType);
		}

		/// <summary>
		/// Suspends the binding for the object. 
		/// </summary>
		/// <param name="bindable">bindable object</param>
		public static void CloseBindings(this IBindable bindable)
		{
			GetTypeBindingData(bindable).CloseBindings(bindable);
		}

		/// <summary>
		/// Suspends the binding for the object .
		/// </summary>
		/// <param name="bindable">The bound object</param>
		/// <param name="bindingType">Type of binding (model/view helper).</param>
		public static void CloseBindings(this IBindable bindable, TypeBindingData.EBindingSourceType bindingType)
		{
			GetTypeBindingData(bindable).CloseBindings(bindable, bindingType);
		}
	}
}