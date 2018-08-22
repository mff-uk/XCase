using System.Windows;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls, whose bounding rectangle used for
	/// mouse hittesting is different
	/// than the rectangle returned by <see cref="IHasBounds.GetBounds"/>
	/// </summary>
	public interface IHasSelectionBounds: ISelectable
	{
		/// <summary>
		/// Rectangle used for mouse hittesting. When mouseclicks occurs
		/// in the returned rectangle the control is selected. 
		/// </summary>
		/// <returns>rectangle</returns>
		Rect GetSelectionBounds();
	}
}