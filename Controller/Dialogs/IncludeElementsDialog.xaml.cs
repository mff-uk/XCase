using System.Collections.Generic;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XCase.Model;
using System.Linq;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for IncludeElementsDialog.xaml
	/// </summary>
	public partial class IncludeElementsDialog
	{
		public object PrimaryContent
		{
			get
			{
				return label1.Content;
			}
			set
			{
				label1.Content = value;
			}
		}

		public object SecondaryContent
		{
			get
			{
				return label2.Content;
			}
			set
			{
				label2.Content = value;
			}
		}

		public object NoElementsContent { get; set; }

		public object CancelButtonContent
		{
			get
			{
				return bCancel.Content;
			}
			set
			{
				bCancel.Content = value;
			}
		}

		public object OkButtonContent
		{
			get
			{
				return bOK.Content;
			}
			set
			{
				bOK.Content = value;
			}
		}

		private IEnumerable<Element> items;

		public IEnumerable<Element> Items
		{
			get { return items; }
			set 
			{ 
				items = value;
				if (Items.Count() > 0)
					lbElements.ItemsSource = items;
				else
				{
					SecondaryContent = NoElementsContent;
					lbElements.Visibility = Visibility.Hidden;
				}
			}
		}

		public IncludeElementsDialog()
		{
			InitializeComponent();

            lbElements.SelectionChanged += delegate { lbElements.SelectedIndex = -1; };
		}

		public IList<Element> SelectedElements { get; set; }

		public IEnumerable<Element> DeselectedElements { get; set; }

		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			SelectedElements = new List<Element>();
			foreach (Element item in lbElements.Items)
			{
				CheckBox cb = GetCheckboxOfItem(item);	
				if (cb.IsChecked == true)
					SelectedElements.Add(item);
			}
			DialogResult = true;
			Close();
		}

		public CheckBox GetCheckboxOfItem(Element item)
		{
			ListBoxItem myListBoxItem =
				(ListBoxItem)(lbElements.ItemContainerGenerator.ContainerFromItem(item));

			return TemplateHelper.GetPartOfDataTemplate<CheckBox>(myListBoxItem, "cb");
		}

		private void bCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
