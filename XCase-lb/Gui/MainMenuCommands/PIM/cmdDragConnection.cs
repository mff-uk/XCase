using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using EDraggedConnectionType = XCase.View.Controls.XCaseCanvas.DraggingConnectionState.EDraggedConnectionType;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Command sets current diagram into <see cref="ECanvasState.DraggingConnection"/> state.
	/// </summary>
	/// <seealso cref="XCaseCanvas.DraggingConnectionState"/>
	public class cmdDragConnection : MainMenuCommandBase, IDraggedConnectionProcessor
	{
		public IEnumerable<IDraggedConnectionProcessor> ToggleButtonGroup { get; set; }

		public EDraggedConnectionType DraggedConnectionType { get; private set; }

		public cmdDragConnection(MainWindow mainWindow, Control control, EDraggedConnectionType draggedConnectionType)
			: base(mainWindow, control)
		{
			Debug.Assert(control is ToggleButton);
			DraggedConnectionType = draggedConnectionType;
			MainWindow.ActiveDiagramChanged += delegate
			                                   	{
													UntoggleButtons(); 
			                                   		OnCanExecuteChanged(null);
			                                   	};
		}

		/// <summary>
		/// Sets active diagram into <see cref="XCaseCanvas.DraggingConnectionState"></see>.
		/// When connection is dragged, <see cref="DragConnectionCompleted"/> is called to finalize the process.
		/// </summary>
		/// <param name="parameter">ignored</param>
		public override void Execute(object parameter)
		{
			if (((ToggleButton) Control).IsChecked == true)
			{
				UntoggleButtons();
				ActiveDiagramView.State = ECanvasState.DraggingConnection;
				ActiveDiagramView.draggingConnectionState.ToggleButtonsGroup = ToggleButtonGroup;
				ActiveDiagramView.draggingConnectionState.DraggedConnectionProcessor = this;
				ActiveDiagramView.draggingConnectionState.DraggedConnectionType = DraggedConnectionType;
			}
			else
			{
				ActiveDiagramView.State = ECanvasState.Normal;
				ActiveDiagramView.draggingConnectionState.DraggedConnectionProcessor = null;
			}
		}

		/// <summary>
		/// Untoggles all buttons in <see cref="ToggleButtonGroup"/>
		/// </summary>
		private void UntoggleButtons()
		{
			foreach (IDraggedConnectionProcessor processor in ToggleButtonGroup)
			{
				processor.StateLeft();
			}
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null;
		}

		public void DragConnectionCompleted(Element sourceElement, Element targetElement)
		{
			if (DraggedConnectionType == EDraggedConnectionType.Generalization)
			{
				if (sourceElement != null && targetElement != null && sourceElement != targetElement
					&& sourceElement is Class && targetElement is Class)
				{
					ActiveDiagramView.Controller.NewGeneralization((Class)targetElement, (Class)sourceElement);
				}
			}
			else 
			{	
				if (sourceElement != null && targetElement != null
					&& sourceElement is Class && targetElement is Class)
				{
					CreationResult<Association, AssociationViewHelper> resA;
					if (sourceElement != targetElement)
					{
						resA = ActiveDiagramView.Controller.NewAssociation(null, (Class) sourceElement, (Class) targetElement);
					}
					else // self reference
					{
						resA = ActiveDiagramView.Controller.NewAssociation(null, (Class)sourceElement);
					}
					if (resA.ModelElement != null &&
						(DraggedConnectionType == EDraggedConnectionType.Composition
						|| DraggedConnectionType == EDraggedConnectionType.Aggregation))
					{
						((PIM_Association)ActiveDiagramView.ElementRepresentations[resA.ModelElement]).Controller.ChangeAggregation(resA.ModelElement.Ends.Last(), XCaseCanvas.DraggingConnectionState.GetAggregationType(DraggedConnectionType));
					}

					if (DraggedConnectionType == EDraggedConnectionType.NavigableAssociation)
					{
						throw new NotImplementedException("Method or operation is not implemented.");
					}
				} else if (sourceElement != null && targetElement != null 
					&& sourceElement is Association && targetElement is PIMClass)
				{
					// adding new association end to an existing association
					((PIM_Association)ActiveDiagramView.ElementRepresentations[sourceElement]).Controller.AddAssociationEnd((Association) sourceElement, (PIMClass) targetElement);
				}
			}
		}

		public void StateActivated(EDraggedConnectionType connectionType)
		{
			((ToggleButton)this.Control).IsChecked = connectionType == DraggedConnectionType;
		}

		public void StateLeft()
		{
			((ToggleButton)this.Control).IsChecked = false;
		}
	}
}