using System.Collections.Generic;
using System.Windows;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that can create <see cref="JunctionPoint"/>s and 
	/// can be connected via <see cref="XCaseJunction"/>s.
	/// </summary>
    public interface IConnectable : IHasBounds
    {
		/// <summary>
		/// Creates <see cref="JunctionPoint"/>.
		/// </summary>
		/// <returns>new <see cref="JunctionPoint"/></returns>
        JunctionPoint CreateJunctionEnd();

		/// <summary>
		/// Creates <see cref="JunctionPoint"/> on a specific position.
		/// </summary>
		/// <param name="preferedPosition">The prefered position for a new JunctionPoint</param>
		/// <returns>new <see cref="JunctionPoint"/></returns>
		JunctionPoint CreateJunctionEnd(Point preferedPosition);

		/// <summary>
		/// Gets a value indicating whether the position and size of the control are already known and valid
		/// </summary>
		/// <value>
		/// 	<c>true</c> if position and size are valid; otherwise, <c>false</c>.
		/// </value>
		bool IsMeasureValid { get; }

		/// <summary>
		/// Highlights this control.
		/// </summary>
        void Highlight();

		/// <summary>
		/// Unhighlights this control.
		/// </summary>
		void UnHighlight();
    }
}
