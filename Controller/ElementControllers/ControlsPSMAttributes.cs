using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Controller.Dialogs;
using XCase.Controller.Interfaces;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller part for a PSM Class Controller or AttributeContainer Controller implementing
    /// the PSM Attribute management part
    /// </summary>
    public class ControlsPSMAttributes : IControlsPSMAttributes
	{
		public DiagramController DiagramController { get; private set; }

		public ControlsPSMAttributes(IHasPSMAttributes attributeHolder, DiagramController diagramController)
		{
			AttributeHolder = attributeHolder;
			DiagramController = diagramController;
		}

		public IHasPSMAttributes AttributeHolder { get; private set; }

		private PSMClass Class
		{
			get
			{
				if (AttributeHolder is PSMClass)
					return (PSMClass)AttributeHolder;
				else if (AttributeHolder is PSMAttributeContainer)
				{
					PSMSuperordinateComponent parent = ((PSMAttributeContainer)AttributeHolder).Parent;
					while (parent != null && !(parent is PSMClass) && (parent is PSMSubordinateComponent))
					{
						parent = ((PSMSubordinateComponent)parent).Parent;
					}
					if (parent is PSMClass)
						return (PSMClass)((PSMAttributeContainer)AttributeHolder).Parent;
				}
				return null;
			}
		}

		public void AddNewAttribute()
		{
			NewAttributeCommand c = NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController) as NewAttributeCommand;
			c.Owner = AttributeHolder as IHasAttributes;
			c.Name = NameSuggestor<Property>.SuggestUniqueName(((IHasAttributes) AttributeHolder).Attributes, "Attribute", property => property.Name);
			if (c.CanExecute()) c.Execute();
		}

		public void ChangeAttributeType(PSMAttribute attribute, ElementHolder<DataType> newType)
		{
			ChangeAttributeTypeCommand c = (ChangeAttributeTypeCommand)ChangeAttributeTypeCommandFactory.Factory().Create(DiagramController.ModelController);
			c.AssociatedElements.Add(Class);
			c.Attribute = attribute;
			c.NewType = newType;
			c.Execute();
		}

		public void RenameAttribute(PSMAttribute attribute, string newName)
		{
			RenameElementCommand<PSMAttribute> renameElementCommand = (RenameElementCommand<PSMAttribute>)RenameElementCommandFactory<PSMAttribute>.Factory().Create(DiagramController.ModelController);
			renameElementCommand.NewName = newName;
			renameElementCommand.RenamedElement = attribute;
			renameElementCommand.ContainingCollection = Class.PSMAttributes;
			renameElementCommand.Execute();
		}

		public void ChangeAttributeDefaultValue(PSMAttribute attribute, string newDefaultValue)
		{
			ChangeAttributeDefaultValueCommand changeAttributeDefaultValueCommand = (ChangeAttributeDefaultValueCommand)ChangeAttributeDefaultValueCommandFactory.Factory().Create(DiagramController.ModelController);
			changeAttributeDefaultValueCommand.Attribute = attribute;
			changeAttributeDefaultValueCommand.AssociatedElements.Add(Class);
			changeAttributeDefaultValueCommand.NewDefault = newDefaultValue;
			changeAttributeDefaultValueCommand.Execute();
		}

		public void RemoveAttribute(PSMAttribute attribute)
		{
			RemovePSMAttributeCommand2 removeAttributeCommand = (RemovePSMAttributeCommand2) RemovePSMAttributeCommand2Factory.Factory().Create(DiagramController);
			removeAttributeCommand.DeletedAttribute = attribute;
			if (AttributeHolder is PSMAttributeContainer) removeAttributeCommand.AttributeContainer = AttributeHolder as PSMAttributeContainer;
			removeAttributeCommand.Execute();
		}

		public void ChangeAttributeAlias(PSMAttribute attribute, string newAlias)
		{
			ChangeAttributeAliasCommand changeAttributeAliasCommand = (ChangeAttributeAliasCommand)ChangeAttributeAliasCommandFactory.Factory().Create(DiagramController);
			changeAttributeAliasCommand.Attribute = attribute;
			changeAttributeAliasCommand.NewAlias = newAlias;
			changeAttributeAliasCommand.Execute();
		}

		public void ShowAttributeDialog(PSMAttribute attribute)
		{
			AttributeDialog attributeDialog = new AttributeDialog(attribute, this, DiagramController.ModelController);
			attributeDialog.ShowDialog();
		}

		public void PropagatePIMLess(PSMAttribute attribute)
		{
			PropagatePIMLessMacroCommand c = PropagatePIMLessMacroCommandFactory.Factory().Create(DiagramController.ModelController) as PropagatePIMLessMacroCommand;
			c.Set(attribute);
			if (c.Commands.Count > 0) c.Execute();
		}

        public void MoveAttributeUp(PSMAttribute attribute)
        {
            ComponentsReorderCommand<PSMAttribute> command = (ComponentsReorderCommand<PSMAttribute>) ComponentsReorderCommandFactory<PSMAttribute>.Factory().Create(DiagramController);
            command.Action = EReorderAction.MoveOneItem;
            command.ItemIndex = AttributeHolder.PSMAttributes.IndexOf(attribute);
            command.NewItemIndex = command.ItemIndex - 1;
            command.Collection = AttributeHolder.PSMAttributes;
            command.Execute();
        }

        public void MoveAttributeDown(PSMAttribute attribute)
        {
            ComponentsReorderCommand<PSMAttribute> command = (ComponentsReorderCommand<PSMAttribute>)ComponentsReorderCommandFactory<PSMAttribute>.Factory().Create(DiagramController);
            command.Action = EReorderAction.MoveOneItem;
            command.ItemIndex = AttributeHolder.PSMAttributes.IndexOf(attribute);
            command.NewItemIndex = command.ItemIndex + 1;
            command.Collection = AttributeHolder.PSMAttributes;
            command.Execute();           
        }
	}
}