using System;

namespace XCase.Model
{
    /// <summary>
    /// A MultiplicityElement embeds multiplicity information to specify the allowable
    /// cardinalities for an instantiation of this element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A multiplicity is a definition of an inclusive interval 
    /// of non-negative integers beginning with a lower bound and ending
    /// with a (possibly infinite) upper bound.
    /// </para>
    /// <para>
    /// A MultiplicityElement is an abstract metaclass that includes 
    /// optional attributes for defining the bounds of a multiplicity.
    /// A MultiplicityElement also includes specifications of whether 
    /// the values in an instantiation of this element must be unique or ordered.
    /// </para>
    /// </remarks>
    public interface MultiplicityElement
    {
        /// <summary>
        /// Gets the cardinality string of this multiplicity element.
        /// It is a string in a form "{Lower}..{Upper}" (or "{Lower}" when <see cref="Lower"/> equals <see cref="Upper"/>).
        /// </summary>
        string MultiplicityString
        {
            get;
        }

        /// <summary>
        /// Gets or sets the IsOrdered attribute of the UML MultiplicityElement.<br />
        /// For a multivalued multiplicity, this attribute specifies whether 
        /// the values in an instantiation of this element are sequentially ordered.<br />
        /// Default is <i>false</i>.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// </value>
        Boolean IsOrdered
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsUnique attribute of the UML MultiplicityElement.
        /// For a multivalued multiplicity, this attributes specifies whether the 
        /// values in an instantiation of this element are unique.<br />
        /// Default is <i>true</i>.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// </value>
        Boolean IsUnique
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Lower attribute of the UML MultiplicityElement.
        /// </summary>
        /// <value>
        /// Type: uint<br />
        /// Specifies the lower bound of the multiplicity interval, if it is 
        /// expressed as an integer.<br />
        /// The value may be either an unsigned integer or null if the lower bound
        /// is not set.
        /// </value>
        uint ? Lower
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Upper attribute of the UML MultiplicityElement.
        /// </summary>
        /// <value>
        /// Type: NUml.Uml2.UnlimitedNatural<br />
        /// Specifies the upper bound of the multiplicity interval, if it is 
        /// expressed as an unlimited natural.<br />
        /// The value may be either an unlimited natural or null if the upper bound
        /// is not set.
        /// </value>
        NUml.Uml2.UnlimitedNatural Upper
        {
            get;
            set;
        }
    }
}
