using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Auxiliary interface for an association class.
    /// </summary>
    internal interface _ImplAssociationClass : _ImplAssociation, _ImplClass, AssociationClass
    {
        /// <summary>
        /// Gets a reference to the adapted association class.
        /// </summary>
        NUml.Uml2.AssociationClass AdaptedAssociationClass
        {
            get;
        }
    }
}
