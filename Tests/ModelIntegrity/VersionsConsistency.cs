using XCase.Model;

namespace Tests.ModelIntegrity
{
    public static class VersionsConsistency
    {
        public static void CheckVersionsConsistency(VersionManager versionManager)
        {
            foreach (System.Collections.DictionaryEntry item in versionManager)
            {
                ComposedKey<IVersionedElement, Version> key = (ComposedKey<IVersionedElement, Version>) item.Key;
                IVersionedElement element = (IVersionedElement) item.Value;

                IVersionedElement firstElementVersion = key.First;
                Version version = key.Second;

                if (version.Number <= firstElementVersion.Version.Number)
                    throw new ModelConsistencyException(string.Format("Schema of element {0} differs.", element));
                if (version == null || firstElementVersion == null ||
                    version.CreatedFrom == null ||
                    firstElementVersion.Version == null || firstElementVersion.VersionManager == null || firstElementVersion.FirstVersion == null ||
                    element.Version == null || element.VersionManager == null || element.FirstVersion == null)
                    throw new ModelConsistencyException();

                if (element.FirstVersion != firstElementVersion)
                    throw new ModelConsistencyException();

                if (element.FirstVersion.Version != firstElementVersion.Version)
                    throw new ModelConsistencyException();

                if (!firstElementVersion.Version.ElementsCreatedInVersion.Contains(firstElementVersion))
                    throw new ModelConsistencyException();

                
            }

            foreach (Version version in versionManager.Versions)
            {
                if (version.CreatedFrom != null)
                {
                    if (!version.CreatedFrom.BranchedVersions.Contains(version))
                        throw new ModelConsistencyException();
                }
            }
        }
    }
}