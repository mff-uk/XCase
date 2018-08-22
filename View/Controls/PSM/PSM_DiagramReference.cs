using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using XCase.Controller.Dialogs;
using XCase.View.Interfaces;
using System.Windows.Media;
using System.Diagnostics;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;

namespace XCase.View.Controls
{
    /// <summary>
    /// View representation of Diagram reference
    /// </summary>
    public class PSM_DiagramReference : PSMElementViewBase
    {
        readonly Border Border;
        readonly EditableTextBox TextBox;
        private readonly EditableTextBox tLocal;
        private readonly EditableTextBox tSchemaLocation;
        private readonly EditableTextBox tNamespace;
        private readonly EditableTextBox tNamespacePrefix;
        private readonly EditableTextBox tReferencedDiagram;

        #region Properties

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                if (value)
                {
                    if (HighlightByEffect) Effect = MediaLibrary.SelectedHighlight;
                    else Border.BorderBrush = Brushes.Red;
                    Canvas.SetZIndex(this, 2);
                }
                else
                {
                    if (HighlightByEffect) Effect = null;
                    else Border.BorderBrush = Brushes.Black;
                    TextBox.myEditable = false;
                    Canvas.SetZIndex(this, 0);
                }
            }
        }

        public override string ElementName
        {
            get { return TextBox.Text; }
            set
            {
                if (TextBox.Text != value)
                {
                    UpdateTextFields();
                    //HACK: I don't know why I have to call this explicitily
                    this.UpdateLayout();
                    XCaseCanvas.InvokeElementSizeChanged(this, null);
                }
            }
        }

        private PSMDiagram referencedDiagram;

        [ModelPropertyMappingAttribute("ReferencedDiagram")]
        public PSMDiagram ReferencedDiagram
        {
            get { return referencedDiagram; }
            set
            {
                referencedDiagram = value;
                UpdateTextFields();
            }
        }

        private string schemaLocation;
        [ModelPropertyMappingAttribute("SchemaLocation")]
        public string SchemaLocation
        {
            get { return schemaLocation; }
            set { schemaLocation = value; UpdateTextFields(); }
        }

        private bool local;
        [ModelPropertyMappingAttribute("Local")]
        public bool Local
        {
            get { return local; }
            set
            {
                local = value;
                UpdateTextFields();
            }
        }

        public override NamedElementController Controller { get { return DiagramReferenceController; } }

        public PSMDiagramReference DiagramReference { get { return DiagramReferenceController != null ? DiagramReferenceController.DiagramReference : null; } }

        private PSM_DiagramReferenceController controller;

        public PSM_DiagramReferenceController DiagramReferenceController
        {
            get { return controller; }
            set
            {
                Debug.Assert(controller == null, "Controller should be assigned only once.");
                Debug.Assert(value != null, "Controller can be assigned only with not null value.");
                controller = value;
            }
        }
        #endregion

        private void UpdateTextFields()
        {
            tLocal.Text = local ? "Local" : "External";
            tLocal.ToolTip = local ? "Local" : "External";
            tSchemaLocation.Text = schemaLocation;
            tSchemaLocation.ToolTip = schemaLocation;
            TextBox.Text = String.Format("{0}", DiagramReference.Name);
            TextBox.ToolTip = String.Format("{0}", DiagramReference.Name);
            tReferencedDiagram.Text = DiagramReference.ReferencedDiagram.Caption;
            tReferencedDiagram.ToolTip = DiagramReference.ReferencedDiagram.Caption;
            tNamespace.Text = DiagramReference.Namespace;
            tNamespace.ToolTip = DiagramReference.Namespace;
            tNamespacePrefix.Text = DiagramReference.NamespacePrefix;
            tNamespacePrefix.ToolTip = DiagramReference.NamespacePrefix;
        }

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
        {
            this.DiagramReferenceController = (PSM_DiagramReferenceController)controller;
            this.ViewHelper = (ClassViewHelper)viewHelper;
            XCaseCanvas.Children.Add(this);
            this.StartBindings();
            InitContextMenu();
            TextBox.MouseDoubleClick += delegate { miProperties.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); };
        }

        ContextMenuItem miProperties, miDelete;

        private void InitContextMenu()
        {
            miProperties = new ContextMenuItem("Properties");
            miProperties.Icon = ContextMenuIcon.GetContextIcon("properties");
            miProperties.Click += delegate
                                      {
                                          new DiagramReferenceDialog(this.DiagramReference,
                                                                     this.DiagramReferenceController,
                                                                     this.DiagramReferenceController.DiagramController.
                                                                         ModelController).ShowDialog();
                                      };
            TextBox.ContextMenu.Items.Add(miProperties);

            miDelete = new ContextMenuItem("Remove");
            miDelete.Icon = ContextMenuIcon.GetContextIcon("delete2");
            miDelete.Click += Remove;
            this.ContextMenu.Items.Add(miDelete);

        }

        void Remove(object sender, RoutedEventArgs e)
        {
            Controller.Remove();
        }

        public PSM_DiagramReference(XCaseCanvas xCaseCanvas)
            : base(xCaseCanvas)
        {
            #region Template Init
            base.Template = Application.Current.Resources["PSM_DiagramReferenceTemplate"] as ControlTemplate;
            ApplyTemplate();

            PSM_DiagramReferenceTemplate gr = Template.FindName("PSM_DiagramReferenceGrid", this) as PSM_DiagramReferenceTemplate;

            Border = gr.FindName("Border") as Border;
            TextBox = gr.FindName("txtName") as EditableTextBox;

            tLocal = gr.FindName("tLocal") as EditableTextBox;
            tSchemaLocation = gr.FindName("tSchemaLocation") as EditableTextBox;
            tNamespacePrefix = gr.FindName("tNamespacePrefix") as EditableTextBox;
            tNamespace = gr.FindName("tNamespace") as EditableTextBox;
            tReferencedDiagram = gr.FindName("tReferencedDiagram") as EditableTextBox;

            connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
            connectorDecorator.ApplyTemplate();
            #endregion
        }

        public new ContextMenu ContextMenu
        {
            get
            {
                return TextBox.ContextMenu;
            }
        }
    }
}
