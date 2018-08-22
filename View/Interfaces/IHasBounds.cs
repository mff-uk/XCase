using System.Windows;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that can return bounding rectangle.
	/// </summary>
	public interface IHasBounds
	{
		/// <summary>
		/// Gets the bounding rectangle
		/// </summary>
		/// <returns>bounding rectangle</returns>
		Rect GetBounds();

		/// <summary>
		/// When bounding rectangle of the control is tilted, this property returns the 
		/// angle in degrees. 
		/// </summary>
		/// <value><see cref="double"/>, default 0</value>
		double BoundsAngle { get; }
	}
}