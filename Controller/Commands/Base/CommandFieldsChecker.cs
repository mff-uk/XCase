using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller.Commands.Helpers;
using XCase.Controller.DynamicCompilation;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Class provides methods to check whether all command's
	/// Mandatory arguments (properties with <see cref="MandatoryArgumentAttribute"/> attribute) are properly initialized
	/// before executing the command and all command results (properties with <see cref="CommandResultAttribute"/> attribute)
	/// are set after executiing the command.
	/// </summary>
	public class CommandFieldsChecker
	{
		private readonly Dictionary<Type, Dictionary<String, GetHandler>> mandatoryArguments = new Dictionary<Type, Dictionary<string, GetHandler>>();

		private readonly Dictionary<Type, Dictionary<String, GetHandler>> commandResults = new Dictionary<Type, Dictionary<string, GetHandler>>();

		/// <summary>
		/// <see cref="Debug.Fail(string)">Fails</see> if some of the 
		/// mandatory arguments (properties with <see cref="MandatoryArgumentAttribute"/> attribute) have null value
		/// </summary>
		/// <param name="command">checked command</param>
		public void CheckMandatoryArguments(CommandBase command)
		{
			if (String.IsNullOrEmpty(command.Description))
			{
				Debug.WriteLine("WARNING: Command executed without description: " + command);
			}
			CheckAttributesNotNull(command.GetType(), command, mandatoryArguments, typeof(MandatoryArgumentAttribute), CommandError.CMDERR_MANDATORY_ARGUMENT_NOT_INITIALIZED_2, false);
		}

		/// <summary>
		/// <see cref="Debug.Fail(string)">Fails</see> if some of the 
		/// command results (properties with <see cref="CommandResultAttribute"/> attribute) have null value
		/// </summary>
		/// <param name="command">checked command</param>
		public void CheckCommandResults(CommandBase command)
		{
			CheckAttributesNotNull(command.GetType(), command, commandResults, typeof(CommandResultAttribute), CommandError.CMDERR_RESULT_ARGUMENT_NULL, true);
		}

		private static void CheckAttributesNotNull(Type type, CommandBase command, IDictionary<Type, Dictionary<string, GetHandler>> getterDictionary, Type attributeType, string errorMsg, bool holderValues)
		{
			if (!getterDictionary.ContainsKey(type))
			{
				getterDictionary[type] = new Dictionary<String, GetHandler>();
				foreach (PropertyInfo property in type.GetProperties())
				{
					if (property.GetCustomAttributes(attributeType, true).Length > 0)
					{
						getterDictionary[type][property.Name] = DynamicMethodCompiler.CreateGetHandler(type, property);
					}
				}
			}

			foreach (KeyValuePair<string, GetHandler> getHandler in getterDictionary[type])
			{
				object value = getHandler.Value(command);
				if (value == null)
				{
					Debug.Fail(String.Format(errorMsg, command, getHandler.Key));
				}
				if (holderValues)
				{
					IHolder holder = value as IHolder;
					if (holder != null)
					{
						if (holder.WrappedValue == null)
                            Debug.Fail(String.Format(errorMsg, command, getHandler.Key));
					}
				}
			}
		}
	}
}