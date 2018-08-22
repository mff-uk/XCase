using System;
using NUml.Uml2;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Changes <see cref="AssociationEnd"/>'s aggregation kind to value of <see cref="NewAggregationKind"/>.
	/// </summary>
	public class ChangeAssociationAggregationCommand : ModelCommandBase
	{
		/// <summary>
		/// <see cref="AssociationEnd"/> whose aggregation kind is altered.
		/// </summary>
		[MandatoryArgument]
		public AssociationEnd AssociationEnd { get; set; }

		/// <summary>
		/// Setted <see cref="AggregationKind"/> 
		/// </summary>
		[MandatoryArgument]
		public AggregationKind NewAggregationKind { get; set; }

		private AggregationKind oldAggregationKind;

		/// <summary>
		/// Creates new instance of <see cref="ChangeAssociationAggregationCommand" />. 
		/// </summary>
		/// <param name="controller">command's controller</param>
		public ChangeAssociationAggregationCommand(ModelController controller)
			: base(controller)
		{
			Description = String.Format(CommandDescription.CHANGE_AGGREGATION, NewAggregationKind);
		}

		/// <summary>
		/// Returns true since <see cref="ChangeAssociationAggregationCommand"/> can be 
		/// always executed.
		/// </summary>
		/// <returns>returns always true</returns>
		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			AssociatedElements.Add(AssociationEnd.Association);
			oldAggregationKind = AssociationEnd.Aggregation;
			AssociationEnd.Aggregation = NewAggregationKind;
		}

		internal override OperationResult UndoOperation()
		{
			AssociationEnd.Aggregation = oldAggregationKind;
			return OperationResult.OK;
		}
	}

	#region ChangeAssociationAggregationCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeAssociationAggregationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeAssociationAggregationCommandFactory : ModelCommandFactory<ChangeAssociationAggregationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeAssociationAggregationCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeAssociationAggregationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new ChangeAssociationAggregationCommand(modelController);
		}
	}

	#endregion
}