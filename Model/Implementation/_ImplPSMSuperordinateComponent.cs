using System.Collections.Generic;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for a PSM subordinate component.
    /// </summary>
    internal interface _ImplPSMSuperordinateComponent : PSMSuperordinateComponent
    {
        /// <summary>
        /// Gets a reference to the adapted nUML Class element.
        /// </summary>
        NUml.Uml2.Class AdaptedClass
        {
            get;
        }
    }
}
