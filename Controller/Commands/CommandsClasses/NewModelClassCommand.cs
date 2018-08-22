using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new class and adds it to current <see cref="Model"/>.
    /// </summary>
    public class NewModelClassCommand : ModelCommandBase
    {
		/// <summary>
		/// The Package in which the class is created
		/// </summary>
		public Package Package { get; set; }

    	public string ClassName { get; set; }

    	/// <summary>
		/// The elementHolder, in which the reference to the newly created class can be stored
		/// </summary>
        [CommandResult]
		public ElementHolder<PIMClass> CreatedClass { get; set; }

    	public NewModelClassCommand(ModelController modelController) : base(modelController) 
        {
            Description = CommandDescription.ADD_MODEL_CLASS;
        }

    	public override bool CanExecute()
        {
            // Error messages missing
            if (Package == null) return false;
            if (CreatedClass != null && CreatedClass.Element != null)
            {
                return NameSuggestor<PIMClass>.IsNameUnique(Package.Classes, CreatedClass.Element.Name, modelClass => modelClass.Name);
            }
            else return true;
        }

    	internal override void CommandOperation()
        {
			if (CreatedClass == null)
				CreatedClass = new ElementHolder<PIMClass>();
            CreatedClass.Element = Package.AddClass();
			if (!String.IsNullOrEmpty(ClassName))
				CreatedClass.Element.Name = ClassName;
            Debug.Assert(CreatedClass.HasValue);
			AssociatedElements.Add(CreatedClass.Element);
        }

    	internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedClass.HasValue);
            if (CreatedClass.Element.DerivedPSMClasses.Count > 0)
            {
                ErrorDescription = string.Format(CommandError.CMDERR_DELETE_PSM_DEPENDENT_CLASS, CreatedClass.Element);
                return OperationResult.Failed;
            }
            CreatedClass.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedClass.Element.PutMeBackToModel();
        }
    }

	#region NewModelClassCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelClassCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewModelClassCommandFactory : ModelCommandFactory<NewModelClassCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelClassCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelClassCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewModelClassCommand(modelController);
		}
	}

	#endregion
}