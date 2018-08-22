using System.Windows;
using System.Windows.Controls;
using XCase.View.Interfaces;

namespace XCase.View.Controls
{
	/// <summary>
	/// Label that can be snapped (to a <see cref="DragThumb"/>).
	/// </summary>
    public class SnappableLabel: Label, ISnappable
    {
		/// <summary>
		/// Control that the element is snapped to.
		/// </summary>
		/// <value></value>
        public IReferentialElement ReferentialElement
        {
            get; set;
        }

		/// <summary>
		/// X coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
        public double SnapOffsetX
        { 
            get;
            set; 
        }

		/// <summary>
		/// Y coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
        public double SnapOffsetY
        {
            get; set;
        }
    }
}