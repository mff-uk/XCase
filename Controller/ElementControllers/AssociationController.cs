using System;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Controller.Commands.CommandsAssociations;
using XCase.Model;
using Association=XCase.Model.Association;

namespace XCase.Controller
{
	/// <summary>
	/// This is a Controller for a Association used to receive requests from View and create commands
    /// for changing the model accordingly
	/// </summary>
    public class AssociationController : ConnectionController
	{
		public Association Association { get { return (Association)Element; } }

		public AssociationController(Association association, DiagramController diagramController) :
			base(association, diagramController)
		{

		}

		public void RenameElement(string newName)
		{
			RenameElementCommand<Association> command = (RenameElementCommand<Association>)RenameElementCommandFactory<Association>.Factory().Create(DiagramController.ModelController);
			command.RenamedElement = Association;
			command.NewName = newName;
			command.Execute();
		}

		public void ChangeMultiplicity(AssociationEnd end, string newCardinality)
		{
			uint? lower;
			UnlimitedNatural upper;
			if (!MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper))
				return;
			MultiplicityElementController.ChangeMultiplicityOfElement(end, Association, lower, upper, DiagramController.ModelController);
		}

		public void ChangeRole(AssociationEnd end, string role)
		{
			RenameElementCommand<AssociationEnd> command = (RenameElementCommand<AssociationEnd>)RenameElementCommandFactory<AssociationEnd>.Factory().Create(DiagramController.ModelController);
			command.NewName = role;
			command.RenamedElement = end;
			command.AssociatedElements.Add(Association);
			command.Execute();
		}

		public void ChangeAggregation(AssociationEnd end, AggregationKind newAggregation)
		{
			ChangeAssociationAggregationCommand associationAggregationCommand =
				(ChangeAssociationAggregationCommand)ChangeAssociationAggregationCommandFactory.Factory().Create(DiagramController.ModelController);
			associationAggregationCommand.AssociationEnd = end;
			associationAggregationCommand.NewAggregationKind = newAggregation;
			associationAggregationCommand.Execute();
		}

		public void ShowAssociationDialog()
		{
			var associationDialog = new Dialogs.AssociationDialog(this, DiagramController.ModelController);
			associationDialog.ShowDialog();
		}


		public void RemoveAssociationEnd(AssociationEnd end)
		{
			RemoveAssociationEndCommmand removeAssociationEndCommmand =
				(RemoveAssociationEndCommmand) RemoveAssociationEndCommmandFactory.Factory().Create(DiagramController.ModelController);
			removeAssociationEndCommmand.AssociationEnd = end; 
			removeAssociationEndCommmand.Execute();
		}

		public void AddAssociationEnd(Association association, PIMClass newClass)
		{
			AddAnotherAssociationEndCommand addAnotherAssociationEndCommand =
				(AddAnotherAssociationEndCommand)AddAnotherAssociationEndCommandFactory.Factory().Create(DiagramController.ModelController);
			addAnotherAssociationEndCommand.Association = association;
			addAnotherAssociationEndCommand.Class = newClass;
			addAnotherAssociationEndCommand.Execute();
		}
	}
}
