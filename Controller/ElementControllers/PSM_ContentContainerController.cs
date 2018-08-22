using System;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM Content Container used to receive requests from View and create commands
    /// for changing the model accordingly.
    /// </summary>
    public class PSM_ContentContainerController : NamedElementController
    {
		public PSMContentContainer ContentContainer { get { return (PSMContentContainer)NamedElement; } }

        public PSM_ContentContainerController(PSMContentContainer contentContainer, DiagramController diagramController) :
            base(contentContainer, diagramController)
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
            c.InitializeCommand(new[] { ContentContainer }, contentContainer);
            if (c.Commands.Count > 0)
                c.Execute();
        }

        public void MoveToContentChoice(PSMContentChoice contentChoice)
        {
            MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice> c = (MoveSubordinateToSuperordinateMacroCommand<PSMContentChoice>)MoveSubordinateToSuperordinateMacroCommandFactory<PSMContentChoice>.Factory().Create(DiagramController);
            c.InitializeCommand(new[] { ContentContainer }, contentChoice);
            if (c.Commands.Count > 0)
                c.Execute();
        }
    }
}