using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents a path going from one PIM class to another
    /// through associations between classes.
    /// </summary>
    public interface PIMPath
    {
        /// <summary>
        /// Adds a new step to the end of the path.
        /// </summary>
        /// <param name="start">Reference to the starting PIM class</param>
        /// <param name="end">Reference to the ending PIM class</param>
        /// <param name="association">Association used to get from start to end</param>
        void AddStep(PIMClass start, PIMClass end, Association association);

        /// <summary>
        /// Gets a reference to the nesting join that owns this path.
        /// </summary>
        NestingJoin Join
        {
            get;
        }

        /// <summary>
        /// Gets an ordered collection of the path steps.
        /// </summary>
        ObservableCollection<PIMStep> Steps
        {
            get;
        }
    }
}
