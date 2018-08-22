using System.Windows.Controls;
using System.Windows;
using XCase.View.Interfaces;
using System.Windows.Media;
using System.Diagnostics;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;

namespace XCase.View.Controls
{
    /// <summary>
    /// View representation of Class Union (XSEM)
    /// </summary>
	public class PSM_ClassUnion : PSMElementViewBase
    {
    	readonly Border Border;

        #region Properties

        public override string ElementName
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
        {
            this.ClassUnionController = (PSM_ClassUnionController)controller;
			this.ViewHelper = (PSMElementViewHelper)viewHelper;
            XCaseCanvas.Children.Add(this);
			this.StartBindings();
			InitializeConnector();
        }

        public override NamedElementController Controller { get { return ClassUnionController; } }

        public PSMClassUnion ClassUnion { get { return ClassUnionController != null ? ClassUnionController.ClassUnion : null; } }

        private PSM_ClassUnionController controller;

        public PSM_ClassUnionController ClassUnionController
        {
            get { return controller; }
            set
            {
                Debug.Assert(controller == null, "Controller should be assigned only once.");
                Debug.Assert(value != null, "Controller can be assigned only with not null value.");
                controller = value;
            }
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
                    else Border.BorderBrush = Brushes.Red;
                    Canvas.SetZIndex(this, 2);
                }
                else
                {
                    if (HighlightByEffect) Effect = null;
                    else Border.BorderBrush = Brushes.Black;
                    Canvas.SetZIndex(this, 0);
                }
            }
        }
        #endregion

        public PSM_ClassUnion(XCaseCanvas xCaseCanvas)
            : base(xCaseCanvas)
        {
            #region Template init
            Template = Application.Current.Resources["PSM_ClassUnionTemplate"] as ControlTemplate;
            ApplyTemplate();

            PSM_ClassUnionTemplate gr = Template.FindName("PSM_ClassUnionGrid", this) as PSM_ClassUnionTemplate;
            Border = gr.FindName("Border") as Border;
            connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
            connectorDecorator.ApplyTemplate();
            #endregion

            ContextMenu = new ContextMenu();
            ContextMenuItem m = new ContextMenuItem("Remove");
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
            m.Click += delegate(object sender, RoutedEventArgs e) { ClassUnionController.Remove(); };
            ContextMenu.Items.Add(m);

            ContextMenu.Items.Add(new Separator());
            foreach (ContextMenuItem item in ContextMenuItems)
            {
                ContextMenu.Items.Add(item);
            }
        }


    }
}
