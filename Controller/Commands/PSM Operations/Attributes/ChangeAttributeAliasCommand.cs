using System.Linq;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Changes an alias of a PSMClass' attribute
	/// </summary>
	public class ChangeAttributeAliasCommand : DiagramCommandBase
	{
		/// <summary>
		/// Attribute whose alias is changed
		/// </summary>
		[MandatoryArgument]
		public PSMAttribute Attribute { get; set; }

		private string newAlias;

		/// <summary>
		/// New alias for <see cref="Attribute"/>. Can be null or empty.
		/// </summary>
		public string NewAlias
		{
			get { return newAlias; }
			set
			{
				newAlias = value != string.Empty ? value : null;
			}
		}

		private string oldAlias;

		/// <summary>
		/// Creates new instance of <see cref="ChangeAttributeAliasCommand">ElementToDiagamCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public ChangeAttributeAliasCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_CHANGE_ATTRIBUTE_ALIAS;
		}

		public override bool CanExecute()
		{
            if (Attribute.AttributeContainer != null)
            {
                if (NewAlias != Attribute.Alias &&
                    !NameSuggestor<PSMAttribute>.IsNameUnique(Attribute.AttributeContainer.PSMAttributes, NewAlias, attr => attr.AliasOrName, Attribute))
                {
                    ErrorDescription = string.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, NewAlias);
                    return false;
                }
            }
            else
            {
                if (NewAlias != Attribute.Alias &&
                    !NameSuggestor<PSMAttribute>.IsNameUnique(Attribute.Class.PSMAttributes, NewAlias, attr => attr.AliasOrName, Attribute))
                {
                    ErrorDescription = string.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, NewAlias);
                    return false;
                }
            }
			return true;
		}

		internal override void CommandOperation()
		{
			oldAlias = Attribute.Alias;
			Attribute.Alias = NewAlias;
		}

		internal override OperationResult UndoOperation()
		{
			if (Attribute.Alias != oldAlias)
			{
                if (Attribute.AttributeContainer != null)
                {
                    if (!NameSuggestor<PSMAttribute>.IsNameUnique(Attribute.AttributeContainer.PSMAttributes, oldAlias, attr => attr.Alias))
                    {
                        ErrorDescription = string.Format(CommandError.CMDERR_DUPLICATE_ATTRIBUTE, oldAlias, Attribute.AttributeContainer);
                        return OperationResult.Failed;
                    }
                }
                else
                {
                    if (!NameSuggestor<PSMAttribute>.IsNameUnique(Attribute.Class.PSMAttributes, oldAlias, attr => attr.Alias))
                    {
                        ErrorDescription = string.Format(CommandError.CMDERR_DUPLICATE_ATTRIBUTE, oldAlias, Attribute.Class);
                        return OperationResult.Failed;
                    }
                }
                Attribute.Alias = oldAlias;
			}
			return OperationResult.OK;
		}
	}

	#region ChangeAttributeAliasCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeAttributeAliasCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ChangeAttributeAliasCommandFactory : DiagramCommandFactory<ChangeAttributeAliasCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeAttributeAliasCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeAttributeAliasCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ChangeAttributeAliasCommand(diagramController);
		}
	}

	#endregion
}
