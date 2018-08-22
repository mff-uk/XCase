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
using System.Windows.Navigation;
using System.Windows.Shapes;
using XCase.Model;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// Interaction logic for DependentElementsSelector.xaml
    /// </summary>
    public partial class DependentElementsSelector
    {
        private ElementDependencies dependencies;
        public ElementDependencies Dependencies
        {
            get
            {
                return dependencies;
            }
            set
            {
                dependencies = value;
                View = new Dictionary<Element, object[]>(dependencies.Count);
                if (value != null)
                {
                    foreach (KeyValuePair<Element, List<Element>> pair in dependencies)
                    {
                        RowDefinition rd = new RowDefinition();
                        dependenciesgrid.RowDefinitions.Add(rd);

                        CheckBox cb = new CheckBox
                        {
                            IsChecked = true,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
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

                        foreach (Element element in pair.Value)
                        {
							//if (element is IFormattable)
							//    tbDependents.Text += String.Format("{0:F}, ", (IFormattable)element);
							//else
								tbDependents.Text += String.Format("{0} \r\n", element);
                        }
                        if (tbDependents.Text.EndsWith("\r\n"))
                            tbDependents.Text = tbDependents.Text.Remove(tbDependents.Text.Length - 2, 2);

                        int rownum = dependenciesgrid.RowDefinitions.Count - 1;
                        cb.SetValue(Grid.RowProperty, rownum);
                        tbElement.SetValue(Grid.RowProperty, rownum);
                        tbDependents.SetValue(Grid.RowProperty, rownum);

                        cb.SetValue(Grid.ColumnProperty, 0);
                        tbElement.SetValue(Grid.ColumnProperty, 1);
                        tbDependents.SetValue(Grid.ColumnProperty, 2);

                        dependenciesgrid.Children.Add(cb);
                        dependenciesgrid.Children.Add(tbDependents);
                        dependenciesgrid.Children.Add(tbElement);
                        dependenciesgrid.InvalidateVisual();
                        dependenciesgrid.UpdateLayout();




                        /*
                         * <CheckBox Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center" Grid.IsSharedSizeScope="True"></CheckBox>
                         * <TextBlock Grid.Row="1" Grid.Column="1" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="3">Element</TextBlock>
                         * <TextBlock Grid.Row="1" Grid.Column="2" TextWrapping="Wrap" Margin="2" VerticalAlignment="Center">Dependent1, dependent2, dependent3Dependent1, dependent2, dependent3Dependent1, dependent2, dependent3</TextBlock>
                         */

                        View[pair.Key] = new object[]
						                 	{
						                 		cb,
						                 		tbElement,
						                 		tbDependents
						                 	};

                    }
                }
            }
        }

        public Dictionary<Element, object[]> View { get; private set; }

        public bool IsChecked(Element element)
        {
            if (View.ContainsKey(element))
            {
                return ((CheckBox)View[element][0]).IsChecked == true;
            }
            else
                return false;
        }

        public DependentElementsSelector()
        {
            InitializeComponent();
        }
    }
}
