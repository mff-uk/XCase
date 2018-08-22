using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Package metaclass adapter.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adapted nUML element, has to inherit from NUml.Uml2.Package
    /// </typeparam>
	[NotCloneable]
	internal class _Package<NUmlType> : _NamedElement<NUmlType>, _ImplPackage
                            where NUmlType : NUml.Uml2.Package
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty package.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Package(Schema _schema) : this(true, _schema)
        {
            
        }

        /// <summary>
        /// Creates a new empty package.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee is also created, otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Package(bool createAdaptee, Schema _schema) : base(_schema, StereotypeTarget.Package)
        {
            if (createAdaptee)
                adaptedElement = (NUmlType)NUml.Uml2.Create.Package();
            
            classes = new ObservableCollection<PIMClass>();
            nestedPackages = new ObservableCollection<Package>();
            ownedTypes = new ObservableCollection<DataType>();
            psmClasses = new ObservableCollection<PSMClass>();

            classes.CollectionChanged += OnClassesChanged;
            ownedTypes.CollectionChanged += OnDataTypesChanged;
            nestedPackages.CollectionChanged += OnNestedPackagesChanged;
            psmClasses.CollectionChanged += OnPSMClassesChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the classes collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PIMClass cls in e.NewItems)
                {
                    _ImplClass implCls = cls as _ImplClass;

                    if (implCls == null)
                    {
                        classes.Remove(cls);
                        throw new ArgumentException("A class created outside the model library inserted into classes!");
                    }

                    implCls.Package = this;

                    if (!adaptedElement.OwnedType.Contains(implCls.AdaptedClass))
                        adaptedElement.OwnedType.Add(implCls.AdaptedClass);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PIMClass cls in e.OldItems)
                {
                    adaptedElement.OwnedType.Remove(((_ImplClass)cls).AdaptedClass);

                    ((_ImplClass)cls).Package = null;
                }
            }
        }
        
        /// <summary>
        /// Called whenever the owned types collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnDataTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (DataType type in e.NewItems)
                {
                    _ImplDataType implType = type as _ImplDataType;
                    if (implType == null)
                        throw new ArgumentException("A type created outside the model library was " + 
                            "added to the collection!");
                    
                    if (!adaptedElement.OwnedType.Contains(implType.AdaptedType))
                        adaptedElement.OwnedType.Add(implType.AdaptedType);
                    implType.Package = this;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (DataType type in e.OldItems)
                {
                    _ImplDataType implType = type as _ImplDataType;
                    adaptedElement.OwnedType.Remove(implType.AdaptedType);
                    implType.Package = null;
                }
            }
        }

        /// <summary>
        /// Called whenever the nested packages collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnNestedPackagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Package pkg in e.NewItems)
                {
                    _ImplPackage implPkg = pkg as _ImplPackage;
                    if (implPkg == null)
                        throw new ArgumentException("A package created outside the model library " +
                            "has been added to the collection!");

                    implPkg.NestingPackage = this;
                    adaptedElement.NestedPackage.Add(implPkg.AdaptedPackage);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Package pkg in e.OldItems)
                {
                    _ImplPackage implPkg = pkg as _ImplPackage;
                    implPkg.NestingPackage = null;
                    adaptedElement.NestedPackage.Remove(implPkg.AdaptedPackage);
                }
            }
        }

        /// <summary>
        /// Called whenever the PSM classes collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnPSMClassesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PSMClass cls in e.NewItems)
                {
                    _PSMClass implClass = cls as _PSMClass;
                    if (implClass == null)
                    {
                        psmClasses.Remove(cls);
                        throw new ArgumentException("A PSMClass created outside the model library was added " + 
                            "to the PSMClasses collection!");
                    }

                    adaptedElement.OwnedType.Add(implClass.AdaptedClass);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PSMClass cls in e.OldItems)
                {
                    adaptedElement.OwnedType.Remove((cls as _PSMClass).AdaptedClass);
                }
            }
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            Debug.Assert(removedFromPackage != null);
            if (!removedFromPackage.NestedPackages.Contains(this))
                removedFromPackage.NestedPackages.Add(this);
            removedFromPackage = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromPackage == null);
            removedFromPackage = nestingPackage;
            nestingPackage.NestedPackages.Remove(this);
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		if (targetModel != Schema.Model || createdCopies.ContainsKey(NestingPackage))
    		{
    			Package parentPackage = (Package) createdCopies[NestingPackage];
    			return parentPackage.AddNestedPackage(); 
    		}
			else
    		{
    			return NestingPackage.AddNestedPackage();
    		}
    	}

    	#endregion 

        #endregion

        #region Package Members

        public PIMClass AddClass()
        {
            _PIMClass cls = new _PIMClass(Schema);
            classes.Add(cls);
            cls.Name = "Class" + classes.Count;

            return cls;
        }

        public SimpleDataType AddSimpleDataType()
        {
            _SimpleDataType type = new _SimpleDataType(Schema);
            ownedTypes.Add(type);

            return type;
        }

        public SimpleDataType AddSimpleDataType(SimpleDataType baseType)
        {
            _SimpleDataType type = new _SimpleDataType(Schema, baseType);
            ownedTypes.Add(type);

            return type;
        }

        public Package AddNestedPackage()
        {
            _Package<NUml.Uml2.Package> pkg = new _Package<NUml.Uml2.Package>(Schema);
            nestedPackages.Add(pkg);

            return pkg;
        }

        public override NamedElement GetChildByQualifiedName(string qName)
        {
            NamedElement ae;
            string sName;

            if (BasicGetByQualifiedName(qName, out sName, out ae))
            {
                if ((ae = nestedPackages.GetByQualifiedName(sName)) != null)
                    return ae;
                if ((ae = ownedTypes.GetByQualifiedName(sName)) != null)
                    return ae;
            }

            return ae;
        }

		public IEnumerable<DataType> AllTypes
        {
            get
            {
				IEnumerable<DataType> retList = Schema.PrimitiveTypes.Cast<DataType>();
            	retList = retList.Union(OwnedTypes);
                Package parent = nestingPackage;

                while (parent != null)
                {
                	retList = retList.Union(parent.OwnedTypes);
                    parent = parent.NestingPackage;
                }

                return retList;
            }
        }

        public ObservableCollection<PIMClass> Classes
        {
            get { return classes; }
        }

        public Package NestingPackage
        {
            get { return nestingPackage; }
            set
            {
                if (nestingPackage != value)
                {
                    nestingPackage = value;
                    if (nestingPackage != null)
                        adaptedElement.NestingPackage = (value as _ImplPackage).AdaptedPackage;
                    else
                        adaptedElement.NestingPackage = null;
                    
                    NotifyPropertyChanged("NestingPackage");
                }
            }
        }

        public ObservableCollection<Package> NestedPackages
        {
            get { return nestedPackages; }
        }

        public ObservableCollection<DataType> OwnedTypes
        {
            get { return ownedTypes; }
        }

        public ObservableCollection<PSMClass> PSMClasses
        {
            get { return psmClasses; }
        }

        #endregion

        #region _ImplPackage Members

        public NUml.Uml2.Package AdaptedPackage
        {
            get { return Adaptee; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of owned classes.     
        /// </summary>
        protected ObservableCollection<PIMClass> classes;
        /// <summary>
        /// Collection of packages nested in this one.
        /// </summary>
        protected ObservableCollection<Package> nestedPackages;
        /// <summary>
        /// Collection of owned types.
        /// </summary>
        protected ObservableCollection<DataType> ownedTypes;
        /// <summary>
        /// References the nesting package.
        /// </summary>
        protected Package nestingPackage;
        /// <summary>
        /// Collection of PSM classes.
        /// </summary>
        protected ObservableCollection<PSMClass> psmClasses;
        /// <summary>
        /// References the package that this one was removed from.
        /// Null if the package is currently in the model.
        /// </summary>
        protected Package removedFromPackage = null;

        #endregion
    }
}
