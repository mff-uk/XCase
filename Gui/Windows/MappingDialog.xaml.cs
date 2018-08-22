using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Gui.Windows
{
    /// <summary>
    /// Interaction logic for MappingDialog.xaml
    /// </summary>
    public partial class MappingDialog : Window
    {
        public MappingDialog()
        {
            InitializeComponent();
        }

        public bool DR = false;
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DR = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DR = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillTable();
        }

        public Diagram DiagramOldVersion { get; set; }

        public Diagram DiagramNewVersion { get; set; }

        public Version OldVersion { get { return DiagramOldVersion.Version; } }

        public DataGridComboBoxColumn mapToColumn
        {
            get
            {
                return (DataGridComboBoxColumn)dataGrid1.Columns[2];
            }
        }

        public readonly List<MappingGridItem> ItemsList = new List<MappingGridItem>();
        private readonly List<Element> comboItemsList = new List<Element>();

        private static bool NameMatch<T>(IEnumerable<T> elements, T element, out T result) where T : class
        {
            result = elements.FirstOrDefault(e => e.GetType() == element.GetType() && e.ToString() == element.ToString());
            return result != null;
        }

        private void FillTable()
        {
            // load all constructs in old version    
            foreach (Element oldE in DiagramOldVersion.DiagramElements.Keys.Where(k => !(k is Comment)))
            {
                comboItemsList.Add(oldE);
                if (oldE is IHasAttributes)
                {
                    comboItemsList.AddRange(((IHasAttributes)oldE).Attributes);
                }
                if (oldE is IHasPSMAttributes)
                {
                    comboItemsList.AddRange(((IHasPSMAttributes)oldE).PSMAttributes);
                }
            }
            // load all constructs in new version 
            List<Element> newElements = DiagramNewVersion.DiagramElements.Keys.Where(k => !(k is Comment)).ToList();
            // fill comboboxes with new version items

            ItemsList.Clear();
            foreach (Element newElement in newElements)
            {
                #region item

                MappingGridItem mappingGridItem = new MappingGridItem { NewVersionConstructName = newElement.ToString(), NewVersionConstruct = newElement, Type = newElement.GetType().Name.Substring(1) };
                ItemsList.Add(mappingGridItem);
                IVersionedElement elementOldVersion = newElement.GetInVersion(OldVersion);
                if (elementOldVersion != null)
                {
                    mappingGridItem.OldVersionConstructName = elementOldVersion.ToString();
                    mappingGridItem.OldVersionConstruct = (Element)elementOldVersion;
                    mappingGridItem.OriginalMapping = mappingGridItem.OldVersionConstruct;
                }
                else
                {
                    Element found;
                    if (NameMatch(comboItemsList, newElement, out found))
                    {
                        mappingGridItem.OldVersionConstructName = found.ToString();
                        mappingGridItem.OldVersionConstruct = found;
                    }
                }

                #endregion

                #region psm attributes

                if (newElement is IHasPSMAttributes)
                {
                    foreach (PSMAttribute attribute in ((IHasPSMAttributes)newElement).PSMAttributes)
                    {
                        MappingGridItem attributeItem = new MappingGridItem
                                                            {
                                                                NewVersionConstructName = attribute.ToString(),
                                                                NewVersionConstruct = attribute,
                                                                Type = String.Format("PSMA in {0}", newElement)
                                                            };
                        attributeItem.Color = Brushes.DarkGreen;
                        IHasPSMAttributes elementOldVersionHA = null;
                        if (mappingGridItem.OldVersionConstruct != null)
                        {
                            elementOldVersionHA = (IHasPSMAttributes)mappingGridItem.OldVersionConstruct;
                        }
                        IVersionedElement atOld = attribute.GetInVersion(OldVersion);
                        if (atOld != null)
                        {
                            attributeItem.OldVersionConstruct = (Element)atOld;
                            attributeItem.OriginalMapping = mappingGridItem.OldVersionConstruct;
                            attributeItem.OldVersionConstructName = attributeItem.OldVersionConstruct.ToString();
                        }
                        else if (elementOldVersionHA != null)
                        {
                            PSMAttribute found;
                            if (NameMatch(elementOldVersionHA.PSMAttributes, attribute, out found))
                            {
                                attributeItem.OldVersionConstruct = found;
                                attributeItem.OldVersionConstructName = attributeItem.OldVersionConstruct.ToString();
                            }
                        }
                        ItemsList.Add(attributeItem);
                    }
                }
                    #endregion

                #region pim attributes

                else if (newElement is IHasAttributes)
                {
                    foreach (Property attribute in ((IHasAttributes)newElement).Attributes)
                    {
                        MappingGridItem attributeItem = new MappingGridItem
                                                            {
                                                                NewVersionConstructName = attribute.ToString(),
                                                                NewVersionConstruct = attribute,
                                                                Type = String.Format("PSMA in {0}", newElement)
                                                            };
                        attributeItem.Color = Brushes.DarkGreen;
                        IHasAttributes elementOldVersionHA = null;
                        if (mappingGridItem.OldVersionConstruct != null)
                        {
                            elementOldVersionHA = (IHasAttributes)mappingGridItem.OldVersionConstruct;
                        }
                        IVersionedElement atOld = attribute.GetInVersion(OldVersion);
                        if (atOld != null)
                        {
                            attributeItem.OldVersionConstruct = (Element)atOld;
                            attributeItem.OriginalMapping = mappingGridItem.OldVersionConstruct;
                            attributeItem.OldVersionConstructName = attributeItem.OldVersionConstruct.ToString();
                        }
                        else if (elementOldVersionHA != null)
                        {
                            Property found;
                            if (NameMatch(elementOldVersionHA.Attributes, attribute, out found))
                            {
                                attributeItem.OldVersionConstruct = found;
                                attributeItem.OldVersionConstructName = attributeItem.OldVersionConstruct.ToString();
                            }
                        }
                        ItemsList.Add(attributeItem);
                    }
                }

                #endregion
            }

            mapToColumn.ItemsSource = comboItemsList;
            dataGrid1.ItemsSource = ItemsList;

            lUnmapped.Content = ItemsList.Count(i => i.OldVersionConstruct == null);
        }

        public class MappingGridItem
        {
            public string Type { get; set; }
            public Element OldVersionConstruct { get; set; }

            public string OldVersionConstructName { get; set; }
            public Element NewVersionConstruct { get; set; }
            public string NewVersionConstructName { get; set; }
            public Brush Color { get; set; }

            public Element OriginalMapping { get; set; }


            public MappingGridItem()
            {
                Color = Brushes.Black;
                OriginalMapping = null;
            }
        }


        private void ToggleButton_CheckChange(object sender, RoutedEventArgs e)
        {
            dataGrid1.CommitEdit();
            if (tbHideMapped.IsChecked == true)
            {
                foreach (MappingGridItem item in ItemsList)
                {
                    if (item.OldVersionConstruct != null)
                    {
                        excludedItems.AddIfNotContained(item);
                        excludedComboItems.AddIfNotContained(item.OldVersionConstruct);
                    }
                    else
                    {

                    }
                }
                foreach (MappingGridItem item in excludedItems)
                {
                    ItemsList.Remove(item);
                }
                foreach (Element excludedComboItem in excludedComboItems)
                {
                    comboItemsList.Remove(excludedComboItem);
                }
            }
            else
            {
                foreach (MappingGridItem item in excludedItems)
                {
                    ItemsList.AddIfNotContained(item);
                }
                excludedItems.Clear();
                foreach (Element excludedComboItem in excludedComboItems)
                {
                    comboItemsList.AddIfNotContained(excludedComboItem);
                }
                excludedComboItems.Clear();
            }
            dataGrid1.Items.Refresh();
            mapToColumn.ItemsSource = comboItemsList;
        }

        private readonly List<MappingGridItem> excludedItems = new List<MappingGridItem>();
        private readonly List<Element> excludedComboItems = new List<Element>();


        private void dataGrid1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //search the object hierarchy for a datagrid row
            DependencyObject source = (DependencyObject)e.OriginalSource;
            DataGridRow row = UIExtensions.TryFindParent<DataGridRow>(source);

            //the user did not click on a row
            if (row == null) return;

            MappingGridItem item = (MappingGridItem)row.Item;

            DiagramView.SelectElement(item.NewVersionConstruct);
            if (DiagramViewOldVersion != null && 
                item.OldVersionConstruct != null && item.OldVersionConstruct is Element)
            {
                DiagramViewOldVersion.SelectElement(item.OldVersionConstruct);
            }

            e.Handled = true;
        }

        public View.Controls.XCaseCanvas DiagramView { get; set; }
        public View.Controls.XCaseCanvas DiagramViewOldVersion { get; set; }
    }



    /// <summary>
    /// Converter for displaying of derived PSM class
    /// </summary>
    public class GetTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is PSMElement)
            {
                return (value as PSMElement).GetType().Name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
