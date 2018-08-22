using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using System.Text.RegularExpressions;

namespace XCase.Gui
{
    /// <summary>
    /// Interface for class grids displayable in the Properties window
    /// </summary>
    public class ClassGridBase : XCaseGridBase
    {
        protected List<TextBox> attBoxes = null;

        protected NamedElementController SelectedClassController = null;

        #region Event handlers

        /// <summary>
        /// Event occurs when user doubleclicks on an attribute in attributes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void attBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if (SelectedClassController is ClassController)
                    ((ClassController)SelectedClassController).ShowAttributeDialog((Property)((Button)sender).DataContext);

                if (SelectedClassController is PSM_ClassController)
                    ((PSM_ClassController)SelectedClassController).ShowAttributeDialog((PSMAttribute)((Button)sender).DataContext);

            }
        }

        #endregion

    }
}