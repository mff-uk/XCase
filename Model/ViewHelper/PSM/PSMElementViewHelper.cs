using System;
using System.Collections.Generic;

namespace XCase.Model
{
	/// <summary>
	/// View helper used for PSM subordinate elements that are connected 
	/// via association or component connector (the class is not used in 
	/// PSM_Association, since PSM_Association itself acts as a connector). 
	/// </summary>
	public class PSMElementViewHelper: ClassViewHelper	
	{
		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		public PSMElementViewHelper()
		{
		}

		#region Implementation of ISubordinateComponentViewHelper

		public PSMElementViewHelper(Diagram diagram) : base(diagram)
		{
			ConnectorViewHelper = new ComponentConnectorViewHelper(diagram);
		}

		public ComponentConnectorViewHelper ConnectorViewHelper
		{
			get; private set;
		}

		#endregion

		public override ViewHelper Clone(Diagram diagram)
		{
			return new PSMElementViewHelper(diagram);
		}

		public override void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			base.FillCopy(copy, modelMap);
			PSMElementViewHelper copyPSMElementViewHelper = (PSMElementViewHelper) copy;
			ConnectorViewHelper.FillCopy(copyPSMElementViewHelper.ConnectorViewHelper, modelMap);
		}
	}
}