using System;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Abstract CommandFactory. 
    /// Offers reference to singleton <typeparamref name="ConcreteCommandFactory"/>.
    /// </summary>
    /// <typeparam name="ConcreteCommandFactory">Type of the concrete factory</typeparam>
    public abstract class CommandFactoryBase<ConcreteCommandFactory> 
                                            where ConcreteCommandFactory : class
    {
        private static ConcreteCommandFactory instance = default(ConcreteCommandFactory);

		/// <summary>
		/// Returns singleton instance of the factory
		/// </summary>
		/// <returns></returns>
        public static ConcreteCommandFactory Factory()
        {
            if (instance == null)
                instance = (ConcreteCommandFactory)Activator.CreateInstance(typeof(ConcreteCommandFactory), true);
            return instance;
        } 
    }
}