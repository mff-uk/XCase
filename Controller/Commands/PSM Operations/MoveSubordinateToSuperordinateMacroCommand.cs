using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
    public class MoveSubordinateToSuperordinateMacroCommand<SUPERORDINATE_TYPE> : MacroCommand<DiagramController>
        where SUPERORDINATE_TYPE : class, PSMElement, PSMSuperordinateComponent
	{
        public MoveSubordinateToSuperordinateMacroCommand(DiagramController controller)
            : base(controller)
		{
            Description = CommandDescription.ADD_PSM_CONTENT_CONTAINER;
		}

        private SUPERORDINATE_TYPE Container;

        public void InitializeCommand(IEnumerable<PSMSubordinateComponent> components, SUPERORDINATE_TYPE container)
        {
            if (components.Count() == 0)
                return; 

            if (container == null)
            {
                PSMClass parentClass;
                if (
                    !MoveSubordinateToSuperordinateCommand<SUPERORDINATE_TYPE>.CheckCommonParentClass(components,
                                                                                                      out parentClass))
                    return;

                List<SUPERORDINATE_TYPE> candidates = new List<SUPERORDINATE_TYPE>();
                FindContainerRecursive(parentClass, candidates);

                bool createNew = false;
                
                SelectItemsDialog d = new SelectItemsDialog();
                d.Title = "Select container";
                d.ShortMessage = "Select container";
                d.LongMessage = String.Empty;
                d.UseRadioButtons = true;
                ArrayList _c = new ArrayList(candidates.ToList());
                const string _newAC = "<< new container >>";
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
                        Container = d.selectedObjects.FirstOrDefault() as SUPERORDINATE_TYPE;
                    }
                }

                if (createNew)
                {
                    if (typeof(PSMContentContainer).IsAssignableFrom(typeof(SUPERORDINATE_TYPE)))
                    {
                        NewPSMContentContainerCommand createCCcommand =
                            (NewPSMContentContainerCommand)
                            NewPSMContentContainerCommandFactory.Factory().Create(Controller);
                        createCCcommand.Parent = components.First().Parent;
                        createCCcommand.ContainedComponents.AddRange(components);
                        Commands.Add(createCCcommand);
                    }
                    else if (typeof(PSMContentChoice).IsAssignableFrom(typeof(SUPERORDINATE_TYPE)))
                    {
                        NewPSMContentChoiceCommand createCCcommand =
                            (NewPSMContentChoiceCommand)
                            NewPSMContentChoiceCommandFactory.Factory().Create(Controller);
                        createCCcommand.Parent = components.First().Parent;
                        createCCcommand.ContainedComponents.AddRange(components);
                        Commands.Add(createCCcommand);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    return;
                }
            }
            else
            {
                Container = container;
            }

            if (Container != null)
            {
                MoveSubordinateToSuperordinateCommand<SUPERORDINATE_TYPE> moveCommand =
                    (MoveSubordinateToSuperordinateCommand<SUPERORDINATE_TYPE>)
                    MoveSubordinateToSuperordinateCommandFactory<SUPERORDINATE_TYPE>.Factory().Create(Controller);
                moveCommand.Container = Container;
                moveCommand.Components.AddRange(components);
                Commands.Add(moveCommand);
            }
        }

        private static void FindContainerRecursive(PSMSuperordinateComponent superordinateComponent, List<SUPERORDINATE_TYPE> candidates)
	    {
            candidates.AddRange(superordinateComponent.Components.OfType<SUPERORDINATE_TYPE>());
            foreach (PSMSuperordinateComponent component in superordinateComponent.Components.OfType<PSMSuperordinateComponent>().Where(c => !(c is PSMClass)))
	        {
                FindContainerRecursive(component, candidates);
	        }
	    }
	}

	#region MoveSubordinateToContentContainerMacroCommandFactory

	/// <summary>
    /// Factory that creates instances of <see cref="MoveSubordinateToSuperordinateMacroCommand{SUPERORDINATE_TYPE}"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveSubordinateToSuperordinateMacroCommandFactory<SUPERORDINATE_TYPE> : DiagramCommandFactory<MoveSubordinateToSuperordinateMacroCommandFactory<SUPERORDINATE_TYPE>> 
        where SUPERORDINATE_TYPE : class, PSMSuperordinateComponent
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveSubordinateToSuperordinateMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of <see cref="MoveSubordinateToSuperordinateMacroCommand{SUPERORDINATE_TYPE}"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveSubordinateToSuperordinateMacroCommand<SUPERORDINATE_TYPE>(diagramController);
		}
	}

	#endregion
}