using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML InstanceSpecification metaclass adapter.
    /// InstanceSpecification is used uniquely for the instanciation of stereotypes
    /// in this library.
    /// </summary>
	[NoDeleteUndoRedoSupport]
	[NotCloneable]
    internal class _StereotypeInstance : _NamedElement<NUml.Uml2.InstanceSpecification>, StereotypeInstance
    {
        #region Constructors

        /// <summary>
        /// Creates an instance of the given stereotype.
        /// </summary>
        /// <param name="stereotype">
        /// Reference to the stereotype class.
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _StereotypeInstance(_Stereotype stereotype, Schema _schema) 
            : base(_schema, StereotypeTarget.InstanceSpecification)
        {
            adaptedElement = NUml.Uml2.Create.InstanceSpecification();
            adaptedElement.Name = stereotype.Name;
            adaptedElement.Classifier.Add(stereotype.Adaptee);
            this.stereotype = stereotype;

            List<InstantiatedProperty> tmpList = new List<InstantiatedProperty>();

            foreach (Property property in stereotype.Attributes)
            {
                _InstantiatedProperty tmpProperty = new _InstantiatedProperty(property, Schema);
                tmpList.Add(tmpProperty);
                adaptedElement.Slot.Add(tmpProperty.Adaptee);
                tmpProperty.Adaptee.OwningInstance = adaptedElement;
            }

            properties = new ReadOnlyCollection<InstantiatedProperty>(tmpList);
        }

        #endregion

        #region StereotypeInstance Members

        public System.Collections.ObjectModel.ReadOnlyCollection<InstantiatedProperty> Attributes
        {
            get { return properties; }
        }

        public Stereotype Stereotype
        {
            get { return stereotype; }
        }

        #endregion

		#region ElementMembers

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("Clone should not be called on objects of type type StereotypeInstance");
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		throw new InvalidOperationException("CreateCopy should not be called on objects of type type StereotypeInstance");
    	}

    	#endregion 

		#endregion 

		#region Fields

		/// <summary>
        /// Collection of attributes of the stereotype instance.
        /// </summary>
		readonly ReadOnlyCollection<InstantiatedProperty> properties;

        /// <summary>
        /// References the instantiated stereotype class.
        /// </summary>
        readonly Stereotype stereotype;

        #endregion
    }
}
