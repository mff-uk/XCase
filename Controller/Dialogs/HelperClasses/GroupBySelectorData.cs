using XCase.Model;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// This class is used by the GroupBy dialog and command for association selection
    /// </summary>
    public class GroupBySelectorData
    {
        /// <summary>
        /// The PSMClass at the end of the Association (for getting name)
        /// </summary>
        public PSMClass PSMClass { get; set; }

        /// <summary>
        /// The PSMAssociation represented by this item
        /// </summary>
        public PSMAssociation PSMAssociation { get; set; }

        /// <summary>
        /// Whether or not is this item selected (corresponds to the checkbox in the Dialog)
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
