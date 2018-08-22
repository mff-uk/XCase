using System;

namespace XCase.Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class NotCloneableAttribute: Attribute
	{
		
	}
}