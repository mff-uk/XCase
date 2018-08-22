using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents the XSem PSM Content Container.
    /// For further details refer to http://www.necasky.net/xcase/xsem-spec.html
    /// </summary>
    public interface PSMContentContainer : PSMSuperordinateComponent, PSMSubordinateComponent
    {
        /// <summary>
        /// Gets or sets the name of the label of this component
        /// in the derived XML schema.
        /// </summary>
        string ElementLabel
        {
            get;
            set;
        }
    }
}
