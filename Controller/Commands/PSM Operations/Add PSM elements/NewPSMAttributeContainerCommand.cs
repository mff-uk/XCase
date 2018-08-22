using System;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new PSM attribute container component and adds it
    /// to the end of the Components collection of a given PSM class.
    /// </summary>
    public class NewPSMAttributeContainerCommand : DiagramCommandBase
    {
        /// <summary>
        /// Gets or sets a reference to the PSM class, under which the container is created
        /// unless the PSMSuper property is specified.
        /// </summary>
        [MandatoryArgument]
        public PSMClass PSMClass { get; set; }

        /// <summary>
        /// Gets or sets a reference to the PSM Superordinate Component under which the container is created.
        /// </summary>
        public PSMSuperordinateComponent PSMSuper { get; set; }

		private readonly List<PSMAttribute> psmAttributes;

		/// <summary>
		/// Attributes moved to an attribute container
		/// </summary>
		public List<PSMAttribute> PSMAttributes
		{
			get { return psmAttributes; }
		}

        /// <summary>
        /// Gets or sets a reference to the element holder that can be used to store a reference
        /// to the new attribute container.
        /// </summary>
        public Helpers.ElementHolder<PSMAttributeContainer> CreatedContainer { get; set; }

		public PSMElementViewHelper ViewHelper { get; set; }

        /// <summary>
        /// Creates a new instance of the command.
        /// </summary>
		/// <param name="diagramController">Reference to the associated model controller</param>
		public NewPSMAttributeContainerCommand(DiagramController diagramController)
			: base(diagramController)
        {
        	Description = CommandDescription.ADD_PSM_ATTRIBUTE_CONTAINER;
        	this.psmAttributes = new List<PSMAttribute>();
			
			ViewHelper = new PSMElementViewHelper(diagramController.Diagram);
        }

    	public override bool CanExecute()
        {
			foreach (PSMAttribute attribute in psmAttributes)
			{
                if (attribute.Class != PSMClass)
                {
					ErrorDescription = CommandError.CMDERR_INCLUDED_ATTRIBUTES_INCONSISTENCE;
					return false;
				}
			}
        	return true;
        }

        internal override void CommandOperation()
        {
            if (CreatedContainer == null)
                CreatedContainer = new Helpers.ElementHolder<PSMAttributeContainer>();

            PSMAttributeContainer psmAttributeContainer;
            if (PSMSuper != null)
            {
                psmAttributeContainer = (PSMAttributeContainer)PSMSuper.AddComponent(PSMAttributeContainerFactory.Instance);
            }
            else
            {
                psmAttributeContainer = (PSMAttributeContainer)PSMClass.AddComponent(PSMAttributeContainerFactory.Instance);
            }

        	foreach (PSMAttribute attribute in psmAttributes)
        	{
        		//PSMAttribute containerAttribute = psmAttributeContainer.AddAttribute(attribute.RepresentedAttribute);
        		//containerAttribute.Alias = attribute.Alias;
        		PSMClass.PSMAttributes.Remove(attribute);
                psmAttributeContainer.PSMAttributes.Add(attribute);
        	}

			CreatedContainer.Element = psmAttributeContainer;

            Debug.Assert(CreatedContainer.HasValue);

			Diagram.AddModelElement(psmAttributeContainer, ViewHelper);
			AssociatedElements.Add(psmAttributeContainer);
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedContainer.HasValue);

			List<PSMAttribute> returnedAttributes = new List<PSMAttribute>();
            foreach (PSMAttribute attribute in psmAttributes)
            {
                //PSMAttribute createdAttribute = PSMClass.AddAttribute(attribute.RepresentedAttribute);
                PSMClass.PSMAttributes.Add(attribute);
				returnedAttributes.Add(attribute);
            }

			psmAttributes.Clear();
        	psmAttributes.AddRange(returnedAttributes);
			Diagram.RemoveModelElement(CreatedContainer.Element);
            CreatedContainer.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedContainer.Element.PutMeBackToModel();
            Diagram.AddModelElement(CreatedContainer.Element, ViewHelper);

            foreach (PSMAttribute attribute in psmAttributes)
            {
                //PSMAttribute containerAttribute = psmAttributeContainer.AddAttribute(attribute.RepresentedAttribute);
                //containerAttribute.Alias = attribute.Alias;
                PSMClass.PSMAttributes.Remove(attribute);
                //CreatedContainer.Element.PSMAttributes.Add(attribute);
            }

        }
    }

	#region NewPSMAttributeContainerCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewPSMAttributeContainerCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewPSMAttributeContainerCommandFactory : DiagramCommandFactory<NewPSMAttributeContainerCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewPSMAttributeContainerCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewPSMAttributeContainerCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewPSMAttributeContainerCommand(diagramController);
		}
	}

	#endregion
}
