namespace XCase.Controller.Commands
{
	/// <summary>
	/// DiagramCommandFactory is a base class for all factories that creates 
	/// diagram commands (subclasses of <see cref="DiagramCommandBase"/>)
	/// </summary>
	/// <typeparam name="ConcreteCommandFactory">Type of concrete factory</typeparam>
	public abstract class DiagramCommandFactory<ConcreteCommandFactory> 
		: CommandFactory<ConcreteCommandFactory, DiagramController>
		where ConcreteCommandFactory : class
	{
		
	}
}