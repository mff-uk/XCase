using System.Collections.Generic;
using System.Linq;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Moves attributes from Attribute container to class.
    /// </summary>
    public class MoveAttributesFromClassToAttributeContainerCommand : DiagramCommandBase
    {
        /// <summary>
        /// Creates new instance of <see cref="MoveAttributesFromClassToAttributeContainerCommand">MoveAttributesFromClassToAttributeContainerCommand</see>. 
        /// </summary>
        /// <param name="diagramController">command controller</param>
        public MoveAttributesFromClassToAttributeContainerCommand(DiagramController diagramController)
            : base(diagramController)
        {
            Description = CommandDescription.PSM_ATTRIBUTE_CONTAINER;
            attributes = new List<PSMAttribute>();
        }

        private List<PSMAttribute> attributes;

        public List<PSMAttribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        [MandatoryArgument]
        public PSMAttributeContainer AttributeContainer { get; set; }

        public override bool CanExecute()
        {
            PSMClass c = null;
            if (attributes.Count > 0)
            {
                c = attributes.First().Class;
            }
            if (attributes.Any(attribute => attribute.Class != c))
                return false;
            return true;
        }

        private PSMClass parentClass;
        private List<PSMAttribute> movedAttributes;

        internal override void CommandOperation()
        {
            if (attributes.Count > 0)
            {
                parentClass = attributes.First().Class;
                movedAttributes = new List<PSMAttribute>();
                foreach (PSMAttribute attribute in attributes)
                {
                    parentClass.PSMAttributes.Remove(attribute);
                    AttributeContainer.PSMAttributes.Add(attribute);
                    movedAttributes.Add(attribute);
                }
                AssociatedElements.Add(AttributeContainer);
            }
        }

        internal override OperationResult UndoOperation()
        {
            if (parentClass != null)
            {
                foreach (PSMAttribute attribute in movedAttributes)
                {
                    AttributeContainer.PSMAttributes.Remove(attribute);
                    parentClass.PSMAttributes.Add(attribute);
                }
				
            }
            return OperationResult.OK;
        }
    }


	#region MoveAttributesFromClassToAttributeContainerCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveAttributesFromClassToAttributeContainerCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveAttributesFromClassToAttributeContainerCommandFactory : DiagramCommandFactory<MoveAttributesFromClassToAttributeContainerCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveAttributesFromClassToAttributeContainerCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of <see cref="MoveAttributesFromClassToAttributeContainerCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveAttributesFromClassToAttributeContainerCommand(diagramController);
		}
	}

	#endregion
}