using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new package and adds it to given <see cref="Package"/>.
    /// </summary>
    public class NewModelPackageCommand : ModelCommandBase
    {
        /// <summary>
        /// The Package in which the package is created
        /// </summary>
        [MandatoryArgument]
        public Package Package { get; set; }

        public string PackageName { get; set; }

        /// <summary>
        /// The elementHolder, in which the reference to the newly created package can be stored
        /// </summary>
        public Helpers.ElementHolder<Package> CreatedPackage { get; set; }

        public NewModelPackageCommand(ModelController modelController) : base(modelController) 
        {
            Description = CommandDescription.ADD_PACKAGE;
        }

        public override bool CanExecute()
        {
            if (CreatedPackage != null && CreatedPackage.Element != null)
            {
                if (!NameSuggestor<Package>.IsNameUnique(Package.NestedPackages, CreatedPackage.Element.Name, package => package.Name))
                {
                    ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, CreatedPackage.Element.Name);
                    return false;
                }
            }
            return true;
        }

        internal override void CommandOperation()
        {
            if (CreatedPackage == null)
                CreatedPackage = new ElementHolder<Package>();
            CreatedPackage.Element = Package.AddNestedPackage();
            if (!String.IsNullOrEmpty(PackageName))
                CreatedPackage.Element.Name = PackageName;
            Debug.Assert(CreatedPackage.HasValue);
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedPackage.HasValue);
            if (Package.NestedPackages.Contains(CreatedPackage.Element))
            {
                Package.NestedPackages.Remove(CreatedPackage.Element);
                return OperationResult.OK;
            }
            else
            {
                ErrorDescription = string.Format(CommandError.CMDERR_NOT_FOUND, CreatedPackage.Element);
                return OperationResult.Failed;
            }
        }
    }

    #region NewModelPackageCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="NewModelPackageCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class NewModelPackageCommandFactory : ModelCommandFactory<NewModelPackageCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private NewModelPackageCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of NewModelPackageCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new NewModelPackageCommand(modelController);
        }
    }

    #endregion
}
