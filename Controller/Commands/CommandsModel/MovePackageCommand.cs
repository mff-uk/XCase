using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Moves package into different nesting package.
    /// </summary>
    public class MovePackageCommand : ModelCommandBase
    {
        /// <summary>
        /// Initialize command.
        /// </summary>
        /// <param name="controller">Model controller to be associated.</param>
        public MovePackageCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.MOVE_PACKAGE;
        }

        /// <summary>
        /// Original nesting package.
        /// </summary>
        [MandatoryArgument]
        public Package OldPackage { get; set; }

        /// <summary>
        /// New nesting package.
        /// </summary>
        [MandatoryArgument]
        public Package NewPackage { get; set; }

        /// <summary>
        /// Package to be moved between packages.
        /// </summary>
        [MandatoryArgument]
        public Package MovedPackage { get; set; }

        public override bool CanExecute()
        {
            if (OldPackage == NewPackage)
            {
                ErrorDescription = String.Format(CommandError.CMDERR_ADDING_PRESENT, MovedPackage);
                return false;
            }
            if (MovedPackage == NewPackage) 
                return false;
            if (!NameSuggestor<Package>.IsNameUnique(NewPackage.NestedPackages, MovedPackage.Name, modelPackage => modelPackage.Name))
            {
                ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, MovedPackage.Name);
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            OldPackage.NestedPackages.Remove(MovedPackage);
            NewPackage.NestedPackages.Add(MovedPackage);
        }

        internal override OperationResult UndoOperation()
        {
            if (OldPackage == null)
            {
                ErrorDescription = CommandError.CMDERR_NULL_ON_UNDO;
                return OperationResult.Failed;
            }
            NewPackage.NestedPackages.Remove(MovedPackage);
            OldPackage.NestedPackages.Add(MovedPackage);
            return OperationResult.OK;
        }
    }

    #region MovePackageCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="MoveClassToPackageCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class MovePackageCommandFactory : ModelCommandFactory<MovePackageCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private MovePackageCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of MoveClassToPackageCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new MovePackageCommand(modelController);
        }
    }

    #endregion
}