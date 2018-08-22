using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Moves element left in the PSM tree
	/// </summary>
	public class cmdMoveToLeft : MainMenuCommandBase
	{
		public cmdMoveToRight cmdMoveToRight { get; set; }

		public cmdMoveToLeft(MainWindow mainWindow, Control control)
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

				Element component = GetComponentAndCollection(selectedItem, (PSMDiagram)ActiveDiagramView.Diagram, out rootsCollection, out associationChildCollection, out subordinateComponentCollection);

				if (component != null)
				{
					int index;
					if (rootsCollection != null)
					{
						if ((index = GetIndexInCollection(rootsCollection, (PSMClass)component)) > 0)
						{
                            ComponentsReorderCommand<PSMSuperordinateComponent> command =
                                (ComponentsReorderCommand<PSMSuperordinateComponent>)
                                ComponentsReorderCommandFactory<PSMSuperordinateComponent>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = rootsCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index - 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToRight.OnCanExecuteChanged(new EventArgs());
						}
					}
					if (subordinateComponentCollection != null)
					{
						if ((index = GetIndexInCollection(subordinateComponentCollection, (PSMSubordinateComponent)component)) > 0)
						{
							ComponentsReorderCommand<PSMSubordinateComponent> command =
								(ComponentsReorderCommand<PSMSubordinateComponent>)
								ComponentsReorderCommandFactory<PSMSubordinateComponent>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = subordinateComponentCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index - 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToRight.OnCanExecuteChanged(new EventArgs());
						}
					}
					if (associationChildCollection != null)
					{
						if ((index = GetIndexInCollection(associationChildCollection, (PSMAssociationChild)component)) > 0)
						{
							ComponentsReorderCommand<PSMAssociationChild> command =
								(ComponentsReorderCommand<PSMAssociationChild>)
								ComponentsReorderCommandFactory<PSMAssociationChild>.Factory().Create(ActiveDiagramView.Controller);
							command.Action = EReorderAction.MoveOneItem;
							command.Collection = associationChildCollection;
							command.ItemIndex = index;
							command.NewItemIndex = index - 1;
							command.Execute();
							OnCanExecuteChanged(new EventArgs());
							cmdMoveToRight.OnCanExecuteChanged(new EventArgs());
						}
					}
				}
			}
		}

		internal static Element GetComponentAndCollection(ISelectable selectedItem, PSMDiagram diagram, out IList<PSMSuperordinateComponent> rootsCollection, out IList<PSMAssociationChild> associationChildCollection, out IList<PSMSubordinateComponent> subordinateComponentCollection)
		{
			rootsCollection = null;
			associationChildCollection = null;
			subordinateComponentCollection = null;

			if (selectedItem is IPSMSubordinateComponentRepresentant)
			{
				PSMSubordinateComponent subordinateComponent = ((IPSMSubordinateComponentRepresentant)selectedItem).ModelSubordinateComponent;
				if (subordinateComponent.Parent != null)
				{
					subordinateComponentCollection = subordinateComponent.Parent.Components;
					return subordinateComponent;
				}
			}

			PSMAssociationChild associationChild = null;
			if (selectedItem is PSM_Class)
			{
				associationChild = ((PSM_Class)selectedItem).PSMClass;
				PSMClass psmClass = (PSMClass)associationChild;

				if (diagram.Roots.Contains(psmClass))
				{
					rootsCollection = diagram.Roots;
					return psmClass;
				}
			}
			if (selectedItem is PSM_ClassUnion)
				associationChild = ((PSM_ClassUnion)selectedItem).ClassUnion;

			if (associationChild != null)
			{
				if (associationChild.ParentAssociation != null && associationChild.ParentAssociation.Parent != null)
				{
					subordinateComponentCollection = associationChild.ParentAssociation.Parent.Components;
					return associationChild.ParentAssociation;
				}
				else if (associationChild.ParentUnion != null)
				{
					associationChildCollection = associationChild.ParentUnion.Components;
					return associationChild;
				}
			}

			return null;
		}

		internal static int GetIndexInCollection<Type>(IList<Type> collection, Type element)
		{
			return collection.IndexOf(element);
		}

		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.Diagram is PSMDiagram && ActiveDiagramView.SelectedItems.Count == 1)
			{
				ISelectable selectedItem = ActiveDiagramView.SelectedItems.First();
                IList<PSMSuperordinateComponent> rootsCollection;
				IList<PSMSubordinateComponent> subordinateComponentCollection;
				IList<PSMAssociationChild> associationChildCollection;

				Element component = GetComponentAndCollection(selectedItem, (PSMDiagram)ActiveDiagramView.Diagram, out rootsCollection,
				                                              out associationChildCollection, out subordinateComponentCollection);

				if (component != null)
				{
					if (rootsCollection != null)
					{
                        if ((GetIndexInCollection(rootsCollection, (PSMSuperordinateComponent)component)) > 0)
						{
							return true;
						}
					}
					if (subordinateComponentCollection != null)
					{
						if ((GetIndexInCollection(subordinateComponentCollection, (PSMSubordinateComponent)component)) > 0)
						{
							return true;
						}
					}
					if (associationChildCollection != null)
					{
						if ((GetIndexInCollection(associationChildCollection, (PSMAssociationChild)component)) > 0)
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