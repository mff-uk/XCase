using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Model
{
    /// <summary>
    /// A parameter is a specification of an argument used to pass 
    /// information into or out of an invocation of a behavioral feature.
    /// </summary>
    public interface Parameter : TypedElement, MultiplicityElement
    {
        /// <summary>
        /// Gets the direction of the parameter.
        /// Indicates whether a parameter is being sent into or out of a behavioral element. 
        /// <i>The default value is in.</i>
        /// </summary>
        NUml.Uml2.ParameterDirectionKind Direction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a reference to the operation that owns this parameter.
        /// </summary>
        Operation Operation
        {
            get;
        }
    }
}
