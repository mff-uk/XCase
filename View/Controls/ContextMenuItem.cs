using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XCase.View.Controls
{
	/// <summary>
	/// Context menu item
	/// </summary>
    public class ContextMenuItem : MenuItem
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="ContextMenuItem"/> class.
		/// </summary>
		/// <param name="Text">Item caption</param>
        public ContextMenuItem(string Text) : base() 
        {
            Header = new TextBlock() { TextAlignment = TextAlignment.Left, Text = Text };
        }
	}
}
