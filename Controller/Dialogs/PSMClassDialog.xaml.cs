using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
//using Microsoft.Windows.Controls;
using NUml.Uml2;
using XCase.Controller.Commands;
using System.Linq;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using Class = XCase.Model.Class;
using DataType = XCase.Model.DataType;
using Property = XCase.Model.Property;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// Interaction logic for PSMClassDialog.xaml
    /// </summary>
    public partial class PSMClassDialog
    {
        private readonly PSM_ClassController psmClassController;
        private readonly DiagramController diagramController;
        private bool dialogReady = false;

        public PSMClass psmClass
        {
            get
            {
                return psmClassController.Class;
            }
        }

        private class FakePSMAttribute : IEditableObject
        {
            public string Name { get; set; }
            public DataType Type { get; set; }
            private string multiplicity;
            public string Multiplicity
            {
                get { return multiplicity; }
                set
                {
                    bool isValid;
                    try
                    {
                        isValid = MultiplicityElementController.IsMultiplicityStringValid(value);
                    }
                    catch
                    {
                        isValid = false;
                    }
                    if (!isValid)
                        throw new ArgumentException("Multiplicity string is invalid. ");
                    multiplicity = value;
                }
            }
            public PIMClass ComesFrom { get; set; }
            public string Default { get; set; }
            public string Alias { get; set; }
            public PSMAttribute SourceAttribute { get; set; }
            public Property RepresentedAttribute { get; set; }
            public bool Checked { get; set; }
            public string AliasOrName
            {
                get
                {
                    if (!string.IsNullOrEmpty(Alias))
                    {
                        return Alias;
                    }
                    else
                    {
                        return Name;
                    }
                }
            }

            public bool IsReadonlyType
            {
                get
                {
                    return RepresentedAttribute == null;
                }
            }

            public FakePSMAttribute()
            {
                Multiplicity = "1";
            }

            public FakePSMAttribute(PSMAttribute p)
                : this()
            {
                Name = p.Name;
                Type = p.Type;
                Multiplicity = p.MultiplicityString;
                Default = p.Default;
                SourceAttribute = p;
                RepresentedAttribute = p.RepresentedAttribute;
                if (RepresentedAttribute != null)
                    ComesFrom = (PIMClass) p.RepresentedAttribute.Class;
                Alias = p.Alias;
                Checked = true;
            }

            public FakePSMAttribute(Property p)
                : this()
            {
                Name = p.Name;
                Type = p.Type;
                Multiplicity = p.MultiplicityString;
                Default = p.Default;
                SourceAttribute = null;
                RepresentedAttribute = p;
                Alias = null;
                ComesFrom = (PIMClass) p.Class;
                Checked = false;
            }

            public void BeginEdit()
            {
                //Checked = true;
            }

            public void EndEdit()
            {

            }

            public void CancelEdit()
            {

            }

            public bool SomethingChanged()
            {
                if (SourceAttribute == null)
                {
                    return true;
                }
                else
                {
                    uint? lower;
                    UnlimitedNatural upper;
                    MultiplicityElementController.ParseMultiplicityString(Multiplicity, out lower, out upper);
                    return SourceAttribute.Name != Name || SourceAttribute.Type != Type
                           || SourceAttribute.Default != Default || SourceAttribute.Alias != Alias
                           || SourceAttribute.Lower != lower || SourceAttribute.Upper != upper;
                }
            }
        }

        private class FakeAttributeCollection : ListCollectionView
        {

            public FakeAttributeCollection(IList attributes)
                : base(attributes)
            {

            }

            public FakeAttributeCollection(ObservableCollection<FakePSMAttribute> attributesList, PSMClass psmClass)
                : base(attributesList)
            {
                foreach (PSMAttribute psmAttribute in psmClass.Attributes)
                {
                    attributesList.Add(new FakePSMAttribute(psmAttribute));
                }

                bool classEmpty = psmClass.Attributes.Count == 0;

                foreach (Property attribute in psmClass.RepresentedClass.AllAttributes)
                {
                    if (!attributesList.Any(p => p.SourceAttribute != null && p.SourceAttribute.RepresentedAttribute == attribute))
                    {
                        attributesList.Add(new FakePSMAttribute(attribute) {Checked = classEmpty});
                    }
                }
            }


        }

        private readonly FakeAttributeCollection fakeAttributes;

        public PSMClassDialog()
        {
            InitializeComponent();


        }

        public PSMClassDialog(PSM_ClassController classController, DiagramController controller)
            : this()
        {

            this.diagramController = controller;
            this.psmClassController = classController;

            this.Title = string.Format(this.Title, psmClass);

            tbName.Text = psmClass.Name;
            tbElementLabel.Text = psmClass.ElementName;
            cbAbstract.IsChecked = psmClass.IsAbstract;
            cbAnyAttribute.IsChecked = psmClass.AllowAnyAttribute;

            typeColumn.ItemsSource = classController.Class.Package.AllTypes;
            
            ObservableCollection<FakePSMAttribute> fakeAttributesList = new ObservableCollection<FakePSMAttribute>();

            fakeAttributes = new FakeAttributeCollection(fakeAttributesList, psmClass);
            fakeAttributesList.CollectionChanged += delegate { UpdateApplyEnabled(); };
            gridAttributes.ItemsSource = fakeAttributesList;

            dialogReady = true;
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            bApply_Click(sender, e);
            if (!error)
            {
                DialogResult = true;
                Close();
            }
        }

        private bool error = false;
        
        private void bApply_Click(object sender, RoutedEventArgs e)
        {
            bApply.Focus();

            error = false;

            diagramController.BeginMacro();
            diagramController.CreatedMacro.Description = CommandDescription.UPDATE_CLASS_MACRO;
            if (tbName.ValueChanged)
            {
                psmClassController.RenameElementWithDiagramController(tbName.Text);
                tbName.ForgetOldValue();
            }

            if (tbElementLabel.ValueChanged)
            {
                psmClassController.ChangeElementName(tbName.Text);
                tbElementLabel.ForgetOldValue();
            }

            if (psmClass.IsAbstract != cbAbstract.IsChecked)
            {
                psmClassController.ChangeAbstract(cbAbstract.IsChecked == true);
            }

            if (psmClass.AllowAnyAttribute != cbAnyAttribute.IsChecked)
            {
                psmClassController.ChangeAllowAnyAttributeDefinition(cbAnyAttribute.IsChecked == true);
            }

            #region check for deleted attributes

            List<Property> removedAttributes = new List<Property>();
            List<FakePSMAttribute> addedAttributes = new List<FakePSMAttribute>();
            foreach (PSMAttribute psmAttribute in psmClassController.Class.PSMAttributes)
            {
                bool found = false;
                foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
                {
                    if (fakeAttribute.SourceAttribute == psmAttribute && fakeAttribute.Checked)
                    {
                        found = true;
                        break;
                    }
                    else if (fakeAttribute.SourceAttribute == psmAttribute && !fakeAttribute.Checked)
                    {
                        fakeAttribute.SourceAttribute = null;
                    }
                }
                if (!found)
                {
                    removedAttributes.Add(psmAttribute);
                    psmClassController.RemoveAttribute(psmAttribute);
                }
            }

            #endregion

            #region remove dummy entries in fake collection

            List<FakePSMAttribute> toRemove = new List<FakePSMAttribute>();
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
            {
                if (String.IsNullOrEmpty(fakeAttribute.Name))
                {
                    if (fakeAttribute.SourceAttribute != null)
                    {
                        removedAttributes.Add(fakeAttribute.SourceAttribute);
                        psmClassController.RemoveAttribute(fakeAttribute.SourceAttribute);
                    }
                    toRemove.Add(fakeAttribute);
                }
            }

            foreach (FakePSMAttribute attribute in toRemove)
            {
                fakeAttributes.Remove(attribute);
            }

            #endregion

            Dictionary<PSMAttribute, string> namesDict = new Dictionary<PSMAttribute, string>();
            foreach (PSMAttribute a in psmClass.PSMAttributes)
            {
                if (!removedAttributes.Contains(a))
                {
                    namesDict.Add(a, a.AliasOrName);
                }
            }

            // check for changes and new attributes
            var modified = from FakePSMAttribute a in fakeAttributes 
                           where a.SourceAttribute != null && !removedAttributes.Contains(a.SourceAttribute) && a.SomethingChanged()
                           select a;
            var added    = from FakePSMAttribute a in fakeAttributes where a.SourceAttribute == null select a; 
            
            // editing exisiting attribute
            foreach (FakePSMAttribute modifiedAttribute in modified)
            {
                PSMAttribute sourceAttribute = modifiedAttribute.SourceAttribute;
                uint? lower;
                UnlimitedNatural upper;
                if (
                    !MultiplicityElementController.ParseMultiplicityString(modifiedAttribute.Multiplicity, out lower,
                                                                           out upper))
                {
                    error = true;
                }
                psmClassController.ModifyAttribute(sourceAttribute, modifiedAttribute.Name, modifiedAttribute.Alias,
                                                   lower, upper, modifiedAttribute.Type, modifiedAttribute.Default);
                namesDict[sourceAttribute] = modifiedAttribute.AliasOrName;
            }

            List<string> names = namesDict.Values.ToList();
            // new attribute
            foreach (FakePSMAttribute addedAttribute in added)
            {
                if (!string.IsNullOrEmpty(addedAttribute.Name) && addedAttribute.Checked)
                {
                    uint? lower = 1;
                    UnlimitedNatural upper = 1;
                    if (!String.IsNullOrEmpty(addedAttribute.Multiplicity))
                        if (!MultiplicityElementController.ParseMultiplicityString(addedAttribute.Multiplicity, out lower, out upper))
                        {
                            error = true;
                        }
                    psmClassController.AddAttribute(addedAttribute.RepresentedAttribute, addedAttribute.Name, addedAttribute.Alias, lower, upper, addedAttribute.Type, addedAttribute.Default, names.ToList());
                    addedAttributes.Add(addedAttribute);
                    names.Add(addedAttribute.AliasOrName);
                }
            }
                

            if (error)
            {
                diagramController.CancelMacro();
            }
            else
            {
                CommandBase tmp = (CommandBase)diagramController.CreatedMacro;
                diagramController.CommitMacro();
                if (string.IsNullOrEmpty(tmp.ErrorDescription))
                {
                    foreach (FakePSMAttribute attribute in addedAttributes)
                    {
                        if (attribute.RepresentedAttribute != null)
                        {
                            attribute.SourceAttribute = (PSMAttribute)psmClassController.Class.AllAttributes.Where
                                (property => ((PSMAttribute)property).RepresentedAttribute == attribute.RepresentedAttribute).SingleOrDefault();
                        }
                        else
                        {
                            attribute.SourceAttribute = psmClassController.Class.PSMAttributes.Where
                                (property => property.AliasOrName == attribute.AliasOrName).SingleOrDefault();
                        }
                        //else
                        //{
                        //    attribute.SourceAttribute = (PSMAttribute)psmClassController.Class.AllAttributes.Where
                        //        (property => (property.RepresentedAttribute == attribute.RepresentedAttribute).SingleOrDefault();
                        //}
                        //if (attribute.SourceAttribute.RepresentedAttribute != null)
                        //    attribute.RepresentedAttribute = attribute.SourceAttribute.RepresentedAttribute;
                    }
                    addedAttributes.RemoveAll(attribute => attribute.SourceAttribute == null);
                    bApply.IsEnabled = false;
                    dialogReady = true;
                    error = false;
                }
                else
                {
                    error = true;
                }
            }
            gridAttributes.Items.Refresh();
        }

        private void tbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void UpdateApplyEnabled()
        {
            int errors = System.Windows.Controls.Validation.GetErrors(gridAttributes).Count;

            if (dialogReady && errors == 0)
            {
                bApply.IsEnabled = true;
                bOk.IsEnabled = true; 
            }
            
            if (errors > 0)
            {
                bApply.IsEnabled = false;
                bOk.IsEnabled = false;
            }
        }

        private void tbElementLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void cbAbstract_Checked(object sender, RoutedEventArgs e)
        {
            UpdateApplyEnabled();
        }

        private void gridAttributes_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //if (e.Column == checkedColumn)
            //    return;

            if (e.Row != null && e.Row.Item is FakePSMAttribute)
            {
                FakePSMAttribute editedAttribute = ((FakePSMAttribute)e.Row.Item);
                if (!editedAttribute.Checked)
                {
                }

                if (e.Column == nameColumn && editedAttribute.RepresentedAttribute != null)
                {
                    ErrDialog d = new ErrDialog();
                    d.SetText("Attribute name can be changed only for PIM-less attributes. ", "You can change the represented attribute's name instead. ");
                    d.ShowDialog();
                    e.Cancel = true;
                }

                if (e.Column == typeColumn && editedAttribute.RepresentedAttribute != null)
                {
                    ErrDialog d = new ErrDialog();
                    d.SetText("Type can be changed only for PIM-less attributes. ", "You can change the represented attribute's type instead. ");
                    d.ShowDialog();
                    e.Cancel = true;
                }
            }

        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
            {
                fakeAttribute.Checked = true;
            }
        }

        private void DeselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (FakePSMAttribute fakeAttribute in fakeAttributes)
            {
                fakeAttribute.Checked = false;
            }
        }

        #region SINGLE CLICK EDITING

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            System.Windows.Controls.CheckBox cb = cell.Content as System.Windows.Controls.CheckBox;
            if (cb != null)
            {

                if (cell != null &&
                    !cell.IsEditing)
                {
                    try
                    {
                        if (!cell.IsFocused)
                        {
                            cell.Focus();
                        }
                    }
                    catch (Exception)
                    {

                    }

                    DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
                        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                        {
                            if (!cell.IsSelected)
                                cell.IsSelected = true;
                        }
                        else
                        {
                            DataGridRow row = FindVisualParent<DataGridRow>(cell);

                            if (row != null && !row.IsSelected)
                            {
                                row.IsSelected = true;
                            }

                            if (row.Item is FakePSMAttribute)
                            {
                                cb.IsChecked = !cb.IsChecked.Value;
                                ((FakePSMAttribute)row.Item).Checked = cb.IsChecked.Value;
                                UpdateApplyEnabled();
                            }

                        }
                        dataGrid.SelectedItem = null;
                    }

                }

            }
        }


        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        #endregion

    }
}