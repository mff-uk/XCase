using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper for <see cref="XCase.Model.AssociationEnd"/>. Stores position of 
	/// labels and points of that part of association that connects the <see cref="AssociationEnd"/>
	/// to the view helper.
	/// </summary>
	public class AssociationEndViewHelper : PositionableElementViewHelper
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public AssociationEndViewHelper()
		{
		}

		public AssociationEndViewHelper(Diagram diagram, AssociationViewHelper associationViewHelper, AssociationEnd associationEnd)
			: base(diagram)
		{
			AssociationViewHelper = associationViewHelper;
			MultiplicityLabelViewHelper = new AssociationLabelViewHelper(diagram);
			RoleLabelViewHelper = new AssociationLabelViewHelper(diagram);
			AssociationEnd = associationEnd;

			points = new ObservablePointCollection();
		}

		private AssociationEnd associationEnd;
		public AssociationEnd AssociationEnd
		{
			get
			{
				return associationEnd;
			}
			private set
			{
				associationEnd = value;
				OnPropertyChanged("AssociationEnd");
			}
		}

		private AssociationViewHelper associationViewHelper;
		public AssociationViewHelper AssociationViewHelper
		{
			get
			{
				return associationViewHelper;
			}
			private set
			{
				associationViewHelper = value;
				OnPropertyChanged("AssociationViewHelper");
			}
		}

		public AssociationLabelViewHelper MultiplicityLabelViewHelper { get; set; }

		public AssociationLabelViewHelper RoleLabelViewHelper { get; set; }

		private readonly ObservablePointCollection points = new ObservablePointCollection();

		public ObservablePointCollection Points
		{
			get
			{
				return points;
			}
		}

		public override ViewHelper Clone(Diagram diagram)
		{
			throw new InvalidOperationException("Clone should not be called on objects of type type AssociationEndViewHelper");
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);

			AssociationEndViewHelper copyAssociationEndViewHelper = (AssociationEndViewHelper) copy;
			this.MultiplicityLabelViewHelper.FillCopy(copyAssociationEndViewHelper.MultiplicityLabelViewHelper, modelMap);
			this.RoleLabelViewHelper.FillCopy(copyAssociationEndViewHelper.RoleLabelViewHelper, modelMap);
			copyAssociationEndViewHelper.Points.AppendRangeAsCopy(this.Points);
		}
	}
}