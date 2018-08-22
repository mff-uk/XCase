using System;

namespace XCase.Controller
{
	/// <summary>
	/// Declares that a property contains a class from Model. Another properties of a class from View
	/// can be marked with <see cref="ModelPropertyMappingAttribute"/> to bound to a model class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ModelElementAttribute: System.Attribute
	{
		
	}
}