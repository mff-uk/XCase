using System;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// Declares that the property is bound to a property of an object from Model . 
	/// See <see cref="ModelElementAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class ModelPropertyMappingAttribute : PropertyMappingAttribute
	{
		/// <summary>
		/// Creates new ModelPropertyMappingAttribute.
		/// Use this constructor when <see cref="ModelPropertyMappingAttribute"/>'s target is 
		/// a property.
		/// </summary>
		/// <param name="sourcePropertyName">Name of the property of the class from Model</param>
		public ModelPropertyMappingAttribute(string sourcePropertyName)
			: base(sourcePropertyName)
		{
		}

		/// <summary>
		/// Declares binding of a property to a property of a binding source. 
		/// Use this constructor when <see cref="ModelPropertyMappingAttribute"/>'s target is 
		/// a class or a struct.
		/// </summary>
		/// <param name="sourcePropertyName">name of the binding source to bound to. Field must support INotifyPropertyChanged</param>
		/// <param name="targetPropertyName">name of the target field of the bound class</param>
		public ModelPropertyMappingAttribute(string sourcePropertyName, string targetPropertyName) : base(sourcePropertyName, targetPropertyName)
		{
		}
	}
}