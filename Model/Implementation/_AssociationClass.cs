using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML AssociationClass construct.
    /// </summary>
	[NoDeleteUndoRedoSupport]
    internal class _AssociationClass : _Association<NUml.Uml2.AssociationClass>, _ImplAssociationClass
    {
        #region Constructors

        /// <summary>
        /// Creates a new association class.
        /// </summary>
        /// <param name="classes">Collection of classes to be associated.</param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _AssociationClass(IEnumerable<Class> classes, Schema _schema) 
            : base(classes, _schema, false, StereotypeTarget.AssociationClass)
        {
            adaptedElement = NUml.Uml2.Create.AssociationClass();
            
            associations = new ObservableCollection<Association>();
            attributes = new ObservableCollection<Property>();
            generalizations = new ObservableCollection<Generalization>();
            operations = new ObservableCollection<Operation>();
            specifications = new ObservableCollection<Generalization>();
            derivedPSMClasses = new ObservableCollection<PSMClass>();

            attributes.CollectionChanged += OnAttributesChanged;
            operations.CollectionChanged += OnOperationsChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the attributes collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected virtual void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Property attribute in e.OldItems)
                {
                    _Property implAttr = attribute as _Property;

                    implAttr.Class = null;
                    adaptedElement.OwnedAttribute.Remove(implAttr.Adaptee);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Property attribute in e.NewItems)
                {
                    _Property implAttr = attribute as _Property;

                    if (implAttr == null)
                        throw new ArgumentException("A property element created outside the model library " +
                            "has been added to the collection!");

                    implAttr.Class = this;
                    adaptedElement.OwnedAttribute.Add(implAttr.Adaptee);
                }
            }
        }

        /// <summary>
        /// Called whenever the operations collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnOperationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Operation operation in e.OldItems)
                {
                    _Operation implOp = operation as _Operation;

                    implOp.Class = null;
                    adaptedElement.OwnedOperation.Remove(implOp.Adaptee);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Operation operation in e.NewItems)
                {
                    _Operation implOp = operation as _Operation;
                    if (implOp == null)
                        throw new ArgumentException("An operation element created outside the model " +
                            "library has been added to the collection!");

                    implOp.Class = this;
                    adaptedElement.OwnedOperation.Add(implOp.Adaptee);
                }
            }
        }

        /// <summary>
        /// Creates a list grouping all the associations from the Associations collection
        /// of the given class and all the more general classes (recursively).
        /// </summary>
        /// <param name="cls">References the most specific class in the inspected hierarchy</param>
        /// <returns>List of XCase.Model.Association</returns>
        protected List<Association> GetAssociationsThroughInheritance(Class cls)
        {
            List<Association> retList = new List<Association>();

            if (cls != null)
            {
                retList.AddRange(cls.Assocations);
                foreach (Generalization generalization in cls.Generalizations)
                    retList.AddRange(GetAssociationsThroughInheritance(generalization.General));
            }

            return retList;
        }

        /// <summary>
        /// Gets a list grouping all the ancestors
        /// of this class including this class and all the more general classes (recursively).
        /// </summary>
        /// <param name="cls">References the most specific class in the inspected hierarchy</param>
        /// <returns>List of XCase.Model.Class</returns>
        protected List<Class> GetMeAndAncestors(Class cls)
        {
            List<Class> retList = new List<Class>();

            if (cls != null)
            {
                retList.Add(cls);
                foreach (Generalization generalization in cls.Generalizations)
                    retList.AddRange(GetMeAndAncestors(generalization.General));
            }

            return retList;
        }

        /// <summary>
        /// Creates a list grouping all the attributes from the Attributes collection
        /// of the given class and all the more general classes (recursively).
        /// </summary>
        /// <param name="cls">References the most specific class in the inspected hierarchy</param>
        /// <returns>List of XCase.Model.Property</returns>
        protected List<Property> GetAttributesThroughInheritance(Class cls)
        {
            List<Property> retList = new List<Property>();

            if (cls != null)
            {
                retList.AddRange(cls.Attributes);
                foreach (Generalization generalization in cls.Generalizations)
                    retList.AddRange(GetAttributesThroughInheritance(generalization.General));
            }

            return retList;
        }

        #endregion

		#region Element Members 

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
			IEnumerable<Class> classes;
    		if (targetModel == Schema.Model)
    		{
    			classes = this.Ends.Select(end => end.Class);
    		}
			else
    		{
    			classes = this.Ends.Select(end => (Class) createdCopies[end.Class]);
    		}
    		AssociationClass clone = targetModel.Schema.CreateAssociationClass(classes);
    		return clone;
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		AssociationClass copyAssociationClass = (AssociationClass) copyElement;
			copyAssociationClass.IsAbstract = IsAbstract;

            SubElementCopiesMap subElementCopiesMap = createdCopies.GetSubElementsList(this);

			foreach (Property attribute in Attributes)
			{
				Property copyAttribute = copyAssociationClass.AddAttribute();
				attribute.FillCopy(copyAttribute, targetModel, createdCopies);
                subElementCopiesMap[attribute] = copyAttribute;
			}

			foreach (Operation operation in Operations)
			{
				Operation copyOperation = copyAssociationClass.AddOperation();
                operation.FillCopy(copyOperation, targetModel, createdCopies);
                subElementCopiesMap.Add(operation, copyOperation);
			}
    	}

    	#endregion 

		#endregion 

		#region PIMClass Members

		public virtual Property AddAttribute()
        {
            _Property attr = new _Property(Schema);
            attributes.Add(attr);
            attr.Name = "Attribute" + attributes.Count;

            return attr;
        }

        public Operation AddOperation()
        {
            _Operation operation = new _Operation(Schema);
            operations.Add(operation);
            operation.Name = "Operation" + operations.Count;

            return operation;
        }

        public PSMClass DerivePSMClass()
        {
            _PSMClass psm = new _PSMClass(this, Schema);

            derivedPSMClasses.Add(psm);
            package.PSMClasses.Add(psm);
            
            return psm;
        }

        public ObservableCollection<PSMClass> DerivedPSMClasses
        {
            get { return derivedPSMClasses; }
        }

        public bool IsAbstract
        {
            get { return adaptedElement.IsAbstract; }
            set
            {
                if (adaptedElement.IsAbstract != value)
                {
                    adaptedElement.IsAbstract = value;
                    NotifyPropertyChanged("IsAbstract");
                }
            }
        }

        public List<Association> AllAssociations
        {
            get
            {
                return GetAssociationsThroughInheritance(this);
            }
        }

        public List<Property> AllAttributes
        {
            get
            {
                return GetAttributesThroughInheritance(this);
            }
        }

        public List<Class> MeAndAncestors
        {
            get
            {
                return GetMeAndAncestors(this);
            }
        }

        public ObservableCollection<Association> Assocations
        {
            get { return associations; }
        }

        public ObservableCollection<Property> Attributes
        {
            get { return attributes; }
        }

        public ObservableCollection<Generalization> Generalizations
        {
            get { return generalizations; }
        }

        public ObservableCollection<Operation> Operations
        {
            get { return operations; }
        }

        public ObservableCollection<Generalization> Specifications
        {
            get { return specifications; }
        }

        public List<Generalization> GetPathToAncestor(Class ancestor)
        {
            List<Generalization> retList = null;
            List<Generalization> tmpList = null;

            foreach (Generalization gen in generalizations)
            {
                if (gen.General == ancestor)
                {
                    retList = new List<Generalization>();
                    retList.Add(gen);
                }
                else
                {
                    tmpList = gen.General.GetPathToAncestor(ancestor);
                    if (tmpList != null)
                    {
                        tmpList.Add(gen);
                        retList = tmpList;
                        break;
                    }
                }
            }

            return retList;
        }

        #endregion

        #region DataType Members

        public override NamedElement GetChildByQualifiedName(string qName)
        {
            NamedElement ae;
            string sName;

            if (BasicGetByQualifiedName(qName, out sName, out ae))
            {
                if ((ae = associationEnds.GetByQualifiedName(sName)) != null)
                    return ae;
                if ((ae = attributes.GetByQualifiedName(sName)) != null)
                    return ae;
                if ((ae = operations.GetByQualifiedName(sName)) != null)
                    return ae;
            }

            return ae;
        }

        public Package Package
        {
            get { return package; }
            set
            {
                if (package != value)
                {
                    package = value;
                    if (value == null)
                        adaptedElement.Package = null;
                    else
                        adaptedElement.Package = (package as _ImplPackage).AdaptedPackage;

                    NotifyPropertyChanged("Package");
                }
            }
        }

        #endregion

        #region _ImplAssociationClass Members

        public NUml.Uml2.AssociationClass AdaptedAssociationClass
        {
            get { return adaptedElement; }
        }

        #endregion

        #region _ImplClass Members

        public NUml.Uml2.Class AdaptedClass
        {
            get { return adaptedElement; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of attributes of this association class.
        /// </summary>
        protected ObservableCollection<Property> attributes;
        /// <summary>
        /// Collection of associations that this association class is a part of.<br />
        /// The collection does not contain the association of this association class!
        /// </summary>
        protected ObservableCollection<Association> associations;
        /// <summary>
        /// Collection of generalizations.
        /// </summary>
        protected ObservableCollection<Generalization> generalizations;
        /// <summary>
        /// Collection of operations of this associaion class.
        /// </summary>
        protected ObservableCollection<Operation> operations;
        /// <summary>
        /// Reference to the package that owns this class.
        /// </summary>
        protected Package package;
        /// <summary>
        /// Collection of specifications;
        /// </summary>
        protected ObservableCollection<Generalization> specifications;
        /// <summary>
        /// Collection of PSM classes that were derived from this class.
        /// </summary>
        protected ObservableCollection<PSMClass> derivedPSMClasses;

        #endregion

		public override string ToString()
		{
			String name = "";
			foreach (AssociationEnd end in associationEnds)
			{
				name += end + " - ";
			}
			name = name.Substring(0, name.Length - 3);
			return string.Format("Association Class {0}( {1} )", Name, name);
		}
    }
}
