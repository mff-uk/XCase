using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NUml.Uml2;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using DataType = XCase.Model.DataType;
using Property = XCase.Model.Property;
using XCase.Controller.Interfaces;
using Package=XCase.Model.Package;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for AttributeDialog.xaml
	/// </summary>
	public partial class AttributeDialog
	{
		private readonly Property attribute;

		private readonly IControlsAttributes pimClassController;

		private readonly IControlsPSMAttributes psmClassController;

		private readonly ModelController modelController;

		private readonly List<Object> itemsSource;

		private AttributeDialog(Property attribute, ModelController controller)
		{
			InitializeComponent();

			this.modelController = controller;
			this.attribute = attribute;
			this.Title = string.Format("Attribute: {0}.{1}", attribute.Class.Name, attribute.Name);

			tbName.Text = attribute.Name;
			tbDefaultValue.Text = attribute.Default;
			tbLower.Text = attribute.Lower.ToString();
			tbUpper.Text = attribute.Upper.ToString();
			cbDataType.Text = (attribute.Type != null) ? attribute.Type.ToString() : string.Empty;
			oldType = attribute.Type;
			itemsSource = new List<Object>(attribute.Class.Package.AllTypes.Cast<object>());
			cbDataType.ItemsSource = itemsSource;
			if (attribute.Type != null)
				cbDataType.SelectedIndex = cbDataType.Items.IndexOf(attribute.Type);

			SimpleDataType simpleDataType = attribute.Type as SimpleDataType;
			if (simpleDataType != null)
			{
				PrimitiveTypeName = simpleDataType.Name;
				PrimitiveTypePackage = simpleDataType.Package;
				PrimitiveTypeParent = simpleDataType.Parent;
				PrimitiveTypeXSD = simpleDataType.DefaultXSDImplementation;
			}
			this.psmOnly = new UIElement[] { cvAlias };
			this.psmDisabled = new Control[] { tbName, cbDataType };
			this.psmHidden = new UIElement[] { cvSimpleType };
		}

		public AttributeDialog(Property attribute, IControlsAttributes pimClassController, ModelController controller)
			: this(attribute, controller)
		{
			this.pimClassController = pimClassController;

			foreach (UIElement element in psmOnly)
			{
				element.Visibility = Visibility.Collapsed;
			}
            tbOnto.Text = attribute.OntologyEquivalent;
        }

		public AttributeDialog(PSMAttribute attribute, IControlsPSMAttributes psmClassController, ModelController controller)
			: this(attribute, controller)
		{
			this.psmClassController = psmClassController;

			if (attribute.RepresentedAttribute != null) //pimless attribute
			{
				foreach (Control control in psmDisabled)
				{
					control.IsEnabled = false;
				}

				foreach (UIElement control in psmHidden)
				{
					control.Visibility = Visibility.Collapsed;
				}
			}

			tbAlias.Text = attribute.Alias;
		}

		private readonly DataType oldType;
		private readonly UIElement[] psmOnly;
		private readonly Control[] psmDisabled;
		private readonly UIElement[] psmHidden;

		private void bOk_Click(object sender, RoutedEventArgs e)
		{
			#region parse multiplicity

			uint? lower;
			UnlimitedNatural upper;
			try
			{
				lower = MultiplicityElementController.ParseNullabelUint(tbLower.Text);
				upper = MultiplicityElementController.ParseUnlimitedNatural(tbUpper.Text);
			}
			catch (FormatException)
			{
				CommandCantExecuteDialog dialog = new CommandCantExecuteDialog();
				dialog.tbCommand.Content = "Wrong multiplicity format";
				dialog.tbExMsg.Content = String.Format("{0}..{1} is not a correct format for multiplicity.", tbLower.Text, tbUpper.Text);
				dialog.ShowDialog();
				return;
			}

			#endregion

			modelController.BeginMacro();
			modelController.CreatedMacro.Description = CommandDescription.UPDATE_PROPERTY_MACRO;

			if (pimClassController != null)
			{
				if (tbName.ValueChanged)
				{
					NamedElementController.RenameElement(attribute, tbName.Text, modelController, attribute.Class.Attributes);
				}
                if (tbOnto.ValueChanged)
                {
                    NamedElementController.ChangeOntologyEquivalent(attribute, tbOnto.Text, modelController);
                }

				if (tbDefaultValue.ValueChanged)
				{
					pimClassController.ChangeAttributeDefaultValue(attribute, tbDefaultValue.Text);
				}

				if (tbLower.ValueChanged || tbUpper.ValueChanged)
				{
					MultiplicityElementController.ChangeMultiplicityOfElement(attribute, attribute.Class, lower, upper, modelController);
				}

				#region change attribute type (create new simple type if required)

				if (cbDataType.SelectedItem != null && cbDataType.SelectedItem is DataType)
				{
					SimpleDataType t = cbDataType.SelectedItem as SimpleDataType;
					if (t != null && editedSimpleType)
					{
						modelController.AlterSimpleType(t, PrimitiveTypeName, PrimitiveTypeXSD);						
					}
					if (!(cbDataType.SelectedItem.Equals(oldType)))
						pimClassController.ChangeAttributeType(attribute, new ElementHolder<DataType>((DataType)cbDataType.SelectedItem));
				}
				else
				{
					if (newSimpleType)
					{
						ElementHolder<DataType> type = new ElementHolder<DataType>();
						modelController.CreateSimpleType(PrimitiveTypeName, PrimitiveTypePackage, PrimitiveTypeParent, PrimitiveTypeXSD, type);
						pimClassController.ChangeAttributeType(attribute, type);
					}
					else if (oldType != null)
					{
						pimClassController.ChangeAttributeType(attribute, new ElementHolder<DataType>());
					}
				}

				#endregion

			}
			if (psmClassController != null)
			{
				PSMAttribute psmAttribute = (PSMAttribute)attribute;
				
				if (tbName.ValueChanged)
				{
					NamedElementController.RenameElement(psmAttribute, tbName.Text, modelController, attribute.Class.Attributes);
				}

				#region change attribute type (create new simple type if required)

				if (cbDataType.SelectedItem != null && cbDataType.SelectedItem is DataType)
				{
					SimpleDataType t = cbDataType.SelectedItem as SimpleDataType;
					if (t != null && editedSimpleType)
					{
						modelController.AlterSimpleType(t, PrimitiveTypeName, PrimitiveTypeXSD);
					}
					if (!(cbDataType.SelectedItem.Equals(oldType)))
						psmClassController.ChangeAttributeType(psmAttribute, new ElementHolder<DataType>((DataType)cbDataType.SelectedItem));
				}
				else
				{
					if (newSimpleType)
					{
						ElementHolder<DataType> type = new ElementHolder<DataType>();
						modelController.CreateSimpleType(PrimitiveTypeName, PrimitiveTypePackage, PrimitiveTypeParent, PrimitiveTypeXSD, type);
						psmClassController.ChangeAttributeType(psmAttribute, type);
					}
					else if (oldType != null)
					{
						psmClassController.ChangeAttributeType(psmAttribute, new ElementHolder<DataType>());
					}
				}

				#endregion

				if (tbDefaultValue.ValueChanged)
				{
					psmClassController.ChangeAttributeDefaultValue(psmAttribute, tbDefaultValue.Text);
				}

				if (tbLower.ValueChanged || tbUpper.ValueChanged)
				{
					MultiplicityElementController.ChangeMultiplicityOfElement(psmAttribute, attribute.Class, lower, upper, modelController);
				}

				if (tbAlias.ValueChanged)
				{
					psmClassController.ChangeAttributeAlias(psmAttribute, tbAlias.Text);
				}
			}
			modelController.CommitMacro();
			DialogResult = true;
			Close();
		}

		private string PrimitiveTypeName;
		private SimpleDataType PrimitiveTypeParent;
		private string PrimitiveTypeXSD;
		private Package PrimitiveTypePackage;
		private bool newSimpleType;
		private bool editedSimpleType;
		


		private void bSimpleDataTypeNew_Click(object sender, RoutedEventArgs e)
		{
			SimpleDataTypeDialog subdialog;
			if (pimClassController != null)
			{
				subdialog = new SimpleDataTypeDialog(modelController);
			}
			else if (psmClassController != null)
			{
				subdialog = new SimpleDataTypeDialog(modelController);
			}
			else
			{
				return;
			}
			
			if (subdialog.ShowDialog() == true)
			{
				newSimpleType = true;
				PrimitiveTypeName = subdialog.tbName.Text;
				PrimitiveTypeParent = (SimpleDataType)subdialog.cbParent.SelectedItem;
				PrimitiveTypeXSD = subdialog.tbXSD.Text;
				PrimitiveTypePackage = (Package)subdialog.cbPackage.SelectedItem;
				itemsSource.Clear();
				itemsSource.AddRange(attribute.Class.Package.AllTypes.Cast<object>());
				itemsSource.Add(PrimitiveTypeName);
				cbDataType.SelectedItem = PrimitiveTypeName;
			}
		}

		private void bSimpleDataTypeEdit_Click(object sender, RoutedEventArgs e)
		{
			SimpleDataTypeDialog subdialog;
			if (pimClassController != null)
			{
				subdialog = new SimpleDataTypeDialog(modelController);
			}
			else if (psmClassController != null)
			{
				subdialog = new SimpleDataTypeDialog(modelController);
			}
			else
			{
				return;
			}

			if (cbDataType.SelectedItem is string)
			{
				subdialog.tbName.Text = PrimitiveTypeName;
				subdialog.cbParent.SelectedItem = PrimitiveTypeParent;
				subdialog.tbXSD.Text = PrimitiveTypeXSD;
				subdialog.cbPackage.SelectedItem = PrimitiveTypePackage;
			}
			else
			{
				subdialog.EditedType = (SimpleDataType)cbDataType.SelectedItem;
			}
			subdialog.cbParent.IsEnabled = false;
			subdialog.cbPackage.IsEnabled = false; 
			if (subdialog.ShowDialog() == true)
			{
				editedSimpleType = true; 
				PrimitiveTypeName = subdialog.tbName.Text;
				PrimitiveTypeParent = (SimpleDataType)subdialog.cbParent.SelectedItem;
				PrimitiveTypeXSD = subdialog.tbXSD.Text;
				PrimitiveTypePackage = (Package)subdialog.cbPackage.SelectedItem;
				if (subdialog.EditedType == null)
				{
					itemsSource.Clear();
					itemsSource.AddRange(attribute.Class.Package.AllTypes.Cast<object>());
					itemsSource.Add(PrimitiveTypeName);
					cbDataType.SelectedItem = PrimitiveTypeName;
				}
				else
				{
                    itemsSource.Clear();
                    itemsSource.AddRange(attribute.Class.Package.AllTypes.Cast<object>());
					cbDataType.SelectedItem = subdialog.EditedType;
					if (cbDataType.SelectedItem.ToString() != PrimitiveTypeName)
					{
						//cbDataType.IsEditable = true;
						//cbDataType.Text = PrimitiveTypeName;
					}
				}
					
 			}
		}

		private void cbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SimpleDataType t = cbDataType.SelectedItem as SimpleDataType;
			if (t != null && t.Parent != null)
			{
				bSimpleDataTypeEdit.IsEnabled = true;
			}
			else
			{
				if (cbDataType.SelectedItem is string)
					bSimpleDataTypeEdit.IsEnabled = true; 
				else
					bSimpleDataTypeEdit.IsEnabled = false; 
			}
			if (!(cbDataType.SelectedItem is string))
				newSimpleType = false;
			//editedSimpleType = false; 
		}
	}
}