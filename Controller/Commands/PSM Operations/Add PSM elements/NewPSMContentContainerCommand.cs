using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;
using System.Linq;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Creates a new PSM Content Container under the given PSM superordinate
    /// component.
    /// </summary>
    public class NewPSMContentContainerCommand : DiagramCommandBase
    {
        private ViewHelper ViewHelper;
        
        /// <summary>
        /// Gets or sets a reference to the PSM component under which
        /// the Content Container will be created. When null, it is added as a new root
        /// </summary>
        //[MandatoryArgument] - no longer. if null -> make a root
		public PSMSuperordinateComponent Parent { get; set; }

    	private readonly List<PSMSubordinateComponent> containedComponents;
    	
		public List<PSMSubordinateComponent> ContainedComponents
    	{
    		get { return containedComponents; }
    	}

		public string Name { get; set; }

    	/// <summary>
        /// An element holder, where a reference to the created content container can be stored.
        /// </summary>
        [CommandResult]
        public Helpers.ElementHolder<PSMContentContainer> CreatedContainer { get; set; }

		/// <summary>
		/// Creates a new instance of the command.
		/// </summary>
		/// <param name="diagramController">Reference to the associated model controller</param>
		public NewPSMContentContainerCommand(DiagramController diagramController)
			: base(diagramController) 
        {
            Description = CommandDescription.ADD_PSM_CONTENT_CONTAINER;
			containedComponents = new List<PSMSubordinateComponent>();
        }
        
        /// <summary>
        /// Checks whether the command can be executed.
        /// In this case if the Parent component is set.
        /// </summary>
        /// <returns>True if the command can be executed, false otherwise</returns>
        public override bool CanExecute()
        {
            //return Parent != null;
            return true;
        }

        private readonly Dictionary<PSMSubordinateComponent, int> oldIndexes = new Dictionary<PSMSubordinateComponent, int>();

        internal override void CommandOperation()
        {
            if (CreatedContainer == null)
                CreatedContainer = new Helpers.ElementHolder<PSMContentContainer>();

            PSMSubordinateComponent first = Parent.Components.FirstOrDefault(component => containedComponents.Contains(component));
            PSMContentContainer psmContainer;
            if (Parent != null)
            {
                if (first == null)
                {
                    psmContainer = (PSMContentContainer)Parent.AddComponent(PSMContentContainerFactory.Instance);
                }
                else
                {
                    psmContainer = (PSMContentContainer)Parent.AddComponent(
                        PSMContentContainerFactory.Instance, Parent.Components.IndexOf(first));
                }
            }
            else //add as root
            {
                PSMDiagram diagram = (PSMDiagram) Diagram;
                PSMContentContainer contentContainer = (PSMContentContainer)PSMContentContainerFactory.Instance.Create(null, diagram.Project.Schema);
                contentContainer.Diagram = diagram;
                psmContainer = contentContainer;
                diagram.Roots.Add(psmContainer);
            }
        	psmContainer.Name = Name;

            CreatedContainer.Element = psmContainer;

			AssociatedElements.Add(psmContainer);

        	foreach (PSMSubordinateComponent containedComponent in containedComponents)
        	{
        	    oldIndexes[containedComponent] = containedComponent.ComponentIndex();
				Parent.Components.Remove(containedComponent);
        		psmContainer.Components.Add(containedComponent);
        	}
			
			Debug.Assert(CreatedContainer.HasValue);
			Diagram.AddModelElement(psmContainer, ViewHelper = new PSMElementViewHelper(Diagram));
        }

        internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedContainer.HasValue);
        	foreach (PSMSubordinateComponent containedComponent in containedComponents)
			{
				CreatedContainer.Element.Components.Remove(containedComponent);
				Parent.Components.Insert(oldIndexes[containedComponent], containedComponent);
			}
            if (Parent == null) ((PSMDiagram) Diagram).Roots.Remove(CreatedContainer.Element);
            Diagram.RemoveModelElement(CreatedContainer.Element);
            CreatedContainer.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedContainer.Element.PutMeBackToModel();
            Diagram.AddModelElement(CreatedContainer.Element, ViewHelper);

            foreach (PSMSubordinateComponent containedComponent in containedComponents)
            {
                Parent.Components.Remove(containedComponent);
                CreatedContainer.Element.Components.Add(containedComponent);
            }
        }
    }

	#region NewPSMContentContainerCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewPSMContentContainerCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewPSMContentContainerCommandFactory : DiagramCommandFactory<NewPSMContentContainerCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewPSMContentContainerCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewPSMContentContainerCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewPSMContentContainerCommand(diagramController);
		}
	}

	#endregion
}
