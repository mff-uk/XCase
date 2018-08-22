namespace XCase.Controller.Commands
{
	/// <summary>
	/// ModelCommandFactory is a base class for all factories that creates 
	/// model commands (subclasses of <see cref="ModelCommandBase"/>)
	/// </summary>
	/// <typeparam name="ConcreteCommandFactory">Type of concrete factory</typeparam>
	public abstract class ModelCommandFactory<ConcreteCommandFactory> 
		: CommandFactory<ConcreteCommandFactory, ModelController>
		where ConcreteCommandFactory : class
	{

	}
}