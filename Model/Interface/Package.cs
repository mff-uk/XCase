using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// A package is used to group elements, and provides a namespace 
    /// for the grouped elements.
    /// </summary>
    /// <remarks>
    /// A package is a namespace for its members, and may contain other packages. 
    /// Only packageable elements can be owned members of a package. 
    /// By virtue of being a namespace, a package can import either individual 
    /// members of other packages, or all the members of other packages.
    /// </remarks>
    public interface Package : NamedElement
    {
        /// <summary>
        /// Creates a new empty class in the actual package.
        /// </summary>
        /// <returns>Reference to the new class</returns>
        PIMClass AddClass();

        /// <summary>
        /// Creates a new simple data type in the actual package.
        /// </summary>
        /// <returns>Reference to the new data type</returns>
        SimpleDataType AddSimpleDataType();

        /// <summary>
        /// Creates a new simple data type in the actual package that inherits from baseType.
        /// </summary>
        /// <param name="baseType">Reference to the base type</param>
        /// <returns>Reference to the new data type</returns>
        SimpleDataType AddSimpleDataType(SimpleDataType baseType);

        /// <summary>
        /// Creates a new empty package nested in the actual one.
        /// </summary>
        /// <returns>Reference to the new package</returns>
        Package AddNestedPackage();

        /// <summary>
        /// Gets a collection of all datatypes accesible in this package.
        /// Includes primitiv types and datatypes from nesting packages.
        /// </summary>
        IEnumerable<DataType> AllTypes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of classes owned by this package.<br />
        /// </summary>
        ObservableCollection<PIMClass> Classes
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the package that owns this one.
        /// </summary>
        Package NestingPackage
        {
            get;
        }

        /// <summary>
        /// Gets a collection of packages owned by this one.
        /// </summary>
        ObservableCollection<Package> NestedPackages
        {
            get;
        }

        /// <summary>
        /// Gets a collection of data types owned by this package.<br />
        /// </summary>
        ObservableCollection<DataType> OwnedTypes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of PSM classes owned by this package.
        /// This collection has an empty intersection with the Classes
        /// and OwnedTypes collections!
        /// </summary>
        ObservableCollection<PSMClass> PSMClasses
        {
            get;
        }
    }
}
