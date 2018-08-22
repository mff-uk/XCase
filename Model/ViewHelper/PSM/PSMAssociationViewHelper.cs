using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// ViewHelper for <see cref="PSMAssociation"/>.
	/// </summary>
	public class PSMAssociationViewHelper: ConnectionViewHelper<PSMAssociation>
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public PSMAssociationViewHelper()
		{
		}

		public PSMAssociationViewHelper(Diagram diagram) : base(diagram)
		{
			points = new ObservablePointCollection();
			MultiplicityLabelViewHelper = new AssociationLabelViewHelper(diagram);
		}

		private readonly ObservablePointCollection points;

		public override ObservablePointCollection Points
		{
			get { return points; }
			
		}

		public AssociationLabelViewHelper MultiplicityLabelViewHelper { get; set; }

		public override ViewHelper Clone(Diagram diagram)
		{
			return new PSMAssociationViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			PSMAssociationViewHelper copyPSMAssociationViewHelper = (PSMAssociationViewHelper) copy;
			this.MultiplicityLabelViewHelper.FillCopy(copyPSMAssociationViewHelper.MultiplicityLabelViewHelper, modelMap);
			copyPSMAssociationViewHelper.Points.AppendRangeAsCopy(Points);
		}

	}
}