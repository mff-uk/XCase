using System.Collections;
using System.Collections.Generic;

namespace XCase.Model
{
	public interface IVersionedElement
	{
		/// <summary>
		/// Version of the element
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Version where the element appeared first. 
		/// Returns value of <see cref="Version"/> property if 
		/// this is the first version of the element. 
		/// </summary>
		IVersionedElement FirstVersion { get; }

		VersionManager VersionManager { get; }

        bool IsFirstVersion { get; }
	}

	public static class IVersionedElementExt
	{
		/// <summary>
		/// Returns desired version of current element or null if the
		/// element does not exist in the desired version.
		/// </summary>
		/// <param name="versionedElement">The versioned element.</param>
		/// <param name="version">desired version</param>
		public static IVersionedElement GetInVersion(this IVersionedElement versionedElement, Version version)
		{
			if (versionedElement.Version == version)
			{
				return versionedElement;
			}

            if (versionedElement.FirstVersion.Version == version)
			{
                return versionedElement.FirstVersion;
			}
			else
			{
				return versionedElement.VersionManager.LookupElementVersion(versionedElement.FirstVersion, version);
			}
		}

        public static bool ExistsInVersion(this IVersionedElement versionedElement, Version version)
        {
            return versionedElement.GetInVersion(version) != null;
        }
	}
}