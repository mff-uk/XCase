using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Removes package from model.
    /// </summary>
    public class RemovePackageCommand : ModelCommandBase
    {
        /// <summary>
        /// Initialize command.
        /// </summary>
        /// <param name="controller">Model controller to be associated.</param>
        public RemovePackageCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_PACKAGE;
        }

        /// <summary>
        /// Package to be deleted.
        /// </summary>
        [MandatoryArgument]
        public Model.Package DeletedPackage { get; set; }

        /// <summary>
        /// Nesting package of the deleted package.
        /// </summary>
        private Model.Package nestingPackage;

        public override bool CanExecute()
        {
            if (DeletedPackage.NestingPackage == null)
            {
                ErrorDescription = CommandError.CMDERR_DELETE_MODEL;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            nestingPackage = DeletedPackage.NestingPackage;
            nestingPackage.NestedPackages.Remove(DeletedPackage);
        }

        internal override OperationResult UndoOperation()
        {
            if (nestingPackage == null)
            {
                ErrorDescription = CommandError.CMDERR_NULL_ON_UNDO;
                return OperationResult.Failed;
            }
            nestingPackage.NestedPackages.Add(DeletedPackage);
            return OperationResult.OK;
        }
    }

    #region RemovePackageCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RemovePackageCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RemovePackageCommandFactory : ModelCommandFactory<RemovePackageCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemovePackageCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RemovePackageCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new RemovePackageCommand(modelController);
        }
    }

    #endregion
}