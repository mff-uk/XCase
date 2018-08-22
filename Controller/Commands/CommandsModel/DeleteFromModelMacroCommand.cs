using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.Controller.Dialogs;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// This command is used for deleting from model. It also discovers dependencies.
    /// </summary>
    public class DeleteFromModelMacroCommand : MacroCommand<ModelController>
    {
        DiagramController ActiveDiagramController = null;
        List<Element> Deleted = null;
        DeleteFromModelCommand deleteFromModelCommand;
        DeleteFromDiagramCommand deleteFromDiagramCommand;
        
        public DeleteFromModelMacroCommand(ModelController controller)
			: base(controller)
		{
            Description = CommandDescription.REMOVE_FROM_MODEL_MACRO;
		}

        public bool Set(Element deleted, DiagramController activeDiagramController)
        {
            List<Element> List = new List<Element>();
            List.Add(deleted);
            return Set(List, activeDiagramController);
        }
        
        public bool Set(IEnumerable<Element> deleted, DiagramController activeDiagramController)
        {
            ActiveDiagramController = activeDiagramController;
            Deleted = new List<Element>(deleted.Count());
            foreach (Element deletable in deleted)
            {
                Deleted.Add(deletable);
            }

			ElementDependencies dependencies = ElementDependencies.FindElementDependenciesInModel(Deleted);
			DeleteDependentElementsDialog dialog = new DeleteDependentElementsDialog(dependencies);
			dialog.DeleteLevel = DeleteDependentElementsDialog.EDeleteLevel.Model;

            if (dependencies.Count == 0 || dialog.ShowDialog() == true)
            {
                if (dependencies.Count == 0) dialog.Close();
                // add all selected dependent elements, remove those that were not selected
                foreach (KeyValuePair<Element, bool> _flag in dependencies.Flags)
                {
                    if (_flag.Value == false)
                    {
                        Deleted.Remove(_flag.Key);
                    }
                    else
                    {
                        foreach (Element element in dependencies[_flag.Key])
                        {
                            if (!Deleted.Contains(element))
                                Deleted.Add(element);
                        }
                    }
                }

                // test whether elements are not used in other diagrams
                ElementDiagramDependencies dependentDiagrams;
                if (ActiveDiagramController != null)
                    dependentDiagrams = ElementDiagramDependencies.FindElementDiagramDependencies(Controller.Project, Deleted, ActiveDiagramController.Diagram);
                else
                    dependentDiagrams = ElementDiagramDependencies.FindElementDiagramDependencies(Controller.Project, Deleted, null);

                if (dependentDiagrams.Count > 0
                    && !(dependentDiagrams.Count == 1
                            && ActiveDiagramController != null
                            && dependentDiagrams.Single().Value.Count == 1
                            && dependentDiagrams.Single().Value.Single() == ActiveDiagramController.Diagram
                            )
                    )
                {
                    ReferencingDiagramsDialog referencingDiagramsDialog = new ReferencingDiagramsDialog(dependentDiagrams);
                    referencingDiagramsDialog.ShowDialog();
                    if (referencingDiagramsDialog.cbOpen.IsChecked == true)
                    {
                        foreach (KeyValuePair<Element, List<Diagram>> pair in dependentDiagrams)
                        {
                            foreach (Diagram diagram in pair.Value)
                            {
                                ActivateDiagramCommand c = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
                                c.Set(diagram);
                                c.Execute();
                            }
                        }
                    }
                }
                else
                {
                    deleteFromModelCommand =
                        (DeleteFromModelCommand)DeleteFromModelCommandFactory.Factory().Create(Controller);
                    deleteFromModelCommand.DeletedElements = Deleted;
                    if (ActiveDiagramController != null)
                        deleteFromModelCommand.CallingDiagram = ActiveDiagramController.Diagram;
                    else deleteFromModelCommand.CallingDiagram = null;

                    if (ActiveDiagramController != null && ActiveDiagramController.Diagram != null)
                    {
                        deleteFromDiagramCommand =
                            (DeleteFromDiagramCommand)DeleteFromDiagramCommandFactory.Factory().Create(ActiveDiagramController);
                        deleteFromDiagramCommand.DeletedElements = Deleted.Where(e => ActiveDiagramController.Diagram.IsElementPresent(e)).ToList();
                        Commands.Add(deleteFromDiagramCommand);
                    }

                    Commands.Add(deleteFromModelCommand);
                    return true;
                }
            }
            return false;
		}
    }

    #region DeleteFromModelMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="DeleteFromModelMacroCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class DeleteFromModelMacroCommandFactory : ModelCommandFactory<DeleteFromModelMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private DeleteFromModelMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="DeleteFromModelMacroCommand"/>
        /// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new DeleteFromModelMacroCommand(modelController);
        }
    }

    #endregion

}
