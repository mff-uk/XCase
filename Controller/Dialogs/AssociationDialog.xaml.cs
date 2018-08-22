using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Model;
using Association=XCase.Model.Association;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for AssociationDialog.xaml
	/// </summary>
	public partial class AssociationDialog
	{
		private List<AssociationEndEditor> endEditors;

		public AssociationDialog(AssociationController associationController, ModelController controller)
		{
			InitializeComponent();
			
			this.modelController = controller;
			this.associationController = associationController;
			this.Title = string.Format(this.Title, this.Association);

			tbName.Text = Association.Name;
            tbOnto.Text = Association.OntologyEquivalent;

			BindEnds();
		}

		private void BindEnds()
		{
			endEditors = new List<AssociationEndEditor>();
			foreach (AssociationEnd associationEnd in Association.Ends)
			{
				TabItem item = new TabItem { Header = associationEnd.ToString() };
				AssociationEndEditor endEditor = new AssociationEndEditor(associationEnd);
				item.Content = new Grid();
				endEditor.Width = double.NaN;
				endEditor.Height = double.NaN;
				(item.Content as Grid).Children.Add(endEditor);
				tabControl1.Items.Add(item);
				endEditors.Add(endEditor);
			}
		}

		private Association Association
		{
			get 
			{ 
				return associationController.Association; 
			}
		}
		
		private readonly AssociationController associationController;
		private readonly ModelController modelController;

		private void bOk_Click(object sender, RoutedEventArgs e)
		{
			modelController.BeginMacro();
			modelController.CreatedMacro.Description = CommandDescription.UPDATE_ASSOCIATION_MACRO;

			if (tbName.ValueChanged)
			{
				associationController.RenameElement(tbName.Text);
			}
            if (tbOnto.ValueChanged)
            {
                NamedElementController.ChangeOntologyEquivalent(Association, tbOnto.Text, modelController);
            }

			foreach (AssociationEndEditor endEditor in endEditors)
			{
				if (endEditor.tbRole.ValueChanged)
				{
					NamedElementController.RenameElement(endEditor.AssociationEnd, endEditor.tbRole.Text, modelController, null);
				}

				if (endEditor.tbLower.ValueChanged ||
					endEditor.tbUpper.ValueChanged)
				{
					uint? lower;
					UnlimitedNatural upper;
					try
					{
						lower = MultiplicityElementController.ParseNullabelUint(endEditor.tbLower.Text);
						upper = MultiplicityElementController.ParseUnlimitedNatural(endEditor.tbUpper.Text);
					}
					catch (FormatException)
					{
						CommandCantExecuteDialog dialog = new CommandCantExecuteDialog();
						dialog.tbCommand.Content = "Wrong multiplicity format";
						dialog.tbExMsg.Content = String.Format("{0}..{1} is not a correct format for multiplicity.", endEditor.tbLower.Text, endEditor.tbUpper.Text);
						dialog.ShowDialog();
						return;
					}
					MultiplicityElementController.ChangeMultiplicityOfElement(endEditor.AssociationEnd, Association, lower, upper, modelController);
				}

				if (endEditor.cbType.SelectedIndex != endEditor.oldKindIndex)
				{
					AggregationKind aggregationKind = AggregationKind.none;
					switch (endEditor.cbType.SelectedIndex)
					{
						case 0:
							aggregationKind = AggregationKind.none;
							break;
						case 1:
							aggregationKind = AggregationKind.shared;
							break;
						case 2:
							aggregationKind = AggregationKind.composite;
							break;
					}

					associationController.ChangeAggregation(endEditor.AssociationEnd, aggregationKind);
				}
			}

			modelController.CommitMacro();
			DialogResult = true;
			Close();
		}
	}
}