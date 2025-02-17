﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using NUml.Uml2;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.View.Geometries
{
	/// <summary>
	/// This class contains several methods for drawing junctions and 
	/// computations used by junctions.
	/// </summary>
	internal class JunctionGeometryHelper
	{
        public static Rect GetFirstElementBounds(XCaseJunction junction)
        {
            return ((IConnectable)junction.SourceElement).GetBounds();
        }

        public static Rect GetFirstButOneElementBounds(XCaseJunction junction)
        {
            return junction.Points.Count <= 2 ? GetLastElementBounds(junction) : junction.Points[1].GetBounds();
        }

        public static Rect GetLastButOneElementBounds(XCaseJunction junction)
        {
            return junction.Points.Count <= 2 ? GetFirstElementBounds(junction) : junction.Points[junction.Points.Count - 2].GetBounds();
        }

        public static Rect GetLastElementBounds(XCaseJunction junction)
        {
            return junction.TargetElement != null ? ((IConnectable)junction.TargetElement).GetBounds() : junction.EndPoint.GetBounds();
        }

		public static Point RectangleRectangleCenterIntersection(Rect rect, Rect rect2, bool relative, double angle)
		{
			return RectangleLineIntersection(
				rect,
				new Point(rect2.X + rect2.Width / 2, rect2.Y + rect2.Height / 2),
				relative, angle);
		}

		public static Point RectangleLineIntersection(Rect rect, Point point, bool relative, double angle)
		{
			Point c = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
			Point result = new Point();

			if (angle != 0)
			{
				Point old = point;
				old.X -= c.X;
				old.Y -= c.Y;
				point.X = (old.X * Math.Cos(angle) - old.Y * Math.Sin(angle));
				point.Y = (old.Y * Math.Sin(angle) + old.X * Math.Cos(angle));
				point.X += c.X;
				point.Y += c.Y;

			}

			if ((point.X == rect.X && point.Y >= rect.Y && point.Y <= rect.Bottom)
				|| (point.Y == rect.Y && point.X >= rect.X && point.X <= rect.Height)
				|| (point.X == rect.Right && point.Y >= rect.Y && point.Y <= rect.Bottom)
				|| (point.Y == rect.Bottom && point.X >= rect.X && point.X <= rect.Height)
				)
				result = point;
			else
			{
				Point candidate;
				double v_x = point.X - c.X;
				double v_y = point.Y - c.Y;
				double inc_y = v_y / v_x;
				double inc_x = v_x / v_y;

				// compute intersection with right edge
				if (!double.IsNaN(inc_y) && !double.IsInfinity(inc_y))
				{
					candidate = new Point(rect.Right, c.Y + inc_y * rect.Width / 2);
					if (candidate.Y >= rect.Y && candidate.Y <= rect.Bottom && c.X <= point.X)
						result = candidate;
				}

				// top 
				if (!double.IsNaN(inc_x) && !double.IsInfinity(inc_x))
				{
					candidate = new Point((float)(c.X - inc_x * rect.Height / 2), rect.Top);
					if (candidate.X >= rect.X && candidate.X <= rect.Right && c.Y >= point.Y)
						result = candidate;
				}

				//left
				if (!double.IsNaN(inc_y) && !double.IsInfinity(inc_y))
				{
					candidate = new Point(rect.Left, c.Y - inc_y * rect.Width / 2);
					if (candidate.Y >= rect.Y && candidate.Y <= rect.Bottom && c.X >= point.X)
						result = candidate;
				}

				//bottom
				if (!double.IsNaN(inc_x) && !double.IsInfinity(inc_x))
				{
					candidate = new Point(c.X + inc_x * rect.Height / 2, rect.Bottom);
					if (candidate.X >= rect.X && candidate.X <= rect.Right && c.Y <= point.Y)
						result = candidate;
				}
			}

			if (angle != 0)
			{
				Point old = result;
				old.X -= c.X;
				old.Y -= c.Y;
				angle -= Math.PI / 2;
				result.X = (old.X * Math.Cos(angle) - old.Y * Math.Sin(angle));
				result.Y = -(old.Y * Math.Sin(angle) + old.X * Math.Cos(angle));
				result.X += c.X;
				result.Y += c.Y;
			}

			if (relative)
			{
				result.X -= rect.Left;
				result.Y -= rect.Top;
			}

			return result;
		}

		public static bool AreInBadPosition(Rect r1, Rect r2, JunctionPoint point1, JunctionPoint point2)
		{
			bool onBottom = false, onTop = false, onLeft = false, onRight = false;
			bool badPosBottom = false, badPosTop = false, badPosLeft = false, badPosRight = false;
			if (point1.X == r1.Left)
				onLeft = true;
			if (point1.X == r1.Right)
				onRight = true;
			if (point1.Y == r1.Top)
				onTop = true;
			if (point1.Y == r1.Bottom)
				onBottom = true;

			bool result = false;

			if (onBottom && point1.Y > point2.Y)
				badPosBottom = true;

			if (onTop && point1.Y < point2.Y)
				badPosTop = true;

			if (onRight && point1.X > point2.X)
				badPosRight = true;

			if (onLeft && point1.X < point2.X)
				badPosLeft = true;

			if (badPosBottom)
			{
				if (!((!badPosLeft && onLeft) || (!badPosRight && onRight)))
				{
					result = true;
				}
			}

			if (badPosLeft)
			{
				if (!((!badPosTop && onTop) || (!badPosBottom && onBottom)))
				{
					result = true;
				}
			}

			if (badPosRight)
			{
				if (!((!badPosTop && onTop) || (!badPosBottom && onBottom)))
				{
					result = true;
				}
			}

			if (badPosTop)
			{
				if (!((!badPosLeft && onLeft) || (!badPosRight && onRight)))
				{
					result = true;
				}
			}

			return result;
		}

		public static Point[] ComputePointsForSelfAssociation(Rect elementBounds)
		{
			Point pos = new Point(elementBounds.Width,
				elementBounds.Height / 2);

			Point[] result = new Point[]
			                 	{
			                 		new Point(pos.X, Math.Max(pos.Y - 15, 0)),
			                 		new Point(elementBounds.Width + 55, pos.Y - 45),
			                 		new Point(elementBounds.Width + 55, pos.Y + 45),
			                 		new Point(pos.X, Math.Min(pos.Y + 15, elementBounds.Height))
			                 	};
			return result;
		}

		public static Point[] ComputeOptimalConnection(Rect sourceRect, Rect targetRect)
		{
			return ComputeOptimalConnection(sourceRect, targetRect, false);
		}

		public static Point[] ComputeOptimalConnection(Rect sourceRect, Rect targetRect, bool relative)
		{
			Point[] points = new Point[2];
			points[0] = RectangleRectangleCenterIntersection(
				sourceRect, targetRect, relative, 0);
			points[1] = RectangleRectangleCenterIntersection(
				targetRect, sourceRect, relative, 0);
			return points;
		}

		public static Point[] ComputeOptimalConnection(IConnectable SourceElement, IConnectable TargetElement)
		{
			Point[] points = new Point[2];
			points[0] = RectangleRectangleCenterIntersection(
				SourceElement.GetBounds(), TargetElement.GetBounds(), true, SourceElement.BoundsAngle);
			points[1] = RectangleRectangleCenterIntersection(
                TargetElement.GetBounds(), SourceElement.GetBounds(), true, TargetElement.BoundsAngle);
			return points;
		}

		public static int FindHitSegmentIndex(Point p, IList<JunctionPoint> points)
		{
			double closest = double.MaxValue;
			int index = 0;

			//find the line which is closest to p
			for (int i = 0; i < points.Count - 1; i++)
			{
				Point p1 = points[i].CanvasPosition;
				Point p2 = points[i + 1].CanvasPosition;

				Vector normal_vector = new Vector((p2 - p1).Y, -(p2 - p1).X);

				// C from the normal line equation
				double c = -(normal_vector.X * p1.X + normal_vector.Y * p1.Y);
				// distance of point p from line (p1,p2)
				double dist = Math.Abs(normal_vector.X * p.X + normal_vector.Y * p.Y + c) / Math.Sqrt(Math.Pow(normal_vector.X, 2) + Math.Pow(normal_vector.Y, 2));

				if (dist < closest)
				{
					closest = dist;
					index = i;
				}
			}

			return index;
		}

		#region EndCaps Calculations

		public static PathFigure CalculateDiamond(PathFigure pathfig, Point pt1, Point pt2, ref Point pt2_shifted)
		{
			const double ArrowLength = 15;
			const double ArrowAngle = 40;

			Matrix matx = new Matrix();
			Vector vect = pt1 - pt2;
			vect.Normalize();
			vect *= ArrowLength;

			if (!double.IsNaN(vect.X) && !double.IsNaN(vect.Y))
				pt2_shifted = new Point(pt2.X + 2 * vect.X, pt2.Y + 2 * vect.Y);

			PolyLineSegment polyseg = (PolyLineSegment)pathfig.Segments[0];
			polyseg.Points.Clear();
			matx.Rotate(ArrowAngle / 2);
			pathfig.StartPoint = pt2 + 2 * vect;

			polyseg.Points.Add(pt2 + vect * matx);
			polyseg.Points.Add(pt2);
			matx.Rotate(-ArrowAngle);
			polyseg.Points.Add(pt2 + vect * matx);
			polyseg.Points.Add(pt2 + 2 * vect);

			pathfig.IsClosed = false;
			pathfig.IsFilled = true;

			return pathfig;
		}

		public static PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2, bool closed, ref Point pt2_shifted)
		{
			const double ArrowLength = 15;
			const double ArrowAngle = 40;

			Matrix matx = new Matrix();
			Vector vect = pt1 - pt2;
			vect.Normalize();
			vect *= ArrowLength;

			if (closed && !double.IsNaN(vect.X) && !double.IsNaN(vect.Y))
				pt2_shifted = new Point(pt2.X + vect.X, pt2.Y + vect.Y);

			PolyLineSegment polyseg = (PolyLineSegment)pathfig.Segments[0];
			polyseg.Points.Clear();
			matx.Rotate(ArrowAngle / 2);
			pathfig.StartPoint = pt2 + vect * matx;
			polyseg.Points.Add(pt2);

			matx.Rotate(-ArrowAngle);
			polyseg.Points.Add(pt2 + vect * matx);

			pathfig.IsClosed = closed;
			pathfig.IsFilled = closed;

			return pathfig;
		}
		#endregion

		public static EJunctionCapStyle GetCap(AggregationKind kind)
		{
			switch (kind)
			{
				case AggregationKind.composite:
					return EJunctionCapStyle.FullDiamond;
				case AggregationKind.shared:
					return EJunctionCapStyle.Diamond;
				default: return EJunctionCapStyle.Straight;
			}
		}

		public static EJunctionCapStyle GetCap(XCaseCanvas.DraggingConnectionState.EDraggedConnectionType type)
		{
			switch (type)
			{
				case XCaseCanvas.DraggingConnectionState.EDraggedConnectionType.Composition:
					return EJunctionCapStyle.FullDiamond;
				case XCaseCanvas.DraggingConnectionState.EDraggedConnectionType.Aggregation:
					return EJunctionCapStyle.Diamond;
				case XCaseCanvas.DraggingConnectionState.EDraggedConnectionType.Generalization:
					return EJunctionCapStyle.Triangle;
				case XCaseCanvas.DraggingConnectionState.EDraggedConnectionType.NavigableAssociation:
					return EJunctionCapStyle.Arrow;
				default: return EJunctionCapStyle.Straight;
			}
		}

		public static Point FindClosestPoint(XCaseJunction junction, Point point)
		{
			int dummy;
			return FindClosestPoint(junction, point, out dummy);
		}

		public static Point FindClosestPoint(XCaseJunction junction, Point point, out int segmentNumber)
		{
			segmentNumber = 0;
			if (junction.Points.Count < 2)
				return new Point(0, 0);

			double min = double.MaxValue;
			Point result = new Point();
			for (int segmentIndex = 0; segmentIndex < junction.Points.Count - 1; segmentIndex++)
			{
				Point p1 = junction.Points[segmentIndex].CanvasPosition;
				Point p2 = junction.Points[segmentIndex + 1].CanvasPosition;
				Point segmentCenter = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
				Vector v = segmentCenter - point;
				if (v.Length < min)
				{
					segmentNumber = segmentIndex;
					min = v.Length;
					result = segmentCenter;
				}
			}
			return result;
		}
	}
}