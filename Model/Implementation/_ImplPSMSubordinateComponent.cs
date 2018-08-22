namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for a PSM subordinate component.
    /// </summary>
    internal interface _ImplPSMSubordinateComponent : PSMSubordinateComponent
    {
        /// <summary>
        /// Gets or sets a reference to the PSM component that owns this one.
        /// </summary>
        new PSMSuperordinateComponent Parent
        {
            get;
            set;
        }
    }
}
