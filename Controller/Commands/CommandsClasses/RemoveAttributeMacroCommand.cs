using XCase.Model;
using System;
using XCase.Controller.Dialogs;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes attribute of an element implementing
	/// <see cref="IHasAttributes"/> (usually subtype of Class) with its PSM dependencies
	/// </summary>
	public class RemoveAttributeMacroCommand: MacroCommand<ModelController>
	{
		/// <summary>
		/// Creates new instance of <see cref="RemoveAttributeCommand" />. 
		/// </summary>
		/// <param name="controller">command controller</param>
        public RemoveAttributeMacroCommand(ModelController controller)
			: base(controller)
		{
			Description = CommandDescription.REMOVE_ATTRIBUTE;
		}

		/// <summary>
		/// Deleted attribute
		/// </summary>
		[MandatoryArgument]
		private Property DeletedAttribute { get; set; }
		
		[CommandResult]
		private IHasAttributes modelClass { get; set; } 

		public void Set(Property deletedAttribute)
        {
            if (deletedAttribute.DerivedPSMAttributes.Count > 0)
            {
                OkCancelDialog d = new OkCancelDialog();
                d.PrimaryContent = "Delete dependent PSM attributes";
                d.Title = "Delete dependent PSM attributes";
                string attr = "Attribute \"{0}\" has PSM dependecies. Those will be deleted as well:" + Environment.NewLine + Environment.NewLine;
                foreach (PSMAttribute A in deletedAttribute.DerivedPSMAttributes)
                {
                    attr += A.Class.Diagram.Caption + ": " + A.Class.Name + "." + A.Name + Environment.NewLine;
                }
                attr = attr.Remove(attr.Length - 1);
                d.SecondaryContent = string.Format(attr, deletedAttribute.Name);
                if (d.ShowDialog() == false) return;
                else foreach (PSMAttribute A in deletedAttribute.DerivedPSMAttributes)
                {
                    RemovePSMAttributeCommand cPSM = RemovePSMAttributeCommandFactory.Factory().Create(Controller) as RemovePSMAttributeCommand;
                    if (A.AttributeContainer != null) cPSM.AttributeContainer = A.AttributeContainer;
                    cPSM.DeletedAttribute = A;
                    Commands.Add(cPSM);
                }
            }

            RemoveAttributeCommand c = RemoveAttributeCommandFactory.Factory().Create(Controller) as RemoveAttributeCommand;
            c.DeletedAttribute = deletedAttribute;
            Commands.Add(c);
        }
    }

    #region RemoveAttributeMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RemoveAttributeMacroCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RemoveAttributeMacroCommandFactory : ModelCommandFactory<RemoveAttributeMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private RemoveAttributeMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of RemoveAttributeMacroCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new RemoveAttributeMacroCommand(modelController);
		}
	}

	#endregion
}
