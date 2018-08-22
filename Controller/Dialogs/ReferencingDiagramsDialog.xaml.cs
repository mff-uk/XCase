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
    public partial class ReferencingDiagramsDialog : Window
    {
        private ElementDiagramDependencies dependencies;
        public ElementDiagramDependencies Dependencies
        {
            get { return dependencies; }
            set
            {
                dependencies = value;
                if (value != null)
                {
                    foreach (KeyValuePair<Element, List<Diagram>> pair in dependencies)
                    {
                        RowDefinition rd = new RowDefinition();
                        dependenciesgrid.RowDefinitions.Add(rd);

                        TextBlock tbElement = new TextBlock
                        {
                            Text = pair.Key.ToString(),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(3)
                        };
                        TextBlock tbDependents = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(2),
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        foreach (Diagram diagram in pair.Value)
                        {
                            tbDependents.Text += diagram.Caption + ", ";
                        }
                        if (tbDependents.Text.EndsWith(", "))
                            tbDependents.Text = tbDependents.Text.Remove(tbDependents.Text.Length - 2, 2);

                        int rownum = dependenciesgrid.RowDefinitions.Count - 1;
                        tbElement.SetValue(Grid.RowProperty, rownum);
                        tbDependents.SetValue(Grid.RowProperty, rownum);

                        tbElement.SetValue(Grid.ColumnProperty, 1);
                        tbDependents.SetValue(Grid.ColumnProperty, 2);

                        dependenciesgrid.Children.Add(tbDependents);
                        dependenciesgrid.Children.Add(tbElement);
                        dependenciesgrid.InvalidateVisual();
                        dependenciesgrid.UpdateLayout();
                    }
                }
            }
        }

        public ReferencingDiagramsDialog()
        {
            InitializeComponent();
        }

        public ReferencingDiagramsDialog(ElementDiagramDependencies dependencies) :
            this()
        {
            Dependencies = dependencies;
        }
    }
}
