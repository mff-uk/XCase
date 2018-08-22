using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using XCase.Model;
using System.Linq;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for SelectAttributesDialog.xaml
	/// </summary>
	public partial class SelectAttributesDialog
	{
		public string MessageText
		{
			get { return lbMessage.Content.ToString(); }
			set { lbMessage.Content = value; }
		}

		private IHasPSMAttributes element;
		public IHasPSMAttributes Element
		{
			get { return element; }
			set
			{
				element = value;
				List<Property> tmp = new List<Property>(element.PSMAttributes.Cast<Property>());
				if (!PSMElementsOnly)
				{
					foreach (Property property in element.RepresentedClass.AllAttributes)
					{
						if (!element.PSMAttributes.Any(attr => attr.RepresentedAttribute == property))
						{
							tmp.Add(property);
						}
					}
				}
				Items = tmp;
			}
		}

		private List<Property> items;

		private List<Property> Items
		{
			get { return items; }
			set 
			{ 
				items = value;
				if (Items.Count() > 0)
					lbAttributes.ItemsSource = items;
			}
		}
		 
		public bool PSMElementsOnly { get; set; }

		public bool ShowAliasTextBox { get; set; }

		public SelectAttributesDialog()
		{
			InitializeComponent();

            lbAttributes.SelectionChanged += delegate { lbAttributes.SelectedIndex = -1; };
		}

		public Dictionary<Property, string> SelectedPIMAttributes { get; set; }
		
		public List<PSMAttribute> SelectedPSMAttributes { get; set; }

		public List<PSMAttribute> NotSelectedPSMAttributes { get; set; }

		public Dictionary<PSMAttribute, string> RenamedPSMAttributes { get; set; }

		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			SelectedPSMAttributes = new List<PSMAttribute>();
			SelectedPIMAttributes = new Dictionary<Property, string>();
			RenamedPSMAttributes = new Dictionary<PSMAttribute, string>();
			NotSelectedPSMAttributes = new List<PSMAttribute>();
			foreach (Property item in lbAttributes.Items)
			{
				CheckBox cbSelected = GetCheckboxOfItem(item);
				RememberingTextBox tbAlias = GetAliasOfItem(item);
				if (item is PSMAttribute)
				{
					if (cbSelected.IsChecked == true && tbAlias.ValueChanged)
					{
						RenamedPSMAttributes.Add((PSMAttribute)item, tbAlias.Text);
					}
					if (cbSelected.IsChecked == true)
					{
						SelectedPSMAttributes.Add((PSMAttribute)item);
					}
					else
					{
						NotSelectedPSMAttributes.Add((PSMAttribute)item);
					}
				}
				else
				{
					if (cbSelected.IsChecked == true)
						SelectedPIMAttributes[item] = GetAliasOfItem(item).Text;
				}
			}
			DialogResult = true;
			Close();
		}

		public CheckBox GetCheckboxOfItem(Element item)
		{
			ListBoxItem myListBoxItem =
				(ListBoxItem)(lbAttributes.ItemContainerGenerator.ContainerFromItem(item));

			return TemplateHelper.GetPartOfDataTemplate<CheckBox>(myListBoxItem, "cb");
		}

		public RememberingTextBox GetAliasOfItem(Element item)
		{
			ListBoxItem myListBoxItem =
				(ListBoxItem)(lbAttributes.ItemContainerGenerator.ContainerFromItem(item));

			return TemplateHelper.GetPartOfDataTemplate<RememberingTextBox>(myListBoxItem, "tbAlias");
		}

		public Label GetAliasLabelOfItem(Element item)
		{
			ListBoxItem myListBoxItem =
				(ListBoxItem)(lbAttributes.ItemContainerGenerator.ContainerFromItem(item));

			return TemplateHelper.GetPartOfDataTemplate<Label>(myListBoxItem, "lbAlias");
		}

		private void bCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			foreach (Property item in lbAttributes.Items)
			{
				CheckBox cb = GetCheckboxOfItem(item);
				cb.IsChecked = PSMElementsOnly || (item is PSMAttribute && Element.PSMAttributes.Contains((PSMAttribute)item) || Element.PSMAttributes.Count() == 0);
				if (item is PSMAttribute)
				{
					GetAliasOfItem(item).Text = ((PSMAttribute)item).Alias;
					GetAliasLabelOfItem(item).Content = ((PSMAttribute)item).Alias;
				}
				else
				{
					GetAliasOfItem(item).Text = item.Name;
				}
				if (ShowAliasTextBox == false)
				{
					GetAliasOfItem(item).Visibility = Visibility.Collapsed;
					GetAliasLabelOfItem(item).Visibility = Visibility.Visible;
				}
			}
		}
	}
}
