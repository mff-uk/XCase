using System.Collections.Generic;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.Controller.Interfaces;
using DataType=XCase.Model.DataType;
using Property=XCase.Model.Property;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM Class used to receive requests from View and create commands
    /// for changing the model accordingly. Delegates the PSM Attribute part to ControlsPSMAttributes.
    /// </summary>
    public class PSM_ClassController : NamedElementController, IControlsPSMAttributes
    {
		public PSMClass Class { get { return (PSMClass)NamedElement; } }

		public IHasOperations OperationHolder { get { return Class; } }

		private readonly ControlsPSMAttributes controlsPSMAttributes;

        public PSM_ClassController(PSMClass @class, DiagramController diagramController) :
            base(@class, diagramController)
        {
			controlsPSMAttributes = new ControlsPSMAttributes(@class, diagramController);
        }

		public void ShowClassDialog()
		{
            PSMClassDialog dialog = new PSMClassDialog(this, this.DiagramController);
            dialog.ShowDialog();
		}

        /// <summary>
        /// Renames the class
        /// </summary>
        /// <param name="newName">new name for the class</param>
        public void RenameElementWithDiagramController(string newName)
        {
            RenameElementCommandDiagram<PSMClass> command = (RenameElementCommandDiagram<PSMClass>)RenameElementCommandFactory<PSMClass>.Factory().Create(DiagramController);
            command.RenamedElement = Class;
            command.NewName = newName;
            command.Execute();
        }

		#region Attribute Commands

        public PSMAttribute AddAttribute(Property representedAttribute, string name, string alias, uint? lower, UnlimitedNatural upper, DataType type, string @default, IEnumerable<string> names)
        {
            AddPSMClassAttributeCommand c = (AddPSMClassAttributeCommand)AddPSMClassAttributeCommandFactory.Factory().Create(DiagramController);
            c.PSMClass = Class;
            c.Alias = alias;
            c.Name = name;
            c.Lower = lower;
            c.Upper = upper;
            c.Type = type;
            c.Default = @default;
            c.RepresentedAttribute = representedAttribute;
            c.UsedAliasesOrNames = names;
            c.Execute();

            return c.CreatedAttribute;
        }

        public void ModifyAttribute(PSMAttribute attribute, string name, string alias, uint? lower, UnlimitedNatural upper, DataType type, string @default)
        {
            ModifyPSMClassAttributeCommand c = (ModifyPSMClassAttributeCommand)ModifyPSMClassAttributeCommandFactory.Factory().Create(DiagramController);
            c.PSMAttribute = attribute;
            c.Alias = alias;
            c.Name = name;
            c.Lower = lower;
            c.Upper = upper;
            c.Type = type;
            c.Default = @default;
            c.Execute();
        }
		
        /// <summary>
		/// Changes the allow any attribute definition.
		/// </summary>
		/// <param name="allow">if set to <c>true</c> [allow].</param>
		public void ChangeAllowAnyAttributeDefinition(bool allow)
		{
			ChangeAllowAnyAttributeDefinitionCommand changeAllowAnyAttributeDefinitionCommand =
				(ChangeAllowAnyAttributeDefinitionCommand) ChangeAllowAnyAttributeDefinitionCommandFactory.Factory().Create(DiagramController);
			changeAllowAnyAttributeDefinitionCommand.AllowAnyAttribute = allow;
			changeAllowAnyAttributeDefinitionCommand.PSMClass = Class;
			changeAllowAnyAttributeDefinitionCommand.Execute();
		}
		
        #endregion

		public void ChangeAbstract(bool value)
		{
			ChangePSMClassAbstractCommand changePSMClassAbstractCommand =
				(ChangePSMClassAbstractCommand)ChangePSMClassAbstractCommandFactory.Factory().Create(DiagramController);
			changePSMClassAbstractCommand.PSMClass = Class;
			changePSMClassAbstractCommand.Value = value; 
			changePSMClassAbstractCommand.Execute();
		}

		public void ChangeElementName(string newElementName)
        {
            ChangePSMClassElementNameCommand c = (ChangePSMClassElementNameCommand)ChangePSMClassElementNameCommandFactory.Factory().Create(DiagramController);
            c.ElementName = newElementName;
            c.PSMClass = Class;
            c.Execute();
        }

        /// <summary>
        /// Removes the element this controller controls
        /// </summary>
        public override void Remove()
        {
			DiagramController.RemovePSMSubtree(Element);
        }

        public void DeriveNewRootPSMClass()
        {
            NewPSMClassAsRootMacroCommand c = (NewPSMClassAsRootMacroCommand)NewPSMClassAsRootMacroCommandFactory.Factory().Create(DiagramController);
            c.Set(Class.RepresentedClass);
            if (c.Commands.Count > 0) c.Execute();
        }

        public void SetPSMRepresentedClass(bool setnull)
        {
            if (setnull)
            {
                SetRepresentedPSMClassCommand c = (SetRepresentedPSMClassCommand)SetRepresentedPSMClassCommandFactory.Factory().Create(DiagramController);
                c.Set(null, Class);
                if (c.CanExecute()) c.Execute();
            }
            else
            {
                SetRepresentedPSMClassCommand c = (SetRepresentedPSMClassCommand)SetRepresentedPSMClassCommandFactory.Factory().Create(DiagramController);
                c.Set(Class, DiagramController.Diagram as PSMDiagram);
                if (c.CanExecute()) c.Execute();
            }
        }

    	public void AddClassSpecialization(PIMClass specificClass)
    	{
            AddPSMSpecializationMacroCommand command = AddPSMSpecializationMacroCommandFactory.Factory().Create(DiagramController) as AddPSMSpecializationMacroCommand;
            command.Set(Class, specificClass);
            command.Execute();
            /*AddPSMSpecializationCommand command = (AddPSMSpecializationCommand)AddPSMSpecializationCommandFactory.Factory().Create(DiagramController);
			command.GeneralPSMClass = Class;
			command.SpecificPIMClass = specificClass;
			command.Execute();*/
		}

        public void GroupBy()
        {
            GroupByCommand c = (GroupByCommand)GroupByCommandFactory.Factory().Create(DiagramController);
            c.Set(Class);
            c.Execute();
        }

        public void AddChildren()
        {
            AddPSMChildrenMacroCommand c = (AddPSMChildrenMacroCommand)AddPSMChildrenMacroCommandFactory.Factory().Create(DiagramController);
            c.Set(Class);
            c.Execute();
        }

		#region delegation IControlsPSMAttributes

		public IHasPSMAttributes AttributeHolder
		{
			get
			{
				return controlsPSMAttributes.AttributeHolder;
			}
		}

		public void AddNewAttribute()
		{
			controlsPSMAttributes.AddNewAttribute();
		}

		public void ChangeAttributeAlias(PSMAttribute attribute, string newAlias)
		{
			controlsPSMAttributes.ChangeAttributeAlias(attribute, newAlias);
		}

		public void ChangeAttributeDefaultValue(PSMAttribute attribute, string newDefaultValue)
		{
			controlsPSMAttributes.ChangeAttributeDefaultValue(attribute, newDefaultValue);
		}

		public void RemoveAttribute(PSMAttribute attribute)
		{
			controlsPSMAttributes.RemoveAttribute(attribute);
		}

		public void RenameAttribute(PSMAttribute attribute, string newName)
		{
			controlsPSMAttributes.RenameAttribute(attribute, newName);
		}

		public void ShowAttributeDialog(PSMAttribute attribute)
		{
			controlsPSMAttributes.ShowAttributeDialog(attribute);
		}

		public void ChangeAttributeType(PSMAttribute attribute, ElementHolder<DataType> newType)
		{
			controlsPSMAttributes.ChangeAttributeType(attribute, newType);
		}

        public void PropagatePIMLess(PSMAttribute attribute)
        {
            controlsPSMAttributes.PropagatePIMLess(attribute);
        }

        public void MoveAttributeUp(PSMAttribute attribute)
        {
            controlsPSMAttributes.MoveAttributeUp(attribute);
        }

        public void MoveAttributeDown(PSMAttribute attribute)
        {
            controlsPSMAttributes.MoveAttributeDown(attribute);
        }

        #endregion

        public void FindRepresentedClass()
        {
            ActivateDiagramCommand c = (ActivateDiagramCommand) ActivateDiagramCommandFactory.Factory().Create(DiagramController.ModelController);
            c.Element = Class.RepresentedClass;
            c.Execute();
        }

        public void FindRepresentedPSMClass()
        {
            ActivateDiagramCommand c = (ActivateDiagramCommand)ActivateDiagramCommandFactory.Factory().Create(DiagramController.ModelController);
            c.Set(Class.RepresentedPSMClass.Diagram);
            c.Element = Class.RepresentedPSMClass;
            c.Execute();
        }


        /// <summary>Moves attributes from class to an attribute container</summary>
        /// <param name="attributes">attributes to move</param>
        /// <param name="attributeContainer">target attribute container; if left to <c>null</c>, the attribute container is selected in a dialog</param>
        public void MoveAttributesToAttributeContainer(PSMAttributeContainer attributeContainer, params PSMAttribute[] attributes)
        {
            MoveAttributesFromClassToAttributeContainerMacroCommand c = (MoveAttributesFromClassToAttributeContainerMacroCommand)MoveAttributesFromClassToAttributeContainerMacroCommandFactory.Factory().Create(DiagramController);
            c.InitializeCommand(attributes, Class, null);
            if (c.Commands.Count > 0)
                c.Execute();
        }
    }
}