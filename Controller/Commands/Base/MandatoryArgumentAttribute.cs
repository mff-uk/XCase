using System;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Denotes property of a command as a command argument that must be specified before
	/// <see cref="CommandBase.Execute()"/> is called. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class MandatoryArgumentAttribute: Attribute
	{
		
	}
}