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
	public partial class DiagramReferenceDialog
	{
		private readonly PSMDiagramReference diagramReference;

		private readonly PSM_DiagramReferenceController psmDiagramReferenceController;

		private readonly ModelController modelController;

		private readonly List<Object> itemsSource;

        public DiagramReferenceDialog(PSMDiagramReference diagramReference, PSM_DiagramReferenceController psmDiagramReferenceController, ModelController controller)
        {
            InitializeComponent();
            this.psmDiagramReferenceController = psmDiagramReferenceController;
            this.modelController = controller;
            this.diagramReference = diagramReference;
            
            Title = string.Format("DiagramReference: {0}", diagramReference.ReferencedDiagram);

            tbName.Text = diagramReference.Name;
            tbNamespace.Text = diagramReference.Namespace;
            tbNamespacePrefix.Text = diagramReference.NamespacePrefix;
            tbSchemaLocation.Text = diagramReference.SchemaLocation;
            cbLocal.IsChecked = diagramReference.Local;
            itemsSource =
                new List<Object>(controller.Project.PSMDiagrams.Where(d => d != diagramReference.ReferencingDiagram));
            cbDiagram.ItemsSource = itemsSource;
            if (diagramReference.ReferencedDiagram != null)
                cbDiagram.SelectedIndex = cbDiagram.Items.IndexOf(diagramReference.ReferencedDiagram);
        }

	    private void bOk_Click(object sender, RoutedEventArgs e)
	    {
	        modelController.BeginMacro();
	        modelController.CreatedMacro.Description = CommandDescription.DIAGRAM_REFERENCE_MACRO;
	        
            if (tbName.ValueChanged)
	        {
	            NamedElementController.RenameElement(diagramReference, tbName.Text, modelController, null);
	        }
	        
	        if (tbNamespace.ValueChanged)
	        {
	            psmDiagramReferenceController.ChangeNamespace(tbNamespace.Text);
	        }

	        if (tbNamespacePrefix.ValueChanged)
	        {
                psmDiagramReferenceController.ChangeNamespacePrefix(tbNamespacePrefix.Text);
	        }

	        if (tbSchemaLocation.ValueChanged)
	        {
	            psmDiagramReferenceController.ChangeSchemaLocation(tbSchemaLocation.Text);
	        }

	        if (cbLocal.IsChecked != diagramReference.Local)
	        {
	            psmDiagramReferenceController.ChangeLocal(cbLocal.IsChecked == true);
	        }

	        if (cbDiagram.SelectedItem != diagramReference.ReferencedDiagram)
	        {
	            psmDiagramReferenceController.ChangeReferencedDiagram((PSMDiagram) cbDiagram.SelectedItem);
	        }

	        modelController.CommitMacro();
	        DialogResult = true;
	        Close();
	    }
	}
}