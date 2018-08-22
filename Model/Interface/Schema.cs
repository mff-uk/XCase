using System.Collections.Generic;
using System.Collections.ObjectModel;
using XCase.Model.Implementation;
using System;
using System.ComponentModel;

namespace XCase.Model
{
    /// <summary>
    /// Facade covering the UML model.
    /// </summary>
    public class Schema : INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Creates a new schema.
        /// </summary>
        public Schema()
        {
            userModel = new _Model(this);
            metaClasses = new _Model(this);
            profiles = new ObservableCollection<Profile>();
            primitiveTypes = new ObservableCollection<SimpleDataType>();

            userModel.Name = "User model";
            metaClasses.Name = "UML";

            CreateUMLMetamodel();
        }

        #endregion

        #region AuxiliaryMethods

        /// <summary>
        /// Tests if the tested class can create a cyclic iheritance relation.
        /// </summary>
        /// <param name="tested">References the tested class</param>
        /// <param name="start">References the class at which the search starts</param>
        /// <param name="specific">
        ///     If true, the search descends through the Specifications collection of the start class,
        ///     otherwise through the Generalizations.
        /// </param>
        /// <returns>False if the tested class does not cause a cyclic inheritance, true otherwise.</returns>
        private static bool CheckCyclicInheritance(Class tested, Class start, bool specific)
        {
            if (tested == start)
                return true;

            if (specific)
            {
                foreach (Generalization spec in start.Specifications)
                    if (CheckCyclicInheritance(tested, spec.Specific, specific))
                        return true;
            }
            else
            {
                foreach (Generalization gen in start.Generalizations)
                    if (CheckCyclicInheritance(tested, gen.General, specific))
                        return true;
            }

            return false;
        }

        private void CreateUMLMetamodel()
        {
            PIMClass classMetaClass = metaClasses.AddClass();
            classMetaClass.Name = "Class";
            PIMClass associationMetaClass = metaClasses.AddClass();
            associationMetaClass.Name = "Association";
            PIMClass associationClassMetaClass = metaClasses.AddClass();
            associationClassMetaClass.Name = "AssociationClass";
            PIMClass packageMetaClass = metaClasses.AddClass();
            packageMetaClass.Name = "Package";
            PIMClass generalizationMetaClass = metaClasses.AddClass();
            generalizationMetaClass.Name = "Generalization";
            PIMClass propertyMetaClass = metaClasses.AddClass();
            propertyMetaClass.Name = "Property";
            PIMClass operationMetaClass = metaClasses.AddClass();
            operationMetaClass.Name = "Operation";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new primitve type and adds it to the Primitive Types collection.
        /// </summary>
        /// <returns>Reference to the new primitve type</returns>
        public SimpleDataType AddPrimitiveType()
        {
            _SimpleDataType dt = new _SimpleDataType(this);
            primitiveTypes.Add(dt);

            return dt;
        }

		
    	 
		/// <summary>
		/// Searches for primitive type by name (e.g. 'integer').
		/// </summary>
		/// <param name="name">name of the primitive type</param>
		/// <param name="primitiveType">found type</param>
		/// <returns>true on successful search, false otherwise</returns>
		/// <exception cref="ArgumentNullException">Argument "name" is null or empty.</exception>
    	public bool TryFindPrimitiveTypeByName(string name, out SimpleDataType primitiveType)
    	{
    		if (String.IsNullOrEmpty(name))
    		{
    			throw new ArgumentNullException("name", "Name can not be null or empty string");
    		}
    		foreach (SimpleDataType type in PrimitiveTypes)
    		{
				if (type.Name == name)
				{
					primitiveType = type;
					return true;
				}
    		}
			primitiveType = null;
			return false; 
    	}

        /// <summary>
        /// Creates a new empty profile and adds it to the profiles collection.
        /// </summary>
        /// <returns>Reference to the new profile</returns>
        public Profile AddProfile()
        {
            _Profile profile = new _Profile(this);

            profiles.Add(profile);
            profile.MetamodelReference.Add(metaClasses);

            return profile;
        }

        /// <summary>
        /// Creates a new association relating the given classes.
        /// </summary>
        /// <param name="classes">
        /// Classes to be ralated by the new association
        /// </param>
        /// <returns>Reference to the new association</returns>
        public Association AssociateClasses(IEnumerable<Class> classes)
        {
            _Association<NUml.Uml2.Association> association = new _Association<NUml.Uml2.Association>(classes, this);
            Model.Associations.Add(association);

            return association;
        }

        /// <summary>
        /// Creates a new association class relating the given classes.
        /// The class is inserted either directly into user model (if related classes
        /// are not in the same package or are all directly in the model) or to the package
        /// owning all the related classes.
        /// </summary>
        /// <param name="classes">
        /// Classes to be ralated by the new association
        /// </param>
        /// <returns>Reference to the new association class</returns>
        public AssociationClass CreateAssociationClass(IEnumerable<Class> classes)
        {
            _AssociationClass ac = new _AssociationClass(classes, this);
            Model.Associations.Add(ac);
        	ac.Name = "AssociationClass" + Model.Associations.Count;

            Model.Classes.Add(ac);
            
            return ac;
        }

        /// <summary>
        /// Exports the whole schema (user model, UML metamodel and profiles)
        /// to a given XMI file.
        /// </summary>
        /// <param name="fileName">Name of the exported file</param>
        public void ExportToXMIFile(string fileName)
        {
            NUml.Xmi2.SerializationDriver ser = new NUml.Xmi2.SerializationDriver();
            ser.AddSerializer(new NUml.Uml2.Serialization.Serializer());

            System.Collections.ArrayList objList = new System.Collections.ArrayList();
            objList.Add(((_Model)userModel).Adaptee);
            objList.Add(((_Model)metaClasses).Adaptee);
            foreach (Profile profile in profiles)
            {
                objList.Add(((_Profile)profile).Adaptee);
            }

            ser.Serialize(objList, fileName);
        }

        /// <summary>
        /// Creates a new generalization relation between two classes.
        /// </summary>
        /// <param name="general">Reference to the more general class</param>
        /// <param name="specific">Reference to the more specific class</param>
        /// <returns>Reference to the new generalization</returns>
        /// <exception cref="ArgumentException">If a cyclic inheritance relation would be created</exception>
		public Generalization SetGeneralization(Class general, Class specific)
		{
			if (general != null && specific != null)
			{
				if (CheckCyclicInheritance(general, specific, true) || CheckCyclicInheritance(specific, general, false))
					throw new ArgumentException("The generalization could not be created because a cyclic inheritance" +
					                            "relation would be created!");
			}

        	_Generalization generalization = new _Generalization(false, this);

			generalization.General = general;
			generalization.Specific = specific;

            Model.Generalizations.Add(generalization);

			return generalization;
		}


        /// <summary>
        /// Finds a named UML element by its qualified name.
        /// </summary>
        /// <param name="qualifiedName">Qualified name of the element</param>
        /// <returns>
        /// Reference to the element or null if no element with the given name is found.
        /// </returns>
        public NamedElement FindByQualifiedName(string qualifiedName)
        {
            return Model.GetChildByQualifiedName(qualifiedName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a reference to the user UML model.
        /// </summary>
        public Model Model
        {
            get { return userModel; }
        }

        /// <summary>
        /// Gets a collection of primitive data types.
        /// </summary>
        public IEnumerable<SimpleDataType> PrimitiveTypes
        {
            get { return primitiveTypes; }
        }

        /// <summary>
        /// Gets a collection of profiles.
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get { return profiles; }
        }

        /// <summary>
        /// Gets or sets the default namespace for the generated xml schemas.
        /// </summary>
        public string XMLNamespace
        {
            get { return xmlnamespace; }
            set { xmlnamespace = value; NotifyPropertyChanged("XMLNamespace"); }
        }

        public string XMLNamespaceOrDefaultNamespace
        {
            get
            {
                if (!String.IsNullOrEmpty(XMLNamespace))
                {
                    return XMLNamespace.EndsWith("/") ? XMLNamespace : XMLNamespace + "/";
                }
                else
                {
                    return "http://www.example.org/";
                }
            }
        }

		/// <summary>
		/// Schema name
		/// </summary>
    	public string Name
    	{
    		get { return name; }
    		set
    		{
    			name = value;
				NotifyPropertyChanged("Name");
    		}
    	}

		public override string ToString()
		{
			return Name;
		}

    	#endregion

        #region Fields

        /// <summary>
        /// References the user UML model.
        /// </summary>
        protected Model userModel;
        /// <summary>
        /// References the UML metamodel.
        /// </summary>
        protected Model metaClasses;
        /// <summary>
        /// Collection of primitive types.
        /// </summary>
        protected internal ObservableCollection<SimpleDataType> primitiveTypes;
        /// <summary>
        /// Collection of profiles.
        /// </summary>
        protected ObservableCollection<Profile> profiles;
        /// <summary>
        /// Specifies the default namespace of the user generated schemas.
        /// </summary>
        protected string xmlnamespace;

		/// <summary>
		/// Schema name
		/// </summary>
    	protected string name;

        #endregion

        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

		
    }
}
