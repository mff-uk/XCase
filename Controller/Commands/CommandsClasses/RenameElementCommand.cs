using System;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Changes name of an element
	/// </summary>
	/// <typeparam name="ElementType">type of the renamed element</typeparam>
    /// <typeparam name="ControllerType">type of controller</typeparam>
    public abstract class RenameElementCommandGeneric<ElementType, ControllerType> : StackedCommandBase<ControllerType> 
		where ElementType: NamedElement where ControllerType : CommandControllerBase
	{
		/// <summary>
		/// Creates new instance of <see cref="RenameElementCommand{Type}" />. 
		/// </summary>
		/// <param name="controller">command controller</param>
        public RenameElementCommandGeneric(ControllerType controller)
    		: base(controller)
    	{
			Description = CommandDescription.RENAME_ELEMENT;
    	}

		/// <summary>
		/// Renamed element
		/// </summary>
    	public ElementType RenamedElement { get; set; }

		/// <summary>
		/// Collection of elements of which <see cref="RenamedElement"/> is a member. 
		/// <see cref="CanExecute"/> checks, whether <see cref="NewName"/> is unique
		/// in the collection. Can be left to null. 
		/// </summary>
		public IEnumerable<ElementType> ContainingCollection { get; set; }

		/// <summary>
		/// New name for <see cref="RenamedElement"/>
		/// </summary>
		[MandatoryArgument]
		public string NewName { get; set; }

    	private string oldName; 

        public override bool CanExecute()
        {
			if (ContainingCollection != null)
			{
				if (NewName != RenamedElement.Name && NewName != "" &&
					!NameSuggestor<ElementType>.IsNameUnique(ContainingCollection, NewName, element => element.Name))
				{
					ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, NewName);
					return false; 
				}
			}
        	return true; 
        }

        internal override void CommandOperation()
        {
        	oldName = RenamedElement.Name;
        	RenamedElement.Name = NewName;
			AssociatedElements.Add(RenamedElement);
        }

        internal override OperationResult UndoOperation()
        {
        	RenamedElement.Name = oldName;
        	return OperationResult.OK;
        }
    }

    public class RenameElementCommand<ElementType> : RenameElementCommandGeneric<ElementType, ModelController> where ElementType : NamedElement
    {
        public RenameElementCommand(ModelController controller) : base(controller)
        {
        }
    }

    public class RenameElementCommandDiagram<ElementType> : RenameElementCommandGeneric<ElementType, DiagramController> where ElementType : NamedElement
    {
        public RenameElementCommandDiagram(DiagramController controller)
            : base(controller)
        {
        }
    }

	#region RenameElementCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RenameElementCommand{ElementType}"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RenameElementCommandFactory<ElementType> : ModelCommandFactory<RenameElementCommandFactory<ElementType>>
		where ElementType : NamedElement
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private RenameElementCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of RenameElementCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new RenameElementCommand<ElementType>(modelController);
		}

        /// <summary>
        /// Creates new instance of RenameElementCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public IStackedCommand Create(DiagramController diagramController)
        {
            return new RenameElementCommandDiagram<ElementType>(diagramController);
        }


	}

	#endregion
}