using System;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.Controller.Interfaces;
using Class=XCase.Model.Class;
using DataType=XCase.Model.DataType;
using Operation=XCase.Model.Operation;
using Package=XCase.Model.Package;
using Property=XCase.Model.Property;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PIM Class used to receive requests from View and create commands
    /// for changing the model accordingly
    /// </summary>
    public class ClassController : NamedElementController, IControlsAttributes, IControlsOperations
    {
		public PIMClass Class { get { return (PIMClass)NamedElement; } }

        public IHasAttributes AttributeHolder { get { return Class; } }

        public IHasOperations OperationHolder { get { return Class; } }

        public ClassController(PIMClass @class, DiagramController diagramController) :
            base(@class, diagramController)
        {
        }

    	#region Attribute Commands

    	public void AddNewAttribute(string attributeName)
    	{
    		NewAttributeCommand c = (NewAttributeCommand)NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController);
			if (!String.IsNullOrEmpty(attributeName))
				c.Name = attributeName;
			else
				c.Name = NameSuggestor<Property>.SuggestUniqueName(Class.Attributes, "Attribute", property => property.Name);
    		c.Owner = Class;
    		c.Execute();
    	}

		public void AddNewAttribute(string attributeName, string newSimpleDataType, uint ? lower, UnlimitedNatural ? upper, string @default)
		{
			MacroCommand<ModelController> command = MacroCommandFactory<ModelController>.Factory().Create(DiagramController.ModelController);
			AddSimpleTypeCommand simpleTypeCommand = (AddSimpleTypeCommand)AddSimpleTypeCommandFactory.Factory().Create(DiagramController.ModelController);
			ElementHolder<DataType> type = new ElementHolder<DataType>();
			simpleTypeCommand.CreatedSimpleType = type;
			command.Commands.Add(simpleTypeCommand);
			NewAttributeCommand attributeCommand = (NewAttributeCommand)NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController);
			if (!String.IsNullOrEmpty(attributeName))
				attributeCommand.Name = attributeName;
			else
				attributeCommand.Name = NameSuggestor<Property>.SuggestUniqueName(Class.Attributes, "Attribute", property => property.Name);
			attributeCommand.Type = type;
			attributeCommand.Lower = lower;
			attributeCommand.Upper = upper;
			attributeCommand.Default = @default;
			attributeCommand.Owner = Class;
			command.Commands.Add(attributeCommand);
			command.Execute();
		}

    	public void AddNewAttribute(string attributeName, DataType type, uint ? lower, UnlimitedNatural ? upper, string @default)
		{
			NewAttributeCommand c = (NewAttributeCommand)NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController);
			if (!String.IsNullOrEmpty(attributeName))
				c.Name = attributeName;
			else
				c.Name = NameSuggestor<Property>.SuggestUniqueName(Class.Attributes, "Attribute", property => property.Name);
    		c.Type = new ElementHolder<DataType> { Element = type };
			c.Lower = lower;
			c.Upper = upper;
			c.Default = @default;
    		c.Owner = Class;
    		c.Execute();
		}

    	public void RenameAttribute(Property attribute, string newName)
    	{
			RenameElementCommand<Property> renameElementCommand = (RenameElementCommand<Property>)RenameElementCommandFactory<Property>.Factory().Create(DiagramController.ModelController);
    		renameElementCommand.NewName = newName;
			renameElementCommand.AssociatedElements.Add(Class);
    		renameElementCommand.RenamedElement = attribute;
    		renameElementCommand.ContainingCollection = Class.Attributes;
    		renameElementCommand.Execute();
    	}

    	public void ChangeAttributeType(Property attribute, ElementHolder<DataType> newType)
    	{
    		ChangeAttributeTypeCommand c = (ChangeAttributeTypeCommand)ChangeAttributeTypeCommandFactory.Factory().Create(DiagramController.ModelController);
    		c.AssociatedElements.Add(Class);
			c.Attribute = attribute;
    		c.NewType = newType;	
    		c.Execute();
    	}

    	public void ChangeAttributeDefaultValue(Property attribute, string newDefaultValue)
    	{
    		ChangeAttributeDefaultValueCommand c = (ChangeAttributeDefaultValueCommand)ChangeAttributeDefaultValueCommandFactory.Factory().Create(DiagramController.ModelController);
    		c.Attribute = attribute;
			c.AssociatedElements.Add(Class);
    		c.NewDefault = newDefaultValue;
    		c.Execute();
    	}

    	public void RemoveAttribute(Property attribute)
    	{
            RemoveAttributeMacroCommand removeAttributeCommand = (RemoveAttributeMacroCommand)RemoveAttributeMacroCommandFactory.Factory().Create(DiagramController.ModelController);
            removeAttributeCommand.Set(attribute);
			removeAttributeCommand.AssociatedElements.Add(Class);
            if (removeAttributeCommand.Commands.Count > 0) removeAttributeCommand.Execute();
    	}

		public void ShowClassDialog()
		{
			ClassDialog dialog = new ClassDialog(this, DiagramController.ModelController);

			dialog.ShowDialog();
		}

		public void ShowAttributeDialog(Property attribute)
		{
			AttributeDialog attributeDialog = new AttributeDialog(attribute, this, DiagramController.ModelController);

			attributeDialog.ShowDialog();
		}

    	#endregion

    	#region Operations Commands

    	public void AddNewOperation(string operationName)
    	{
    		NewOperationCommand c = (NewOperationCommand)NewOperationCommandFactory.Factory().Create(DiagramController.ModelController);
			if (!String.IsNullOrEmpty(operationName))
				c.Name = operationName;
			else
				c.Name = NameSuggestor<Operation>.SuggestUniqueName(Class.Operations, "Operation", operation => operation.Name);

    		c.Owner = Class;
    		c.Execute();
    	}

		public void RemoveOperation(Operation operation)
		{
			RemoveOperationCommand removeOperationCommand = (RemoveOperationCommand)RemoveOperationCommandFactory.Factory().Create(DiagramController.ModelController);
			removeOperationCommand.DeletedOperation = operation;
			removeOperationCommand.Execute();
		}

		public void RenameOperation(Operation operation, string newName)
		{
			RenameElementCommand<Operation> renameElementCommand =
				(RenameElementCommand<Operation>)RenameElementCommandFactory<Operation>.Factory().Create(DiagramController.ModelController);
			renameElementCommand.NewName = newName;
			renameElementCommand.RenamedElement = operation;
			renameElementCommand.ContainingCollection = Class.Operations;
			renameElementCommand.AssociatedElements.Add(Class);
			renameElementCommand.Execute();
		}

    	#endregion

        public PSMClass DerivePSMClassToNewDiagram()
        {
            DerivePSMClassToNewDiagramCommand c = 
                (DerivePSMClassToNewDiagramCommand) DerivePSMClassToNewDiagramCommandFactory.Factory().Create(DiagramController.ModelController);
            c.Set(Class);
            c.Execute();
        	return c.pSMClassHolder.Element;
        }

        public void DerivePSMClassToDiagram(PSMDiagram d)
        {
            DerivePSMClassToDiagramCommand c =
                (DerivePSMClassToDiagramCommand)DerivePSMClassToDiagramCommandFactory.Factory().Create(DiagramController.ModelController);
            c.Set(Class, d);
            c.Execute();
        }

    	public void MoveToPackage(Package package)
    	{
			MoveClassToPackageCommand moveClassCommand = (MoveClassToPackageCommand)MoveClassToPackageCommandFactory.Factory().Create(DiagramController.ModelController);
			moveClassCommand.OldPackage = Class.Package;
			moveClassCommand.NewPackage = package;
			moveClassCommand.MovedClass = Class;
    		moveClassCommand.Execute();
    	}
    }
}
