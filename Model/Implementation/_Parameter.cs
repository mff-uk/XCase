using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Parameter metaclass adapter.
    /// </summary>
	[NotCloneable]
	internal class _Parameter : _TypedElement<NUml.Uml2.Parameter>, Parameter
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty operation.
        /// The adaptee is created but the Operation is left null.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Parameter(Schema _schema) : this(true, _schema)
        {
        }

        /// <summary>
        /// Creates a new empty operation.
        /// </summary>
        /// <param name="createAdaptee">
        /// If true, the adaptee is created, otherwise it is left null.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Parameter(bool createAdaptee, Schema _schema) : base(_schema, StereotypeTarget.Parameter)
        {
            if (createAdaptee)
                adaptedElement = NUml.Uml2.Create.Parameter();

            lower = 1;
            operation = null;
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            Debug.Assert(removedFromOperation != null);
            if (!removedFromOperation.Parameters.Contains(this))
                removedFromOperation.Parameters.Add(this);

            removedFromOperation = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromOperation == null);
            removedFromOperation = operation;
            operation.Parameters.Remove(this);
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type Parameter");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		Parameter copyParameter = (Parameter) copyElement;

    		copyParameter.Direction = this.Direction;
			copyParameter.IsOrdered = this.IsOrdered;
			copyParameter.IsUnique = this.IsUnique;
			copyParameter.Lower = this.Lower;
			copyParameter.Upper = this.Upper;
    	}

    	#endregion 

        #endregion

        #region Parameter Members

        public NUml.Uml2.ParameterDirectionKind Direction
        {
            get
            {
                return adaptedElement.Direction;
            }
            set
            {
                if (adaptedElement.Direction != value)
                {
                    adaptedElement.Direction = value;
                    NotifyPropertyChanged("Direction");
                }
            }
        }

        public Operation Operation
        {
            get { return operation; }
            set 
            {
                if (operation != value)
                {
                    if (value == null)
                    {
                        operation = null;
                        adaptedElement.Operation = null;
                    }
                    else
                    {

                        _Operation implOperation = operation as _Operation;
                        if (implOperation == null)
                            throw new ArgumentException("An operation element created outside the model library " +
                                "cannot own the parameter!");

                        operation = value;
                        adaptedElement.Operation = implOperation.Adaptee;
                    }

                    NotifyPropertyChanged("Operation");
                }
            }
        }

        #endregion

        #region MultiplicityElement Members

        public string MultiplicityString
        {
			get
			{
                if (Lower != Upper)
                    return String.Format("{0}..{1}", Lower, Upper);
                else if (Lower == null)
                    return Upper.ToString();
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
                    NotifyPropertyChanged("CardinalityString");
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
                    NotifyPropertyChanged("CardinalityString");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Lower bound of the multiplicity element.
        /// Since it can be either set to a number or null and the adaptee
        /// does not offer this possibility, we have to add it ourselves.
        /// </summary>
        protected uint? lower;
        
        /// <summary>
        /// References the operation that owns this parameter.
        /// </summary>
        protected Operation operation;

        /// <summary>
        /// References the operation that this parameter was removed from.
        /// Null if the parameter is currently in the model.
        /// </summary>
        protected Operation removedFromOperation = null;

        #endregion
    }
}
