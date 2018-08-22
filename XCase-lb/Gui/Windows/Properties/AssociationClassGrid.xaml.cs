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
    /// AssociationClassGrid displays properties of Association class selected on the canvas
    /// </summary>
    public partial class AssociationClassGrid
    {
        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent() 
        {
            pimClassGrid.UpdateContent();
            ends.UpdateContent();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public AssociationClassGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays Association class
        /// </summary>
        /// <param name="a"></param>
        /// <param name="mv"></param>
        public void Display(PIM_AssociationClass a, MainWindow mv)
        {
            pimClassGrid.Display(a, "Association", mv);
            pimClassGrid.scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ends.Display(a.Association.Ends, a.Controller.DiagramController.ModelController);
        }

        #region Event handlers
        /// <summary>
        /// Occurs when Association class grid size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociationClassGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                pimClassGrid.Width = e.NewSize.Width;
                ends.endsBox.Width = e.NewSize.Width - 6; 
                foreach (TextBox t in ends.endBoxes)
                    t.Width = e.NewSize.Width - 70;
            }

            e.Handled = true;
        }

        #endregion
    }
}
