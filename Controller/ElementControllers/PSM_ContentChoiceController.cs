using System;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM Content Choice used to receive requests from View and create commands
    /// for changing the model accordingly.
    /// </summary>
    public class PSM_ContentChoiceController : NamedElementController
    {
		public PSMContentChoice ContentChoice { get { return (PSMContentChoice)NamedElement; } }

        public PSM_ContentChoiceController(PSMContentChoice contentChoice, DiagramController diagramController) :
            base(contentChoice, diagramController)
        {
        }

        /// <summary>
        /// Removes the element this controller controls
        /// </summary>
        public override void Remove()
        {
			DiagramController.RemovePSMSubtree(Element);
        }

        public void MoveToContentContainer(PSMContentContainer contentContainer)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentContainer>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentContainer>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { ContentChoice }, contentContainer);
            if (c.Commands.Count > 0)
                c.Execute();
        }

        public void MoveToContentChoice(PSMContentChoice contentChoice)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentChoice>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { ContentChoice }, contentChoice);
            if (c.Commands.Count > 0)
                c.Execute();
        }
    }
}