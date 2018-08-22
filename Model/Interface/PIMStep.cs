namespace XCase.Model
{
    /// <summary>
    /// Represents one step of a PIMPath.
    /// One step is an ordered pair of PIM classes (Start, End)
    /// and a reference to the association in the PIM used to get
    /// from Start to End.
    /// </summary>
    public interface PIMStep
    {
        /// <summary>
        /// Gets a reference to the PIMPath that owns this step.
        /// </summary>
        PIMPath Path
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the starting PSMClass of the step.
        /// </summary>
        PIMClass Start
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the ending PSMClass of the step.
        /// </summary>
        PIMClass End
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the association used to get from Start to End.
        /// </summary>
        Association Association
        {
            get;
        }
    }
}
