using System;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using XCase.Controller.Commands;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM AttributeContainer used to receive requests from View and create commands
    /// for changing the model accordingly. Delegates the PSM Attribute part to ControlsPSMAttributes.
    /// </summary>
    public class PSM_AttributeContainerController : NamedElementController, Interfaces.IControlsPSMAttributes
    {
		public PSMAttributeContainer AttributeContainer { get { return (PSMAttributeContainer)NamedElement; } }

    	private readonly ControlsPSMAttributes controlsPSMAttributes;

    	public PSM_AttributeContainerController(PSMAttributeContainer attributeContainer, DiagramController diagramController) :
            base(attributeContainer, diagramController)
        {
			controlsPSMAttributes = new ControlsPSMAttributes(AttributeContainer, DiagramController);
        }

        /// <summary>
        /// Removes the element this controller controls
        /// </summary>
        public override void Remove()
        {
			DiagramController.RemovePSMSubtree(Element);
        }

		public void MoveAttributeBackToClass(PSMAttribute attribute)
		{
			MoveAttributesFromAttributeContainerBackToClassMacroCommand command = (MoveAttributesFromAttributeContainerBackToClassMacroCommand)MoveAttributesFromAttributeContainerBackToClassMacroCommandFactory.Factory().Create(DiagramController);
			command.InitializeCommand(AttributeContainer, new[] { attribute });
			command.Execute();
		}

    	#region delegations of IControlsPSMAttributes

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
    		throw new System.NotImplementedException();
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

        public void MoveToContentContainer(PSMContentContainer contentContainer)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentContainer>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { AttributeContainer }, contentContainer);
            if (c.Commands.Count > 0)
                c.Execute();
        }

        public void MoveToContentChoice(PSMContentChoice contentChoice)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentChoice>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { AttributeContainer }, contentChoice);
            if (c.Commands.Count > 0)
                c.Execute();
        }
    }
}