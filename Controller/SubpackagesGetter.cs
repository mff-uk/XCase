using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// Serves for fetching a list of subpackages of a given package.
    /// The structure of subpackages is generally a tree, so to get a flat list
    /// of subpackages, this tree has to be traversed.
    /// </summary>
    class SubpackagesGetter
    {
        /// <summary>
        /// A package whose subpackages are to be fetched (including the package itself).
        /// </summary>
        private Package package;

        /// <summary>
        /// A collection to store the list of packages which is being built.
        /// </summary>
        private Collection<Package> packages = new Collection<Package>();

        /// <summary>
        /// Prepares an instance of SubpackagesGetter for use.
        /// </summary>
        /// <param name="pack">A package whose subpackages (including the package itself) are to be fetched.</param>
        public SubpackagesGetter(Package pack)
        {
            package = pack;
        }

        /// <summary>
        /// Adds <paramref name="package"/> and its subpackages (excluding <paramref name="excludedPackage"/>
        /// and its subpackages) into the collection packages.
        /// </summary>
        /// <param name="package">Package whose subpackages tree is to be inserted</param>
        /// <param name="excludedPackage">Package whose subpackages tree will be excluded</param>
        private void addSubpackages(Package package, Package excludedPackage)
        {
            if (package.NestedPackages != null && package.NestedPackages.Count > 0 && package != excludedPackage)
            {
                foreach (Package item in package.NestedPackages)
                {
                    if (item != excludedPackage)
                    {
                        packages.Add(item);
                        addSubpackages(item, excludedPackage);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection containing the package used to construct this instance and its subpackages.
        /// </summary>
        /// <param name="excludedPackage">Subpackage which should be excluded from the collection with all
        /// its subpackages tree.</param>
        /// <returns>Collection of package and its subpackages</returns>
        public Collection<Package> GetSubpackages(Package excludedPackage)
        {
            if (package != null && package != excludedPackage)
            {
                packages.Add(package);
                addSubpackages(package, excludedPackage);
            }
            return packages;
        }
    }
}
