using System.Linq;
using System.Collections.Generic;
using XCase.Model;
using XCase.Controller.Dialogs;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Deletes elements from a diagram. Use method 
	/// <see cref="InitializeCommand(XCase.Model.Element)"/> or 
	/// <see cref="InitializeCommand(System.Collections.Generic.IEnumerable{XCase.Model.Element})"/>
	/// to set deleted elements. The command uses dialog windows to specify 
	/// the set of deleted elements and to ask whether to delete unused elements
	/// from the model. 
	/// </summary>
    public class DeleteFromDiagramMacroCommand : MacroCommand<DiagramController>
    {
        public DeleteFromDiagramMacroCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_FROM_DIAGRAM_MACRO;
        }

        /// <summary>
        /// Tells the command which element to delete. The method uses 
        /// interactive dialogs to specify the initial set of delted element and 
        /// to ask whether to delete unused elements from the model. 
        /// </summary>
        /// <param name="deleted">Deleted element</param>
        public void InitializeCommand(Element deleted)
        {
            List<Element> List = new List<Element>();
            List.Add(deleted);
            InitializeCommand(List);
        }
        
        /// <summary>
		/// Tells the command which elements to delete. The method uses 
		/// interactive dialogs to specify the initial set of delted elements and 
		/// to ask whether to delete unused elements from the model. 
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
        public bool InitializeCommand(IEnumerable<Element> deletedElements)
        {
			List<Element> _deleted = new List<Element>(deletedElements);
			ElementDependencies dependencies = ElementDependencies.FindElementDependenciesInDiagram(_deleted, Controller.Diagram);
			DeleteDependentElementsDialog dialog = new DeleteDependentElementsDialog(dependencies);
			if (dependencies.Count == 0 || dialog.ShowDialog() == true)
			{
                if (dependencies.Count == 0) dialog.Close();
                // add all selected dependent elements, remove those that were not selected
				foreach (KeyValuePair<Element, bool> _flag in dependencies.Flags)
				{
					if (_flag.Value == false)
					{
						_deleted.Remove(_flag.Key);
					}
				}
				foreach (KeyValuePair<Element, bool> _flag in dependencies.Flags)
				{
					if (_flag.Value)
					{
						foreach (Element element in dependencies[_flag.Key])
						{
							if (!_deleted.Contains(element))
								_deleted.Add(element);
						}	
					}
				}
				
                DeleteFromDiagramCommand command = (DeleteFromDiagramCommand)DeleteFromDiagramCommandFactory.Factory().Create(Controller);
				command.DeletedElements = _deleted;
                Commands.Add(command);

				//
				// Elements that are not used in any diagrams are offered to be
				// deleted from model. 
				//
				List<Element> deleteFromModelCandidates = new List<Element>();
				ElementDependencies modelDependencies = ElementDependencies.FindElementDependenciesInModel(deleteFromModelCandidates);
				foreach (Element element in _deleted)
				{
					if (!Controller.ModelController.IsElementUsedInDiagrams(element, Controller.Diagram)
						&& (!modelDependencies.ContainsKey(element) || modelDependencies[element].All(dependentElement => !Controller.ModelController.IsElementUsedInDiagrams(dependentElement))))
					{
						deleteFromModelCandidates.Add(element);
						if (!modelDependencies.ContainsKey(element))
							modelDependencies[element] = new List<Element>();
					}
				}
				
				if (deleteFromModelCandidates.Count > 0)
				{
					
					DeleteDependentElementsDialog modelDialog = new DeleteDependentElementsDialog(modelDependencies);
					modelDialog.tbShort.Content = "Some elements remained unused in model";
					modelDialog.tbLong.Text =
						"Some elements were removed from diagram and it was their only usage. Select elements you wish to remove also from model from the following list.";
					modelDialog.Title = "Delete from model";
                    modelDialog.bCancel.Content = "Cancel";
                    if (modelDialog.ShowDialog() == true) //User clicked on Delete
                    {
                        // add all selected dependent elements, remove those that were not selected
                        foreach (KeyValuePair<Element, bool> _flag in modelDependencies.Flags)
                        {
                            if (_flag.Value == false)
                            {
                                deleteFromModelCandidates.Remove(_flag.Key);
                            }
                        }
                        if (deleteFromModelCandidates.Count > 0)
                        {
                            DeleteFromModelCommand deleteFromModelCommand = (DeleteFromModelCommand)DeleteFromModelCommandFactory.Factory().Create(Controller.ModelController);
                            deleteFromModelCommand.DeletedElements = deleteFromModelCandidates;
                            deleteFromModelCommand.CallingDiagram = Controller.Diagram;
                            Commands.Add(deleteFromModelCommand);
                        }
						// both dialog were closed with OK
                    	return true; 
                    }
                    else 
                    {
						// user clicked Cancel in the second dialog
                    	return false; 
                    }
				}
				else
				{
					// first dialog closed OK, second dialog was not shown
					return true; 
				}
			}
			else
			{
				// first dialog closed OK, but second closed Cancel
				return false; 
			}
		}
    }

    #region DeleteFromDiagramMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="DeleteFromDiagramMacroCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class DeleteFromDiagramMacroCommandFactory : DiagramCommandFactory<DeleteFromDiagramMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private DeleteFromDiagramMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of DeleteFromDiagramMacroCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new DeleteFromDiagramMacroCommand(diagramController);
        }
    }

    #endregion
}
