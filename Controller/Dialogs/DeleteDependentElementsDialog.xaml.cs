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
using XCase.Model;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// Interaction logic for DeleteDependentElements.xaml
    /// </summary>
    public partial class DeleteDependentElementsDialog : Window
    {
        public ElementDependencies Dependencies { get; private set; }

		/// <summary>
		/// Deleting from diagram or from model.
		/// </summary>
        public enum EDeleteLevel
        {
            Diagram,
            Model
        }

        private EDeleteLevel deleteLevel;
        public EDeleteLevel DeleteLevel
        {
            get
            {
                return deleteLevel;
            }
            set
            {
                if (deleteLevel != value)
                {
                    if (value == EDeleteLevel.Diagram)
                    {
                        tbLong.Text = "Some elements have other elements that depend on them present on the diagram. Select the elements that you wish to delete from the diagram (dependent elements will be deleted too)";
                        tbShort.Content = "Elements dependent on selected elements found on the diagram";
                    }
                    if (value == EDeleteLevel.Model)
                    {
                        tbLong.Text = "Some elements have other elements that depend on them present in the model. Select the elements that you wish to delete from the model (dependent elements will be deleted too)";
                        tbShort.Content = "Elements dependent on selected elements found in the model";
                    }
                }
                deleteLevel = value;
            }
        }

        public DeleteDependentElementsDialog()
        {
            InitializeComponent();

            deleteLevel = EDeleteLevel.Diagram;
        }

        //DependentElementsSelector dependenciesSelector;

        public DeleteDependentElementsDialog(ElementDependencies dependencies) :
            this()
        {

            //dependenciesSelector = new DependentElementsSelector();

            Dependencies = dependencies;
            dependenciesSelector.Dependencies = dependencies;
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            foreach (Element element in Dependencies.Keys)
            {
                if (dependenciesSelector.IsChecked(element))
                {
                    Dependencies.Flags[element] = true;
                }
                else
                {
                    Dependencies.Flags[element] = false;
                }
            }
            DialogResult = true;
            Close();
        }
    }
}
