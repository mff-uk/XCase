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
using XCase.Model;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for VersionedElementInfo.xaml
    /// </summary>
    public partial class VersionedElementInfo : Window
    {
        private IVersionedElement element;

        public VersionedElementInfo()
        {
            InitializeComponent();
        }

        // ReSharper disable LocalizableElement
        public IVersionedElement Element
        {
            get {
                return element;
            }
            set {
                element = value;
                border.Background = Brushes.LightYellow;
                if (Element != null)
                {
                    if (Element.Version != null && Element.FirstVersion.Version == Element.Version)
                        border.BorderBrush = Brushes.Green;
                    else if (Element.Version == null || Element.VersionManager == null || element.FirstVersion == null)
                    {
                        border.Background = Brushes.Pink;
                        border.BorderBrush = Brushes.Red;
                    }
                    else
                        border.BorderBrush = Brushes.Blue;

                    if (Element.VersionManager != null && Element.VersionManager != null && element.FirstVersion != null)
                    {    
                        if (!Element.VersionManager.Versions.All(v => Element.GetInVersion(v) != null))
                            border.Background = Brushes.LavenderBlush;
                        else
                            border.Background = Brushes.LightYellow;
                    }


                    lName.Content = Element.ToString();
                    lVersionManager.Content = (Element.VersionManager != null) ? "defined" : "not defined";
                    lCurrentVersion.Content = (Element.Version != null) ? Element.Version.ToString() : "(null)";

                    lAppearedIn.Content = (Element.FirstVersion != null) ? Element.FirstVersion.Version.ToString() : "(null)";
                    lAllVersions.Content = (Element.VersionManager != null) ? Element.VersionManager.Versions.Aggregate(string.Empty,
                                                                                 (s, version) => s += version + ", ",
                                                                                 result => result.Substring(0, result.Length - 2)) : "(null)";

                    lPresent.Content = (Element.VersionManager != null) ? 
                        Element.VersionManager.Versions.Aggregate(
                            string.Empty,
                            (s, version) => element.GetInVersion(version) != null ? s += version + ", " : s += " - ,",
                            result => result.Substring(0, result.Length - 2)) 
                        : "(null)";
                }
                else
                {
                    lName.Content = "(null)";
                    lCurrentVersion.Content = "(null)";
                    lAppearedIn.Content = "(null)";
                    lAllVersions.Content = "(null)";
                }
            }
        }
        // ReSharper restore LocalizableElement
    }
}