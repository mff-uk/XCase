using System;
using XCase.Model;
using XCase.Controller.Commands;
using NUml.Uml2;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM Association used to receive requests from View and create commands
    /// for changing the model accordingly
    /// </summary>
    public class PSM_AssociationController : ConnectionController
	{
		public PSMAssociation PSMAssociation
		{
			get
			{
				return (PSMAssociation)Element;
			}
		}

		public PSM_AssociationController(PSMAssociation element, DiagramController diagramController)
			: base(element, diagramController)
		{
		}

        /// <summary>
        /// Removes the element this controller controls
        /// </summary>
        public override void Remove()
		{
			DiagramController.RemovePSMSubtree(Element);
		}

		public void ChangeMultiplicity(string newCardinality)
		{
			uint? lower;
			UnlimitedNatural upper; 
			if (!MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper))
				return;
			ChangeMultiplicity(lower, upper);

		}

		private void ChangeMultiplicity(uint? lower, UnlimitedNatural upper)
		{
			MultiplicityElementController.ChangeMultiplicityOfElement(PSMAssociation, PSMAssociation, lower, upper, DiagramController.ModelController);
		}

		public void CutToRoot()
		{
			CutAssociationToRootCommand cutAssociationToRootCommand = (CutAssociationToRootCommand)CutAssociationToRootCommandFactory.Factory().Create(DiagramController);
			cutAssociationToRootCommand.PSMAssociation = PSMAssociation;
			cutAssociationToRootCommand.Execute();
		}

        public void MoveToContentContainer(PSMContentContainer contentContainer)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentContainer>.Factory().Create(DiagramController);
            c.InitializeCommand(new []{ PSMAssociation }, contentContainer);
            if (c.Commands.Count > 0)
                c.Execute();
        }

        public void MoveToContentChoice(PSMContentChoice contentChoice)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentChoice>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { PSMAssociation }, contentChoice);
            if (c.Commands.Count > 0)
                c.Execute();
        }

        public void MoveToClassUnion(PSMClassUnion classUnion)
        {
            MoveClassToClassUnionMacroCommand c = (MoveClassToClassUnionMacroCommand)MoveClassToClassUnionMacroCommandFactory.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { PSMAssociation }, classUnion);
            if (c.Commands.Count > 0)
                c.Execute();
        }
	}
}