using System;
using System.Windows;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using System.Windows.Media;
using System.Windows.Input;

namespace XCase.View.Controls
{
	/// <summary>
	/// Represents comments on canvas.
	/// </summary>
	[ViewHelperPropertyMapping("X", "X")]
	[ViewHelperPropertyMapping("Y", "Y")]
	[ViewHelperPropertyMapping("Width", "Width")]
	[ViewHelperPropertyMapping("Height", "Height")]
	public class XCaseComment : ConnectableDragThumb, ISelectable, IModelElementRepresentant, IResizable, IAlignable, IF2Renamable
	{
	    #region FIELDS

	    readonly Border CommentBorder;
	    readonly EditableTextBox CommentTextBox;
	    readonly Control resizeDecorator;

	    private bool selected;

	    private XCaseJunction junction;
	    private XCasePrimitiveJunction primitiveJunction;

	    #endregion

        public Comment Comment { get { return Controller.Comment; } }

        /// <summary>
        /// Initializes a view representation of a model element
        /// </summary>
        /// <param name="modelElement">Element to be represented</param>
        /// <param name="viewHelper">Element's viewHelper</param>
        /// <param name="controller">Element's controller</param>
        public void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller)
	    {
            XCaseCanvas.Children.Add(this);
	        Controller = (CommentController)controller;
            ViewHelper = (CommentViewHelper)viewHelper;
			/* CommentText must be assigned now :( because it defines comment dimension
			 * which have to be known before initializing the junction */
        	this.CommentText = ((Comment) modelElement).Body;
            if (ModelComment.AnnotatedElement != null && !(ModelComment.AnnotatedElement is Package))
            {
                X = ViewHelper.X;
                Y = ViewHelper.Y;
                UpdateLayout();
                if (XCaseCanvas.ElementRepresentations[ModelComment.AnnotatedElement] is DragThumb)
                    SnapTo((DragThumb)XCaseCanvas.ElementRepresentations[ModelComment.AnnotatedElement], false);

                object target = XCaseCanvas.ElementRepresentations[ModelComment.AnnotatedElement];
				
                if (target is PIM_Association && ((PIM_Association)target).ViewHelper.UseDiamond)
                {
                    target = ((PIM_Association)target).Diamond;
                }
                if (target is IConnectable)
                {
                    if (ViewHelper.LinePoints.Count == 0)
                    {
                        ViewHelper.LinePoints.AppendRange(JunctionGeometryHelper.ComputeOptimalConnection(this, (IConnectable)target));
                    }
                    junction = new XCaseJunction(XCaseCanvas, ViewHelper.LinePoints) { Pen = MediaLibrary.DashedBlackPen };
                	XCaseCanvas.Children.Add(junction);
                    junction.NewConnection(this, null, (IConnectable)target, null, ViewHelper.LinePoints);
                }
                else if (target is IPrimitiveJunctionTarget)
                {
                    primitiveJunction = new XCasePrimitiveJunction(XCaseCanvas, this, (IPrimitiveJunctionTarget)target);
                }
                else
                {
                    throw new ArgumentException("Commentary can be connected only to IConnectable or IPrimitiveJunctionTarget");
                }
            }
			this.StartBindings();
	    }

	    public XCaseComment(XCaseCanvas xCaseCanvas)
	        : base(xCaseCanvas)
	    {
	        #region Commentary Template Init

	        Template = (ControlTemplate)Application.Current.Resources["XCaseCommentaryTemplate"];
	        ApplyTemplate();

	        XCaseCommentTemplate gr = (XCaseCommentTemplate)Template.FindName("CommentaryGrid", this);

	        if (gr != null)
	        {
	            CommentBorder = gr.FindName("CommentBorder") as Border;
	            CommentTextBox = (EditableTextBox)gr.FindName("txtText");

	            resizeDecorator = (Control)gr.FindName("ResizeDecorator");
	            resizeDecorator.ApplyTemplate();
	            Grid g = (Grid)resizeDecorator.Template.FindName("ResizeDecoratorGrid", resizeDecorator);
	            foreach (ResizeThumb t in g.Children) t.belongsTo = this;

	            connectorDecorator = (Control)gr.FindName("ConnectorDecorator");
	            connectorDecorator.ApplyTemplate();
	        }

	        #endregion

	        #region Commentary Context Menu

            ContextMenuItem m = new ContextMenuItem("Remove from diagram");
            m.Icon = ContextMenuIcon.GetContextIcon("delete2");
            m.Click += new RoutedEventHandler(Remove_Click);
            CommentTextBox.ContextMenu.Items.Add(m);
            CommentTextBox.mi_Rename.Header = "Change";

	        #endregion

	        CommentTextBox.TextEdited += delegate(object sender, StringEventArgs args) { Controller.ChangeComment(args.Data); };
	        PositionChanged += delegate { CommentTextBox.myEditable = false; };
	    }

        void Remove_Click(object sender, RoutedEventArgs e)
        {
            Controller.Remove();
        }

	    public bool IsSelected
	    {
	        get { return selected; }
	        set
	        {
	            selected = value;
	            if (selected)
	            {
	                CommentBorder.BorderBrush = Brushes.Red;
	                Canvas.SetZIndex(this, 2);
	                resizeDecorator.Visibility = Visibility.Visible;
	            }
	            else
	            {
	                CommentBorder.BorderBrush = Brushes.Black;
	                CommentTextBox.myEditable = false;
	                Canvas.SetZIndex(this, 0);
	                resizeDecorator.Visibility = Visibility.Collapsed;
	            }
	        }
	    }

		[ModelElement]
		public Comment ModelComment { get { return Controller != null ? Controller.Comment : null; } }

		[ViewHelperElement]
		public CommentViewHelper ViewHelper { get; protected set; }

		public CommentController Controller { get; protected set; }

		PositionableElementViewHelper IResizable.ViewHelper { get { return ViewHelper; } }

		PositionableElementViewHelper IAlignable.ViewHelper { get { return ViewHelper; } }

		[ModelPropertyMapping("Body")]
		public string CommentText
		{
			get { return CommentTextBox.Text; }
			set { CommentTextBox.Text = value; }
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			CommentTextBox.myEditable = false;
		}

		#region IDeletable Members

		public override void DeleteFromCanvas()
		{
			if (junction != null)
			{
				junction.DeleteFromCanvas();
			}
			if (primitiveJunction != null)
			{
				primitiveJunction.DeleteFromCanvas();
			}
			base.DeleteFromCanvas();
			this.CloseBindings();
		}

		#endregion

        public void F2Rename()
        {
            CommentTextBox.myEditable = true;
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            if (primitiveJunction != null)
            {
                primitiveJunction.Visibility = Visibility.Visible;
            }
            if (junction != null)
            {
                junction.Visibility = Visibility.Visible;
            }
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            if (primitiveJunction != null)
            {
                primitiveJunction.Visibility = Visibility.Collapsed;
            }
            if (junction != null)
            {
                junction.Visibility = Visibility.Collapsed;
            }
        }

        #region Versioned element highlighting support

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            XCaseCanvas.InvokeVersionedElementMouseEnter(this, Comment);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            XCaseCanvas.InvokeVersionedElementMouseLeave(this, Comment);
        }

        #endregion 
	}
}
