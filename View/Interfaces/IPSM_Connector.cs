using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls, that act as edges in PSM diagram tree. 
	/// </summary>
	public interface IPSM_Connector
	{
		/// <summary>
		/// Deletes the control from canvas.
		/// </summary>
		void DeleteFromCanvas();

		/// <summary>
		/// Reference to an underlying junction
		/// </summary>
		/// <value><see cref="XCaseJunction"/></value>
		XCaseJunction Junction { get; }
	}
}