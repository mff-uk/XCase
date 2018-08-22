using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Base class for all commands altering UML model. For commands altering visualization of 
	/// model in diagrams, see <see cref="DiagramCommandBase"/>
	/// </summary>
	/// <remarks>
	/// Beside logical separation there is an effective difference between model commands and 
	/// diagram commands - executed diagram commands are placed on a particular diagram undo/redo
	/// stack, executed model commands are placed on model undo/redo stack (and thus are accessible
	/// for all diagrams)
	/// </remarks>
	/// <seealso cref="DiagramCommandBase"/>
    public abstract class ModelCommandBase : StackedCommandBase<ModelController>
    {
		/// <summary>
		/// Creates new instance of ModelCommandBase.
		/// </summary>
		/// <param name="controller">model controller that will manage command execution</param>
    	protected ModelCommandBase(ModelController controller)
    		: base(controller)
    	{
    	}

		/// <summary>
		/// Returns reference to a model
		/// </summary>
    	protected Model.Model Model
        {
            get { return Controller.Model; }
        }
    }
}
