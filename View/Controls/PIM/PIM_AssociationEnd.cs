using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using NUml.Uml2;
using XCase.Controller;
using XCase.Model;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using Action=NUml.Uml2.Action;

namespace XCase.View.Controls
{
	/// <summary>
	/// <para>
	/// This control is an extension of <see cref="JunctionPoint"/> used for 
	/// end points of <see cref="XCaseJunction"/> that are binded to 
	/// model <see cref="XCase.Model.AssociationEnd"/>'s.
	/// </para>
	/// <para>
	/// Properties of the represented <see cref="XCase.Model.AssociationEnd"/>
	/// are shown in labels.
	/// </para>
	/// </summary>
	public class PIM_AssociationEnd: JunctionPoint, IDeletable
	{ 
		/// <summary>
		/// ViewHelper of the control, stores visualization information.
		/// </summary>
		/// <value></value>
		[ViewHelperElement]
		public AssociationEndViewHelper ViewHelper { get; protected set; }

		/// <summary>
		/// Model <see cref="XCase.Model.AssociationEnd"/> that is represented by this control
		/// </summary>
		/// <value><see cref="AssociationEnd"/></value>
		[ModelElement]
		public AssociationEnd AssociationEnd { get; private set; }

		#region Role Label

		private AssociationLabel roleLabel;

		[ModelPropertyMapping("Name")]
		public string Role
		{
			get { return roleLabel != null ? roleLabel.Text : String.Empty; }
		    set
			{
                if (String.IsNullOrEmpty(value))
                {
                    if (roleLabel != null)
                    {
                        //remove unused label
                        roleLabel.IsSelected = false;
                        UnsnapElement(roleLabel);
                        Junction.XCaseCanvas.Children.Remove(roleLabel);
						roleLabel.CloseBindings(TypeBindingData.EBindingSourceType.View);
                        roleLabel = null;
                    }
                }
                else
                {
                    if (roleLabel == null)
                    {
                        //create label
						roleLabel = CreateFellowLabel(ViewHelper.RoleLabelViewHelper); 
                        roleLabel.TextBox.TextEdited += delegate(object sender, StringEventArgs args)
                        {
                            Association.Controller.ChangeRole(AssociationEnd, args.Data);
                        };
                        roleLabel.TextBox.mi_Rename.Header = "Change association role";
                        roleLabel.ToolTip = "Association role";
                    }
                    roleLabel.Text = value;
                    roleLabel.UpdateLayout();
					AdjustLabelsPositions();
                }
			}
		}

	    #endregion

		#region Multiplicity label



		private AssociationLabel multiplicityLabel;

		[ModelPropertyMapping("MultiplicityString")]
		public string Multiplicity
		{
			get { return multiplicityLabel != null ? multiplicityLabel.Text : String.Empty; }
		    set
			{
				if (String.IsNullOrEmpty(value))
                {
                    if (multiplicityLabel != null)
                    {
                        //remove unused label
                        multiplicityLabel.IsSelected = false;
                        UnsnapElement(multiplicityLabel);
                        Junction.XCaseCanvas.Children.Remove(multiplicityLabel);
						multiplicityLabel.CloseBindings(TypeBindingData.EBindingSourceType.View);
                        multiplicityLabel = null;
                    }
                }
                else
                {
                    if (multiplicityLabel == null)
                    {
                        //create label
                        multiplicityLabel = CreateFellowLabel(ViewHelper.MultiplicityLabelViewHelper);
                        multiplicityLabel.TextBox.TextEdited += delegate(object sender, StringEventArgs args)
                        {
                            Association.Controller.ChangeMultiplicity(AssociationEnd, args.Data);
                        };
                        multiplicityLabel.TextBox.mi_Rename.Header = "Change association multiplicity";
                        multiplicityLabel.ToolTip = "Association multiplicity";
                    }
                    multiplicityLabel.Text = value;
                    multiplicityLabel.UpdateLayout();
                    AdjustLabelsPositions();
                }
			}
		}

		#endregion

		#region Aggregation

		private AggregationKind aggregationKind;

		[ModelPropertyMapping("Aggregation")]
		public AggregationKind AggregationKind
		{
			get
			{
				return aggregationKind;
			}
			set
			{
				aggregationKind = value;
				EJunctionCapStyle style = JunctionGeometryHelper.GetCap(aggregationKind);
				if (Junction.StartPoint == this)
					Junction.StartCapStyle = style;
				else
					Junction.EndCapStyle = style;
			}
		}

		#endregion 

		/// <summary>
		/// Initializes a new instance of the <see cref="PIM_AssociationEnd"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas where the control is placed</param>
		/// <param name="viewHelper">ViewHelper of the control, stores visualization information.</param>
		/// <param name="associationEnd">reprsented association end.</param>
		public PIM_AssociationEnd(XCaseCanvas xCaseCanvas, AssociationEndViewHelper viewHelper, AssociationEnd associationEnd)
			: base(xCaseCanvas)	
		{
		    PositionChanged += AdjustLabelsPositions;
			AssociationEnd = associationEnd;
			ViewHelper = viewHelper;
			if (multiplicityLabel != null) multiplicityLabel.ViewHelper = ViewHelper.MultiplicityLabelViewHelper;
		}

		private AssociationLabel CreateFellowLabel(AssociationLabelViewHelper labelViewHelper)   
		{
			AssociationLabel associationLabel = new AssociationLabel(this.XCaseCanvas, labelViewHelper);
			associationLabel.Association = this.Association;
			Junction.XCaseCanvas.Children.Add(associationLabel);
			associationLabel.Text = " ";
            associationLabel.SnapTo(this, false);
			associationLabel.AssociationEnd = this.AssociationEnd;
			associationLabel.UpdateLayout();
			Canvas.SetZIndex(associationLabel, Canvas.GetZIndex(Junction.SourceElement) + 1);
			associationLabel.Dropped += (() => LabelDropped(associationLabel));
			return associationLabel;
		}

		private void LabelDropped(AssociationLabel label)
		{
			IConnectable junctionElement;
			if (Junction.StartPoint == this)
				junctionElement = (IConnectable)Junction.SourceElement;
			else
				junctionElement = (IConnectable)Junction.TargetElement;

			Rect elementBounds = junctionElement.GetBounds();

			if (LabelInBadPosition(label, elementBounds))
			{
				AdjustLabelsPositions();
			}
		}

		/// <summary>
		/// Resets the labels positions.
		/// </summary>
		public void ResetLabelsPositions()
		{
			if (Junction != null)
			{
                IConnectable junctionElement;
				if (Junction.StartPoint == this)
					junctionElement = (IConnectable)Junction.SourceElement;
				else
					junctionElement = (IConnectable)Junction.TargetElement;
				junctionElement.GetBounds().AlignLabelsToPoint(this.Position, FellowTravellers.OfType<AssociationLabel>());
			}
		}

		/// <summary>
		/// If label is placed under a control 
		/// its position is adjusted so it is fully visible
		/// </summary>
		internal void AdjustLabelsPositions()
		{
			#region this piece prevents overlaying labels
            if (LabelsInBadPosition())
            {
                ResetLabelsPositions();
            }

			#endregion
		}

	    private bool LabelsInBadPosition()
	    {
            if (Junction != null && FellowTravellers != null && FellowTravellers.Count > 0)
            {
                IConnectable junctionElement;
                if (Junction.StartPoint == this)
                    junctionElement = (IConnectable)Junction.SourceElement;
                else
                    junctionElement = (IConnectable)Junction.TargetElement;

                if (junctionElement != null)
                {
                    Rect elementBounds = junctionElement.GetBounds();
                    foreach (Control control in FellowTravellers)
                    {
                        if (control is AssociationLabel)
                        {
							if (LabelInBadPosition((AssociationLabel)control, elementBounds)) return true;
                        }
                    }
                }
            }
	        return false; 
	    }

		private static bool LabelInBadPosition(AssociationLabel label, Rect elementBounds)
		{
			const int rectangleCorrectionH = 3;
			const int rectangleCorrectionV = 6;

			Rect bounds = label.GetBounds();
                        	
			bounds.X += rectangleCorrectionV;
			bounds.Y += rectangleCorrectionH;
			bounds.Width = Math.Max(bounds.Width - 2 * rectangleCorrectionV, 0);
			bounds.Height = Math.Max(bounds.Height - 2 * rectangleCorrectionH, 0);
			if (bounds.IntersectsWith(elementBounds))
			{
				return true; 
			}
			return false;
		}

		/// <summary>
		/// Reference to a control representing the association where 
		/// <see cref="AssociationEnd"/> belongs to
		/// </summary>
		/// <value><see cref="PIM_Association"/></value>
	    public PIM_Association Association { get; set; }

		/// <summary>
		/// Deletes control from canvas.
		/// </summary>
		public override void DeleteFromCanvas()
		{
			Multiplicity = null;
			Role = null;
		
			base.DeleteFromCanvas();
			this.CloseBindings();
		}
	}
}