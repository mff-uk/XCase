using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
	public class MoveAttributesFromClassToAttributeContainerMacroCommand: MacroCommand<DiagramController>
	{
        public MoveAttributesFromClassToAttributeContainerMacroCommand(DiagramController controller)
            : base(controller)
		{
			Description = CommandDescription.ADD_PSM_ATTRIBUTE_CONTAINER;
		}

        private PSMAttributeContainer AttributeContainer;

        public void InitializeCommand(IEnumerable<PSMAttribute> attributes, PSMClass parentClass, PSMAttributeContainer attributeContainer)
        {
            if (attributes.Count() == 0)
                return; 

            if (attributeContainer == null)
            {
                List<PSMAttributeContainer> candidates = new List<PSMAttributeContainer>();
                FindACRecursive(parentClass, candidates);
               
                bool createNew = false;
                if (candidates.Count() == 0)
                {
                    createNew = true;
                }
                else
                {
                    SelectItemsDialog d = new SelectItemsDialog();
                    d.Title = "Select attribute container";
                    d.ShortMessage = "Select attribute container";
                    d.LongMessage = String.Empty;
                    d.UseRadioButtons = true;
                    ArrayList _c = new ArrayList(candidates.ToList());
                    const string _newAC = "<< new attribute container >>";
                    _c.Add(_newAC);
                    d.SetItems(_c);

                    if (d.ShowDialog() == true)
                    {
                        if (d.selectedObjects.FirstOrDefault().Equals(_newAC))
                        {
                            createNew = true;
                        }
                        else
                        {
                            AttributeContainer = d.selectedObjects.FirstOrDefault() as PSMAttributeContainer;
                        }
                    }
                }

                if (createNew)
                {
                    NewPSMAttributeContainerCommand createACcommand = (NewPSMAttributeContainerCommand)NewPSMAttributeContainerCommandFactory.Factory().Create(Controller);
                    createACcommand.PSMClass = parentClass;
                    createACcommand.PSMAttributes.AddRange(attributes);
                    Commands.Add(createACcommand);
                    return;
                }
            }
            else
            {
                AttributeContainer = attributeContainer;
            }

            if (AttributeContainer != null)
            {
                MoveAttributesFromClassToAttributeContainerCommand moveCommand =
                    (MoveAttributesFromClassToAttributeContainerCommand)
                    MoveAttributesFromClassToAttributeContainerCommandFactory.Factory().Create(Controller);
                moveCommand.AttributeContainer = AttributeContainer;
                moveCommand.Attributes.AddRange(attributes);
                Commands.Add(moveCommand);
            }
        }

        private static void FindACRecursive(PSMSuperordinateComponent superordinateComponent, List<PSMAttributeContainer> candidates)
	    {
            candidates.AddRange(superordinateComponent.Components.OfType<PSMAttributeContainer>());
            foreach (PSMSuperordinateComponent component in superordinateComponent.Components.OfType<PSMSuperordinateComponent>().Where(c => !(c is PSMClass)))
	        {
                FindACRecursive(component, candidates);
	        }
	    }
	}

	#region MoveAttributesFromClassToAttributeContainerMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveAttributesFromClassToAttributeContainerMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveAttributesFromClassToAttributeContainerMacroCommandFactory : DiagramCommandFactory<MoveAttributesFromClassToAttributeContainerMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveAttributesFromClassToAttributeContainerMacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of <see cref="MoveAttributesFromClassToAttributeContainerMacroCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveAttributesFromClassToAttributeContainerMacroCommand(diagramController);
		}
	}

	#endregion
}