using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class ComponentsReorderCommand<Type> : DiagramCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="ComponentsReorderCommand{Type}">ElementToDiagamCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public ComponentsReorderCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_COMPONENT_REORDER;
		}

		
		[MandatoryArgument]
		public EReorderAction ? Action { get; set; }

		public int ? ItemIndex { get; set; }

		public int ? NewItemIndex { get; set; }

		[MandatoryArgument]
		public IList<Type> Collection { get; set; }

		public override bool CanExecute()
		{
			if (Action.Value == EReorderAction.MoveOneItem)
			{
				if (!ItemIndex.HasValue || !NewItemIndex.HasValue)
				{
					ErrorDescription = CommandError.CMDERR_INDICES_NOT_SPECIFIED;
					return false; 	
				}
				if (ItemIndex > Collection.Count - 1 || NewItemIndex > Collection.Count - 1)
				{
					ErrorDescription = CommandError.CMDERR_INDEX_OUT_OF_RANGE;
					return false;
				}
			}
			return true;
		}

		internal override void CommandOperation()
		{
			Type item = Collection[ItemIndex.Value];
			Collection.Remove(item);
			Collection.Insert(NewItemIndex.Value > ItemIndex.Value ? NewItemIndex.Value : NewItemIndex.Value, item);
			
		}

		internal override OperationResult UndoOperation()
		{
			if (ItemIndex > Collection.Count - 1 || NewItemIndex > Collection.Count - 1)
				return OperationResult.Failed;
			Type item = Collection[NewItemIndex.Value];
			Collection.Remove(item);
			Collection.Insert(ItemIndex.Value > NewItemIndex.Value ? ItemIndex.Value : ItemIndex.Value, item);
			return OperationResult.OK;
		}
	}

	#region ComponentsReorderCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ComponentsReorderCommand{Type}"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ComponentsReorderCommandFactory<Type> : DiagramCommandFactory<ComponentsReorderCommandFactory<Type>>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ComponentsReorderCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ComponentsReorderCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ComponentsReorderCommand<Type>(diagramController);
		}
	}

	#endregion
}
