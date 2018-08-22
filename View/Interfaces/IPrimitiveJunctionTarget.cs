using System.Windows;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls to which <see cref="XCasePrimitiveJunction"/> can 
	/// be attached.
	/// </summary>
	public interface IPrimitiveJunctionTarget
	{
		/// <summary>
		/// Point returned by this function will be used 
		/// as an endpoint by the <see cref="XCasePrimitiveJunction"/> that
		/// is attached to the control
		/// </summary>
		/// <param name="point">point where the attached <see cref="XCasePrimitiveJunction"/> starts</param>
		/// <returns>point where the attached <see cref="XCasePrimitiveJunction"/> should end</returns>
		Point FindClosestPoint(Point point);
	}
}