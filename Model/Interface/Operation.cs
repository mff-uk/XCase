using System.Collections.ObjectModel;
namespace XCase.Model
{
    /// <summary>
    /// An operation is a behavioral feature of a classifier that specifies the name, 
    /// type, parameters, and constraints for invoking an associated behavior.
    /// </summary>
    public interface Operation : TypedElement, MultiplicityElement
    {
        /// <summary>
        /// Creates a new parameter and adds it to the end
        /// of the oepration parameter list.
        /// </summary>
        /// <returns>Reference to the new parameter</returns>
        Parameter AddParameter();
        
        /// <summary>
        /// Gets a reference to the class that owns this operation.
        /// </summary>
        Class Class
        {
            get;
        }

        /// <summary>
        /// Gets a collection of this operation parameters.
        /// </summary>
        ObservableCollection<Parameter> Parameters
        {
            get;
        }
    }
}
