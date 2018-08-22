using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
    public class MoveClassToExistingClassUnionMacroCommand : MacroCommand<DiagramController>
    {
        /// <summary>
        /// Creates new instance of <see cref="MoveClassToExistingClassUnionMacroCommand">MoveClassToExistingClassUnionMacroCommand</see>. 
        /// </summary>
        /// <param name="diagramController">command controller</param>
        public MoveClassToExistingClassUnionMacroCommand(DiagramController diagramController)
            : base(diagramController)
        {
            Description = CommandDescription.PSM_COMPONENTS_MOVED;
            AddedAssociations = new List<PSMAssociation>();
        }

        private List<PSMAssociation> AddedAssociations;
        
        [MandatoryArgument]
        public PSMClassUnion ClassUnion { get; private set; }

        public override bool CanExecute()
        {
            return ClassUnion.ParentAssociation != null && AddedAssociations.Count > 0 && PSMTree.AreComponentsOfCommonParent(AddedAssociations);
        }

        private readonly Dictionary<PSMSubordinateComponent, KeyValuePair<PSMSuperordinateComponent, int>> parents = 
            new Dictionary<PSMSubordinateComponent, KeyValuePair<PSMSuperordinateComponent, int>>();


        public void Set(PSMClassUnion classUnion, IEnumerable<PSMAssociation> associations)
        {
            AddedAssociations.AddRange(associations);
            ClassUnion = classUnion;

            ElementHolder<PSMAssociation> leadingAssociationHolder = new ElementHolder<PSMAssociation>(ClassUnion.ParentAssociation);
            ElementHolder<PSMClassUnion> unionHolder = new ElementHolder<PSMClassUnion>(ClassUnion);

            CopyNestingJoinsCommand c3 = CopyNestingJoinsCommandFactory.Factory().Create(Controller.ModelController) as CopyNestingJoinsCommand;
            c3.Set( leadingAssociationHolder, AddedAssociations);
            this.Commands.Add(c3);

            /*GetClassUnionContentCommand c4 = GetClassUnionContentCommandFactory.Factory().Create(Controller.ModelController) as GetClassUnionContentCommand;
            c4.Set(joinedAssociations, CreatedUnion);
            Commands.Add(c4);*/

            DeleteFromPSMDiagramCommand delCommand = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
            delCommand.DeletedElements = new List<Element>(AddedAssociations.Cast<Element>());
            delCommand.CheckOrdering = false;
            Commands.Add(delCommand);

            PutClassesToUnionCommand c4 = PutClassesToUnionCommandFactory.Factory().Create(Controller.ModelController) as PutClassesToUnionCommand;
            c4.Set(AddedAssociations, unionHolder);
            Commands.Add(c4);

            foreach (PSMAssociation assoc in AddedAssociations)
            {
                PSMClassUnion union = assoc.Child as PSMClassUnion;
                if (union != null)
                {
                    MoveClassUnionContentCommand moveCommand = MoveClassUnionContentCommandFactory.Factory().Create(Controller) as MoveClassUnionContentCommand;
                    moveCommand.Set(union, unionHolder);
                    Commands.Add(moveCommand);
                    DeleteFromPSMDiagramCommand delUnion = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
                    delUnion.DeletedElements = new List<Element>();
                    delUnion.DeletedElements.Add(union);
                    Commands.Add(delUnion);
                }
            }        
        }

        //internal override void CommandOperation()
        //{
        //    foreach (PSMSubordinateComponent psmSubordinateComponent in AddedAssociations)
        //    {
        //        parents[psmSubordinateComponent] = new KeyValuePair<PSMSuperordinateComponent, int>(psmSubordinateComponent.Parent, psmSubordinateComponent.ComponentIndex());
        //        parents[psmSubordinateComponent].Key.Components.Remove(psmSubordinateComponent);
        //        ClassUnion.Components.Add(psmSubordinateComponent);
        //    }
        //}

        //internal override OperationResult UndoOperation()
        //{
        //    foreach (PSMSubordinateComponent psmSubordinateComponent in AddedAssociations)
        //    {
        //        ClassUnion.Components.Remove(psmSubordinateComponent);
        //        parents[psmSubordinateComponent].Key.Components.Insert(parents[psmSubordinateComponent].Value, psmSubordinateComponent);
        //        Debug.Assert(parents[psmSubordinateComponent].Key == psmSubordinateComponent.Parent);
        //    }
        //    return OperationResult.OK;
        //}

        //public static bool CheckCommonParentClass(IEnumerable<PSMAssociation> components, out PSMClass parentClass)
        //{
        //    parentClass = null; 
        //    foreach (PSMSubordinateComponent component in components)
        //    {
        //        PSMSuperordinateComponent p = component.Parent;
                
        //        if (p is PSMClass)
        //        {
        //            if (parentClass == null)
        //            {
        //                parentClass = (PSMClass) p;
        //            }
        //            else
        //            {
        //                if (parentClass != p)
        //                {
        //                    parentClass = null;
        //                    return false; 
        //                }
        //            }
        //        }
        //        else
        //        {
        //            parentClass = null;
        //            return false; 
        //        }
        //    }

        //    return true; 
        //}
    }


	#region MoveClassToExistingClassUnionMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveClassToExistingClassUnionMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
    public class MoveClassToExistingClassUnionMacroCommandFactory : DiagramCommandFactory<MoveClassToExistingClassUnionMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveClassToExistingClassUnionMacroCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of <see cref="MoveClassToExistingClassUnionMacroCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveClassToExistingClassUnionMacroCommand (diagramController);
		}
	}

	#endregion
}