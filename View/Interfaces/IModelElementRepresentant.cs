using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.View.Interfaces
{
    /// <summary>
    /// <para>Denotes classes that represent model elements in view. </para>
    /// <para>Classes implementing IModelRepresentant must also have a public constructor of 
    /// with 'Representant(XCaseCanvas xCaseCanvas)' signature</para>
    /// </summary>
    public interface IModelElementRepresentant: IDeletable
    {
        /// <summary>
        /// Called when element is added to the diagram, initializes the representant to properties.
        /// of <paramref name="modelElement"/>
        /// </summary>
        /// <param name="modelElement">element that the classs should represent</param>
		/// <param name="viewHelper"><see cref="ViewHelper"/> of the modelElement, contains diagram-specific
		/// properties of the element.</param>
        /// <param name="controller">controller for modelElement</param>
        void InitializeRepresentant(Element modelElement, ViewHelper viewHelper, ElementController controller);

		/// <summary>
		/// Canvas where the element is placed
		/// </summary>
		XCaseCanvas XCaseCanvas { get; }
    }

	
}