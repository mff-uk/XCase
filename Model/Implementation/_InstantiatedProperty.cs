using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Slot metaclass adapter.
    /// </summary>
	[NoDeleteUndoRedoSupport]
	[NotCloneable]
    internal class _InstantiatedProperty : _Element<NUml.Uml2.Slot>, InstantiatedProperty
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the given property.
        /// If the property has a default value, it is assigned to the Value property.
        /// </summary>
        /// <param name="property">Reference to the property being instantiated</param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _InstantiatedProperty(Property property, Schema _schema) : base(_schema, StereotypeTarget.Slot)
        {
            propertyName = property.Name;
            adaptedElement = NUml.Uml2.Create.Slot();

            if (property.DefaultValue != null)
            {
                value = property.DefaultValue;
                adaptedElement.Value.Add(property.DefaultValue.AdaptedElement);
            }
            else
            {
                value = null;
                adaptedElement.Value.Add(null);
            }

            adaptedElement.DefiningFeature = (property as _Property).Adaptee;
        }

        #endregion

        #region InstantiatedProperty Members

        public string Name
        {
            get { return propertyName; }
        }

        public ValueSpecification Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    adaptedElement.Value.Clear();
                    adaptedElement.Value.Add(value.AdaptedElement);
                    NotifyPropertyChanged("Value");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Name of the instantiated property.
        /// </summary>
        protected string propertyName;
        /// <summary>
        /// Value of the instantiated property.
        /// </summary>
        protected ValueSpecification value;

        #endregion

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type _InstantiatedProperty");
    	}

		public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
		{
			throw new InvalidOperationException("FillCopy should not be called on objects of type type _InstantiatedProperty");
		}

    	#endregion  
    }
}
