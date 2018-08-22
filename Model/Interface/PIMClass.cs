using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents a PIM class construct.
    /// It is an UML class enriched by a method to derive a PSM class
    /// representing this one and a collection referencing
    /// the derived PSM classes.
    /// </summary>
    public interface PIMClass : Class
    {
        /// <summary>
        /// Creates a new PSM class representing this PIM class.
        /// The method also inserts the class correctly into its package.
        /// </summary>
        /// <returns>Reference to the new PSM class</returns>
        PSMClass DerivePSMClass();
        
        /// <summary>
        /// Gets a collection of PSM classes derived from this PIM class.
        /// </summary>
        ObservableCollection<PSMClass> DerivedPSMClasses
        {
            get;
        }
    }
}
