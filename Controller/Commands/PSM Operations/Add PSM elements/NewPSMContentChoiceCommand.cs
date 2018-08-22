using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using System.Linq;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new PSM content choice component and adds it
    /// to the end of the Components collection of a given PSM component.
    /// </summary>
    public class NewPSMContentChoiceCommand : DiagramCommandBase
    {
        private ViewHelper ViewHelper;
        
        /// <summary>
        /// Gets or sets a reference to the PSM component under which the choice will be created.
        /// </summary>
        [MandatoryArgument]
        public PSMSuperordinateComponent Parent { get; set; }

		private readonly List<PSMSubordinateComponent> containedComponents;

		public List<PSMSubordinateComponent> ContainedComponents
		{
			get { return containedComponents; }
		}

		public string Name { get; set; }

        /// <summary>
        /// Gets or sets a reference to the element holder that can be used to store
        /// a reference to the new content choice.
        /// </summary>
        public Helpers.ElementHolder<PSMContentChoice> CreatedChoice { get; set; }

        /// <summary>
        /// Creates a new instance of the command.
        /// </summary>
		/// <param name="diagramController">Reference to the associated model controller</param>
        public NewPSMContentChoiceCommand(DiagramController diagramController) : base(diagramController) 
        {
            Description = CommandDescription.ADD_PSM_CONTENT_CHOICE;
			containedComponents = new List<PSMSubordinateComponent>();
        }

		/// <summary>
		/// Checks whether the command can be executed.
		/// In this case if the Parent component is set.
		/// </summary>
		/// <returns>True if the command can be executed, false otherwise</returns>
		public override bool CanExecute()
        {
            return Parent != null;
        }

        private readonly Dictionary<PSMSubordinateComponent, int> oldIndexes = new Dictionary<PSMSubordinateComponent, int>();

        internal override void CommandOperation()
        {
            if (CreatedChoice == null)
                CreatedChoice = new Helpers.ElementHolder<PSMContentChoice>();

            PSMSubordinateComponent first = Parent.Components.FirstOrDefault(component => containedComponents.Contains(component));
            PSMContentChoice psmChoice;
            if (first == null)
            {
                psmChoice =
                    (PSMContentChoice)Parent.AddComponent(PSMContentChoiceFactory.Instance);
            }
            else
            {
                psmChoice =
                    (PSMContentChoice)Parent.AddComponent(PSMContentChoiceFactory.Instance, first.ComponentIndex());
            }
            
			psmChoice.Name = Name;

            CreatedChoice.Element = psmChoice;

			AssociatedElements.Add(psmChoice);

			foreach (PSMSubordinateComponent containedComponent in containedComponents)
			{
                oldIndexes[containedComponent] = containedComponent.ComponentIndex();
				Parent.Components.Remove(containedComponent);
				psmChoice.Components.Add(containedComponent);
			}

            Debug.Assert(CreatedChoice.HasValue);
			Diagram.AddModelElement(psmChoice, ViewHelper = new PSMElementViewHelper(Diagram));
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedChoice.HasValue);
			foreach (PSMSubordinateComponent containedComponent in containedComponents)
			{
				CreatedChoice.Element.Components.Remove(containedComponent);
				Parent.Components.Insert(oldIndexes[containedComponent], containedComponent);
			}
			Diagram.RemoveModelElement(CreatedChoice.Element);
            CreatedChoice.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedChoice.Element.PutMeBackToModel();
            Diagram.AddModelElement(CreatedChoice.Element, ViewHelper);

            foreach (PSMSubordinateComponent containedComponent in containedComponents)
            {
                Parent.Components.Remove(containedComponent);
                CreatedChoice.Element.Components.Add(containedComponent);
            }
        }
    }

	#region NewPSMContentChoiceCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewPSMContentChoiceCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewPSMContentChoiceCommandFactory : DiagramCommandFactory<NewPSMContentChoiceCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewPSMContentChoiceCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewPSMContentChoiceCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewPSMContentChoiceCommand(diagramController);
		}
	}

	#endregion
}
