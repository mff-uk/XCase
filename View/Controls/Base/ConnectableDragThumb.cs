using System.Collections.Generic;
using XCase.View.Geometries;
using XCase.View.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace XCase.View.Controls
{
	/// <summary>
	/// Implementation of <see cref="IConnectable"/>, inherits dragging from DragThumb
	/// and can create JunctionPoints and can be connected via <see cref="XCaseJunction"/>.
	/// </summary>
    public class ConnectableDragThumb : DragThumb, IConnectable
    {
        protected Control connectorDecorator;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectableDragThumb"/> class.
		/// </summary>
		/// <param name="xCaseCanvas">canvas</param>
		public ConnectableDragThumb(XCaseCanvas xCaseCanvas)
			: base(xCaseCanvas)
		{
			this.SizeChanged += ConnectableDragThumb_SizeChanged;
		}

		void ConnectableDragThumb_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			foreach (JunctionPoint end in createdJunctionEnds)
			{
				Rect b = this.GetBounds();
				b.Offset(-b.X, -b.Y);
				if (end.Placement == EPlacementKind.AbsoluteSubCanvas)
				{
					Point p = b.SnapPointToRectangle(end.Position);
					end.SetPreferedPosition(p);
					if (end is PIM_AssociationEnd)
					{
						((PIM_AssociationEnd)end).AdjustLabelsPositions();
					}
				}
			}
		}

    	#region IConnectable Members

		/// <summary>
		/// List of all <see cref="JunctionPoint"/>s that were created 
		/// via <see cref="CreateJunctionEnd()"/>. 
		/// </summary>
        protected List<JunctionPoint> createdJunctionEnds = new List<JunctionPoint>();

		/// <summary>
		/// Creates <see cref="JunctionPoint"/> on a specific position.
		/// </summary>
		/// <param name="preferedPosition">The prefered position for a new JunctionPoint</param>
		/// <returns>new <see cref="JunctionPoint"/></returns>
		public virtual JunctionPoint CreateJunctionEnd(Point preferedPosition)
        {
			JunctionPoint junctionEnd = CreateJunctionEnd();
            junctionEnd.SetPreferedPosition(preferedPosition);
			createdJunctionEnds.Add(junctionEnd);
            return junctionEnd;
        }

		/// <summary>
		/// Creates <see cref="JunctionPoint"/>.
		/// </summary>
		/// <returns>new <see cref="JunctionPoint"/></returns>
		public virtual JunctionPoint CreateJunctionEnd()
        {
			JunctionPoint junctionEnd = new JunctionPoint(XCaseCanvas) { OwnerControl = this, Placement = EPlacementKind.ParentAutoPos, ParentControl = this};
            ((Canvas) connectorDecorator.Template.FindName("ConnectorDecoratorGrid", connectorDecorator)).Children.Add(junctionEnd);
            Canvas.SetZIndex(junctionEnd, System.Windows.Controls.Canvas.GetZIndex(connectorDecorator) + 10);
            junctionEnd.Visibility = Visibility.Visible;
			createdJunctionEnds.Add(junctionEnd);
            return junctionEnd;
        }

		/// <summary>
		/// When bounding rectangle of the control is tilted, this property returns the
		/// angle in degrees.
		/// </summary>
		/// <value><see cref="double"/>, default 0</value>
    	public virtual double BoundsAngle
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Highlights this control.
		/// </summary>
        public void Highlight()
        {
            Effect = MediaLibrary.DropShadowEffect;
        }

		/// <summary>
		/// Unhighlights this control.
		/// </summary>
        public void UnHighlight()
        {
            Effect = null;
        }

		#endregion

		/// <summary>
		/// Deletes the control from canvas (with all the JunctionPoints that it created via 
		/// <see cref="CreateJunctionEnd()"/>).
		/// </summary>
    	public virtual void DeleteFromCanvas()
    	{
			foreach (JunctionPoint junctionPoint in createdJunctionEnds)
    		{
    			junctionPoint.DeleteFromCanvas();
    		}
			XCaseCanvas.Children.Remove(this);
    	}
    }
}
