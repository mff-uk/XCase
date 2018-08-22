using System.Collections.ObjectModel;
namespace XCase.Model
{
    /// <summary>
    /// A profile defines limited extensions to a reference metamodel 
    /// with the purpose of adapting the metamodel to a specific platform or domain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Profile is a kind of Package that extends a reference metamodel. 
    /// The primary extension construct is the Stereotype, which is defined as part of Profiles.
    /// </para>
    /// <para>
    /// A profile introduces several constraints, or restrictions, on ordinary metamodeling 
    /// through the use of the metaclasses defined in this package.
    /// </para>
    /// <para>
    /// A profile is a restricted form of a metamodel that must always be related 
    /// to a reference metamodel, such as UML, as described below. 
    /// A profile cannot be used without its reference metamodel, 
    /// and defines a limited capability to extend metaclasses of the reference metamodel. 
    /// The extensions are defined as stereotypes that apply to existing metaclasses.
    /// </para>
    /// </remarks>
    public interface Profile : Package
    {
        /// <summary>
        /// Creates a new stereotype in the actual profile.
        /// </summary>
        /// <returns>Reference to the new stereotype</returns>
        Stereotype AddStereotype();

        /// <summary>
        /// Gets a collection of models containing (directly or indirectly)
        /// metaclasses that may be extended.
        /// </summary>
        ObservableCollection<Model> MetamodelReference
        {
            get;
        }

        /// <summary>
        /// Gets a collection of stereotypes in this profile.
        /// </summary>
        ObservableCollection<Stereotype> Stereotypes
        {
            get;
        }
    }
}
