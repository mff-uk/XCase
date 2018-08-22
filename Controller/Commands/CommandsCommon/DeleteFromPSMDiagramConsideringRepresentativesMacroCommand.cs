using System;
using System.Collections.Generic;
using System.Linq;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Deletes elements from PSM diagram and repaires structural representatives. Uses 
	/// <see cref="DeleteFromPSMDiagramCommand"/> for deleting and all structural representatives,
	/// whose represented classes were deleted, are changed to regular classes. 
	/// </summary>
	public class DeleteFromPSMDiagramConsideringRepresentativesMacroCommand: MacroCommand<DiagramController>
	{
		public DeleteFromPSMDiagramConsideringRepresentativesMacroCommand(DiagramController controller) : base(controller)
		{
			Description = CommandDescription.REMOVE_FROM_DIAGRAM;
		}

		/// <summary>
		/// If set to true, the deleted elements are deleted immediately with
		/// their dependent elements and no dialog window is shown. 
		/// </summary>
		public bool ForceDelete { get; set; }

		/// <summary>
		/// Tells the command which elements to delete. The method uses
		/// interactive dialogs to specify the initial set of deleted elements and
		/// to ask whether to delete unused elements from the model.
		/// </summary>
		/// <param name="deletedElements">set of deleted elements</param>
		/// <param name="selectionCallback">function that is called, when selected elements are specified in the dialog. Can be set to null.</param>
		/// <returns><code>true</code> when user pressed OK, <code>false</code> when user pressed Cancel in the</returns>
		public bool InitializeCommand(Action<IEnumerable<Element>> selectionCallback, IEnumerable<Element> deletedElements)
		{
			DeleteFromPSMDiagramCommand command = (DeleteFromPSMDiagramCommand)DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller);
			command.ForceDelete = ForceDelete;
			if (!command.InitializeCommand(selectionCallback, deletedElements))
				return false;

		    List<PSMClass> referencedPSMClasses = new List<PSMClass>();


			foreach (PSMClass psmClass in command.DeletedElements.OfType<PSMClass>())
			{
				foreach (PSMClass referencing in GetReferencings(psmClass))
				{
					SetRepresentedPSMClassCommand convertCommand =
						(SetRepresentedPSMClassCommand)SetRepresentedPSMClassCommandFactory.Factory().Create(Controller);
					convertCommand.Set(null, referencing);
					Commands.Add(convertCommand);
		            if (!command.DeletedElements.Contains(referencing))
                        referencedPSMClasses.AddIfNotContained(psmClass);
				}
			}

		    if (referencedPSMClasses.Count > 0)
		    {
		        XCase.Controller.Dialogs.OkCancelDialog d = new XCase.Controller.Dialogs.OkCancelDialog();
		        d.PrimaryContent = "Existing structural representatives";
		        d.SecondaryContent = "Following clases are referenced from structural representatives: " +
                                     referencedPSMClasses.ConcatWithSeparator(", ") + ". \r\n\r\nContinue?";
                if (d.ShowDialog() != true)
                    return false; 
		    }

			Commands.Add(command);

			return true; 
		}

		private IEnumerable<PSMClass> GetReferencings(PSMClass psmClass)
		{
			return Controller.Diagram.DiagramElements.Keys.OfType<PSMClass>().Where((rClass => rClass.RepresentedPSMClass == psmClass));
		}
	}

	#region DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="DeleteFromPSMDiagramConsideringRepresentativesMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory : DiagramCommandFactory<DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of DeleteFromPSMDiagramConsideringRepresentativesMacroCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new DeleteFromPSMDiagramConsideringRepresentativesMacroCommand(diagramController);
		}
	}

	#endregion
}