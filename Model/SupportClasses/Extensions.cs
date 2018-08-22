using System.Collections.Generic;

namespace XCase.Model
{
    /// <summary>
    /// This class contains definitions of extension methods used by the Model.
    /// </summary>
    public static class XCaseExtensions
    {
        /// <summary>
        /// Gets an item with the specified name from the collection.
        /// This extension can only be used on collections containing 
        /// descendants of XCase.Model.NamedElement.
        /// </summary>
        /// <typeparam name="T">Type of the collection items</typeparam>
        /// <param name="col">Collection to be searched for the element</param>
        /// <param name="name">Name of the element to be searched</param>
        /// <returns>Reference to the element if found or null if no such exists</returns>
        public static T Get<T>(this IEnumerable<T> col, string name) where T : NamedElement
        {
            foreach (T item in col)
            {
                if (item.Name == name)
                    return item;
            }

            return default(T);
        }

        /// <summary>
        /// Searches the collection for an element having given qualified name.
        /// The qualified name has to be rooted at the level of the elements in the searched collection.
        /// </summary>
        /// <typeparam name="T">Type of the collection items</typeparam>
        /// <param name="col">Collection to be searched for the element</param>
        /// <param name="qName">Qualified name of the element being searched</param>
        /// <returns>Reference to the searched element if found, null otherwise</returns>
		public static NamedElement GetByQualifiedName<T>(this IEnumerable<T> col, string qName) where T : NamedElement
        {
            foreach (T item in col)
            {
                NamedElement ne = item.GetChildByQualifiedName(qName);
                if (ne != null)
                    return ne;
            }

            return null;
        }

        /// <summary>
        /// Gets an item with the specified name from the collection.
        /// This extension can only be used on collections containing 
        /// instances of XCase.Model.InstantiatedProperty.
        /// </summary>
        /// <param name="col">Collection to be searched for the element</param>
        /// <param name="name">Name of the element to be searched</param>
        /// <returns>Reference to the element if found or null if no such exists</returns>
		public static InstantiatedProperty Get(this IEnumerable<InstantiatedProperty> col, string name)
        {
            foreach (InstantiatedProperty item in col)
            {
                if (item.Name == name)
                    return item;
            }

            return null;
        }
    }
}
