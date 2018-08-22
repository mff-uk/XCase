using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace XCase.Model
{
    /// <summary>
    /// Represents a PSM Class described in the XSem specification, 
    /// refer to http://www.necasky.net/xcase/xsem-spec.html for details.
    /// </summary>
    /// <remarks>
    /// PSM class is a special case of an UML class.
    /// From the model point of view, it is just an UML class with an XSem.PSM_Class
    /// stereotype and each of its attributes has an XSem.PSM_Attribute stereotype.<br />
    /// From the programming point of view, it is inherited from the Class interface and so
    /// you can manipulate it just as any other class. However, certain differences exist:
    /// <list type="bullet">
    ///     <item>
    ///         All of its attributes are of PSMAttribute type. You can manipulate them
    ///         through the Property interface, but if you try to insert any attribute
    ///         that is not inherited from PSMAttribute to the Attributes collection, you
    ///         recieved an argument exception.
    ///     </item>
    ///     <item>
    ///         When you invoke parameterless AddAtribute() method, you receive an instance
    ///         of PSMAttribute that does not have an ancestor in the represented PIM class.
    ///         To add an attribute representing some attribute of the PIM class use overloaded
    ///         AddAttribute() variants.
    ///     </item>
    /// </list>
    /// </remarks>
    public interface PSMClass : Class, PSMSuperordinateComponent, PSMAssociationChild, IHasPSMAttributes
    {
        /// <summary>
        /// Gets a collection of all the attributes of this class
        /// including those in subordinate attribute containers.
        /// </summary>
        List<PSMAttribute> AllPSMAttributes
        {
            get;
        }
        
        /// <summary>
        /// Gets the name of the represented PIM Class
        /// </summary>
        string RepresentedClassName
        {
            get;
        }

        /// <summary>
        /// Returns collection DerivedPSMClasses of Represented Class
        /// </summary>
        ObservableCollection<PSMClass> RepresentedClassRepresentants
        {
            get;
        }

        /// <summary>
        /// Gets or sets a reference to the PSM class represented by this structural representative.
        /// If this class is not a structural representative than the value is null.
        /// </summary>
        PSMClass RepresentedPSMClass
        {
            get;
            set;
        }

        bool IsStructuralRepresentative
        { 
            get;
        }

        bool IsStructuralRepresentativeExternal
        {
            get;
        }

        PSMDiagramReference GetStructuralRepresentativeExternalDiagramReference();

        /// <summary>
        /// Gets or sets the name of an XML element that will model
        /// this class in the derived XML schema.
        /// </summary>
        string ElementName
        {
            get;
            set;
        }

		/// <summary>
		/// Indicates whether the class has an element label or not.
		/// </summary>
        bool HasElementLabel
		{ 
			get;
		}

		/// <summary>
		/// Gets or sets a value indicating whether 
		/// class allows any attribute.
		/// </summary>
		bool AllowAnyAttribute
		{
			get; 
			set;
		}
    }

    public static class PSMClassExt
    {
        public static bool IsReferencedFromStructuralRepresentative(this PSMClass psmClass)
        {
            return psmClass.Diagram.DiagramElements.Keys.OfType<PSMClass>().Any(rClass => rClass.RepresentedPSMClass == psmClass);
        }
    }
}
