using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Base class for all commands altering visualization of 
	/// model in diagrams. For commands altering UML model, see <see cref="ModelCommandBase"/>
	/// </summary>
	/// <remarks>
	/// Beside logical separation there is an effective difference between model commands and 
	/// diagram commands - executed diagram commands are placed on a particular diagram undo/redo
	/// stack, executed model commands are placed on model undo/redo stack (and thus are accessible
	/// for all diagrams)
	/// </remarks>
	/// <seealso cref="ModelCommandBase"/>
    public abstract class DiagramCommandBase : StackedCommandBase<DiagramController>
    {
		/// <summary>
		/// Creates new instance of DiagramCommandBase.
		/// </summary>
		/// <param name="controller">diagram controller that will manage command execution</param>
		protected DiagramCommandBase(DiagramController controller)
			: base(controller)
		{
		}
        
		/// <summary>
		/// Returns reference to <see cref="DiagramController"/>'s Diagram
		/// </summary>
		protected Diagram Diagram
		{
			get { return Controller.Diagram; }
		}
    }
}
