using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents the XSem PSM Class Union construct.
    /// For further details refer to http://www.necasky.net/xcase/xsem-spec.html
    /// </summary>
    public interface PSMClassUnion : PSMAssociationChild
    {
        /// <summary>
        /// Gets the collection of classes and class unions in the choice.
        /// </summary>
        ObservableCollection<PSMAssociationChild> Components
        {
            get;
        }
    }
}
