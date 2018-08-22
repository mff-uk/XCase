using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using XCase.Model.Serialization;

namespace XCase.Model
{
	/// <summary>
	/// Represents xcase project at runtime.
	/// </summary>
	public class Project : INotifyPropertyChanged, _ImplVersionedElement 
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public event EventHandler<DiagramEventArgs> DiagramAdded;
        private void NotifyDiagramAdded(Diagram diag)
        {
            if (DiagramAdded != null)
                DiagramAdded(this, new DiagramEventArgs(diag));
        }

        public event EventHandler<DiagramEventArgs> DiagramRemoved;
        private void NotifyDiagramRemoved(Diagram diag)
        {
            if (DiagramRemoved != null)
                DiagramRemoved(this, new DiagramEventArgs(diag));
        }
        #endregion

        public Project()
            : this("Project1")
        {
        }

        public Project(String capt)
        {
            schema = new Schema();
            pIMDiagrams = new ObservableCollection<PIMDiagram>();
            pSMDiagrams = new ObservableCollection<PSMDiagram>();
            caption = capt;
            FilePath = "";
        }

        public void AddDiagram(Diagram diagram)
        {
            diagram.Project = this;
            if (diagram is PIMDiagram) AddPIMDiagram(diagram as PIMDiagram);
            else if (diagram is PSMDiagram) AddPSMDiagram(diagram as PSMDiagram);
            else throw new NotImplementedException();
        }

        public void AddPIMDiagram(PIMDiagram diagram)
        {
            if (diagram != null)
            {
            	diagram.Project = this;
                pIMDiagrams.Add(diagram);
                NotifyDiagramAdded(diagram);
            }
        }

        public void AddPSMDiagram(PSMDiagram diagram)
        {
            if (diagram != null)
            {
				diagram.Project = this;
                pSMDiagrams.Add(diagram);
                NotifyDiagramAdded(diagram);
            }
        }

        public void RemoveDiagram(Diagram diagram)
        {
            if (diagram is PIMDiagram) RemovePIMDiagram(diagram as PIMDiagram);
            else if (diagram is PSMDiagram) RemovePSMDiagram(diagram as PSMDiagram);
            else throw new NotImplementedException();
        }

		public void RemovePIMDiagram(PIMDiagram diagram)
        {
            NotifyDiagramRemoved(diagram);
            pIMDiagrams.Remove(diagram);
        }

		public void RemovePSMDiagram(PSMDiagram diagram)
        {
            NotifyDiagramRemoved(diagram);
            pSMDiagrams.Remove(diagram);
        }

		private String caption;

		public String Caption
        {
            get { return caption; }
            set
            {
                if (value != null)
                {
                    caption = value;
                    NotifyPropertyChanged("Caption");
                }
            }
        }

		public ObservableCollection<PIMDiagram> PIMDiagrams
        {
            get { return pIMDiagrams; }
        }

		public ObservableCollection<PSMDiagram> PSMDiagrams
        {
            get { return pSMDiagrams; }
        }

		public IList<Diagram> Diagrams
        {
            get
            {
            	return PIMDiagrams.Cast<Diagram>().Union(PSMDiagrams.Cast<Diagram>()).ToList().AsReadOnly();
            }
        }

		public Diagram GetDiagram(string name)
        {
            //Not fully working, fails when getting PSM diagram
            Diagram d = pIMDiagrams.First(item => item.Caption == name);
            if (d == null) d = pSMDiagrams.FirstOrDefault(item => item.Caption == name);
            return d;
        }

		private readonly ObservableCollection<PIMDiagram> pIMDiagrams;

		private readonly ObservableCollection<PSMDiagram> pSMDiagrams;

		private readonly Schema schema;

		public Schema Schema
        {
            get { return schema; }
        }

		public string FilePath { get; set; }

        #region Project operations

        public void Save()
        {
            XmlSerializator serializator = this.VersionManager != null ?
                            new XmlSerializator(this.VersionManager) :
                            new XmlSerializator(this);

            serializator.SerilizeTo(this.FilePath);
        }

	    public void SaveAs(string filename)
	    {
            XmlSerializator serializator = this.VersionManager != null ?
                            new XmlSerializator(this.VersionManager) :
                            new XmlSerializator(this);

            serializator.SerilizeTo(filename);

            this.FilePath = filename;
	    }

        #endregion 

        #region Implementation of IVersionedElement

        private IVersionedElement firstVersion;

		/// <summary>
		/// First version of the current element.
		/// </summary>
		public IVersionedElement FirstVersion
		{
			get { return firstVersion; }
			private set
			{
				firstVersion = value;
				NotifyPropertyChanged("FirstVersion");
			}
		}

		/// <summary>
		/// Version where the element appeared first. 
		/// Returns value of <see cref="_ImplVersionedElement.Version"/> property if 
		/// this is the first version of the element. 
		/// </summary>
		IVersionedElement _ImplVersionedElement.FirstVersion
		{
			get { return FirstVersion; }
			set { FirstVersion = value; }
		}

		private Version version;

		/// <summary>
		/// Version of the element
		/// </summary>
		public Version Version
		{
			get { return version; }
			private set
			{
				version = value;
				NotifyPropertyChanged("Version");
			}
		}

		/// <summary>
		/// Version of the element
		/// </summary>
		Version _ImplVersionedElement.Version
		{
			get { return Version; }
			set { Version = value; }
		}


		VersionManager _ImplVersionedElement.VersionManager
		{
			get { return VersionManager; }
			set { VersionManager = value; }
		}

        public bool IsFirstVersion
        {
            get { return FirstVersion == this; }
        }

	    private VersionManager versionManager;

		internal SerializatorIdTable TemplateIdTable { get; set; }

		public VersionManager VersionManager
		{
			get { return versionManager; }
			private set
			{
				versionManager = value;
				NotifyPropertyChanged("VersionManager");
			}
		}

		#endregion
    }
}


