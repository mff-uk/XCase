using System;
using XCase.Model;

namespace XCase.Controller
{
	/// <summary>
	/// Declares that a property contains a <see cref="ViewHelper"/> class. Another 
	/// properties of a class from View can be marked with <see cref="ViewHelperPropertyMappingAttribute"/> 
	/// to bound to a <see cref="ViewHelper"/> class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ViewHelperElementAttribute: System.Attribute
	{
		
	}
}