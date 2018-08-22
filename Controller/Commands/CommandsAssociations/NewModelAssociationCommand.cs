using System;
using System.Linq;
using System.Collections.Generic;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds association between <see cref="AssociatedClasses"/> into the model. 
	/// </summary>
	public class NewModelAssociationCommand: ModelCommandBase
	{
        /// <summary>
        /// Index from which is removed in Undo
        /// </summary>
        private int Index;
        
        /// <summary>
		/// New added model association
		/// </summary>
		[CommandResult]
		public ElementHolder<Association> CreatedAssociation { get; set; }

		/// <summary>
		/// Associated classes (one or more)
		/// </summary>
		[MandatoryArgument]
		public IEnumerable<Class> AssociatedClasses { get; set; }

		/// <summary>
		/// Name of the created association
		/// </summary>
		public string Name { get; set; }

		public NewModelAssociationCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.ADD_MODEL_ASSOCIATION;
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
			if (CreatedAssociation == null)
				CreatedAssociation = new ElementHolder<Association>();
			CreatedAssociation.Element = Controller.Model.Schema.AssociateClasses(AssociatedClasses);
			if (!String.IsNullOrEmpty(Name))
				CreatedAssociation.Element.Name = Name;
			Debug.Assert(CreatedAssociation.HasValue);
			AssociatedElements.Add(CreatedAssociation.Element);
		}

		internal override OperationResult UndoOperation()
		{
			if (!Model.Associations.Contains(CreatedAssociation.Element))
			{
                ErrorDescription = string.Format(CommandError.CMDERR_NOT_FOUND, CreatedAssociation.Element);
                return OperationResult.Failed;	
			}
			if (Controller.IsElementUsedInDiagrams(CreatedAssociation.Element))
			{
                ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                return OperationResult.Failed;
			}
            Index = Model.Associations.IndexOf(CreatedAssociation.Element);
            Model.Associations.Remove(CreatedAssociation.Element);
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            Model.Associations.Insert(Index, CreatedAssociation.Element);
        }
	}

	#region NewModelAssociationCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelAssociationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewModelAssociationCommandFactory : ModelCommandFactory<NewModelAssociationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelAssociationCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelAssociationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewModelAssociationCommand(modelController);
		}
	}

	#endregion
}