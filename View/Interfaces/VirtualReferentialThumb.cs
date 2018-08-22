using System.Collections.Generic;
using System.Windows;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Implementation of <see cref="IReferentialElement"/> that as no
	/// visualization and its "position" can be controlled from 
	/// outside. This can be used for non-standard 
	/// scenarios of snapping.
	/// </summary>
	public class VirtualReferentialThumb: IReferentialElement
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualReferentialThumb"/> class.
		/// </summary>
		public VirtualReferentialThumb()
		{
			FellowTravellers = new List<ISnappable>();
		}

		/// <summary>
		/// Position of the element on canvas (can be set freely from 
		/// outside, otherwise remains constant)
		/// </summary>
		/// <value><see cref="Point"/></value>
		public Point CanvasPosition
		{
			get;
			set;
		}

		/// <summary>
		/// List of controls that were snapped to this element
		/// </summary>
		/// <value></value>
		public IList<ISnappable> FellowTravellers
		{
			get;
			private set;
		}
	}
}