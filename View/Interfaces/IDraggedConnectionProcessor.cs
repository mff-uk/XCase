using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that cooperates with 
	/// <see cref="XCaseCanvas.DraggingConnectionState"/>. 
	/// The object implementing the interface must be assigned to 
	/// <see cref="XCaseCanvas.DraggingConnectionState.DraggedConnectionProcessor"/>
	/// to work as described. 
	/// </summary>
	/// <seealso cref="XCaseCanvas.DraggingConnectionState"/>
	public interface IDraggedConnectionProcessor
	{
		/// <summary>
		/// This function is called when a connection is dragged 
		/// between two connectable elements on a canvas that is in
		/// state <see cref="XCaseCanvas.DraggingConnectionState"/>.
		/// </summary>
		/// <param name="sourceElement">source element of the connection</param>
		/// <param name="targetElement">target element of the connection</param>
		void DragConnectionCompleted(Element sourceElement, Element targetElement);

		/// <summary>
		/// This method is called when state <see cref="XCaseCanvas.DraggingConnectionState"/> is activated.
		/// </summary>
		/// <param name="connectionType">type of the connection that can will be dragged on canvas 
		/// in state <see cref="XCaseCanvas.DraggingConnectionState"/></param>
		/// <seealso cref="XCaseCanvas.DraggingConnectionState.DraggedConnectionType"/>
		void StateActivated(XCaseCanvas.DraggingConnectionState.EDraggedConnectionType connectionType);

		/// <summary>
		/// This method is called when state <see cref="XCaseCanvas.DraggingConnectionState"/> is left.
		/// </summary>
		void StateLeft();
	}
}