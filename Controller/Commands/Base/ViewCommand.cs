using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// This command is meant as a base class for those 
    /// Commands that does not affect the UML model, but should be undo/redo-able 
    /// - like moving a class, breaking a line etc.
    /// </summary>
    public abstract class ViewCommand: DiagramCommandBase
    {
		/// <summary>
		/// Creates new instance of <see cref="ViewCommand" />. 
		/// </summary>
		/// <param name="diagramController">command's controller</param>
		protected ViewCommand(DiagramController diagramController)
			: base(diagramController) 
		{
		}
    }
}
