using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes attribute from a PSM class or attribute container
	/// </summary>
	public class RemovePSMAttributeCommand: ModelCommandBase
	{
		public RemovePSMAttributeCommand(ModelController controller)
			: base(controller)
		{
            Description = CommandDescription.REMOVE_PSM_ATTRIBUTE;
		}

        [MandatoryArgument]
		public PSMAttribute DeletedAttribute { get; set; }
	    
        /// <summary>
        /// Fill this field if the attribute is removed from an attribute container. 
        /// </summary>
        public PSMAttributeContainer AttributeContainer { get; set; }
	    
        private IHasPSMAttributes removedFrom;

	    public override bool CanExecute()
		{
			return true; 
		}

		internal override void CommandOperation()
		{
			if (AttributeContainer == null)
			{
				removedFrom = DeletedAttribute.Class;
			}
			else
			{
				removedFrom = AttributeContainer;
			}
            DeletedAttribute.RemoveMeFromModel();
			associatedElements.Add(removedFrom);
		}

		internal override OperationResult UndoOperation()
		{
			if (removedFrom == null)
			{
                ErrorDescription = CommandError.CMDERR_NULL_ON_UNDO;
                return OperationResult.Failed;
			}
            DeletedAttribute.PutMeBackToModel();
			return OperationResult.OK;
		}
	}

	#region RemoveAttributeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RemovePSMAttributeCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
    public class RemovePSMAttributeCommandFactory : ModelCommandFactory<RemovePSMAttributeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private RemovePSMAttributeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of <see cref="RemoveAttributeCommand"/>
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new RemovePSMAttributeCommand(modelController);
		}
	}

	#endregion

    /// <summary>
    /// Removes attribute from a PSM class or attribute container
    /// </summary>
    public class RemovePSMAttributeCommand2 : DiagramCommandBase
    {
        public RemovePSMAttributeCommand2(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_PSM_ATTRIBUTE;
        }
        
        [MandatoryArgument]
        public PSMAttribute DeletedAttribute { get; set; }

        /// <summary>
        /// Fill this field if the attribute is removed from an attribute container. 
        /// </summary>
        public PSMAttributeContainer AttributeContainer { get; set; }
        
        private IHasPSMAttributes modelClass;

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            if (AttributeContainer == null)
            {
                modelClass = DeletedAttribute.Class;
            }
            else
            {
                modelClass = AttributeContainer;
            }
            DeletedAttribute.RemoveMeFromModel();
            associatedElements.Add(modelClass);
        }

        internal override OperationResult UndoOperation()
        {
            if (modelClass == null)
            {
                ErrorDescription = CommandError.CMDERR_NULL_ON_UNDO;
                return OperationResult.Failed;
            }
            DeletedAttribute.PutMeBackToModel();
            return OperationResult.OK;
        }
    }

    #region RemoveAttributeCommand2Factory

    /// <summary>
    /// Factory that creates instances of <see cref="RemovePSMAttributeCommand2"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands receive reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class RemovePSMAttributeCommand2Factory : DiagramCommandFactory<RemovePSMAttributeCommand2Factory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemovePSMAttributeCommand2Factory()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="RemoveAttributeCommand"/>
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new RemovePSMAttributeCommand2(diagramController);
        }
    }

    #endregion
}
