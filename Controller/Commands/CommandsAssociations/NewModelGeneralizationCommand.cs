using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds generalization relation between two classes
	/// </summary>
	public class NewModelGeneralizationCommand : ModelCommandBase
	{
        /// <summary>
        /// Index from which is removed in Undo
        /// </summary>
        private int Index;

        /// <summary>
		/// General (parent) class
		/// </summary>
		[MandatoryArgument]
		public Class General { get; set; }

		/// <summary>
		/// Specific (child) class
		/// </summary>
		[MandatoryArgument]
		public Class Specific { get; set; }

		/// <summary>
		/// New created generalization
		/// </summary>
		[CommandResult]
		public ElementHolder<Generalization> CreatedGeneralization { get; set; }

		public NewModelGeneralizationCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.ADD_MODEL_GENERALIZATION;
		}

		public override bool CanExecute()
		{
            if (General.MeAndAncestors.Contains(Specific))
            {
                ErrorDescription = CommandError.CMDERR_CYCLIC_INHERITANCE;
                return false;
            }
            else return true;
		}

		internal override void CommandOperation()
		{
			if (CreatedGeneralization == null)
				CreatedGeneralization = new ElementHolder<Generalization>();
			CreatedGeneralization.Element = Model.Schema.SetGeneralization(General, Specific);
			Debug.Assert(CreatedGeneralization.HasValue);
			AssociatedElements.Add(CreatedGeneralization.Element);
		}

		internal override OperationResult UndoOperation()
		{
			if (!Model.Generalizations.Contains(CreatedGeneralization.Element))
			{
                ErrorDescription = string.Format(CommandError.CMDERR_NOT_FOUND, CreatedGeneralization.Element);
                return OperationResult.Failed;
			}
			if (Controller.IsElementUsedInDiagrams(CreatedGeneralization.Element))
			{
                ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                return OperationResult.Failed;
			}
            Index = Model.Generalizations.IndexOf(CreatedGeneralization.Element);
            Model.Generalizations.Remove(CreatedGeneralization.Element);
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            Model.Generalizations.Insert(Index, CreatedGeneralization.Element);
        }
	}

	#region NewModelGeneralizationCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelGeneralizationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewModelGeneralizationCommandFactory : ModelCommandFactory<NewModelGeneralizationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelGeneralizationCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelGeneralizationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewModelGeneralizationCommand(modelController);
		}
	}

	#endregion
}