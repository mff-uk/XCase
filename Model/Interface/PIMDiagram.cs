using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Model
{
    /// <summary>
    /// Platform Independent Model Diagram representation
    /// </summary>
    public class PIMDiagram : Diagram
    {
        public PIMDiagram(string Caption) : base(Caption) { }

		public override Diagram Clone()
		{
			return new PIMDiagram(caption);
		}
    }
}
