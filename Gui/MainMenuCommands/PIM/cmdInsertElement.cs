using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.View.Controls;
using XCase.View.Interfaces;
using System.Windows.Input;
using System.Linq;
using XCase.Model;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Command that initializes dragging of an element into the active diagram.
	/// </summary>
	/// <seealso cref="XCaseCanvas.DraggingElementState"/>
	public abstract class cmdInsertElement : MainMenuCommandBase
	{
		protected cmdInsertElement(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		protected DragButtonData dragData { get; set; }  

		protected abstract DragButtonData PrepareButtonData();
	
		public override void Execute(object parameter)
		{
			ActiveDiagramView.State = ECanvasState.DraggingElement;

			ActiveDiagramView.Cursor = Cursors.Hand;
			dragData = PrepareButtonData();
			dragData.DragCompleted += dragData_DragCompleted;
			ActiveDiagramView.Children.Add(dragData.DraggedObject);
			DragButtonData classButtonData = dragData;
			classButtonData.AssociateWithControl(Control);
			ActiveDiagramView.draggingElementState.DragData = classButtonData;
			classButtonData.DraggedObject.Visibility = Visibility.Hidden;
			classButtonData.Sender = Control;
			ActiveDiagramView.CaptureMouse();
		}

		void dragData_DragCompleted(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ActiveDiagramView.Cursor = Cursors.Arrow;
            dragData.DraggedObject.Visibility = Visibility.Hidden;
			ActiveDiagramView.Children.Remove(dragData.DraggedObject);
			ActiveDiagramView.State = ECanvasState.Normal;

			NewPositionableElementMacroCommand command = dragData.CreateCommand(ActiveDiagramView.Controller);
			
			Point p1 = e.GetPosition(ActiveDiagramView);
			Point p2 = e.GetPosition(dragData.Sender);

			if (p1.X >= 0 && p1.Y >= 0 && p1.X <= ActiveDiagramView.ActualWidth && p1.Y <= ActiveDiagramView.ActualHeight)
			{
				command.X = e.GetPosition(ActiveDiagramView).X;
				command.Y = e.GetPosition(ActiveDiagramView).Y;

			}
			else if (p2.X >= 0 && p2.Y >= 0 && p2.X <= dragData.Sender.ActualWidth && p2.Y <= dragData.Sender.ActualHeight)
			{
				command.X = 40;
				command.Y = 40;
			}
			InitializeCommand(command);
			command.Set(ActivePanelWindow.ModelController, ActivePanelWindow.ModelController.Model);
			command.Execute();
			ElementInserted(command.AssociatedElements);
			
		}

		protected virtual void InitializeCommand(NewPositionableElementMacroCommand command) { }

		protected virtual void ElementInserted(IList<Element> elements)
		{
			
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null; 
		}
	}
}