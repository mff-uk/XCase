using System;

namespace XCase.Model
{
    /// <summary>
    /// Common base interface for PSMClass and PSMClassUnion.
    /// These two constructs can both be assigned as a child to a PSMAssociation.
    /// </summary>
    public interface PSMAssociationChild : PSMElement, IFormattable
    {
        /// <summary>
        /// Gets the association that owns this component (if any).
        /// </summary>
        PSMAssociation ParentAssociation
        {
            get;
        }

        /// <summary>
        /// Gets the PSM Class Union that owns this component if any.
        /// </summary>
        PSMClassUnion ParentUnion
        {
            get;
        }
    }

    public static class PSMAssociationChildExt
    {
        public static int ComponentIndex(this PSMAssociationChild associationChild)
        {
            if (associationChild.ParentUnion == null)
                return -1; 
            return associationChild.ParentUnion.Components.IndexOf(associationChild);
        }
    }
}
