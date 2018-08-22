using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Helper class that initializes <see cref="XCaseCanvas.XCaseCanvasState"/> when dragging element.
	/// Provides sample object that is dragged on the canvas and a commmandFactory that creates a command
	/// that is executed when the object is dropped. Can be associated with a control. 
	/// </summary>
	public class DragButtonData
	{
		/// <summary>
		/// Creates new instance of <see cref="DragButtonData"/>
		/// </summary>
		/// <param name="commandFactory">factory that creates commands of desired type. Factory must create commands of type 
		/// <see cref="NewPositionableElementMacroCommand"/>.</param>
		/// <param name="draggedObject">object that will represent dragged object on the canvas</param>
		public DragButtonData(ICommandFactory<DiagramController> commandFactory, FrameworkElement draggedObject)
		{
			CommandFactory = commandFactory;
			DraggedObject = draggedObject;

			AssociatedControls = new List<Control>();
		}

		/// <summary>
		/// Factory that creates commands of desired type. Factory must create commands of type 
		/// <see cref="NewPositionableElementMacroCommand"/>.
		/// </summary>
		public ICommandFactory<DiagramController> CommandFactory { get; set; }

		/// <summary>
		/// Sample object for a certain kind of objects that is dragged when performing
		/// drag and drop
		/// </summary>
		public FrameworkElement DraggedObject { get; private set; }

		/// <summary>
		/// The control that was clicked when dragging started
		/// </summary>
		/// <value><see cref="Control"/></value>
		public Control Sender { get; set; }

		/// <summary>
		/// List of controls that are associated with this <see cref="DragButtonData"/> object
		/// </summary>
		private List<Control> AssociatedControls { get; set; }

		public event MouseButtonEventHandler DragCompleted;

		/// <summary>
		/// Raises the <see cref="DragCompleted"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		public void OnDragCompleted(MouseButtonEventArgs e)
		{
			MouseButtonEventHandler dragCompletedHandler = DragCompleted;
			if (dragCompletedHandler != null)
			{
				dragCompletedHandler(this, e);
			}
		}

		/// <summary>
		/// Sets <paramref name="control"/>'s Tag property to this <see cref="DragButtonData" /> object.
		/// </summary>
		/// <param name="control">associated control</param>
		public void AssociateWithControl(Control control)
		{
			control.Tag = this;
			AssociatedControls.Add(control);
		}

		/// <summary>
		/// Creaes new command via <see cref="CommandFactory"/>
		/// </summary>
		/// <param name="diagramController">diagram controller</param>
		/// <returns>created command</returns>
		public NewPositionableElementMacroCommand CreateCommand(DiagramController diagramController)
		{
			return (NewPositionableElementMacroCommand)CommandFactory.Create(diagramController);
		}
	}
}