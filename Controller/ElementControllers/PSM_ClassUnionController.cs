using XCase.Model;
using XCase.Controller.Commands;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a PSM Class Union used to receive requests from View and create commands
    /// for changing the model accordingly.
    /// </summary>
    public class PSM_ClassUnionController : NamedElementController
    {
		public PSMClassUnion ClassUnion { get { return (PSMClassUnion)NamedElement;  } }

        public PSM_ClassUnionController(PSMClassUnion classUnion, DiagramController diagramController) :
            base(classUnion, diagramController)
        {
        }

        /// <summary>
        /// Removes the element this controller controls
        /// </summary>
        public override void Remove()
        {
			DiagramController.RemovePSMSubtree(Element);
        }
    }
}