using System.Windows;
using System.Windows.Controls;
using XCase.Model;
using XCase.Controller;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using System.Windows.Media;
 
namespace XCase.View.Controls
{
	/// <summary>
	/// Floating label used to display properties of associations
	/// and association ends.
	/// </summary>
	[ViewHelperPropertyMapping("X", "X")]
	[ViewHelperPropertyMapping("Y", "Y")]
	[ViewHelperPropertyMapping("Width", "Width")]
	[ViewHelperPropertyMapping("Height", "Height")]
	public class AssociationLabel : 
		DragThumb, ISelectable, IResizable, IBindable, IAlignable, IHasBounds
	{
		/// <summary>
		/// Association to which the label belongs to (if this label belongs to PIM association).
		/// Either <see cref="Association"/> or <see cref="PSM_Association"/>
		/// will be null.
		/// </summary>
		public PIM_Association Association { get; set; }
		
		/// <summary>
		/// Association to which the label belongs to (if this label belongs to PSM association).
		/// Either <see cref="Association"/> or <see cref="PSM_Association"/>
		/// will be null.
		/// </summary>
		public PSM_Association PSM_Association { get; set; }

		public AssociationLabel(XCaseCanvas xCaseCanvas, AssociationLabelViewHelper labelViewHelper) :
			base(xCaseCanvas)
		{
			FontWeight = FontWeights.Bold;

			Margin = new Thickness(0);

			Template = (ControlTemplate)Application.Current.Resources["XCaseJunctionLabelTemplate"];
			ApplyTemplate();

			XCaseJunctionLabelTemplate gr = (XCaseJunctionLabelTemplate)Template.FindName("JunctionLabelGrid", this);
			
			TextBox = gr.txtText;

			OriginalTextBrush = Brushes.Blue;
			ViewHelper = labelViewHelper;
			FocusVisualStyle = null;
			
			this.StartBindings(TypeBindingData.EBindingSourceType.View);
		}

		#region ISelectable Members

		private bool selected;

		/// <summary>
		/// Selected flag. Selected elements are highlighted on the canvas and are
		/// target of commands.
		/// </summary>
		/// <value></value>
		public bool IsSelected
		{
			get { return selected; }
			set
			{
				selected = value;
				if (selected)
				{
					Canvas.SetZIndex(this, 2);
					//resizeDecorator.Visibility = Visibility.Visible;
				}
				else
				{
					Canvas.SetZIndex(this, 0);
					//resizeDecorator.Visibility = Visibility.Hidden;
				}
			}
		}

		#endregion

		private XCasePrimitiveJunction highlightJunction;

		/// <summary>
		/// </summary>
		/// <value><see cref="AssociationEnd"/></value>
		public AssociationEnd AssociationEnd { get; internal set; }

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.Input.Mouse.PreviewMouseDownEvent"/> attached event reaches 
		/// an element in its route that is derived from this class. 
		/// The event drops dragging and this method stops highlighting the association to which the association end belongs.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
		protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			IPrimitiveJunctionTarget targetJunction = null;
			if (Association != null)
			{
				Association.Highlight();
			}
			if (PSM_Association != null)
			{
				PSM_Association.Junction.Highlight();
			}
			if (highlightJunction == null)
			{
				if (Association != null && AssociationEnd != null)
				{
					targetJunction = Association.GetJunctionConnectingEnd(AssociationEnd);
				}
				if (PSM_Association != null)
				{
					targetJunction = PSM_Association;
				}
				if (targetJunction != null)
				{
					highlightJunction = new XCasePrimitiveJunction(XCaseCanvas, this, targetJunction)
					                    	{ Pen = MediaLibrary.DashedBlackPen };
				}			
			}
			if (highlightJunction != null)
				highlightJunction.Visibility = Visibility.Visible;
			base.OnPreviewMouseDown(e);
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.Input.Mouse.PreviewMouseUpEvent"/> attached event reaches 
		/// an element in its route that is derived from this class. 
		/// The event starts dragging and during dragging the association to which the association end belongs is highlighted.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that one or more mouse buttons were released.</param>
		protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (Association != null)
			{
				Association.UnHighlight();
			}
			if (PSM_Association != null)
			{
				PSM_Association.Junction.UnHighlight();
			}
			if (highlightJunction != null)
					highlightJunction.Visibility = Visibility.Hidden;
			base.OnPreviewMouseUp(e);
		}

		/// <summary>
		/// TextBox that is a part of the label (allows in-place editing)
		/// </summary>
		/// <value><see cref="EditableTextBox"/></value>
		public EditableTextBox TextBox { get; private set; }

		#region delegations to ClassTextBox
		public string Text
		{
			get { return TextBox.Text; }
			set { TextBox.Text = value; }
		}

		/// <summary>
		/// Text alignment
		/// </summary>
		public TextAlignment TextAlignment
		{
			get { return TextBox.TextAlignment; }
			set { TextBox.TextAlignment = value; }
		}

		/// <summary>
		/// Bacground brush for normal state (not editing)
		/// </summary>
		/// <value><see cref="Brush"/></value>
		public Brush OriginalBackgroundBrush
		{
			get { return TextBox.OriginalBackgroundBrush; }
			set { TextBox.OriginalBackgroundBrush = value; }
		}

		/// <summary>
		/// Brush for drawing text 
		/// </summary>
		/// <value><see cref="Brush"/></value>
		public Brush OriginalTextBrush
		{
			get { return TextBox.OriginalTextBrush; }
			set { TextBox.OriginalTextBrush = value; }
		}

		/// <summary>
		/// Gets or sets a value specifying whether return characters can be inserted into text.
		/// </summary>
		public bool AcceptsReturn
		{
			get { return TextBox.AcceptsReturn; }
			set { TextBox.AcceptsReturn = value; }
		}
		#endregion

		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		[ViewHelperElement]
		public AssociationLabelViewHelper ViewHelper { get; set; }

		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		PositionableElementViewHelper IResizable.ViewHelper { get { return ViewHelper;  } }

		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		PositionableElementViewHelper IAlignable.ViewHelper { get { return ViewHelper; } }

		/// <summary>
		/// Gets or sets a value indicating whether the label is visible
		/// </summary>
		/// <value><c>true</c> if whether the label is visible; otherwise, <c>false</c>.</value>
		[ViewHelperPropertyMapping("LabelVisible")]
		public bool LabelVisible
		{
			get { return  Visibility == Visibility.Visible; }
			set
			{
				Visibility = value ? Visibility.Visible : Visibility.Hidden;
			}
		}

		/// <summary>
		/// When bounding rectangle of the control is tilted, this property returns the
		/// angle in degrees. For this control property always returns 0.
		/// </summary>
		/// <value><see cref="double"/></value>
		public double BoundsAngle
		{
			get
			{
				return 0;
			}
		}
	}
}