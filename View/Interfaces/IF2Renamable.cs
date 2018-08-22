using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.View.Interfaces
{
	/// <summary>
	/// Interface for controls that user can rename and the renaming
	/// should start when F2 is pressed.
	/// </summary>
    public interface IF2Renamable
    {
		/// <summary>
		/// Sets the control in a state where name can be edited.
		/// </summary>
        void F2Rename();
    }
}
