using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Gui.Dialogs
{
	/// <summary>
	/// Interaction logic for PropertyDialog.xaml
	/// </summary>
	public partial class OperationDialog : Window
	{
		public OperationDialog(Model.Operation operation, ModelController controller)
		{
			InitializeComponent();
			
			this.controller = controller;
			this.operation = operation;
			this.Title = string.Format("Property: {0}.{1}", operation.Class.Name, operation.Name);

			oldName = operation.Name;

			tbName.Text = operation.Name;
		}

		private Model.Operation operation;
		private string oldName;
		private ModelController controller;

		private void bOk_Click(object sender, RoutedEventArgs e)
		{
			MacroCommand<ModelController> command = MacroCommandFactory<ModelController>.Factory().Create(controller);
			command.Description = CommandDescription.UPDATE_OPERATION_MACRO;

			if (tbName.Text != oldName)
			{
				RenameElementCommand<Operation> renameElementCommand = (RenameElementCommand<Operation>)RenameElementCommandFactory<Operation>.Factory().Create(controller);
				renameElementCommand.NewName = tbName.Text;
				renameElementCommand.RenamedElement = operation;
				renameElementCommand.ContainingCollection = operation.Class.Operations;
				command.Commands.Add(renameElementCommand);
			}		

			if (command.Commands.Count > 0)
				command.Execute();

			DialogResult = true;

			Close();
		}
	}
}
