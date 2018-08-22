using System;
using System.ComponentModel;
using XCase.Model.Implementation;

namespace XCase.Model
{
    /// <summary>
    /// Idenitifies a value held in the model.
    /// </summary>
    public class ValueSpecification : INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Creates a new value spceification with a string type.
        /// </summary>
        /// <param name="value">Initial value</param>
        public ValueSpecification(string value)
        {
            NUml.Uml2.LiteralString literal = NUml.Uml2.Create.LiteralString();
            literal.Value = value;
            adaptedElement = literal;
            valueType = ValueTypes.String;
        }

        /// <summary>
        /// Creates a new value specification with an element type.
        /// </summary>
        /// <param name="value">Initial value</param>
        public ValueSpecification(Element value)
        {
            NUml.Uml2.ElementValue val = NUml.Uml2.Create.ElementValue();
            elementValue = value;
            val.Element = (value != null ? ((_ImplElement)value).AdaptedElement : null);
            adaptedElement = val;
            valueType = ValueTypes.Element;
        }

        /// <summary>
        /// Creates a new value specification with an integer type.
        /// </summary>
        /// <param name="value">Initial value</param>
        public ValueSpecification(int value)
        {
            NUml.Uml2.LiteralInteger literal = NUml.Uml2.Create.LiteralInteger();
            literal.Value = value;
            adaptedElement = literal;
            valueType = ValueTypes.Integer;
        }

        /// <summary>
        /// Creates a new value specification with a boolean type.
        /// </summary>
        /// <param name="value">Initial value</param>
        public ValueSpecification(bool value)
        {
            NUml.Uml2.LiteralBoolean literal = NUml.Uml2.Create.LiteralBoolean();
            literal.Value = value;
            adaptedElement = literal;
            valueType = ValueTypes.Boolean;
        }

        /// <summary>
        /// Creates a new value specification with an unlimited natural type.
        /// </summary>
        /// <param name="value">Initial value</param>
        public ValueSpecification(NUml.Uml2.UnlimitedNatural value)
        {
            NUml.Uml2.LiteralUnlimitedNatural literal = NUml.Uml2.Create.LiteralUnlimitedNatural();
            literal.Value = value;
            adaptedElement = literal;
            valueType = ValueTypes.UnlimitedNatural;
        }

        /// <summary>
        /// Creates a new value specification with a given type.
        /// </summary>
        /// <param name="value">String representation of the value</param>
        /// <param name="type">Type of the value</param>
        /// <remarks>
        /// If the type is not known (i.e. it is not emong ValueTypes values,
        /// the value is treated like a string.
        /// </remarks>
        /// <exception cref="NullReferenceException">Type is null</exception>
        /// <exception cref="FormatException">Value cannot be parsed to the given type instance</exception>
        public ValueSpecification(string value, DataType type)
        {
            //if (type == null)
            //    throw new NullReferenceException("Type cannot be null!");

            string typeName = type != null ? type.Name.ToLower() : "null";

            switch (typeName)
            {
                case "integer":
                case "int":                 
                    {
                        NUml.Uml2.LiteralInteger literal = NUml.Uml2.Create.LiteralInteger();
                        literal.Value = int.Parse(value);
                        adaptedElement = literal;
                        valueType = ValueTypes.Integer;
                        break;
                    }
                case "boolean":
                case "bool":
                    {
                        NUml.Uml2.LiteralBoolean literal = NUml.Uml2.Create.LiteralBoolean();
                        literal.Value = bool.Parse(value);
                        adaptedElement = literal;
                        valueType = ValueTypes.Boolean;
                        break;
                    }
                case "object":
                    {
                        if (value.ToLower().Equals("null"))
                        {
                            NUml.Uml2.ElementValue elVal = NUml.Uml2.Create.ElementValue();
                            elVal.Element = null;
                            adaptedElement = elVal;
                            valueType = ValueTypes.Element;
                        }
                        break;
                    }
                default:
                    {
                        NUml.Uml2.LiteralString literal = NUml.Uml2.Create.LiteralString();
                        literal.Value = value;
                        adaptedElement = literal;
                        valueType = ValueTypes.String;
                        break;
                    }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Enumerations

        /// <summary>
        /// Identifies the type of the value.
        /// </summary>
        public enum ValueTypes
        {
            /// <summary>
            /// The value is a string literal.
            /// </summary>
            String,
            /// <summary>
            /// The value is an arbitrary element from the model or null.
            /// </summary>
            Element,
            /// <summary>
            /// The value is an integer constant.
            /// </summary>
            Integer,
            /// <summary>
            /// The value is a boolean constant.
            /// </summary>
            Boolean,
            /// <summary>
            /// The value is an unlimited natural constant.
            /// </summary>
            UnlimitedNatural
        }

        #endregion

        #region Auxilary Methods

        /// <summary>
        /// Notifies all the consumers of this event that a property has changed.
        /// </summary>
        /// <param name="name">Name of the property that changed</param>
        protected void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the NUml ValueSpecification instance adapted by this class.
        /// </summary>
        internal NUml.Uml2.ValueSpecification AdaptedElement
        {
            get { return adaptedElement; }
        }

        /// <summary>
        /// Identifies the type of the value held by this value specification.
        /// </summary>
        public ValueTypes ValueType
        {
            get { return valueType; }
        }

        /// <summary>
        /// Gets or sets the value of this value specification, if it has a boolean type.
        /// </summary>
        /// <exception cref="NotSupportedException">The value held has not a boolean type.</exception>
        public bool BooleanValue
        {
            get 
            {
                if (valueType != ValueTypes.Boolean)
                    throw new NotSupportedException("The value has not a boolean type!");
                
                return adaptedElement.BooleanValue(); 
            }
            set
            {
                if (valueType != ValueTypes.Boolean)
                    throw new NotSupportedException("The value has not a boolean type!");

                if (value != adaptedElement.BooleanValue())
                {
                    ((NUml.Uml2.LiteralBoolean)adaptedElement).Value = value;
                    NotifyPropertyChanged("BooleanValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of this value specification, if it has a string type.
        /// </summary>
        /// <exception cref="NotSupportedException">The value held has not a string type.</exception>
        public string StringValue
        {
            get 
            {
                if (valueType != ValueTypes.String)
                    throw new NotSupportedException("The value has not a string type!");

                return adaptedElement.StringValue(); 
            }
            set
            {
                if (valueType != ValueTypes.String)
                    throw new NotSupportedException("The value has not a string type!");

                if (value != adaptedElement.StringValue())
                {
                    ((NUml.Uml2.LiteralString)adaptedElement).Value = value;
                    NotifyPropertyChanged("StringValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of this value specification, if it has an element type.
        /// </summary>
        /// <exception cref="NotSupportedException">The value held has not an element type.</exception>
        public Element ElementValue
        {
            get 
            {
                if (valueType != ValueTypes.Element)
                    throw new NotSupportedException("The value has not an Element type!");

                return elementValue;
            }
            set
            {
                if (valueType != ValueTypes.Element)
                    throw new NotSupportedException("The value has not an Element type!");

                if (elementValue != value)
                {
                    elementValue = value;
                    ((NUml.Uml2.ElementValue)adaptedElement).Element = (value != null ?
                        ((_ImplElement)value).AdaptedElement : null); 
                    NotifyPropertyChanged("ElementValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of this value specification, if it has an integer type.
        /// </summary>
        /// <exception cref="NotSupportedException">The value held has not an integer type.</exception>
        public int IntegerValue
        {
            get 
            {
                if (valueType != ValueTypes.Integer)
                    throw new NotSupportedException("The value has not an integer type!");

                return adaptedElement.IntegerValue(); 
            }
            set
            {
                if (valueType != ValueTypes.Integer)
                    throw new NotSupportedException("The value has not an integer type!");

                if (adaptedElement.IntegerValue() != value)
                {
                    ((NUml.Uml2.LiteralInteger)adaptedElement).Value = value;
                    NotifyPropertyChanged("IntegerValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of this value specification, if it has an unlimited natural type.
        /// </summary>
        /// <exception cref="NotSupportedException">The value held has not an unlimited natural type.</exception>
        public NUml.Uml2.UnlimitedNatural UnlimitedNaturalValue
        {
            get 
            {
                if (valueType != ValueTypes.UnlimitedNatural)
                    throw new NotSupportedException("The value has not an unlimited natural type!");

                return adaptedElement.UnlimitedValue(); 
            }
            set
            {
                if (valueType != ValueTypes.UnlimitedNatural)
                    throw new NotSupportedException("The value has not an unlimited natural type!");

                if (adaptedElement.UnlimitedValue() != value)
                {
                    ((NUml.Uml2.LiteralUnlimitedNatural)adaptedElement).Value = value;
                    NotifyPropertyChanged("UnlimitedNaturalValue");
                }
            }
        }

        /// <summary>
        /// Indicates whether the value specification can be computed in the model.
        /// </summary>
        public bool IsComputable
        {
            get { return adaptedElement.IsComputable(); }
        }

        /// <summary>
        /// Indicates whether the value specification can be computed and has null value.
        /// </summary>
        public bool IsNull
        {
            get { return adaptedElement.IsNull(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tells whether a given value can be parsed from a string and stored
        /// in this value specification according to its type.
        /// </summary>
        /// <param name="value">Tested value</param>
        /// <returns>True if the value can be stored, false otherwise.</returns>
        public bool CanParseValue(string value)
        {
            switch (valueType)
            {
                case ValueTypes.Boolean:
                    {
                        bool dummy;
                        return bool.TryParse(value, out dummy);
                    }
                case ValueTypes.Element:
                    {
                        return value.ToLower().Equals("null");
                    }
                case ValueTypes.Integer:
                case ValueTypes.UnlimitedNatural:
                    {
                        int dummy;
                        return int.TryParse(value, out dummy);
                    }
                default:
                    return true;
            }
        }

        /// <summary>
        /// Parses the given value from a string and stores it to this value specification.
        /// </summary>
        /// <param name="value">Value to be parsed</param>
        /// <exception cref="ArgumentException">Value does not have a corresponding type</exception>
        public void ParseFromString(string value)
        {
            if (!CanParseValue(value))
                throw new ArgumentException("Value does not have a corresponding type!");

            switch (valueType)
            {
                case ValueTypes.Boolean:
                    {
                        BooleanValue = bool.Parse(value);
                        break;
                    }
                case ValueTypes.Element:
                    {
                        ElementValue = null;
                        break;
                    }
                case ValueTypes.Integer:
                case ValueTypes.UnlimitedNatural:
                    {
                        IntegerValue = int.Parse(value);
                        break;
                    }
                default:
                    StringValue = value;
                    break;
            }
        }

        public override string ToString()
        {
            if (AdaptedElement is NUml.Uml2.LiteralString)
            {
                return (AdaptedElement as NUml.Uml2.LiteralString).StringValue();
            }
            return adaptedElement.ToString();
        }

        #endregion

        #region Fields

        /// <summary>
        /// Reference to the adapted NUml.Uml2.ValueSpecification instance.
        /// </summary>
        protected NUml.Uml2.ValueSpecification adaptedElement;
        /// <summary>
        /// References the element held by this value specification if it has an element type.
        /// </summary>
        protected Element elementValue;
        /// <summary>
        /// Indicates the type of the value held by this value specification.
        /// </summary>
        protected ValueTypes valueType;

        #endregion
    }
}
