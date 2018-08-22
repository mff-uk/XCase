using System;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that can be snapped to another control (ReferentialElement).
	/// When ReferentialElement is moved, snapped control is also moved.
	/// </summary>
	public interface ISnappable
    {
		/// <summary>
		/// Control that the element is snapped to.
		/// </summary>
		IReferentialElement ReferentialElement { get; set; }

		/// <summary>
		/// X coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
		/// <value><see cref="Double"/></value>
        Double SnapOffsetX { get; set; }

		/// <summary>
		/// Y coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
		/// </summary>
		/// <value><see cref="Double"/></value>
        Double SnapOffsetY { get; set; }
    }
}