using System;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents an UML model element.
    /// </summary>
    /// <remarks>
    /// The Model construct is defined as a Package. It contains a (hierarchical) 
    /// set of elements that together describe the physical system being modeled. 
    /// A Model may also contain a set of elements that represents the environment 
    /// of the system, typically Actors, together with their interrelationships, 
    /// such as Associations and Dependencies.
    /// </remarks>
    public interface Model : Package
    {
        /// <summary>
        /// Gets a collection of associations present in this model.
        /// </summary>
        ObservableCollection<Association> Associations
        {
            get;
        }

        /// <summary>
        /// Gets a collection of generalizations present in this model.
        /// </summary>
        ObservableCollection<Generalization> Generalizations
        {
            get;
        }

        /// <summary>
        /// Gets or sets he name of the viewpoint that is expressed by a model.
        /// </summary>
        /// <remarks>
        /// This name may refer to a profile definition.
        /// </remarks>
        String ViewPoint
        {
            get;
            set;
        }
    }
}
