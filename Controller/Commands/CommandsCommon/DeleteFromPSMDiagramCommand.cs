using System;
using System.Linq;
using System.Collections.Generic;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes one or more elements from a diagram, presence of the elements in other diagrams is 
	/// not affected. 
	/// </summary>
	public class DeleteFromPSMDiagramCommand : DiagramCommandBase
	{
		/// <summary>
		/// Elements that should be deleted
		/// </summary>
		[MandatoryArgument]
		public IList<Element> DeletedElements { get; set; }

		/// <summary>
		/// If set to true, <see cref="DeletedElements"/> are deleted immediately with
		/// their dependent elements and no dialog window is shown. 
		/// </summary>
		public bool ForceDelete { get; set;}

        private Dictionary<Element, ViewHelper> DeletedElementsViewHelpers { get; set; }

		private bool safeOrdering = false;

		/// <summary>
		/// If set to true (default), command will check whether the PSM diagram is left in 
		/// a consistent state after execution (i.e. all association have both parent
		/// and child present..). If the command leaves the PSM diagram in an inconsistent
		/// state and CheckOrdering is set to true, the command cannot be undone.
		/// </summary>
		public bool CheckOrdering { get; set; }

		private IEnumerable<Element> _deletionOrder;

		/// <summary>
		/// Creates new instance of <see cref="DeleteFromPSMDiagramCommand" />. 
		/// </summary>
		/// <param name="Controller">command controller</param>
		public DeleteFromPSMDiagramCommand(DiagramController Controller)
			: base(Controller)
		{
			DeletedElementsViewHelpers = new Dictionary<Element, ViewHelper>();
			Description = CommandDescription.REMOVE_FROM_DIAGRAM;
			CheckOrdering = true; 
		}
        
		private bool IsInDiagram(Element element)
		{
			return Diagram.IsElementPresent(element);
		}

		public override bool CanExecute()
		{
			foreach (Element element in DeletedElements)
			{
				if (!IsInDiagram(element))
				{
					ErrorDescription = String.Format(CommandError.CMDERR_DELETE_NONEXISTING, element);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Tells the command which elements to delete. The method uses 
		/// interactive dialogs to specify the initial set of delted elements and 
		/// to ask whether to delete unused elements from the model. 
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
		/// <returns><code>true</code> when user pressed OK, <code>false</code> when user pressed Cancel in the</returns>
		public bool InitializeCommand(params Element[] deletedElements)
		{
			return InitializeCommand(null, (IEnumerable<Element>)deletedElements);
		}

		/// <summary>
		/// Tells the command which elements to delete. The method uses 
		/// interactive dialogs to specify the initial set of delted elements and 
		/// to ask whether to delete unused elements from the model. 
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
		/// <param name="selectionCallback">function that is called, when selected elements are specified in the dialog. Can be set to null.</param>
		/// <returns><code>true</code> when user pressed OK, <code>false</code> when user pressed Cancel in the</returns>
		public bool InitializeCommand(Action<IEnumerable<Element>> selectionCallback, params Element[] deletedElements)
		{
			return InitializeCommand(selectionCallback, (IEnumerable<Element>)deletedElements);
		}

		/// <summary>
		/// Tells the command which elements to delete. The method uses
		/// interactive dialogs to specify the initial set of delted elements and
		/// to ask whether to delete unused elements from the model.
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
		/// <param name="selectionCallback">function that is called, when selected elements are specified in the dialog. Can be set to null.</param>
		/// <returns><code>true</code> when user pressed OK, <code>false</code> when user pressed Cancel in the</returns>
		public bool InitializeCommand(Action<IEnumerable<Element>> selectionCallback, IEnumerable<Element> deletedElements)
		{
			List<Element> _deleted = new List<Element>(deletedElements);
			ElementDependencies dependencies = ElementDependencies.FindPSMDependencies(deletedElements);
			DeleteDependentElementsDialog dialog = new DeleteDependentElementsDialog(dependencies);
			if (dependencies.Count > 0 && selectionCallback != null)
			{
				IEnumerable<Element> res = dependencies.Keys;
				foreach (var elements in dependencies.Values)
				{
					res = res.Union(elements);
				}
				selectionCallback(res);
			}
			if (dependencies.Count == 0 || ForceDelete || dialog.ShowDialog() == true)
			{
				if (dependencies.Count == 0 || ForceDelete) dialog.Close();
				// add all selected dependent elements, remove those that were not selected
				foreach (KeyValuePair<Element, bool> _flag in dependencies.Flags)
				{
					if (!ForceDelete && _flag.Value == false)
					{
						_deleted.Remove(_flag.Key);
					}
				}
				foreach (KeyValuePair<Element, bool> _flag in dependencies.Flags)
				{
					if (_flag.Value || ForceDelete)
					{
						foreach (Element element in dependencies[_flag.Key])
						{
							if (!_deleted.Contains(element))
								_deleted.Add(element);
						}
					}
				}

				DeletedElements = _deleted;
				return true;
			}
			else
			{
				return false; 
			}
		}

		internal override void CommandOperation()
		{
			IList<Element> _deleted;
			// try to order elements
			if (PSMTree.ReturnElementsInPSMOrder(DeletedElements, out _deleted, true))
			{
				safeOrdering = true;
				_deletionOrder = _deleted.Reverse();
			}
			else
			{
				// this will forbid undo
				safeOrdering = false;
				_deletionOrder = DeletedElements;
			}

			foreach (Element element in _deletionOrder)
			{
				if (DeletedElements.Contains(element))
				{
					DeletedElementsViewHelpers[element] = Diagram.DiagramElements[element];
					element.RemoveMeFromModel();
					Diagram.RemoveModelElement(element);
					
					
				}
				//AssociatedElements.Add(element);
			}
		}

		internal override OperationResult UndoOperation()
		{
			/* 
			 * forbid undo in those cases where elements couldn't be ordered 
			 * in an ordering that would allow to put them back in the diagram 
			 * safely
			 */
			if (CheckOrdering && !safeOrdering)
				return OperationResult.Failed;
			foreach (Element element in DeletedElements)
			{
                if (IsInDiagram(element))
                {
                    ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                    return OperationResult.Failed;
                }
			}
			foreach (Element element in _deletionOrder.Reverse())
			{
				if (DeletedElements.Contains(element))
				{
					element.PutMeBackToModel();
					Diagram.AddModelElement(element, DeletedElementsViewHelpers[element]);
				}
			}
			return OperationResult.OK;
		}
	}

	#region DeleteFromPSMDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="DeleteFromPSMDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class DeleteFromPSMDiagramCommandFactory : DiagramCommandFactory<DeleteFromPSMDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private DeleteFromPSMDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of DeleteFromPSMDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new DeleteFromPSMDiagramCommand(diagramController);
		}
	}

	#endregion
}