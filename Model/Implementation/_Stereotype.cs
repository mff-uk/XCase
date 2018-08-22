using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML stereotype metaclass adapter.
    /// </summary>
    /// <remarks>
    /// The Package property inherited from DataType is overriden to check
    /// that the stereotype can only be included in a profile and not an other
    /// type of a package.
    /// </remarks>
	[NoDeleteUndoRedoSupport]
	[NotCloneable]
    internal class _Stereotype : _Class<NUml.Uml2.Stereotype>, Stereotype
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty stereotype.
        /// The adapted nUML stereotype object is created but has no attributes nor operations.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Stereotype(Schema _schema) : this(true, _schema)
        {
            
        }

        /// <summary>
        /// Creates a new empty stereotype.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee (NUml.Uml2.Stereotype is created), otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Stereotype(bool createAdaptee, Schema _schema) : base(false, _schema, StereotypeTarget.Stereotype)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Stereotype();

            applicationDomain = new ObservableCollection<string>();
            applicationDomain.CollectionChanged += OnApplicationDomainChanged;

            extensionEnds = new List<NUml.Uml2.Property>();
        }

        #endregion

        #region Methods

    	/// <summary>
        /// Changes the application domain of the stereotype.
        /// It removes extensions to the metaclasses that were removed from the domain
        /// and adds extensions to those that were added.
        /// </summary>
        protected void OnApplicationDomainChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Access the parent profile.
            // The cast is safe as the type is checked when the package property is being set
            _Profile profile = package as _Profile;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (string metaClassName in e.NewItems)
                {
                    _ImplClass metaClass = null;

                    foreach (Model metamodel in profile.MetamodelReference)
                    {
                        // Verify that the metamodel contains the metaclass
                        // Otherwise throw exception
                        metaClass = (_ImplClass)metamodel.Classes.Get(metaClassName);
                        if (metaClass != null)
                            break;
                    }

                    if (metaClass == null)
                        throw new ArgumentException("The profile that contains this stereotype does not reference " +
                                "any metamodel that contains metaclass that the extended element is an instance of!");

                    NUml.Uml2.Extension extension = NUml.Uml2.Create.Extension();
                    NUml.Uml2.Property end0 = NUml.Uml2.Create.Property();
                    NUml.Uml2.ExtensionEnd end1 = NUml.Uml2.Create.ExtensionEnd();

                    // Initialize the extension and its ends
                    extension.Name = adaptedElement.Name + " extends " + metaClassName;
                    end0.Name = "base_" + metaClassName;
                    end0.Visibility = NUml.Uml2.VisibilityKind.@private;
                    end0.Association = extension;
                    end0.Type = metaClass.AdaptedClass;
                    end1.Name = "extension_" + adaptedElement.Name;
                    end1.Owner = extension;
                    end1.Visibility = NUml.Uml2.VisibilityKind.@private;
                    end1.Type = (NUml.Uml2.Stereotype)adaptedElement;

                    // Assign extension ends correctly to both extension and stereotype
                    extension.OwnedEnd = end1;
                    extension.MemberEnd.Add(end0);
                    extensionEnds.Add(end0);
                    adaptedElement.OwnedAttribute.Add(end0);

                    profile.Adaptee.OwnedMember.Add(extension);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (string metaClassName in e.OldItems)
                {
                    // Find the extension end owned by the stereotype (by name)
                    string endName = "base_" + metaClassName;
                    NUml.Uml2.Property end = null;

                    foreach (NUml.Uml2.Property extEnd in extensionEnds)
                    {
                        if (extEnd.Name == endName)
                        {
                            end = extEnd;
                            break;
                        }
                    }

                    // If the extension end was not found, continue 
                    // (it is no more in the collection for some reason)
                    if (end == null)
                        continue;

                    // Remove the extension from the stereotype and the parent profile
                    extensionEnds.Remove(end);
                    adaptedElement.OwnedAttribute.Remove(end);
                    NUml.Uml2.Association extension = end.Association;
                    profile.Adaptee.OwnedMember.Remove(extension);
                }
            }
        }

        #endregion

		#region Element Members

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type _Stereotype");
    	}

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			if (targetModel.Schema == this.Schema)
			{
				throw new InvalidOperationException("Copies of Stereotype should not be created in the same model.");
			} 
			
			base.FillCopy(copyElement, targetModel, createdCopies);

			Stereotype copyStereotype = (Stereotype) copyElement;
			foreach (string appliesTo in AppliesTo)
			{
				copyStereotype.AppliesTo.Add(appliesTo);
			}
		}

    	#endregion 

		#endregion 

		#region Stereotype Members

		public ObservableCollection<string> AppliesTo
        {
            get { return applicationDomain; }
        }

        public StereotypeInstance ApplyTo(Element element)
        {
            // Cast the element
            _ImplElement implElement = element as _ImplElement;
            if (implElement == null)
                throw new ArgumentException("The given element was created outside the model library!");
            
            // Verify that the element can be extended by this stereotype
            Type metaClassType = implElement.AdaptedElement.GetType();
            bool extensibleElement = false;

            foreach (string metaClassName in applicationDomain)
            {
                Type metaClassInterface = metaClassType.GetInterface(metaClassName, true);
                if (metaClassInterface != null)
                {
                    extensibleElement = true;
                    break;
                }
            }

            if (!extensibleElement)
                throw new NotSupportedException("The given element is not an instance of a metaclass that " + 
                    "may be extended by this stereotype!");

            // Apply the stereotype
            _StereotypeInstance instance = new _StereotypeInstance(this, Schema);
            element.AppliedStereotypes.Add(instance);

            // Return the stereotype instance
            return instance;
        }

        #endregion

        #region DataType Members

        public override Package Package
        {
            get
            {
                return base.Package;
            }
            set
            {
                if (!(value is _Profile))
                    throw new ArgumentException("A stereotype can only be included in a profile!");
                else
                    base.Package = value;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of names of the metaclasses that can be extended
        /// by this stereotype.
        /// </summary>
        protected ObservableCollection<string> applicationDomain;

        /// <summary>
        /// Collection of owned extension ends.
        /// </summary>
        protected List<NUml.Uml2.Property> extensionEnds;

        #endregion
    }
}
