using System.Linq;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Moves component from content choice or content container 
	/// </summary>
	public class MoveComponentOutOfContainerCommand : DiagramCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="MoveComponentOutOfContainerCommand">MoveComponentOutOfContainerCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public MoveComponentOutOfContainerCommand(DiagramController diagramController)
			: base(diagramController)
		{
            Description = CommandDescription.PSM_MOVE_CONTENT_FROM_CONTAINER;
		}

		[MandatoryArgument]
		public PSMSuperordinateComponent Container
		{
			get;
			set;
		}

		private PSMSuperordinateComponent parent;

	    private readonly Dictionary<PSMSubordinateComponent, int> oldIndexes = new Dictionary<PSMSubordinateComponent, int>();
	    private readonly List<PSMSubordinateComponent> movedComponents = new List<PSMSubordinateComponent>();

	    public List<PSMSubordinateComponent> MovedComponents
	    {
	        get { return movedComponents; }
	    }


		public override bool CanExecute()
		{
			return (Container != null && Container is PSMContentChoice || Container is PSMContentContainer);
		}

		internal override void CommandOperation()
		{
			parent = ((PSMSubordinateComponent)Container).Parent;
            int index = ((PSMSubordinateComponent)Container).ComponentIndex() + 1;
			foreach (PSMSubordinateComponent component in movedComponents)
			{
			    oldIndexes[component] = component.ComponentIndex();
				Container.Components.Remove(component);
                parent.Components.Insert(index++, component);
			    AssociatedElements.Add(component);
			}
		}

		internal override OperationResult UndoOperation()
		{
			foreach (PSMSubordinateComponent component in movedComponents)
			{
				parent.Components.Remove(component);
				Container.Components.Insert(oldIndexes[component], component);
			}
			return OperationResult.OK;
		}
	}

	#region MoveComponentOutOfContainerCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveComponentOutOfContainerCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveComponentOutOfContainerCommandFactory : DiagramCommandFactory<MoveComponentOutOfContainerCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveComponentOutOfContainerCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MoveComponentOutOfContainerCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveComponentOutOfContainerCommand(diagramController);
		}
	}

	#endregion
}
