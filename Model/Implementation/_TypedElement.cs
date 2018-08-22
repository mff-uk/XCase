using System;
using System.Collections.Generic;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML TypedElement metaclass adapter.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adapted nUML element, has to inherit from NUml.Uml2.TypedElement
    /// </typeparam>
    internal abstract class _TypedElement<NUmlType> : _NamedElement<NUmlType>, TypedElement
                                            where NUmlType : NUml.Uml2.TypedElement
    {
        #region Constructors

        /// <summary>
        /// Creates a new typed element.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="_metaclass">
        /// UML metaclass(es) that this element is instance of.
        /// Bitwise combination of StereotypeTarget values.
        /// </param>
        protected _TypedElement(Schema _schema, StereotypeTarget _metaclass) : base(_schema, _metaclass)
        {
        }

        #endregion

		#region Element Members

    	#region copy

    	public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		base.FillCopy(copyElement, targetModel, createdCopies);
    		TypedElement copyTypedElement = (TypedElement) copyElement;

    		if (Type != null)
    		{
    			if (createdCopies.ContainsKey(Type))
    			{
    				copyTypedElement.Type = (DataType)createdCopies[Type];
    			}
    			else
    			{
    				if (Schema == targetModel.Schema)
    				{
    					if (Type is SimpleDataType)
    					{
    						copyTypedElement.Type = Type;
    					}
    					else
    					{
    						//TODO: copy typed element
    						throw new NotImplementedException();
    					}
    				}
    			}
    		}
    	}

    	#endregion 
		#endregion 

		#region TypedElement Members

		public DataType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type != value)
                {
                    if (type != null)
                        type.PropertyChanged -= type_PropertyChanged;

                    type = value;                
                    if (type != null)
                    {
                        type.PropertyChanged += type_PropertyChanged;
                    }
                    NotifyPropertyChanged("Type");
                }
            }
        }

        void type_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("Type");
        }

        #endregion



        #region Fields

        /// <summary>
        /// Type of the element.
        /// </summary>
        protected DataType type;

        #endregion
    }
}
