using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Type metaclass adapter.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adapted nUML element, must inherit from NUml.Uml2.Type
    /// </typeparam>
    internal abstract class _DataType<NUmlType> : _NamedElement<NUmlType>, _ImplDataType
                                where NUmlType : NUml.Uml2.Type
    {
        #region Constructors

        protected _DataType(Schema _schema)
            : this(_schema, StereotypeTarget.None)
        { }

        /// <summary>
        /// Creates a new empty type without the adaptee.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="_metaclass">
        /// UML metaclass(es) that this element is instance of.
        /// Bitwise combination of StereotypeTarget values.
        /// </param>
        protected _DataType(Schema _schema, StereotypeTarget _metaclass)
            : base(_schema, _metaclass)
        {
            package = null;
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            Debug.Assert(removedFromPackage != null);
            if (!removedFromPackage.OwnedTypes.Contains(this))
                removedFromPackage.OwnedTypes.Add(this);

            removedFromPackage = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            Debug.Assert(removedFromPackage == null);

            removedFromPackage = package;
            package.OwnedTypes.Remove(this);
        }

        #endregion

        #region DataType Members

        public virtual Package Package
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

        #region clone and copy

        public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyElement, targetModel, createdCopies);
            _DataType<NUmlType> copyDataType = (_DataType<NUmlType>)copyElement;
            if (Package != null)
            {
                if (targetModel.Schema != this.Schema || createdCopies.ContainsKey(Package))
                {
                    copyDataType.Package = (Package)createdCopies[Package];
                }
                else
                {
                    copyDataType.Package = Package;
                }
            }
        }

        #endregion

        #region _ImplDataType Members

        public NUml.Uml2.Type AdaptedType
        {
            get { return Adaptee; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the package that owns this type.
        /// </summary>
        protected Package package;

        /// <summary>
        /// References the package that this class was removed from.
        /// Null if the class is currently in the model.
        /// </summary>
        protected Package removedFromPackage = null;

        #endregion
    }
}
