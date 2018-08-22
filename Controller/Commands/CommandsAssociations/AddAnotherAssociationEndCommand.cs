using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands.CommandsAssociations
{
	/// <summary>
	/// Add another association end to an existing association.
	/// </summary>
	public class AddAnotherAssociationEndCommand: ModelCommandBase 
	{
		/// <summary>
		/// Association in which new end is added
		/// </summary>
		[MandatoryArgument]
		public Association Association { get; set; }

		/// <summary>
		/// Class that is added to the association
		/// </summary>
		[MandatoryArgument]
		public PIMClass Class { get; set; }

		/// <summary>
		/// Created association end
		/// </summary>
		[CommandResult]
		public AssociationEnd AssociationEnd { get; private set; }

		/// <summary>
		/// Creates new instance of ModelCommandBase.
		/// </summary>
		/// <param name="controller">model controller that will manage command execution</param>
		public AddAnotherAssociationEndCommand(ModelController controller) : 
			base(controller)
		{
			Description = CommandDescription.ADD_ASSOCIATION_END;
		}

		/// <summary>
		/// Returns true if command can be executed.
		/// </summary>
		/// <returns>True if command can be executed</returns>
		public override bool CanExecute()
		{
			return true; 
		}

		private readonly List<KeyValuePair<Diagram, Element>> addedClasses = new List<KeyValuePair<Diagram, Element>>();

		/// <summary>
		/// Executive function of a command
		/// </summary>
		/// <seealso cref="UndoOperation"/>
		internal override void CommandOperation()
		{
			foreach (Diagram diagram in Controller.Project.Diagrams)
			{
				if (diagram.DiagramElements.ContainsKey(Association))
				{
					if (!diagram.DiagramElements.ContainsKey(Class))
					{
						diagram.AddModelElement(Class, new ClassViewHelper(diagram));
						addedClasses.Add(new KeyValuePair<Diagram, Element>(diagram, Class));
					}
				}
			}

			AssociationEnd = Association.CreateEnd(Class);
			AssociatedElements.Add(Association); 
		}

		/// <summary>
		/// Undo executive function of a command. Should revert the <see cref="CommandOperation"/> executive 
		/// function and return the state to the state before CommandOperation was execute.
		/// <returns>returns <see cref="CommandBase.OperationResult.OK"/> if operation succeeded, <see cref="CommandBase.OperationResult.Failed"/> otherwise</returns>
		/// </summary>
		/// <remarks>
		/// <para>If  <see cref="CommandBase.OperationResult.Failed"/> is returned, whole undo stack is invalidated</para>
		/// </remarks>
		internal override OperationResult UndoOperation()
		{
			foreach (KeyValuePair<Diagram, Element> keyValuePair in addedClasses)
			{
				keyValuePair.Key.RemoveModelElement(keyValuePair.Value);
			}

			if (Association.Ends.Contains(AssociationEnd))
			{
				Association.Ends.Remove(AssociationEnd);
			}
			return OperationResult.OK;
		}
	}

	#region AddAnotherAssociationEndCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AddAnotherAssociationEndCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class AddAnotherAssociationEndCommandFactory : ModelCommandFactory<AddAnotherAssociationEndCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AddAnotherAssociationEndCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of AddAnotherAssociationEndCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new AddAnotherAssociationEndCommand(modelController);
		}
	}

	#endregion
}
