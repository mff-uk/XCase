using System;

namespace XCase.Model
{
	/// <summary>
	/// Marks non-abstract classes inherited from <see cref="Element"/>
	/// that do not need to override <see cref="Element.RemoveMeFromModel"/> and <see cref="Element.PutMeBackToModel"/>
	/// methods.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NoDeleteUndoRedoSupportAttribute: Attribute
	{
		
	}
}