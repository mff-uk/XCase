using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using NUml.Uml2;
using XCase.Controller.Commands;
using System.Linq;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using Class=XCase.Model.Class;
using DataType=XCase.Model.DataType;
using Package=XCase.Model.Package;
using Property=XCase.Model.Property;
using System.Windows.Controls;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// Interaction logic for ClassDialog.xaml
    /// </summary>
    public partial class ClassDialog
    {
        private readonly ClassController classController;
        private readonly ModelController modelController;
        private readonly bool isPSM = false;
        private readonly ObservableCollection<Property> attributes = null;
        private readonly ObservableCollection<Property> checkedAttributes = null;
        private readonly ObservableCollection<PSMAttribute> psmAttributes = null;
        private readonly ObservableCollection<PSMAttribute> checkedPSMAttributes = null;

    	private bool dialogReady = false; 

		public Class modelClass
		{
			get
			{
				return classController.Class;
			}
		}

		private class FakeAttribute
		{
			public string Name { get; set; }
			public DataType Type { get; set; }
			//public uint ? Lower;
			//public UnlimitedNatural Upper;
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
			public string Default { get; set; }
			public Property SourceAttribute { get; set; }

			public FakeAttribute()
			{
				Multiplicity = "1";
			}

			public FakeAttribute(Property p): this()
			{
				Name = p.Name;
				Type = p.Type;
				Multiplicity = p.MultiplicityString;
				Default = p.Default;
				SourceAttribute = p;
			}
		}

		private class FakeAttributeCollection: ListCollectionView, IEditableCollectionView
		{
			public FakeAttributeCollection(IList attributes)
				: base(attributes)
			{
				
			}
		}

    	private FakeAttributeCollection fakeAttributes;

		public ClassDialog(ClassController classController, ModelController controller)
        {
            InitializeComponent();
            this.modelController = controller;
			this.classController = classController;
            if (classController is PSMClass) isPSM = true;
			this.Title = string.Format(this.Title, modelClass);

			tbName.Text = modelClass.Name;
            tbOnto.Text = modelClass.OntologyEquivalent;
            SubpackagesGetter subpackagesGetter = new SubpackagesGetter(controller.Model);
            Collection<Package> packages = subpackagesGetter.GetSubpackages(null);
            cbPackages.ItemsSource = packages;
			cbPackages.SelectedItem = modelClass.Package;
            if (isPSM)
            {
                cbPackages.IsEnabled = false;
                psmAttributes = new ObservableCollection<PSMAttribute>();
                checkedPSMAttributes = new ObservableCollection<PSMAttribute>();
                foreach (PSMAttribute item in ((PSMClass)classController).PSMAttributes)
                {
                    psmAttributes.Add(item);
                    checkedPSMAttributes.Add(item);
                }				
            }
            else
            {
                attributes = new ObservableCollection<Property>();
                checkedAttributes = new ObservableCollection<Property>();
				foreach (Property item in modelClass.Attributes)
                {
                    attributes.Add(item);
                    checkedAttributes.Add(item);
                }
               
				typeColumn.ItemsSource = classController.Class.Package.AllTypes;
				ObservableCollection<FakeAttribute> fakeAttributesList = new ObservableCollection<FakeAttribute>();
            	foreach (Property property in attributes)
            	{
            		fakeAttributesList.Add(new FakeAttribute(property));
            	}
            	fakeAttributes = new FakeAttributeCollection(fakeAttributesList);
            	fakeAttributesList.CollectionChanged += delegate { UpdateApplyEnabled(); };
            	gridAttributes.ItemsSource = fakeAttributesList;
            	dialogReady = true; 

				if (classController.Class is Model.AssociationClass)
					cPackage.Visibility = Visibility.Hidden;
            }
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
			if (isPSM)
			{
				throw new NotImplementedException("Method or operation is not implemented.");
			}

			modelController.BeginMacro();
			modelController.CreatedMacro.Description = CommandDescription.UPDATE_CLASS_MACRO;
			if (tbName.ValueChanged)
			{
				classController.RenameElement(tbName.Text, modelClass.Package.Classes.Cast<Class>());
				tbName.ForgetOldValue();
			}
            if (tbOnto.ValueChanged)
            {
                classController.ChangeOntologyEquivalent(tbOnto.Text);
                tbOnto.ForgetOldValue();
            }
			if (cbPackages.SelectedItem != modelClass.Package)
			{
				classController.MoveToPackage((Package)cbPackages.SelectedItem);
			}

			// check for deleted attributes
			List<Property> removedAttributes = new List<Property>();
			List<FakeAttribute> addedAttributes = new List<FakeAttribute>();
			foreach (Property attribute in classController.Class.Attributes)
			{
				bool found = false;
				foreach (FakeAttribute fakeAttribute in fakeAttributes)
				{
					if (fakeAttribute.SourceAttribute == attribute)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					removedAttributes.Add(attribute);
					classController.RemoveAttribute(attribute);
				}
			}

    		List<FakeAttribute> toRemove = new List<FakeAttribute>();
    		foreach (FakeAttribute fakeAttribute in fakeAttributes)
    		{
    			if (String.IsNullOrEmpty(fakeAttribute.Name))
    			{
					if (fakeAttribute.SourceAttribute != null)
					{
						removedAttributes.Add(fakeAttribute.SourceAttribute);
						classController.RemoveAttribute(fakeAttribute.SourceAttribute);
					}
					toRemove.Add(fakeAttribute);
    			}
    		}

    		foreach (FakeAttribute attribute in toRemove)
    		{
    			fakeAttributes.Remove(attribute);
    		}

			// check for changes and new attributes
			foreach (FakeAttribute fakeAttribute in fakeAttributes)
    		{
    			if (fakeAttribute.SourceAttribute != null && !removedAttributes.Contains(fakeAttribute.SourceAttribute))
    			{
    				// editing old attribute
    				Property sourceAttribute = fakeAttribute.SourceAttribute;
					if (fakeAttribute.Name != sourceAttribute.Name)
						classController.RenameAttribute(sourceAttribute, fakeAttribute.Name);
					if (fakeAttribute.Type != sourceAttribute.Type)
						classController.ChangeAttributeType(sourceAttribute, new ElementHolder<DataType>(fakeAttribute.Type));
					if (fakeAttribute.Default != sourceAttribute.Default)
						classController.ChangeAttributeDefaultValue(sourceAttribute, fakeAttribute.Default);
					if (fakeAttribute.Multiplicity != sourceAttribute.MultiplicityString)
					{
						if (!String.IsNullOrEmpty(fakeAttribute.Multiplicity))
						{
							uint ? lower;
							UnlimitedNatural upper;
							if (!MultiplicityElementController.ParseMultiplicityString(fakeAttribute.Multiplicity, out lower, out upper))
								return;
							MultiplicityElementController.ChangeMultiplicityOfElement(sourceAttribute, classController.Class, lower, upper, classController.DiagramController.ModelController);
						}						
						else
						{
							MultiplicityElementController.ChangeMultiplicityOfElement(sourceAttribute, classController.Class, null, 1, classController.DiagramController.ModelController);
						}
					}
    			}
				else
    			{
					// new attribute
					if (!string.IsNullOrEmpty(fakeAttribute.Name))
					{
						uint ? lower = 1;
						UnlimitedNatural upper = 1;
						if (!String.IsNullOrEmpty(fakeAttribute.Multiplicity))
							if (!MultiplicityElementController.ParseMultiplicityString(fakeAttribute.Multiplicity, out lower, out upper))
								return;
						classController.AddNewAttribute(fakeAttribute.Name, fakeAttribute.Type, lower, upper, fakeAttribute.Default);
						addedAttributes.Add(fakeAttribute);
					}
    			}
    		}

    		CommandBase tmp = (CommandBase)modelController.CreatedMacro;
    		modelController.CommitMacro();
			if (string.IsNullOrEmpty(tmp.ErrorDescription))
			{
				foreach (FakeAttribute attribute in addedAttributes)
				{
					attribute.SourceAttribute =
						classController.Class.Attributes.Where(property => property.Name == attribute.Name).SingleOrDefault();
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

		private void tbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			UpdateApplyEnabled();
		}

    	private void cbPackages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
    		if (dialogReady)
    		{
    			bApply.IsEnabled = true;
    		}
    	}

        private void tbOnto_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateApplyEnabled();
        }
    }

	
}