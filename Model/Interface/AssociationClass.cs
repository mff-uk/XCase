namespace XCase.Model
{
    /// <summary>
    /// A model element that has both association and class properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An AssociationClass can be seen as an association that also has 
    /// class properties, or as a class that also has association properties. 
    /// It not only connects a set of classifiers but also defines a set 
    /// of features that belong to the relationship itself and not to any of the classifiers.
    /// </para>
    /// <para>
    /// In the metamodel, an AssociationClass is a declaration of a semantic relationship 
    /// between Classifiers, which has a set of features of its own. 
    /// AssociationClass is both an Association and a Class.
    /// </para>
    /// </remarks>
    public interface AssociationClass : Association, PIMClass
    {
    }
}
