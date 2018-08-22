using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Moves element right in the PSM tree.
	/// </summary>
	public class cmdMoveToRight : MainMenuCommandBase
	{
		public cmdMoveToLeft cmdMoveToLeft { get; set; }

		public cmdMoveToRight(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count == 1)
			{
				ISelectable selectedItem = ActiveDiagramView.SelectedItems.First();
                IList<PSMSuperordinateComponent> rootsCollection;
				IList<PSMSubordinateComponent> subordinateComponentCollection;
				IList<PSMAssociationChild> associationChildCollection;

				Element component = cmdMoveToLeft.GetComponentAndCollection(selectedItem, (PSMDiagram)ActiveDiagramView.Diagram, out rootsCollection, out associationChildCollection, out subordinateComponentCollection);

				if (component != null)
				{
					int index;
					if (rootsCollection != null)
					{
						if ((index = cmdMoveToLeft.GetIndexInCollection(rootsCollection, (PSMClass)component)) < rootsCollection.Count - 1)
						{
                            ComponentsReorderCommand<PSMSuperordinateComponent> command =
                                (ComponentsReorderCommand<PSMSuperordinateComponent>)
                                ComponentsReorderCommandFactory<PSMSuperordinateComponent>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = rootsCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index + 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToLeft.OnCanExecuteChanged(new EventArgs());
						}
					}
					if (subordinateComponentCollection != null)
					{
						if ((index = cmdMoveToLeft.GetIndexInCollection(subordinateComponentCollection, (PSMSubordinateComponent)component)) < subordinateComponentCollection.Count - 1)
						{
							ComponentsReorderCommand<PSMSubordinateComponent> command =
								(ComponentsReorderCommand<PSMSubordinateComponent>)
								ComponentsReorderCommandFactory<PSMSubordinateComponent>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = subordinateComponentCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index + 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToLeft.OnCanExecuteChanged(new EventArgs());
						}
					}
					if (associationChildCollection != null)
					{
						if ((index = cmdMoveToLeft.GetIndexInCollection(associationChildCollection, (PSMAssociationChild)component)) < associationChildCollection.Count - 1)
						{
							ComponentsReorderCommand<PSMAssociationChild> command =
								(ComponentsReorderCommand<PSMAssociationChild>)
								ComponentsReorderCommandFactory<PSMAssociationChild>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = associationChildCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index + 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToLeft.OnCanExecuteChanged(new EventArgs());
						}
					}
				}
			}
		}

		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.Diagram is PSMDiagram && ActiveDiagramView.SelectedItems.Count == 1)
			{
				ISelectable selectedItem = ActiveDiagramView.SelectedItems.First();
                IList<PSMSuperordinateComponent> rootsCollection;
				IList<PSMSubordinateComponent> subordinateComponentCollection;
				IList<PSMAssociationChild> associationChildCollection;

				Element component = cmdMoveToLeft.GetComponentAndCollection(selectedItem, (PSMDiagram)ActiveDiagramView.Diagram, out rootsCollection,
				                                                            out associationChildCollection, out subordinateComponentCollection);

				if (component != null)
				{
					if (rootsCollection != null)
					{
						if ((cmdMoveToLeft.GetIndexInCollection(rootsCollection, (PSMClass)component)) < rootsCollection.Count - 1)
						{
							return true;
						}
					}
					if (subordinateComponentCollection != null)
					{
						if ((cmdMoveToLeft.GetIndexInCollection(subordinateComponentCollection, (PSMSubordinateComponent)component)) < subordinateComponentCollection.Count - 1)
						{
							return true;
						}
					}
					if (associationChildCollection != null)
					{
						if ((cmdMoveToLeft.GetIndexInCollection(associationChildCollection, (PSMAssociationChild)component)) < associationChildCollection.Count - 1)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}