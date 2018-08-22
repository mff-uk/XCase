using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using System.Text.RegularExpressions;

namespace XCase.Gui
{
    /// <summary>
    /// NestingJoinsGrid displays properties of NestingJoins collection of a PSM association
    /// </summary>
    public partial class NestingJoinsGrid
    {
        /// <summary>
        ///  Initialization
        /// </summary>
        public NestingJoinsGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            // All displayed values are read-only -> no need of update
        }
  }
}
