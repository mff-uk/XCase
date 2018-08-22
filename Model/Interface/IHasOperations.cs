using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Identifies an element that can have operations.
    /// </summary>
    public interface IHasOperations
    {
        /// <summary>
        /// Creates a new operation and adds it to this class.
        /// </summary>
        /// <returns>Reference to the new operation</returns>
        ObservableCollection<Operation> Operations
        {
            get;
        }

        /// <summary>
        /// Gets a collection of the methods of this class.
        /// </summary>
        /// <value>
        /// Type: ObservableCollection of XCase.Model.Operation<br />
        /// </value>
        Operation AddOperation();
    }
}
