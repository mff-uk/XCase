using XCase.Model;
using System;
using System.Collections.Generic;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Removes package and its nested classes.
    /// </summary>
    public class RemovePackageMacroCommand : MacroCommand<ModelController>
    {
        /// <summary>
        /// Creates new instance of <see cref="RemovePackageMacroCommand" />. 
        /// </summary>
        /// <param name="controller">Command controller</param>
        public RemovePackageMacroCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_PACKAGE_MACRO;
        }

        /// <summary>
        /// Deleted package
        /// </summary>
        [MandatoryArgument]
        private Package DeletedPackage { get; set; }
        private Diagram ActiveDiagram { get; set; }
        private bool error = false;
        
        public override bool CanExecute()
        {
            foreach (CommandBase command in Commands)
            {
                if (command is RemovePackageCommand && !command.CanExecute()) return false;
            }
            return true;
        }

        public void Set(Package deletedPackage, Diagram activeDiagram)
        {
            DeletedPackage = deletedPackage;
            ActiveDiagram = activeDiagram;
            DeletePackage(DeletedPackage);
            if (error) Commands.Clear();
        }

        private void DeletePackage(Package deletedPackage)
        {
            if (deletedPackage.NestedPackages.Count > 0)
            {
                foreach (Package package in deletedPackage.NestedPackages)
                {
                    DeletePackage(package);
                }
            }

            if (deletedPackage.Classes.Count > 0)
            {
                List<Element> classes = new List<Element>();
                DeleteFromModelMacroCommand c = DeleteFromModelMacroCommandFactory.Factory().Create(Controller) as DeleteFromModelMacroCommand;
                foreach (Class aClass in deletedPackage.Classes)
                {
                    classes.Add(aClass);
                }
                if (!c.Set(classes, new DiagramController(ActiveDiagram, Controller))) error = true;
                Commands.Add(c);
            }

            RemovePackageCommand d = RemovePackageCommandFactory.Factory().Create(Controller) as RemovePackageCommand;
            d.DeletedPackage = deletedPackage;
            Commands.Add(d);
        }
    }

    #region RemovePackageMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RemoveAttributeMacroCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RemovePackageMacroCommandFactory : ModelCommandFactory<RemovePackageMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemovePackageMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RemovePackageMacroCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new RemovePackageMacroCommand(modelController);
        }
    }

    #endregion
}
