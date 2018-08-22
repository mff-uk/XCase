using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class PackageDialog : Window
	{
		public PackageDialog(Model.Package package, ModelController controller)
		{
			InitializeComponent();
			
			this.controller = controller;
			this.package = package;
			this.Title = string.Format("Package: {0}", package.QualifiedName); 

			oldName = package.Name;
			tbName.Text = package.Name;

            SubpackagesGetter subpackagesGetter = new SubpackagesGetter(controller.Model);
            Collection<Package> packages = subpackagesGetter.GetSubpackages(package);
            cbPackages.ItemsSource = packages;
            cbPackages.SelectedItem = package.NestingPackage;
		}

		private Model.Package package;
		private string oldName;
		private ModelController controller;

		private void bOk_Click(object sender, RoutedEventArgs e)
		{
			MacroCommand<ModelController> command = MacroCommandFactory<ModelController>.Factory().Create(controller);
			command.Description = CommandDescription.UPDATE_PACKAGE_MACRO;

			if (tbName.Text != oldName)
			{
				RenameElementCommand<Package> renameElementCommand = (RenameElementCommand<Package>)RenameElementCommandFactory<Package>.Factory().Create(controller);
				renameElementCommand.NewName = tbName.Text;
				renameElementCommand.RenamedElement = package;
				renameElementCommand.ContainingCollection = package.NestingPackage.NestedPackages;
				command.Commands.Add(renameElementCommand);
			}

            if (cbPackages.SelectedItem != package.NestingPackage)
            {
                MovePackageCommand movePackageCommand = (MovePackageCommand)MovePackageCommandFactory.Factory().Create(controller);
                movePackageCommand.OldPackage = package.NestingPackage;
                movePackageCommand.NewPackage = (Package)cbPackages.SelectedItem;
                movePackageCommand.MovedPackage = package;
                command.Commands.Add(movePackageCommand);
            }

			if (command.Commands.Count > 0)
				command.Execute();

			DialogResult = true;

			Close();
		}
	}
}
