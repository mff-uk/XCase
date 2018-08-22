using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XCase.Model
{
    public class Version: INotifyPropertyChanged
    {
        private Version createdFrom;
        public Version CreatedFrom
        {
            get { return createdFrom; }
            set {
                if (createdFrom != null)
                {
                    createdFrom.BranchedVersions.Remove(this);
                }
                createdFrom = value; 
                if (createdFrom != null)
                {
                    createdFrom.BranchedVersions.Add(this);   
                }
            }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; if (string.IsNullOrEmpty(Label)) Label = string.Format("v{0}", number); InvokePropertyChanged("Number");}
        }

        private string label;
        public string Label
        {
            get { return label; }
            set { label = value; InvokePropertyChanged("Label");}
        }

        private readonly List<IVersionedElement> elementsCreatedInVersion = new List<IVersionedElement>();

        public List<IVersionedElement> ElementsCreatedInVersion
        {
            get { return elementsCreatedInVersion; }
        }

        private readonly List<Version> branchedVersions = new List<Version>();

        public List<Version> BranchedVersions
        {
            get { return branchedVersions; }
        }

        public override string ToString()
        {
            return Label;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            PropertyChangedEventHandler changed = PropertyChanged;
            if (changed != null) changed(this, e);
        }
    }
}