using System.Collections.Generic;
using System.Windows;
using XCase.Model;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// By implementing this interface the element can be 
	/// aligned by its borders.
	/// </summary>
	public interface IAlignable
	{
		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		/// <value></value>
		PositionableElementViewHelper ViewHelper { get; }

		/// <summary>
		/// Y coordinate of the top border
		/// </summary>
		/// <value><see cref="double"/></value>
		double Top { get; }

		/// <summary>
		/// X coordinate of the left border
		/// </summary>
		/// <value><see cref="double"/></value>
		double Left { get; }
        
		/// <summary>
		/// Y coordinate of the bottom border
		/// </summary>
		double Bottom { get; }
		
		/// <summary>
		/// X coordinate of the right bourder
		/// </summary>
		double Right { get; }

		/// <summary>
		/// Returns points significant points of the control to which 
		/// visual aids lines are drawn when other control is 
		/// dragged around this control. 
		/// </summary>
		/// <returns></returns>
		IEnumerable<Point> GetVisualAidsPoints();

		/// <summary>
		/// Returns bounding rectangle
		/// </summary>
		/// <returns>bounding rectangle</returns>
		Rect GetBounds();
	}
}