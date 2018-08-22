using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls.Containers;
using XCase.View.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace XCase.View.Controls
{
	/// <summary>
	/// View representation of PIM Class (UML Class)
	/// </summary>
	public class PIM_Class : XCaseViewBase, ICreatesAssociationEnd, IF2Renamable
	{
		#region visual elements
		protected StackPanel AttributesSection, OperationsSection;
        protected Border PropertiesBorder, MethodsBorder, HeaderBorder;
		protected AttributesContainer classAttributes;
        protected OperationsContainer classOperations;
        protected EditableTextBox txtClassName;
        protected Control resizeDecorator;
        protected ContextMenuItem miCollapseAttributes, miCollapseOperations;
        private ContextMenuItem mDerive;
        #endregion

		#region Properties

		/// <summary>
		/// Container managing textboxes displaying attributes
		/// </summary>
		/// <value><see cref="IAttributesContainer"/></value>
		protected IAttributesContainer ClassAttributes
		{
			get
			{
				return classAttributes;
			}
		}

		/// <summary>
		/// Container managing textboxes displaying operations
		/// </summary>
		/// <value><see cref="IAttributesContainer"/></value>
		protected IOperationsContainer ClassOperations
		{
			get
			{
				return classOperations;
			}
		}

		private bool selected;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are
		/// target of commands.
		/// </summary>
		/// <value></value>
		public override bool IsSelected
		{
			get { return selected; }
			set
			{
				selected = value;
				if (selected)
				{
					Canvas.SetZIndex(this, 2);
					resizeDecorator.Visibility = Visibility.Visible;
				}
				else
				{
					CancelAllEdits();
					Canvas.SetZIndex(this, 0);
					resizeDecorator.Visibility = Visibility.Hidden;
				}
			}
		}

		/// <summary>
		/// Name of the element.
		/// </summary>
		/// <value></value>
		public override string ElementName
		{
			get { return txtClassName.Text; }
			set { txtClassName.Text = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether attributes container is collapsed.
		/// </summary>
		/// <value><c>true</c> if attributes container is collapsed; otherwise, <c>false</c>.</value>
		[ViewHelperPropertyMapping("AttributesCollapsed")]
		public bool AttributesCollapsed
		{
			get { return ClassAttributes.Visibility == Visibility.Collapsed; }
			set
			{
				ClassAttributes.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				miCollapseAttributes.IsChecked = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether operations container is collapsed.
		/// </summary>
		/// <value><c>true</c> if operations container is collapsed; otherwise, <c>false</c>.</value>
		[ViewHelperPropertyMapping("OperationsCollapsed")]
		public bool OperationsCollapsed
		{
			get { return ClassOperations.Visibility == Visibility.Collapsed; }
			set
			{
				ClassOperations.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				miCollapseOperations.IsChecked = value;
			}
		}

		private ClassController classController;

		/// <summary>
		/// Controller for the represented element
		/// </summary>
		/// <value></value>
		public override NamedElementController Controller { get { return ClassController; } }

		/// <summary>
		/// Controller for the represented element.
		/// </summary>
		/// <value><see cref="ClassController"/></value>
		public ClassController ClassController
		{
			get { return classController; }
			private set
			{
				classController = value;
				ClassAttributes.AttributeController = classController;
				ClassOperations.ClassController = classController;
			}
		}


		#endregion

	    /// <summary>
	    /// Initializes a view representation of a model element
	    /// </summary>
	    /// <param name="modelElement">Element to be represented</param>
	    /// <param name="viewHelper">Element's viewHelper</param>
	    /// <param name="controller">Element's controller</param>
        public override void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
	    {
	        this.ClassController = (ClassController)controller;
	        this.ViewHelper = (ClassViewHelper)viewHelper;
	    	this.StartBindings();
			XCaseCanvas.Children.Add(this);
	    }

	    public PIM_Class(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
            InitTemplate();

			classAttributes = new AttributesContainer(AttributesSection, xCaseCanvas);
			classOperations = new OperationsContainer(OperationsSection, xCaseCanvas);
			Border[] stackBorders = new Border[] { HeaderBorder, PropertiesBorder, MethodsBorder };
			ITextBoxContainer[] stackContainers = new ITextBoxContainer[] { classAttributes, classOperations };
			classAttributes.StackBorders = stackBorders;
			classAttributes.StackContainers = stackContainers;
			classOperations.StackBorders = stackBorders;
			classOperations.StackContainers = stackContainers;

            InitContextMenu();

			txtClassName.TextEdited += delegate(object sender, StringEventArgs args) { Controller.RenameElement<PIMClass>(args.Data, ((PIMClass)ModelElement).Package.Classes);  };
			PositionChanged += CancelAllEdits;
	    	txtClassName.MouseDoubleClick += txtClassName_MouseDoubleClick;
		}

        /// <summary>
        /// Initializes PIM Classes Context Menus
        /// </summary>
        protected virtual void InitContextMenu()
        {
            ContextMenuItem m = new ContextMenuItem("Remove");
            m.Click += new RoutedEventHandler(Remove);
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
			txtClassName.ContextMenu.Items.Add(m);
            txtClassName.ContextMenu.Items.Add(new Separator());

            m = new ContextMenuItem("Autosize");
            m.Click += delegate(object sender, RoutedEventArgs e) { ViewController.ResizeElement(double.NaN, double.NaN, ViewHelper, XCaseCanvas.Controller); };
            txtClassName.ContextMenu.Items.Add(m);
            txtClassName.ContextMenu.Items.Add(new Separator());

            miCollapseAttributes = new ContextMenuItem("Hide attributes") { IsCheckable = true };
            txtClassName.ContextMenu.Items.Add(miCollapseAttributes);
            miCollapseAttributes.Checked += 
                delegate (object sender, RoutedEventArgs e)
	            {
		            if (!AttributesCollapsed)
			            ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.CollapseAttributes, XCaseCanvas.Controller);
	            };
            miCollapseAttributes.Unchecked += 
                delegate (object sender, RoutedEventArgs e)
	            {
		            if (AttributesCollapsed)
			            ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.ShowAttributes, XCaseCanvas.Controller);
	            };

            miCollapseOperations = new ContextMenuItem("Hide operations") { IsCheckable = true };
            txtClassName.ContextMenu.Items.Add(miCollapseOperations);
            miCollapseOperations.Checked += 
                delegate(object sender, RoutedEventArgs e)
                {
                    if (!OperationsCollapsed)
                        ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.CollapseOperations, XCaseCanvas.Controller);
                };
            miCollapseOperations.Unchecked += 
                delegate(object sender, RoutedEventArgs e)
                {
                    if (OperationsCollapsed)
                        ViewController.ChangeSectionVisibility(ViewHelper, SectionVisibilityCommand.ESectionVisibilityAction.ShowOperations, XCaseCanvas.Controller);
                };
            
			txtClassName.ContextMenu.Items.Add(new Separator());
			foreach (ContextMenuItem item in classAttributes.PropertiesMenuItems)
			{
				txtClassName.ContextMenu.Items.Add(item);
			}
            foreach (ContextMenuItem item in classOperations.OperationsMenuItems)
            {
                txtClassName.ContextMenu.Items.Add(item);
            }
			foreach (ContextMenuItem item in ContextMenuItems)
			{
				txtClassName.ContextMenu.Items.Add(item);
			}
            txtClassName.ContextMenu.Items.Add(new Separator());
            mDerive = new ContextMenuItem("Derive PSM class to...");
            mDerive.Icon = ContextMenuIcon.GetContextIcon("class");
            txtClassName.ContextMenuOpening += new ContextMenuEventHandler(mDerive_ContextMenuOpening);
            txtClassName.ContextMenu.Items.Add(mDerive);
            txtClassName.ContextMenu.Items.Add(new Separator());
            
            ContextMenuItem miProperties = new ContextMenuItem("Properties...");
            miProperties.Icon = ContextMenuIcon.GetContextIcon("props");
            miProperties.Click += delegate { ClassController.ShowClassDialog(); };
            txtClassName.ContextMenu.Items.Add(miProperties);

        }

		/// <summary>
		/// Inits the template.
		/// </summary>
        protected virtual void InitTemplate()
        {
            Template = Application.Current.Resources["PIM_ClassTemplate"] as ControlTemplate;
            ApplyTemplate();
            PIM_ClassTemplate gr = Template.FindName("ClassGrid", this) as PIM_ClassTemplate;

            if (gr != null)
            {
                AttributesSection = gr.FindName("AttributesSection") as StackPanel;
                OperationsSection = gr.FindName("OperationsSection") as StackPanel;
                PropertiesBorder = gr.FindName("AttributesBorder") as Border;
                MethodsBorder = gr.FindName("OperationsBorder") as Border;
                HeaderBorder = gr.FindName("headerBorder") as Border;
                txtClassName = gr.FindName("txtClassName") as EditableTextBox;
                resizeDecorator = gr.FindName("ResizeDecorator") as Control;
            }
            else
            {
                Debug.Assert(false, "Template not found");
            }

            resizeDecorator.ApplyTemplate();
            Grid g = resizeDecorator.Template.FindName("ResizeDecoratorGrid", resizeDecorator) as Grid;
            foreach (ResizeThumb t in g.Children) t.belongsTo = this;
            connectorDecorator = gr.FindName("ConnectorDecorator") as Control;
            connectorDecorator.ApplyTemplate();
        }
            
		#region Context Menu Handling
        void Remove(object sender, RoutedEventArgs e)
        {
            Controller.Remove();
        }
        
        void mDerive_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenuDiagramItem m;
            mDerive.Items.Clear();
            foreach (PSMDiagram d in Controller.DiagramController.Project.PSMDiagrams)
            {
                m = new ContextMenuDiagramItem(d.Caption, d);
                m.Click += new RoutedEventHandler(Derive_Click);
                mDerive.Items.Add(m);
            }
            if (Controller.DiagramController.Project.PSMDiagrams.Count > 0) mDerive.Items.Add(new Separator());

            m = new ContextMenuDiagramItem("New PSM diagram", null);
            m.Icon = ContextMenuIcon.GetContextIcon("page_white");
            m.Click += new RoutedEventHandler(Derive_Click);
            mDerive.Items.Add(m);

        }

        void Derive_Click(object sender, RoutedEventArgs e)
        {
            ContextMenuDiagramItem m = sender as ContextMenuDiagramItem;
            if (m.Diagram == null)
            {
                ClassController.DerivePSMClassToNewDiagram();
            }
            else
            {
                ClassController.DerivePSMClassToDiagram(m.Diagram as PSMDiagram);
            }
        }

        #endregion

		private void CancelAllEdits()
		{
			ClassAttributes.CancelEdit();
			ClassOperations.CancelEdit();
			txtClassName.myEditable = false;
		}

		/// <summary>
		/// Deletes the control from canvas (with all the JunctionPoints that it created via
		/// <see cref="ConnectableDragThumb.CreateJunctionEnd()"/>).
		/// </summary>
		public override void DeleteFromCanvas()
		{
			base.DeleteFromCanvas();
			this.CloseBindings();
		}

		/// <summary>
		/// Creates visualization for an association end.
		/// </summary>
		/// <param name="preferedPosition">The prefered position of the created point.</param>
		/// <param name="viewHelper">association end's view helper.</param>
		/// <param name="associationEnd">model association end.</param>
		/// <returns>new control representing association end</returns>
		public PIM_AssociationEnd CreateAssociationEnd(Point preferedPosition, AssociationEndViewHelper viewHelper, AssociationEnd associationEnd)
		{
			PIM_AssociationEnd xend = new PIM_AssociationEnd(XCaseCanvas, viewHelper, associationEnd) { OwnerControl = this, Placement = EPlacementKind.ParentAutoPos, ParentControl = this };
			((Canvas)connectorDecorator.Template.FindName("ConnectorDecoratorGrid", connectorDecorator)).Children.Add(xend);
			xend.Visibility = Visibility.Visible;
			xend.SetPreferedPosition(preferedPosition);
			createdJunctionEnds.Add(xend);
			return xend;
		}

        #region Event handlers

		/// <summary>
		/// Handles the MouseDoubleClick event of the txtClassName control - shows class dialog.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void txtClassName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			ClassController.ShowClassDialog();
        }

        #endregion

		/// <summary>
		/// Sets the control in a state where name can be edited.
		/// </summary>
        public void F2Rename()
        {
            txtClassName.myEditable = true;
        }
	}
}
