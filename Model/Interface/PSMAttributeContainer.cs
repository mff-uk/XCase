using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents the PSM Attribute Container construct.
    /// For further details refer to http://www.necasky.net/xcase/xsem-spec.html
    /// </summary>
    public interface PSMAttributeContainer : PSMSubordinateComponent, IHasPSMAttributes
    {
        /// <summary>
        /// Adds a PIMless PSM attribute to the container.
        /// </summary>
        /// <returns>Reference to the new PSM attribute</returns>
        PSMAttribute AddAttribute();

        /// <summary>
        /// Returns the closest ancestor PSMClass
        /// </summary>
        PSMClass PSMClass { get; }
    }
}
