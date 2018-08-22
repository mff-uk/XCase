using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Property metaclass adpater.
    /// </summary>
	[NotCloneable]
    internal class _Property : _TypedElement<NUml.Uml2.Property>, Property
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty property.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Property(Schema _schema) : this(true, _schema)
        {
            
        }

        /// <summary>
        /// Creates a new empty property.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee is also created, otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Property(bool createAdaptee, Schema _schema) : base(_schema, StereotypeTarget.Property)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Property();

            derivedPSMAttributes = new ObservableCollection<PSMAttribute>();
            parentClass = null;
            lower = 1;
        }

        #endregion

        #region Property Members

        public NUml.Uml2.AggregationKind Aggregation
        {
            get { return adaptedElement.Aggregation; }
            set 
            {
                if (adaptedElement.Aggregation != value)
                {
                    adaptedElement.Aggregation = value;
                    NotifyPropertyChanged("Aggregation");
                }
            }
        }

        public Class Class
        {
            get { return parentClass; }
            set 
            {
                if (parentClass != value)
                {
                    parentClass = value;

                    if (parentClass != null)
                        adaptedElement.Class = (parentClass as _ImplClass).AdaptedClass;
                    else
                        adaptedElement.Class = null;

                    NotifyPropertyChanged("Class");
                }
            }
        }

        public string Default
        {
            get
            {
                if (defaultValue != null)
                    return defaultValue.ToString();
                return null;
            }
            set
            {
                if (value == null || value.Equals(""))
                    defaultValue = null;
                else if (defaultValue == null)
                    defaultValue = new ValueSpecification(value, type);
                else if (!defaultValue.ToString().Equals(value))
                    defaultValue.ParseFromString(value);

                NotifyPropertyChanged("Default");
                NotifyPropertyChanged("DefaultValue");
            }
        }

        public ValueSpecification DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                if (defaultValue != value)
                {
                    defaultValue = value;
                    adaptedElement.DefaultValue = defaultValue.AdaptedElement;
                    NotifyPropertyChanged("DefaultValue");
                    NotifyPropertyChanged("Default");
                }
            }
        }

        public ObservableCollection<PSMAttribute> DerivedPSMAttributes
        {
            get { return derivedPSMAttributes; }
        }

        public bool IsComposite
        {
            get
            {
                return adaptedElement.IsComposite;
            }
            set
            {
                if (adaptedElement.IsComposite != value)
                {
                    adaptedElement.IsComposite = value;
                    NotifyPropertyChanged("IsComposite");
                }
            }
        }

        public bool IsDerived
        {
            get
            {
                return adaptedElement.IsDerived;
            }
            set
            {
                if (adaptedElement.IsDerived != value)
                {
                    adaptedElement.IsDerived = value;
                    NotifyPropertyChanged("IsDerived");
                }
            }
        }

        public bool IsDerivedUnion
        {
            get
            {
                return adaptedElement.IsDerivedUnion;
            }
            set
            {
                if (adaptedElement.IsDerivedUnion != value)
                {
                    adaptedElement.IsDerivedUnion = value;
                    NotifyPropertyChanged("IsDerivedUnion");
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return adaptedElement.IsReadOnly;
            }
            set
            {
                if (adaptedElement.IsReadOnly != value)
                {
                    adaptedElement.IsReadOnly = value;
                    NotifyPropertyChanged("IsReadOnly");
                }
            }
        }

        #endregion

        protected virtual IList ContainingCollection
        {
            get
            {
                return Class.Attributes;
            }
        }

        private IList removedFrom;

        private int removedFromIndex; 

        #region Element Members

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFrom == null);
            removedFrom = ContainingCollection;
            removedFromIndex = ContainingCollection.IndexOf(this);
            ContainingCollection.Remove(this);
        }

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            Debug.Assert(removedFrom != null);
            if (!removedFrom.Contains(this))
            {
                removedFrom.Insert(removedFromIndex, this);
            }
            removedFrom = null;
        }

        #region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type Property");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		Property copyProperty = (Property) copyElement;

			copyProperty.IsOrdered = this.IsOrdered;
			copyProperty.IsUnique = this.IsUnique;
			copyProperty.Lower = this.Lower;
			copyProperty.Upper = this.Upper;

    		copyProperty.Aggregation = Aggregation;
    		copyProperty.Default = Default;
    		copyProperty.DefaultValue = DefaultValue;
			copyProperty.IsComposite = IsComposite;
    		copyProperty.IsDerived = IsDerived;
			copyProperty.IsDerivedUnion = IsDerivedUnion;
    		copyProperty.IsReadOnly = IsReadOnly;
    	}

    	#endregion 

        #endregion

        #region MultiplicityElement Members

        public string MultiplicityString
        {
			get
			{
                if (Lower == null)
                    return Upper.ToString();
                else if (Lower != Upper)
                    return String.Format("{0}..{1}", Lower, Upper);
                else
                    return Lower.ToString();
			}
        }

        public bool IsOrdered
        {
            get
            {
                return adaptedElement.IsOrdered;
            }
            set
            {
                if (adaptedElement.IsOrdered != value)
                {
                    adaptedElement.IsOrdered = value;
                    NotifyPropertyChanged("IsOrdered");
                }
            }
        }

        public bool IsUnique
        {
            get
            {
                return adaptedElement.IsUnique;
            }
            set
            {
                if (adaptedElement.IsUnique != value)
                {
                    adaptedElement.IsUnique = value;
                    NotifyPropertyChanged("IsUnique");
                }
            }
        }

        public uint? Lower
        {
            get
            {
                return lower;
            }
            set
            {
                if (lower != value)
                {
                    lower = value;

                    if (value != null)
                        adaptedElement.Lower = (uint)value;
                    
                    NotifyPropertyChanged("Lower");
                    NotifyPropertyChanged("MultiplicityString");
                }
            }
        }

        public NUml.Uml2.UnlimitedNatural Upper
        {
            get
            {
                return adaptedElement.Upper;
            }
            set
            {
                if (adaptedElement.Upper != value)
                {
                    adaptedElement.Upper = value;
                    NotifyPropertyChanged("Upper");
					NotifyPropertyChanged("MultiplicityString");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the parent class.
        /// </summary>
        protected Class parentClass;

        /// <summary>
        /// Lower bound of the multiplicity element.
        /// Since it can be either set to a number or null and the adaptee
        /// does not offer this possibility, we have to add it ourselves.
        /// </summary>
        protected uint? lower;

        /// <summary>
        /// References the specification of the default value of this property.
        /// </summary>
        protected ValueSpecification defaultValue;

        /// <summary>
        /// Collection fo PSM Attributes derived from this attribute.
        /// </summary>
        protected ObservableCollection<PSMAttribute> derivedPSMAttributes;

        #endregion

		public override string ToString()
		{
			return Name;
		}
    }
}
