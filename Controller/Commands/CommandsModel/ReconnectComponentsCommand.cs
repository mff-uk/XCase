using System.Collections.ObjectModel;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Reconnects all the components subordinated to the given source component
    /// to the given target component. The components are removed from the Components
    /// collection of the source and are inserted to the end of the target.
    /// </summary>
    public class ReconnectComponentsCommand : ModelCommandBase
    {
        /// <summary>
        /// Gets or sets a reference to the source component.
        /// </summary>
        public PSMSuperordinateComponent SourceComponent { get; set; }

        /// <summary>
        /// Gets or sets a reference to the target component.
        /// </summary>
        public PSMSuperordinateComponent TargetComponent { get; set; }

        /// <summary>
        /// Collection referencing the components that were moved.
        /// Used for undoing the operation.
        /// </summary>
        private ObservableCollection<PSMSubordinateComponent> movedComponents;

        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <param name="modelController">Reference to the associated model controller</param>
        public ReconnectComponentsCommand(ModelController modelController)
            : base(modelController)
        {
            movedComponents = new ObservableCollection<PSMSubordinateComponent>();
            Description = CommandDescription.RECONNECT_COMPONENTS;
        }

        public override bool CanExecute()
        {
            return (SourceComponent != null) && (TargetComponent != null);
        }

        internal override void CommandOperation()
        {
            foreach (PSMSubordinateComponent component in SourceComponent.Components)
            {
                TargetComponent.Components.Add(component);
                movedComponents.Add(component);
            }
            SourceComponent.Components.Clear();
        }

        internal override OperationResult UndoOperation()
        {
            foreach (PSMSubordinateComponent component in movedComponents)
            {
                TargetComponent.Components.Remove(component);
                SourceComponent.Components.Add(component);
            }
            movedComponents.Clear();

            return OperationResult.OK;
        }
    }

    #region ReconnectComponentsCommandFactory

    /// <summary>
    /// Creates instances of the ReconnectComponentsCommand command.
    /// </summary>
    public class ReconnectComponentsCommandFactory : ModelCommandFactory<ReconnectComponentsCommandFactory>
    {
        private ReconnectComponentsCommandFactory() { }

        public override IStackedCommand Create(ModelController controller)
        {
            return new ReconnectComponentsCommand(controller);
        }
    }

    #endregion
}
