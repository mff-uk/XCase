using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
    public class MoveClassToClassUnionMacroCommand : MacroCommand<DiagramController>
	{
        public MoveClassToClassUnionMacroCommand(DiagramController controller)
            : base(controller)
		{
            Description = CommandDescription.PSM_MORE_CLASSES_TO_UNION;
		}

        private PSMClassUnion ClassUnion;

        public void InitializeCommand(IEnumerable<PSMAssociation> associations, PSMClassUnion container)
        {
            if (associations.Count() == 0)
                return; 

            if (container == null)
            {
                PSMSuperordinateComponent parentClass;
                if (!PSMTree.AreComponentsOfCommonParent(associations))
                    return;
                parentClass = associations.First().Parent;

                List<PSMClassUnion> candidates = new List<PSMClassUnion>();
                FindCURecursive(parentClass, candidates);

                bool createNew = false;
                
                SelectItemsDialog d = new SelectItemsDialog();
                d.Title = "Select class union";
                d.ShortMessage = "Select class union";
                d.LongMessage = String.Empty;
                d.UseRadioButtons = true;
                ArrayList _c = new ArrayList(candidates.ToList());
                const string _newAC = "<< new class union >>";
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
                        ClassUnion = d.selectedObjects.FirstOrDefault() as PSMClassUnion;
                    }
                }

                if (createNew)
                {
                    JoinAssociationsToClassUnionMacroCommand createCUcommand =
                        (JoinAssociationsToClassUnionMacroCommand)
                        JoinAssociationsToClassUnionMacroCommandFactory.Factory().Create(Controller);
                    createCUcommand.Set(parentClass, associations);
                    
                    Commands.Add(createCUcommand);
                    return;
                }
            }
            else
            {
                ClassUnion = container;
            }

            if (ClassUnion != null)
            {
                MoveClassToExistingClassUnionMacroCommand moveCommand =
                    (MoveClassToExistingClassUnionMacroCommand)
                    MoveClassToExistingClassUnionMacroCommandFactory.Factory().Create(Controller);
                moveCommand.Set(ClassUnion, associations);
                Commands.Add(moveCommand);
            }
        }

        private static void FindCURecursive(PSMSuperordinateComponent superordinateComponent, List<PSMClassUnion> candidates)
	    {
            //var a = from PSMSubordinateComponent c in superordinateComponent.Components
            //        where c is PSMAssociation && ((PSMAssociation) c).Child is PSMClassUnion
            //        select ((PSMAssociation) c).Child;
            candidates.AddRange(
                from PSMSubordinateComponent c in superordinateComponent.Components
                where c is PSMAssociation && ((PSMAssociation) c).Child is PSMClassUnion
                select (PSMClassUnion)((PSMAssociation) c).Child);
            //foreach (PSMSuperordinateComponent component in superordinateComponent.Components.OfType<PSMSuperordinateComponent>().Where(c => !(c is PSMClass)))
            //{
            //    FindCURecursive(component, candidates);
            //}
	    }
	}

	#region MoveSubordinateToContentContainerMacroCommandFactory

	/// <summary>
    /// Factory that creates instances of <see cref="MoveClassToClassUnionMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveClassToClassUnionMacroCommandFactory : DiagramCommandFactory<MoveClassToClassUnionMacroCommandFactory> 
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveClassToClassUnionMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of <see cref="MoveClassToClassUnionMacroCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveClassToClassUnionMacroCommand(diagramController);
		}
	}

	#endregion
}