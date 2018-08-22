using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Changes PSMClass' XML element name
    /// </summary>
    public class ChangePSMClassElementNameCommand : DiagramCommandBase 
    {
        [MandatoryArgument]
        public PSMClass PSMClass { get; set; }

        [MandatoryArgument]
        public string ElementName { get; set; }

        private string oldLabel;

        public ChangePSMClassElementNameCommand(DiagramController controller) : base(controller) 
        {
            Description = CommandDescription.CHANGE_ELEMENT_NAME;
        }

        public override bool CanExecute()
        {
            return (PSMClass != null) && (ElementName != null);
        }

        internal override void CommandOperation()
        {
            oldLabel = PSMClass.ElementName;
            PSMClass.ElementName = ElementName;
        }

        internal override OperationResult UndoOperation()
        {
            PSMClass.ElementName = oldLabel;
            return OperationResult.OK;
        }
    }

	#region ChangePSMClassElementNameCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangePSMClassElementNameCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ChangePSMClassElementNameCommandFactory : DiagramCommandFactory<ChangePSMClassElementNameCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangePSMClassElementNameCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangePSMClassElementNameCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ChangePSMClassElementNameCommand(diagramController);
		}
	}

	#endregion
}
