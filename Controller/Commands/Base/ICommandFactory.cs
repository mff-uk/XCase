namespace XCase.Controller.Commands
{
	/// <summary>
	/// ICommandFactory interface allows the caller to create command using
	/// a factory without knowing the type of the factory (this is useful when 
	/// the caller has a reference to some base factory but does not know the concrete
	/// type of the factory)
	/// </summary>
	/// <typeparam name="ControllerType"></typeparam>
	public interface ICommandFactory<ControllerType>
	{
		/// <summary>
		/// Creates new command (type of the command depends on the type of the factory)
		/// </summary>
		/// <param name="controller">controller that will be assigned to created command</param>
		/// <returns>new command created by the factory</returns>
		IStackedCommand Create(ControllerType controller);
	}
}