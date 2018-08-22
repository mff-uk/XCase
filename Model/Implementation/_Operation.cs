using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Operation metaclass adapter.
    /// </summary>
	[NotCloneable]
	internal class _Operation : _TypedElement<NUml.Uml2.Operation>, Operation
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty operation.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Operation(Schema _schema) : this(true, _schema)
        {
        }

        /// <summary>
        /// Creates a new empty operation.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee is also created, otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Operation(bool createAdaptee, Schema _schema) : base(_schema, StereotypeTarget.Operation)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Operation();

            parameters = new ObservableCollection<Parameter>();
            parameters.CollectionChanged += OnParametersChanged;
            parentClass = null;
            lower = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called whenever the parameters collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnParametersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Parameter param in parameters)
                {
                    _Parameter implParam = param as _Parameter;
                    if (implParam == null)
                    {
                        parameters.Remove(param);
                        throw new ArgumentException("Parameter element created outside the model library " +
                            "cannot be inserted to the operation!");
                    }

                    implParam.Operation = this;
                    adaptedElement.OwnedParameter.Add(implParam.Adaptee);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Parameter param in parameters)
                {
                    _Parameter implParam = param as _Parameter;

                    implParam.Operation = null;
                    adaptedElement.OwnedParameter.Remove(implParam.Adaptee);
                }
            }
        }

        #endregion

        #region Element Members

        /// <summary>
        /// References the class that this operation was removed from.
        /// Null if the operation is currently in the model.
        /// </summary>
        private Class removedFromClass = null;

        public int removedFromClassIndex;

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();
            
            Debug.Assert(removedFromClass != null);
            if (!removedFromClass.Operations.Contains(this))
                removedFromClass.Operations.Insert(removedFromClassIndex, this);

            removedFromClass = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromClass == null);
            removedFromClass = parentClass;
            removedFromClassIndex = parentClass.Operations.IndexOf(this);
            parentClass.Operations.Remove(this);
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type _Operation");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		Operation copyOperation = (Operation) copyElement;

    		//copyOperation.IsOrdered = this.IsOrdered;
    		//copyOperation.IsUnique = this.IsUnique;
    		copyOperation.Lower = this.Lower;
    		copyOperation.Upper = this.Upper;

    		foreach (Parameter parameter in Parameters)
    		{
    			Parameter copyParameter = this.AddParameter();
                parameter.FillCopy(copyParameter, targetModel, createdCopies);
    		}
    	}

    	#endregion 

        #endregion

        #region Operation Members

        public Parameter AddParameter()
        {
            _Parameter param = new _Parameter(Schema);

            parameters.Add(param);
            param.Name = "Param" + parameters.Count;
            return param;
        }

        public Class Class
        {
            get { return parentClass; }
            set
            {
                parentClass = value;
            }
        }

        public ObservableCollection<Parameter> Parameters
        {
            get { return parameters; }
        }

        #endregion

        #region MultiplicityElement Members

        public string MultiplicityString
        {
            get
            {
				if (Lower != Upper)
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
                throw new NotSupportedException("Operation.IsOrdered property is read only!");
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
                throw new NotSupportedException("Operation.IsUnique property is read only!");
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
                    if (lower != null)
                        adaptedElement.Lower = (uint)lower;
                    NotifyPropertyChanged("Lower");
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
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of this operation parameters.
        /// </summary>
        protected ObservableCollection<Parameter> parameters;
        /// <summary>
        /// References the class that owns this operation.
        /// </summary>
        protected Class parentClass;
        /// <summary>
        /// Lower bound of the multiplicity element.
        /// Since it can be either set to a number or null and the adaptee
        /// does not offer this possibility, we have to add it ourselves.
        /// </summary>
        protected uint? lower;
        
        #endregion
    }
}
