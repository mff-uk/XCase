using System.Linq;
using System.Collections.Generic;
using XCase.Model;
using XCase.Controller.Dialogs;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Deletes elements from a diagram. Use method <see cref="InitializeCommand"/>
	/// to set deleted elements. The command uses dialog windows to specify 
	/// the set of deleted elements and to ask whether to delete unused elements
	/// from the model. 
	/// </summary>
    public class DeleteFromPSMDiagramMacroCommand : MacroCommand<DiagramController>
    {
        public DeleteFromPSMDiagramMacroCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_FROM_DIAGRAM_MACRO;
        }

        /// <summary>
        /// Tells the command which element to delete. The method uses 
        /// interactive dialogs to specify the initial set of delted elements and 
        /// to ask whether to delete unused elements from the model. 
        /// </summary>
        /// <param name="deletedElement">Deleted element</param>
        public void InitializeCommand(Element deletedElement)
        {
            List<Element> List = new List<Element>();
            List.Add(deletedElement);
            InitializeCommand(List);
        }
        
        /// <summary>
		/// Tells the command which elements to delete. The method uses 
		/// interactive dialogs to specify the initial set of delted elements and 
		/// to ask whether to delete unused elements from the model. 
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
        public void InitializeCommand(IEnumerable<Element> deletedElements)
        {
			List<Element> _deleted = new List<Element>(deletedElements);
			ElementDependencies dependencies = ElementDependencies.FindPSMDependencies(deletedElements);
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

				DeleteFromPSMDiagramCommand command = (DeleteFromPSMDiagramCommand)DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller);
				command.DeletedElements = _deleted;
                Commands.Add(command);
			}
		}
    }

    #region DeleteFromPSMDiagramMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="DeleteFromPSMDiagramMacroCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class DeleteFromPSMDiagramMacroCommandFactory : DiagramCommandFactory<DeleteFromPSMDiagramMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private DeleteFromPSMDiagramMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of DeleteFromPSMDiagramMacroCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new DeleteFromPSMDiagramMacroCommand(diagramController);
        }
    }

    #endregion
}
