using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Common base interface for PSM Class, PSM Content choice
    /// and PSM Content container.
    /// </summary>
    public interface PSMSuperordinateComponent : PSMElement, IFormattable
    {
        /// <summary>
        /// Creates a new subordinate component and adds it to the Components collection.
        /// </summary>
        /// <param name="factory">Reference to the factory creating the concrete component</param>
        /// <returns>Reference to the new component</returns>
        PSMSubordinateComponent AddComponent(PSMSubordinateComponentFactory factory);

        /// <summary>
        /// Creates a new subordinate component and adds it to the specified position
        /// in the Components collection.
        /// </summary>
        /// <param name="factory">Reference to the factory creating the concrete component</param>
        /// <param name="index">Index in the Components collection where the component should be inserted</param>
        /// <returns>Reference to the new component</returns>
        PSMSubordinateComponent AddComponent(PSMSubordinateComponentFactory factory, int index);

        /// <summary>
        /// Creates a new PSM Class Union.
        /// </summary>
        /// <returns>Reference to the new class union</returns>
        PSMClassUnion CreateClassUnion();

        /// <summary>
        /// Gets an ordered collection of components subordinate
        /// to this component.
        /// </summary>
        ObservableCollection<PSMSubordinateComponent> Components
        {
            get;
        }

        /// <summary>
        /// Tells whether the subtree conatins Object or not
        /// </summary>
        /// <param name="Object">Object to be found</param>
        /// <returns></returns>
        bool SubtreeContains(object Object);
    }
}
