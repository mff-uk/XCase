using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds association class that associates <see cref="AssociatedClasses"/> into the model. 
	/// </summary>
	public class NewModelAssociationClassCommand: ModelCommandBase
	{
        /// <summary>
        /// Index from which is removed in Undo
        /// </summary>
        private int Index;
        
        /// <summary>
		/// New association class
		/// </summary>
		public ElementHolder<AssociationClass> CreatedAssociationClass { get; set; }

		/// <summary>
		/// Associated classes (one or more)
		/// </summary>
		public IEnumerable<Class> AssociatedClasses { get; set; }

		/// <summary>
		/// Name of the created AssociationClass
		/// </summary>
		public string Name { get; set; }

		public NewModelAssociationClassCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.ADD_MODEL_ASSOCIATION_CLASS;
		}

		public override bool CanExecute()
		{
			if (AssociatedClasses == null || AssociatedClasses.Count() == 0)
			{
				ErrorDescription = CommandError.CMDERR_MISSING_ASSOCIATED_CLASSES;
				return false;
			}
			return true;	
		}

		internal override void CommandOperation()
		{
			if (CreatedAssociationClass == null)
				CreatedAssociationClass = new ElementHolder<AssociationClass>();
			CreatedAssociationClass.Element = Controller.Model.Schema.CreateAssociationClass(AssociatedClasses);
			if (!String.IsNullOrEmpty(Name))
				CreatedAssociationClass.Element.Name = Name;
			Debug.Assert(CreatedAssociationClass.HasValue);
			AssociatedElements.Add(CreatedAssociationClass.Element);
		}

		internal override OperationResult UndoOperation()
		{
			if (Controller.IsElementUsedInDiagrams(CreatedAssociationClass.Element))
			{
                ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                return OperationResult.Failed;
			}
			if (!Model.Associations.Contains(CreatedAssociationClass.Element))
			{
                ErrorDescription = string.Format(CommandError.CMDERR_NOT_FOUND, CreatedAssociationClass.Element);
                return OperationResult.Failed;
			}
            Index = Model.Associations.IndexOf(CreatedAssociationClass.Element);
            Model.Associations.Remove(CreatedAssociationClass.Element);
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            Model.Associations.Insert(Index, CreatedAssociationClass.Element);
        }
	}

	#region NewModelAssociationClassCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelAssociationClassCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewModelAssociationClassCommandFactory : ModelCommandFactory<NewModelAssociationClassCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelAssociationClassCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelAssociationClassCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewModelAssociationClassCommand(modelController);
		}
	}

	#endregion
}