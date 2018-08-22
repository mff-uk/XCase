using System.Windows.Controls;
using System.Windows;
using XCase.Model;
using XCase.View.Interfaces;
using System.Diagnostics;
using XCase.Controller;

namespace XCase.View.Controls
{
	/// <summary>
	/// Association diamond is a simple control used to display associations
	/// with more than two ends. Each end is then connected to the AssociationDiamond via <see cref="XCaseJunction"/>
	/// </summary>
	[ViewHelperPropertyMapping("X", "X")]
	[ViewHelperPropertyMapping("Y", "Y")]
	[ViewHelperPropertyMapping("Width", "Width")]
	[ViewHelperPropertyMapping("Height", "Height")]
	public class AssociationDiamond : ConnectableDragThumb, ISelectable, IResizable, IDeletable, IAlignable, IRepresentsIndirect
	{
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
					Highlight();
					if (!Association.IsSelected)
						Association.IsSelected = true;
					Canvas.SetZIndex(this, 2);
				}
				else
				{
					UnHighlight();
					if (Association.IsSelected)
						Association.IsSelected = false;
					Canvas.SetZIndex(this, 0);
				}
			}
		}

		/// <summary>
		/// Returns association or association clss to whcih 
		/// this diamond belongs.
		/// </summary>
		public IModelElementRepresentant RepresentedElement
		{
			get
			{
				if (Association != null)
				{
					if (Association.AssociationClass != null)
						return Association.AssociationClass;
					return Association;
				}
				return null;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationDiamond"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">Canvas where the control is placed</param>
		/// <param name="association">Association belonging to the association diamond</param>
		/// <param name="viewHelper">ViewHelper for the control</param>
		public AssociationDiamond(XCaseCanvas xCaseCanvas, PIM_Association association, PositionableElementViewHelper viewHelper)
			: base(xCaseCanvas)
		{
			#region AssociationDiamond Template init
			Template = (ControlTemplate)Application.Current.Resources["AssociationDiamondTemplate"];
			ApplyTemplate();

			AssociationDiamondTemplate gr = (AssociationDiamondTemplate)Template.FindName("AssociationDiamondGrid", this);

			connectorDecorator = (Control)gr.FindName("ConnectorDecorator");
			connectorDecorator.ApplyTemplate();
			#endregion

			Association = association;
			this.viewHelper = viewHelper;

			this.StartBindings(TypeBindingData.EBindingSourceType.View);
		}

		/// <summary>
		/// Association belonging to this association diamond
		/// </summary>
		/// <value><see cref="PIM_Association"/></value>
		public PIM_Association Association { get; private set; }

		private readonly PositionableElementViewHelper viewHelper;

		/// <summary>
		/// ViewHelper for the control (diamond can be dragged around on the canvas)
		/// </summary>
		/// <value><see cref="PositionableElementViewHelper"/></value>
		[ViewHelperElement]
		public PositionableElementViewHelper ViewHelper
		{
			get
			{
				return viewHelper;
			}
		}

		/// <summary>
		/// Returns value of 45 degrees (the control is tilted)
		/// </summary>
		/// <value><see cref="double"/>45</value>
		public override double BoundsAngle
		{
			get
			{
				return 45;
			}
		}

		/// <summary>
		/// Deletes the control from canvas (with all the JunctionPoints that it created via
		/// <see cref="IConnectable.CreateJunctionEnd()"/>).
		/// </summary>
		public override void DeleteFromCanvas()
		{
			base.DeleteFromCanvas();
			this.CloseBindings(TypeBindingData.EBindingSourceType.View);
		}

		/// <summary>
		/// Opens association or association class dialog.
		/// </summary>
		/// <param name="e">The event data.</param>
		protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (Association != null)
			{
				if (Association.AssociationClass != null)
					((ClassController)Association.AssociationClass.Controller).ShowClassDialog();
				else
					Association.Controller.ShowAssociationDialog();
			}
			base.OnMouseDoubleClick(e);
		}
	}
}
