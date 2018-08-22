using System;
using XCase.Model;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new PSM Class Union and adds it to the diagram.
    /// </summary>
    public class NewPSMClassUnionCommand : DiagramCommandBase
    {
        /// <summary>
        /// Gets or sets the PSM component under which the class union will be created.
        /// </summary>
        [MandatoryArgument]
        public PSMSuperordinateComponent Parent { get; set; }

        private ViewHelper ViewHelper;
        
        [CommandResult]
        public Helpers.ElementHolder<PSMClassUnion> CreatedUnion { get; set; }

        public NewPSMClassUnionCommand(DiagramController controller) : base(controller)
        {
            
        }

        public override bool CanExecute()
        {
            return Parent != null;
        }

        internal override void CommandOperation()
        {
            if (CreatedUnion == null)
                CreatedUnion = new Helpers.ElementHolder<PSMClassUnion>();

            PSMClassUnion union = Parent.CreateClassUnion();
			
			if (Parent is PSMClassUnion)
        	{
        		
        	}

            CreatedUnion.Element = union;

            Diagram.AddModelElement(union, ViewHelper = new PSMElementViewHelper(Diagram));
			AssociatedElements.Add(union);
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedUnion != null && CreatedUnion.Element != null);
			Diagram.RemoveModelElement(CreatedUnion.Element);
            CreatedUnion.Element.RemoveMeFromModel();
            
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedUnion.Element.PutMeBackToModel();
            Diagram.AddModelElement(CreatedUnion.Element, ViewHelper);
        }
    }

	#region NewPSMClassUnionCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewPSMClassUnionCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewPSMClassUnionCommandFactory : DiagramCommandFactory<NewPSMClassUnionCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewPSMClassUnionCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewPSMClassUnionCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewPSMClassUnionCommand(diagramController);
		}
	}

	#endregion
}
