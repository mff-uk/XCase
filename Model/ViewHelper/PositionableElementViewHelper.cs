using System;
using System.Collections.Generic;
using System.Windows;

namespace XCase.Model
{
	/// <summary>
	/// View helper for positionable elements (elements that 
	/// have coordinates and width and height). Used as a base 
	/// class for many other view helpers.
	/// </summary>
	public abstract class PositionableElementViewHelper : ViewHelper
	{
		protected double x;
		protected double y;
		protected double width = double.NaN;
		protected double height = double.NaN;

		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete]
		protected PositionableElementViewHelper()
		{
		}

		protected PositionableElementViewHelper(Diagram diagram)
			: base(diagram)
		{
		}

		/// <summary>
		/// X coordinate of the element representation in the diagram
		/// </summary>
		public double X
		{
			get { return x; }
			set
			{
				if (x != value)
				{
					x = value;
					OnPropertyChanged("X");
				}
			}
		}

		/// <summary>
		/// X coordinate of the element representation in the diagram
		/// </summary>
		public double Y
		{
			get { return y; }
			set
			{
				if (y != value)
				{
					y = value;
					OnPropertyChanged("Y");
				}
			}
		}

		public void SetPositionSilent(Point p)
		{
			SetPositionSilent(p.X, p.Y);	
		}

		public void SetPositionSilent(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		/// <summary>
		/// Width of the element representation in the diagram
		/// </summary>
		public double Width
		{
			get { return width; }
			set
			{
				if (width != value)
				{
					width = value;
					OnPropertyChanged("Width");
				}
			}
		}

		/// <summary>
		/// Height of the element representation in the diagram
		/// </summary>
		public double Height
		{
			get { return height; }
			set
			{
				if (height != value)
				{
					height = value;
					OnPropertyChanged("Height");
				}
			}
		}

		/// <summary>
		/// Returns encompassing rectangle of the element on the diagram.
		/// </summary>
		/// <returns>Encompassing rectangle of the element on the diagram, based directly on 
		/// <see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, <see cref="Height"/> properties. </returns>
		public Rect GetBounds()
		{
			return new Rect(X, Y, Width, Height);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);

			PositionableElementViewHelper copyPositionableElementViewHelper = (PositionableElementViewHelper) copy;
			copyPositionableElementViewHelper.X = this.X;
			copyPositionableElementViewHelper.Y = this.Y;
			copyPositionableElementViewHelper.Height = this.Height;
			copyPositionableElementViewHelper.Width = this.Width;
		}
	}
}