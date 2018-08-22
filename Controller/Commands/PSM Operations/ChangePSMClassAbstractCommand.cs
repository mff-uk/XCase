using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Alters PSMClass' <see cref="Class.IsAbstract"/> property. 
	/// </summary>
	public class ChangePSMClassAbstractCommand : DiagramCommandBase
	{
		/// <summary>
		/// Altered class
		/// </summary>
		[MandatoryArgument]
		public PSMClass PSMClass { get; set; }

		/// <summary>
		/// Value assigned to PSMClass' <see cref="Class.IsAbstract"/> property.
		/// </summary>
		[MandatoryArgument]
		public bool? Value { get; set; }

		private bool oldValue; 

		/// <summary>
		/// Creates new instance of <see cref="ChangePSMClassAbstractCommand">ChangePSMClassAbstractCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public ChangePSMClassAbstractCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_CLASS_ALTER_ABSTRACT;
		}

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			AssociatedElements.Add(PSMClass);
			oldValue = PSMClass.IsAbstract;
			PSMClass.IsAbstract = Value.Value;
		}

		internal override OperationResult UndoOperation()
		{
			PSMClass.IsAbstract = oldValue;
			return OperationResult.OK;
		}
	}

	#region ChangePSMClassAbstractCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangePSMClassAbstractCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ChangePSMClassAbstractCommandFactory : DiagramCommandFactory<ChangePSMClassAbstractCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangePSMClassAbstractCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangePSMClassAbstractCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ChangePSMClassAbstractCommand(diagramController);
		}
	}

	#endregion
}
