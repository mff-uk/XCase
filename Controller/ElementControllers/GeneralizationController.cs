using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a Generalization used to receive requests from View and create commands
    /// for changing the model accordingly
    /// </summary>
    public class GeneralizationController : ConnectionController
	{
		public Generalization Generalization { get { return (Generalization)Element; } }

		public GeneralizationController(Generalization generalization, DiagramController diagramController)
			: base(generalization, diagramController)
		{
		
		}
	}
}