using System;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// A property is a structural feature.
    /// </summary>
    public interface Property : TypedElement, MultiplicityElement
    {
        /// <summary>
        /// Gets or sets the aggregation kind that applies to this property.
        /// </summary>
        /// <value>
        /// Type: NUml.Uml2.AggregationKind<br />
        /// One of the AggregationKind values<br />
        /// Specifies the aggregation kind that applies to this property.
        /// Default value is <i>none</i>.
        /// </value>
        NUml.Uml2.AggregationKind Aggregation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a reference to the class that owns this property.
        /// </summary>
        /// <value>
        /// Type: XCase.Model.Class<br />
        /// References the Class that owns the Property.
        /// </value>
        Class Class
        {
            get;
        }

        /// <summary>
        /// Default value of this property.
        /// </summary>
        /// <value>
        /// Type: String<br />
        /// A String that is evaluated to give a default value for the Property 
        /// when an object of the owning Classifier is instantiated. This is a derived value.<br />
        /// Default value is <i>null</i>.
        /// </value>
        /// <remarks>
        /// If this string is null then the property does not have a default value.<br />
        /// String "null" means this property has a default value null.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Set to a string value that has not a type coresponding to the property type.
        /// </exception>
        String Default
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default value for this property.
        /// </summary>
        /// <value>
        /// Type: NUml.Uml2.ValueSpecification<br />
        /// A ValueSpecification that is evaluated to give a default value for the Property 
        /// when an object of the owning Classifier is instantiated.
        /// </value>
        ValueSpecification DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of PSM attributes that were derived from this attribute.
        /// </summary>
        ObservableCollection<PSMAttribute> DerivedPSMAttributes
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets the IsComposite attribute of the UML property.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// This is a derived value, indicating whether the aggregation of the Property 
        /// is composite or not.
        /// </value>
        Boolean IsComposite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsDerived attribute of the UML property.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// Specifies whether the Property is derived, i.e., whether its value 
        /// or values can be computed from other information.<br />
        /// The default value is <i>false</i>.
        /// </value>
        Boolean IsDerived
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsDerivedUnion attribute of the UML property.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// Specifies whether the property is derived as the union of all of 
        /// the properties that are constrained to subset it.<br />
        /// The default value is <i>false</i>.
        /// </value>
        Boolean IsDerivedUnion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsReadOnly attribute of the UML property.
        /// </summary>
        /// <value>
        /// Type: Boolean<br />
        /// If true, the attribute may only be read, and not written.<br />
        /// The default value is <i>false</i>.
        /// </value>
        Boolean IsReadOnly
        {
            get;
            set;
        }
    }
}
