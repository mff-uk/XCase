using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper for elements that are represented as (poly)lines on the diagram . 
	/// </summary>
	/// <typeparam name="ConnectionElementType">The type of the connection element type.</typeparam>
	public abstract class ConnectionViewHelper<ConnectionElementType> : PositionableElementViewHelper
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		protected ConnectionViewHelper()
		{
		}

		protected ConnectionViewHelper(Diagram diagram)
			: base(diagram)
		{
		}

		/// <summary>
		/// Returns connection points.
		/// </summary>
		public abstract ObservablePointCollection Points
		{
			get;
		}
	}
}