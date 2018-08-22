using System.Windows;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Elements implementing ISelectable can be selected via mouse click or mouse drag.
	/// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Selected flag. Selected elements are highlighted on the canvas and are 
        /// target of commands. 
        /// </summary>
		bool IsSelected { get; set; }
        
		/// <summary>
        /// <para>
        /// If set to true, this object will be dragged when selection is dragged. 
        /// </para>
        /// <para>
        /// It is usually handy to be able to drag an object in a group, but not desirable for those 
        /// objects whose position is determined by position of other objects (like junctions and 
        /// some SnapPointHooks).
        /// </para>
        /// </summary>
        bool CanBeDraggedInGroup { get; }

		/// <summary>
		/// Returns bounding rectangle of the element
		/// </summary>
		/// <returns>Bounding rectangle</returns>
        Rect GetBounds();
    }
}
