using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using XCase.View.Interfaces;
using System.Windows.Media;
using System.Diagnostics;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;

namespace XCase.View.Controls
{
    /// <summary>
    /// View representation of Content Container (XSEM)
    /// </summary>
	public class PSM_ContentContainer : PSMElementViewBase, IPSMSubordinateComponentRepresentant, IPSMSuperordinateComponentRepresentant, IF2Renamable
    {
    	readonly Border Border;
    	readonly EditableTextBox TextBox;

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
                    TextBox.Text = value;
                    //HACK: I don't know why I have to call this explicitily
                    this.UpdateLayout();
                    XCaseCanvas.InvokeElementSizeChanged(this, null);
                }
            }
        }

        public override NamedElementController Controller { get { return ContentContainerController; } }

        public PSMContentContainer ContentContainer { get { return ContentContainerController != null ? ContentContainerController.ContentContainer : null; } }

        private PSM_ContentContainerController controller;

        public PSM_ContentContainerController ContentContainerController
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

    	public PSMSubordinateComponent ModelSubordinateComponent
    	{
    		get { return (PSMSubordinateComponent)ModelElement; }
    	}

		public PSMSuperordinateComponent ModelSuperordinateComponent
		{
			get { return (PSMSuperordinateComponent)ModelElement; }
		}

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
        {
            this.ContentContainerController = (PSM_ContentContainerController)controller;
            this.ViewHelper = (ClassViewHelper)viewHelper;
            XCaseCanvas.Children.Add(this);
			this.StartBindings();
			this.UpdateComponentsConnectors();
			InitializeConnector();
        }
        
        public PSM_ContentContainer(XCaseCanvas xCaseCanvas)
            : base(xCaseCanvas)
        {
            #region Template Init
            base.Template = Application.Current.Resources["PSM_ContentContainerTemplate"] as ControlTemplate;
            ApplyTemplate();

            PSM_ContentContainerTemplate gr = Template.FindName("PSM_ContentContainerGrid", this) as PSM_ContentContainerTemplate;

            Border = gr.FindName("Border") as Border;
            TextBox = gr.FindName("txtName") as EditableTextBox;

            connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
            connectorDecorator.ApplyTemplate();
            #endregion

            TextBox.mi_Rename.Header = "Rename content container";
            
            ContextMenuItem m = new ContextMenuItem("Remove content container");
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
            m.Click += delegate(object sender, RoutedEventArgs e) { ContentContainerController.Remove(); };
            TextBox.ContextMenu.Items.Add(m);

            ContextMenuItem moveToContentContainer = new ContextMenuItem("Move to content container");
            moveToContentContainer.Click += delegate
            {
                ContentContainerController.MoveToContentContainer(null);
            };

            TextBox.ContextMenu.Items.Add(moveToContentContainer);

            ContextMenuItem moveToContentChoice = new ContextMenuItem("Move to content choice");
            moveToContentChoice.Click += delegate
            {
                ContentContainerController.MoveToContentChoice(null);
            };

            TextBox.ContextMenu.Items.Add(moveToContentChoice);



            TextBox.ContextMenu.Items.Add(new Separator());
            foreach (ContextMenuItem item in ContextMenuItems)
            {
                TextBox.ContextMenu.Items.Add(item);
            }

			TextBox.TextEdited += delegate(object sender, StringEventArgs args) { Controller.RenameElement<NamedElement>(args.Data, null); XCaseCanvas.InvokeElementSizeChanged(this, null); };
        }

        public new ContextMenu ContextMenu
        {
            get
            {
                return TextBox.ContextMenu;
            }
        }

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			TextBox.myEditable = false;
		}

        public void F2Rename()
        {
            TextBox.myEditable = true;
        }
    }
}
