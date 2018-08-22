using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the XSem PIMClass construct.
    /// </summary>
    internal class _PIMClass : _Class<NUml.Uml2.Class>, PIMClass
    {
        #region Constructors

        /// <summary>
        /// Creates a new PIM class.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _PIMClass(Schema _schema) : base(_schema)
        {
            derivedClasses = new ObservableCollection<PSMClass>();
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            removedFromPackage.Classes.Add(this);
            removedFromPackage = null;
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();
            Debug.Assert(removedFromPackage == null || removedFromPackage == package);
            removedFromPackage = package;
            package.Classes.Remove(this);
        }

    	#region clone and copy

    	public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
    	{
    		Package package;
			if (targetModel.Schema != this.Schema)
    		{
    			package = (Package) createdCopies[Package];
    		}
    		else
    		{
    			package = Package;
    		}
    		PIMClass clone = package.AddClass();

    		return clone;
    	}

    	#endregion 
        #endregion

        #region PIMClass Members

        public PSMClass DerivePSMClass()
        {
            _PSMClass psmClass = new _PSMClass(this, Schema);

            derivedClasses.Add(psmClass);
            package.PSMClasses.Add(psmClass);

            return psmClass;
        }

        public ObservableCollection<PSMClass> DerivedPSMClasses
        {
            get { return derivedClasses; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Collection of PSM classes derived from this PIM class.
        /// </summary>
        protected ObservableCollection<PSMClass> derivedClasses;
        
        #endregion
    }
}
