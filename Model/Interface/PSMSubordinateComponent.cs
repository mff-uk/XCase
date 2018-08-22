namespace XCase.Model
{
    /// <summary>
    /// Common base interface for PSM Association, PSM Content choice 
    /// and PSM Content container.
    /// </summary>
    public interface PSMSubordinateComponent : PSMElement
    {
        /// <summary>
        /// Gets a reference to the parent of this PSM component.
        /// </summary>
        PSMSuperordinateComponent Parent
        {
            get;
        }
    }

    public static class PSMSubordinateComponentExt
    {
        public static int ComponentIndex(this PSMSubordinateComponent subordinate)
        {
            return subordinate.Parent.Components.IndexOf(subordinate);
        }
    }
}
