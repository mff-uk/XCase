using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Command initializes dragging of an existing class into the active diagram.
	/// </summary>
	/// <seealso cref="XCaseCanvas.DraggingElementState"/>
	public class cmdIncludeClass : RoutedCommand
	{
		public MainWindow MainWindow
		{
			get;
			set;
		}

		public static void Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PIMClass modelClass = (PIMClass)(((StackPanel)((Button)e.OriginalSource).Parent).DataContext);
			XCaseCanvas ActiveDiagramView = ((MainWindow)sender).ActiveDiagram;

			if (ActiveDiagramView != null && !ActiveDiagramView.ElementRepresentations.IsElementPresent(modelClass) && !(modelClass is AssociationClass))
			{
				ActiveDiagramView.State = ECanvasState.DraggingElement;

				PIM_Class draggedObject = new PIM_Class(ActiveDiagramView);
				draggedObject.InitializeRepresentant(modelClass, new ClassViewHelper(ActiveDiagramView.Diagram), new ClassController(modelClass, ActiveDiagramView.Controller));

				DragButtonData dragData = new DragButtonData(ElementToDiagramCommandFactory<Class, ClassViewHelper>.Factory(), draggedObject);
				dragData.DragCompleted += dragData_DragCompleted;

				DragButtonData classButtonData = dragData;
				classButtonData.AssociateWithControl((Button)e.OriginalSource);
				ActiveDiagramView.draggingElementState.DragData = classButtonData;
				classButtonData.DraggedObject.Visibility = Visibility.Hidden;
				classButtonData.Sender = (Button)e.OriginalSource;
				ActiveDiagramView.CaptureMouse();
			}
		}

		static void dragData_DragCompleted(object sender, MouseButtonEventArgs e)
		{
			DragButtonData dragData = (DragButtonData)sender;
			PIM_Class draggedClass = ((PIM_Class)dragData.DraggedObject);
			XCaseCanvas ActiveDiagramView = draggedClass.XCaseCanvas;

			dragData.DraggedObject.Visibility = Visibility.Hidden;
			ActiveDiagramView.Children.Remove(dragData.DraggedObject);
			ActiveDiagramView.State = ECanvasState.Normal;

			if (draggedClass.Left > 0 && draggedClass.Top > 0)
			{
				if (ActiveDiagramView.Diagram is PIMDiagram)
				{
					ElementToDiagramCommand<Class, ClassViewHelper> command =
						(ElementToDiagramCommand<Class, ClassViewHelper>)dragData.CommandFactory.Create(ActiveDiagramView.Controller);

					command.ViewHelper.X = draggedClass.Left;
					command.ViewHelper.Y = draggedClass.Top;
					command.IncludedElement = new ElementHolder<Class> { Element = (Class)draggedClass.ModelElement };
					command.Execute();
				}
				else if (ActiveDiagramView.Diagram is PSMDiagram)
				{
					DerivePSMClassToDiagramCommand c = (DerivePSMClassToDiagramCommand)DerivePSMClassToDiagramCommandFactory.Factory().Create(ActiveDiagramView.Controller.ModelController);
					c.Set(draggedClass.ClassController.Class, (PSMDiagram)ActiveDiagramView.Diagram);
					if (c.CanExecute()) c.Execute();
				}
			}
		}
	}
}