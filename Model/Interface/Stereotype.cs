using System;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// A stereotype defines how an existing metaclass may be extended, 
    /// and enables the use of platform or domain specific terminology 
    /// or notation in place of, or in addition to, the ones used 
    /// for the extended metaclass.
    /// </summary>
    /// <remarks>
    /// A stereotype is a limited kind of metaclass that cannot be used by itself, 
    /// but must always be used in conjunction with one of the metaclasses it extends. 
    /// Each stereotype may extend one or more classes through extensions as part of a profile.
    /// Similarly, a class may be extended by one or more stereotypes.
    /// </remarks>
    public interface Stereotype : Class
    {
		///// <summary>
		///// Adds a metaclass to the application domain of this stereotype.
		///// </summary>
		///// <remarks>
		///// Adding a metaclass into the application domain of a stereotype means that
		///// afterwards all the instances of this metaclass can be extended by this stereotype.
		///// </remarks>
		///// <param name="metaclassID">ID of the metaclass</param>
        //void AddExtensibleMetaClass(Schema.MetaClass metaclassID);

        /// <summary>
        /// Applies this stereotype to a given UML element.
        /// </summary>
        /// <param name="element">Reference to the element that should be extended by this stereotype</param>
        /// <returns>Instance of the stereotype</returns>
        /// <exception cref="NotSupportedException">If this stereotype cannot extend given element</exception>
        StereotypeInstance ApplyTo(Element element);

        // <summary>
        // Removes a metaclass from the application domain of the stereotype.
        // </summary>
        // <remarks>
        // When a metaclass is removed from the application domain of a stereotype,
        // the stereotype can no longer be applied to instances of this metaclass.
        // </remarks>
        // <note type="caution">
        // This operation does not check that no instance of the metaclass is extended
        // by this stereotype. You should ensure this before calling this method, otherwise
        // you introduce inconsistencies in your model!
        // </note>
        // <param name="metaclassID">ID of the metaclass</param>
        //void RemoveExtensibleMetaClass(Schema.MetaClass metaclassID);
        
        /// <summary>
        /// Gets a collection of metaclasses that can be extended by this stereotype.
        /// This is given as a collection of strings corresponding to metaclasses names.
        /// </summary>
        ObservableCollection<string> AppliesTo
        {
            get;
        }
    }

    /// <summary>
    /// Enumeration of UML metaclasses that can be extended by a stereotype.
    /// Each stereotype has a property named AppliesTo which is a collection
    /// of the metaclasses names. This enumeration can be used to get the correct
    /// metaclass name.
    /// </summary>
    /// <example>
    /// <code>
    /// myStereotype.AppliesTo.Add(StereotypeTarget.Class.ToString());
    /// </code>
    /// </example>
    [Flags]
    public enum StereotypeTarget
    {
        /// <summary>
        /// The element is not an instance of any UML metaclass.<br />
        /// The stereotype cannot extend any element.
        /// </summary>
        None = 0,
        /// <summary>
        /// The element is an instance of an UML Class metaclass or
        /// another metaclass inherited from Class.<br />
        /// The stereotype can extend classes.
        /// </summary>
        Class = 1,
        /// <summary>
        /// The element is an instance of an UML Association metaclass or
        /// another metaclass inherited from Association.<br />
        /// The stereotype can extend associations.
        /// </summary>
        Association = 2,
        /// <summary>
        /// The element is an instance of an UML Property metaclass or
        /// another metaclass inherited from Property.<br />
        /// The stereotype can extend class attributes or association ends.
        /// </summary>
        Property = 4,
        /// <summary>
        /// The element is an instance of an UML Operation metaclass or
        /// another metaclass inherited from Operation.<br />
        /// The stereotype can extend class behavioral features.
        /// </summary>
        Operation = 8,
        /// <summary>
        /// The element is an instance of an UML Package metaclass or
        /// another metaclass inherited from Package.<br />
        /// The stereotype can extend packages or models.
        /// </summary>
        Package = 16,
        /// <summary>
        /// The element is an instance of an UML Parameter metaclass or
        /// another metaclass inherited from Parameter.<br />
        /// The stereotype can extend parameters of class behvioral features.
        /// </summary>
        Parameter = 32,
        /// <summary>
        /// The element is an instance of an UML Stereotype metaclass or
        /// another metaclass inherited from Stereotype.<br />
        /// Stereotypes cannot be extended by other stereotypes!
        /// </summary>
        Stereotype = 64,
        /// <summary>
        /// The element is an instance of an UML Comment metaclass or
        /// another metaclass inherited from Comment.<br />
        /// The stereotype can extend comments.
        /// </summary>
        Comment = 128,
        /// <summary>
        /// The element is an instance of an UML Generalization metaclass or
        /// another metaclass inherited from Generalization.<br />
        /// The stereotype can extend generalizations.
        /// </summary>
        /// <remarks>
        /// The Generalization metaclass is not inherited from Association!
        /// </remarks>
        Generalization = 256,
        /// <summary>
        /// The element is an instance of an UML Slot metaclass or
        /// another metaclass inherited from Slot.<br />
        /// The stereotype can extend slots.
        /// </summary>
        Slot = 512,
        /// <summary>
        /// The element is an instance of an UML InstanceSpecification metaclass or
        /// another metaclass inherited from InstanceSpecification.<br />
        /// An InstanceSpecification cannot be extended!
        /// </summary>
        InstanceSpecification = 1024,
        /// <summary>
        /// The element is an instance of an UML AssociationClass metaclass or
        /// another metaclass inherited from AssociationClass.<br />
        /// The stereotype can extend association classes.
        /// </summary>
        AssociationClass = 2048
    }
}
