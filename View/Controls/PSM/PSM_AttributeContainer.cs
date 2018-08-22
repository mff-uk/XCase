using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Controls.Containers;
using XCase.View.Interfaces;
using XCase.View.Geometries;

namespace XCase.View.Controls
{
    /// <summary>
    /// View representation of Attribute Container (XSEM)
    /// </summary>
	public class PSM_AttributeContainer : PSMElementViewBase, IPSMSubordinateComponentRepresentant
    {
        StackPanel ACSection;
        System.Windows.Shapes.Rectangle ACBorder;
        //Control resizeDecorator;
        private PSMAttributesContainer classProperties;

        #region Properties

        protected IPSMAttributesContainer ClassProperties
        {
            get
            {
                return classProperties;
            }
        }

        public override string ElementName
        {
            get;
            set;
        }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set 
            {
				base.IsSelected = value;
                if (value)
                {
                    if (HighlightByEffect) Effect = MediaLibrary.SelectedHighlight;
                    else ACBorder.Stroke = Brushes.Red;
                    //ACBorder.StrokeThickness = 3;
                    Canvas.SetZIndex(this, 2);
                    //resizeDecorator.Visibility = Visibility.Visible;
                }
                else
                {
                    if (HighlightByEffect) Effect = null;
                    else ACBorder.Stroke = Brushes.Black;
                    //ACBorder.StrokeThickness = 1;
                    CancelAllEdits();
                    Canvas.SetZIndex(this, 0);
                    //resizeDecorator.Visibility = Visibility.Collapsed;
                }
            }
        }

        public override NamedElementController Controller { get { return AttributeContainerController; } }

        public PSMAttributeContainer AttributeContainer { get { return AttributeContainerController != null ? AttributeContainerController.AttributeContainer : null; } }

        private PSM_AttributeContainerController controller;

        public PSM_AttributeContainerController AttributeContainerController
        {
            get { return controller; }
            set
            {
                Debug.Assert(controller == null, "Controller should be assigned only once.");
                Debug.Assert(value != null, "Controller can be assigned only with not null value.");
                controller = value;
                ClassProperties.AttributeController = controller;
            }
        }
        
        #endregion

		public PSMSubordinateComponent ModelSubordinateComponent
    	{
    		get { return (PSMSubordinateComponent)ModelElement; }
    	}

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
        {
            this.AttributeContainerController = (PSM_AttributeContainerController)controller;
            this.ViewHelper = (ClassViewHelper)viewHelper;
            XCaseCanvas.Children.Add(this);
			this.StartBindings();
			this.InitializeConnector();
        }

        private void InitTemplate()
        {
            base.Template = Application.Current.Resources["PSM_AttributeContainerTemplate"] as ControlTemplate;
            ApplyTemplate();
            PSM_AttributeContainerTemplate gr = Template.FindName("PSM_AttributeContainerGrid", this) as PSM_AttributeContainerTemplate;


            ACSection = gr.FindName("ACStackPanel") as StackPanel;
            ACBorder = gr.FindName("ACBorder") as System.Windows.Shapes.Rectangle;

            //resizeDecorator = gr.FindName("ResizeDecorator") as Control;
            //resizeDecorator.ApplyTemplate();
            //Grid g = resizeDecorator.Template.FindName("ResizeDecoratorGrid", resizeDecorator) as Grid;
            //foreach (ResizeThumb t in g.Children) t.belongsTo = this;

            connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
            connectorDecorator.ApplyTemplate();
            //g = connectorDecorator.Template.FindName("ConnectorDecoratorGrid", connectorDecorator) as Grid;
        }

        private void InitContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenuItem m = new ContextMenuItem("Remove");
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
            m.Click += delegate(object sender, RoutedEventArgs e) { AttributeContainerController.Remove(); };
            ContextMenu.Items.Add(m);

            ContextMenuItem moveToContentContainer = new ContextMenuItem("Move to content container");
            moveToContentContainer.Click += delegate
            {
                AttributeContainerController.MoveToContentContainer(null);
            };

            ContextMenu.Items.Add(moveToContentContainer);

            ContextMenuItem moveToContentChoice = new ContextMenuItem("Move to content choice");
            moveToContentChoice.Click += delegate
            {
                AttributeContainerController.MoveToContentChoice(null);
            };

            ContextMenu.Items.Add(moveToContentChoice);

            ContextMenu.Items.Add(new Separator());
            foreach (ContextMenuItem item in ContextMenuItems)
            {
                ContextMenu.Items.Add(item);
            }
        }
        
        public PSM_AttributeContainer(XCaseCanvas xCaseCanvas)
            : base(xCaseCanvas)
        {
            SizeChanged += delegate(object sender, SizeChangedEventArgs e) { xCaseCanvas.InvokeElementSizeChanged(this, e); };

            InitTemplate();

            InitContextMenu();

            classProperties = new PSMAttributesContainer(ACSection, xCaseCanvas);
            //Movable = false;
        }

    	private void CancelAllEdits()
        {
            ClassProperties.CancelEdit();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            CancelAllEdits();
        }

		[ViewHelperPropertyMapping("AttributesCollapsed")]
		public bool PropertiesCollapsed
		{
			get { return ClassProperties.Visibility == Visibility.Collapsed; }
			set
			{
				ClassProperties.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				//miCollapseAttributes.IsChecked = value;
			}
		}
    }
}
