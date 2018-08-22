using System;
using System.Collections.Generic;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of a simple data type construct.
    /// The adapted UML type is the Primitive type construct.
    /// </summary>
	[NoDeleteUndoRedoSupport]
    internal class _SimpleDataType : _DataType<NUml.Uml2.PrimitiveType>, SimpleDataType
    {
        #region Constructors

        /// <summary>
        /// Creates a new simple data type that is not inherited from any other type.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _SimpleDataType(Schema _schema) : this(_schema, null)
        {

        }

        /// <summary>
        /// Creates a new simple data type that inherits from a given simple type.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="_parent">Reference to the base type</param>
        public _SimpleDataType(Schema _schema, SimpleDataType _parent)
            : base(_schema)
        {
            adaptedElement = NUml.Uml2.Create.PrimitiveType();
            parent = _parent;
        }

        #endregion

		#region Element members

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		SimpleDataType clone = targetModel.Schema.AddPrimitiveType();

    		return clone;
    	}

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		SimpleDataType copySimpleDataType = (SimpleDataType) copyElement;
    		copySimpleDataType.DefaultXSDImplementation = DefaultXSDImplementation;
    	}

    	#endregion 

		#endregion 

		#region SimpleDataType Members

		public SimpleDataType Parent
        {
            get { return parent; }
        }

        public string DefaultXSDImplementation
        {
            get
            {
                return defaultXSD;
            }
            set
            {
                if (defaultXSD != value)
                {
                    defaultXSD = value;
                    NotifyPropertyChanged("DefaultXSDImplementation");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the base type of this one.
        /// </summary>
        protected SimpleDataType parent;
        /// <summary>
        /// Default XSD implementation of this type.
        /// </summary>
        protected string defaultXSD;

        #endregion
    }
}
