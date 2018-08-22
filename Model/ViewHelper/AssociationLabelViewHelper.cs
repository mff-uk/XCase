using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper used for labels of associations, stores position
	/// of the label. 
	/// </summary>
	public class AssociationLabelViewHelper: PositionableElementViewHelper
	{
		public AssociationLabelViewHelper(Diagram diagram) : 
			base(diagram)
		{
			LabelVisible = true; 
		}

		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public AssociationLabelViewHelper()
		{
		}

		private bool labelVisible;
		public bool LabelVisible
		{
			get { return labelVisible; }
			set { labelVisible = value; OnPropertyChanged("LabelVisible"); }
		}

		public override ViewHelper Clone(Diagram diagram)
		{
			throw new InvalidOperationException("Clone should not be called on objects of type type AssociationLabelViewHelper");
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			AssociationLabelViewHelper copyAssociationLabelViewHelper = (AssociationLabelViewHelper) copy;
			copyAssociationLabelViewHelper.LabelVisible = LabelVisible;
		}
	}
}