using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Moves class to given package.
    /// </summary>
    public class MoveClassToPackageCommand : ModelCommandBase
    {
        /// <summary>
        /// Initialize command.
        /// </summary>
        /// <param name="controller">Model controller to be associated.</param>
        public MoveClassToPackageCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.MOVE_CLASS_TO_PACKAGE;
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
        /// Class to be moved between packages.
        /// </summary>
        [MandatoryArgument]
        public PIMClass MovedClass { get; set; }

        public override bool CanExecute()
        {
            if (OldPackage == NewPackage)
            {
                ErrorDescription = String.Format(CommandError.CMDERR_ADDING_PRESENT, MovedClass);
                return false;
            }
            if (!NameSuggestor<PIMClass>.IsNameUnique(NewPackage.Classes, MovedClass.Name, modelClass => modelClass.Name))
            {
                ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, MovedClass.Name);
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            OldPackage.Classes.Remove(MovedClass);
            NewPackage.Classes.Add(MovedClass);
        }

        internal override OperationResult UndoOperation()
        {
            if (OldPackage == null)
            {
                ErrorDescription = CommandError.CMDERR_NULL_ON_UNDO;
                return OperationResult.Failed;
            }
            NewPackage.Classes.Remove(MovedClass);
            OldPackage.Classes.Add(MovedClass);
            return OperationResult.OK;
        }
    }

    #region MoveClassToPackageCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="MoveClassToPackageCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class MoveClassToPackageCommandFactory : ModelCommandFactory<MoveClassToPackageCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private MoveClassToPackageCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of MoveClassToPackageCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new MoveClassToPackageCommand(modelController);
        }
    }

    #endregion
}