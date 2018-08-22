using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XCase.Model
{
	/// <summary>
	/// ViewHelper class stores view-specific data for an instance of an element in the diagram
	/// (such as its position on the diagram or width and height of the element). These data 
	/// are not part of the UML model itself, but need to be saved and loaded to reconstruct a 
	/// previously saved diagram. 
	/// </summary>
	public class ClassViewHelper : PositionableElementViewHelper
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public ClassViewHelper()
		{
		}

		public ClassViewHelper(Diagram diagram)
			: base(diagram)
		{
		}

		private bool attributesCollapsed;
		public bool AttributesCollapsed
		{
			get { return attributesCollapsed; }
			set
			{
				attributesCollapsed = value;
				OnPropertyChanged(this, new PropertyChangedEventArgs("AttributesCollapsed"));
			}
		}

		private bool operationsCollapsed;
		public bool OperationsCollapsed
		{
			get { return operationsCollapsed; }
			set
			{
				operationsCollapsed = value;
				OnPropertyChanged(this, new PropertyChangedEventArgs("OperationsCollapsed"));
			}
		}

        private bool elementNameLabelCollapsed;
        public bool ElementNameLabelCollapsed
        {
            get { return elementNameLabelCollapsed; }
            set
            {
                elementNameLabelCollapsed = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("ElementNameLabelCollapsed"));
            }
        }

        private bool elementNameLabelAlignedRight;
        public bool ElementNameLabelAlignedRight
        {
            get { return elementNameLabelAlignedRight; }
            set
            {
                elementNameLabelAlignedRight = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("ElementNameLabelAlignedRight"));
            }
        }

		public override ViewHelper Clone(Diagram diagram)
		{
			return new ClassViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);

			ClassViewHelper copyClassViewHelper = (ClassViewHelper) copy;
			copyClassViewHelper.AttributesCollapsed = this.AttributesCollapsed;
			copyClassViewHelper.OperationsCollapsed = this.OperationsCollapsed;
			copyClassViewHelper.ElementNameLabelCollapsed = this.elementNameLabelCollapsed;
			copyClassViewHelper.ElementNameLabelAlignedRight = this.ElementNameLabelAlignedRight;
		}
	}
}
