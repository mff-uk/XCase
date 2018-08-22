using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller.Commands.Helpers
{
	/// <summary>
	/// Stores result of a an action that creates new element.
	/// </summary>
	/// <typeparam name="ElementType">The type of the element created.</typeparam>
	/// <typeparam name="ViewHelperType">The type of the view helper of the element created.</typeparam>
	public struct CreationResult<ElementType, ViewHelperType>
		where ElementType: Element
		where ViewHelperType: ViewHelper
	{
		/// <summary>
		/// Created element
		/// </summary>
		public ElementType ModelElement { get; set; }

		/// <summary>
		/// Created view helper.
		/// </summary>
		/// <value></value>
		public ViewHelperType ViewHelper { get; set; }
	}
}
