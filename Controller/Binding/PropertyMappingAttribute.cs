using System;
using System.Reflection;

namespace XCase.Controller
{
	/// <summary>
	/// Declares binding of a property to a property of a binding source. 
	/// a property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class PropertyMappingAttribute : Attribute
	{
		/// <summary>
		/// Declares binding of a property to a property of a binding source. 
		/// Use this constructor when <see cref="ViewHelperPropertyMappingAttribute"/>'s target is 
		/// a property.
		/// </summary>
		/// <param name="sourcePropertyName">field of the binding source to bound to. Field must support INotifyPropertyChanged</param>
		public PropertyMappingAttribute(string sourcePropertyName)
		{
			SourcePropertyName = sourcePropertyName;
		}

		/// <summary>
		/// Declares binding of a property to a property of a binding source. 
		/// Use this constructor when <see cref="PropertyMappingAttribute"/>'s target is 
		/// a class or a struct.
		/// </summary>
		/// <param name="sourcePropertyName">name of the binding source to bound to. Field must support INotifyPropertyChanged</param>
		/// <param name="targetPropertyName">name of the target field of the bound class</param>
		public PropertyMappingAttribute(string sourcePropertyName, string targetPropertyName)
		{
			SourcePropertyName = sourcePropertyName;
			TargetPropertyName = targetPropertyName;
		}

		/// <summary>
		/// Field of the binding source to bound to. Field must support INotifyPropertyChanged.
		/// </summary>
		public string SourcePropertyName { get; set; }

		/// <summary>
		/// This field can be used when <see cref="PropertyMappingAttribute"/>'s target is 
		/// a class or a struct. <see cref="TargetPropertyName"/> is then name of the field or property of 
		/// the class or property, that is bound to <see cref="SourcePropertyName"/>
		/// </summary>
		public string TargetPropertyName { get; set; }

		/// <summary>
		/// Property for which the attribute is declared (not filled automatically!)
		/// </summary>
		/// <value><see cref="PropertyInfo"/></value>
		internal PropertyInfo declaringProperty { get; set; }
	}
}