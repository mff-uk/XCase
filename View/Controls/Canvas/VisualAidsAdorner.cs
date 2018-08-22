using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
    /// <summary>
    /// This adorner is used to draw visual aids between elements during dragging. 
    /// </summary>
    public class VisualAidsAdorner : Adorner
    {
    	private readonly List<Point> otherVisualAidsPoints = new List<Point>();
        
		private readonly IAlignable draggedObject;

		/// <summary>
		/// Adding this vector to the position of the dragged control will snap the 
		/// control to the visual aid.
		/// </summary>
    	public Vector ? adjustmentToSnap = null;

		/// <summary>
		/// DragDeltaEventArgs, should be set during dragging.
		/// </summary>
		public DragDeltaEventArgs d = null;

    	public VisualAidsAdorner(XCaseCanvas designerCanvas, DragThumb draggedObject)
            : base(designerCanvas)
        {
    		this.draggedObject = (IAlignable) draggedObject;

            foreach (object element in designerCanvas.Children)
            {
                if (element is IAlignable && draggedObject != element)
                {
                    otherVisualAidsPoints.AddRange((element as IAlignable).GetVisualAidsPoints());
                }
            }            
        }

        public static List<Point> GetVisualAidsPointsForDiagram(XCaseCanvas diagramView, params Type[] excluded)
        {
            List<Point> points = new List<Point>();
            foreach (object element in diagramView.Children)
            {
                if (element is IAlignable && !excluded.Contains(element.GetType()))
                {
                    points.AddRange((element as IAlignable).GetVisualAidsPoints());
                }
            }
            points.Add(new Point(0, 0));
            points.Add(new Point(0, diagramView.ActualHeight - 10));
            points.Add(new Point(diagramView.ActualWidth - 10, 0));
            points.Add(new Point(diagramView.ActualWidth - 10, diagramView.ActualHeight - 10));
            return points;
        }

        private const int ADJUSTMENT_RATIO = 15;

		/// <summary>
		/// Draws visual aids in the adorner.
		/// </summary>
		/// <param name="dc">The drawing context.</param>
        protected override void OnRender(DrawingContext dc)
        {
			base.OnRender(dc);

			foreach (Point myPoint in draggedObject.GetVisualAidsPoints())
			{
				IEnumerable<Point> points = from point in otherVisualAidsPoints
											where point.X == myPoint.X || point.Y == myPoint.Y
											//Math.Abs(point.X - myPoint.X) < ADJUSTMENT_RATIO || Math.Abs(point.Y - myPoint.Y) < ADJUSTMENT_RATIO
											select point;


				foreach (Point point in points)
				{
					if (point.X == myPoint.X || point.Y == myPoint.Y)
						dc.DrawLine(MediaLibrary.RubberbandPen, myPoint, point);
					//if (Math.Abs(point.X - myPoint.X) < ADJUSTMENT_RATIO && (adjX == null || Math.Abs(adjX.Value) > Math.Abs(point.X - myPoint.X)))
					//{
					//    adjX = point.X - myPoint.X;
					//    //dc.DrawLine(rubberbandPen, myPoint, point);
					//}
					//if (Math.Abs(point.Y - myPoint.Y) < ADJUSTMENT_RATIO && (adjY == null || Math.Abs(adjY.Value) > Math.Abs(point.Y - myPoint.Y)))
					//{
					//    adjY = point.Y - myPoint.Y;
					//    //dc.DrawLine(rubberbandPen, myPoint, point);
					//}
				}

			}
			#region adjustment disabled
			/*
            double? adjY = null;
            double? adjX = null;

            adjustmentToSnap = new Vector(0, 0);

			Vector shift = new Point(((DragThumb)draggedObject).Left, ((DragThumb)draggedObject).Top) - ((DragThumb)draggedObject).MousePoint;

        	IEnumerable<Point> myPoints = draggedObject.GetVisualAidsPoints();
        	foreach (Point _myPoint in myPoints)
            {
            	Point myPoint = _myPoint;
				if (d != null)
				{
					myPoint.X += d.HorizontalChange;
					myPoint.Y += d.VerticalChange;
				}
            	foreach (Point point in otherVisualAidsPoints)
                {
					double value = point.Y - myPoint.Y - shift.Y;
                	if (Math.Abs(value) < ADJUSTMENT_RATIO)
					{
						adjY = value ;
						shift.Y = value; 
					}
                }
            }
			if (adjY >= ADJUSTMENT_RATIO)
			{
				
			}
			if (adjX != null || adjY != null)
			{
				//Debug.WriteLine(string.Format("SNAP : {0}, {1}", adjX, adjY));
				//adjustmentToSnap = new Vector(adjX.HasValue ? adjX.Value : 0, adjY.HasValue ? adjY.Value : 0);
			}
			else
			{
				//Debug.WriteLine("NO SNAP");
				adjustmentToSnap = null;
			}
			*/
			#endregion 
		}
			

    }
}
