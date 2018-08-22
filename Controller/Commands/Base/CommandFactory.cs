namespace XCase.Controller.Commands
{
	/// <summary>
	/// Abstract CommandFactory, base class for all factories creating single commands 
	/// (not macro commands). Subclasses can be derived directly from 
	/// <see cref="CommandFactory{ConcreteCommandFactory,ControllerType}"/>, but 
	/// using <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> or 
	/// <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> is more convenient. 
	/// </summary>
	/// <typeparam name="ConcreteCommandFactory">type of the concrete factory</typeparam>
	/// <typeparam name="ControllerType">type of the controller of created command</typeparam>
	/// <seealso cref="ModelCommandFactory{ConcreteCommandFactory}"/>
	/// <seealso cref="DiagramCommandFactory{ConcreteCommandFactory}"/>
	public abstract class CommandFactory<ConcreteCommandFactory, ControllerType>: 
		CommandFactoryBase<ConcreteCommandFactory>, ICommandFactory<ControllerType>
		where ConcreteCommandFactory : class 
		where ControllerType: CommandControllerBase
	{
		public abstract IStackedCommand Create(ControllerType controller);
	}
}