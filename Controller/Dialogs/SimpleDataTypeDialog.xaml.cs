using System.Windows;
using XCase.Model;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for SimpleDataTypeDialog.xaml
	/// </summary>
	public partial class SimpleDataTypeDialog
	{
		private readonly ModelController modelController;
		private SimpleDataType editedType;
		public SimpleDataType EditedType
		{
			get
			{
				return editedType;
			}
			set
			{
				editedType = value;
				if (value != null)
				{
					tbName.Text = EditedType.Name;
					tbXSD.Text = EditedType.DefaultXSDImplementation;
					cbParent.SelectedItem = EditedType.Parent;
					cbPackage.SelectedItem = EditedType.Package;
				}
			}
		}

		public SimpleDataTypeDialog(ModelController controller)
		{
			InitializeComponent();

			this.modelController = controller;

			cbParent.ItemsSource = modelController.Model.Schema.PrimitiveTypes;
			SubpackagesGetter subpackagesGetter = new SubpackagesGetter(controller.Model);
			cbPackage.ItemsSource = subpackagesGetter.GetSubpackages(null);
			cbPackage.SelectedIndex = 0;
			cbParent.SelectedIndex = 1;
			canvas5.Visibility = Visibility.Collapsed;
		}

		private void bOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void tbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			bOk.IsEnabled = !string.IsNullOrEmpty(tbName.Text);
		}

	}
}