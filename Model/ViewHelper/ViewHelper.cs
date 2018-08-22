using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XCase.Model
{
	/// <summary>
	/// Stores visualization information of an element on a diagram
	/// </summary>
	public abstract class ViewHelper : INotifyPropertyChanged
	{
        private Diagram diagram;

		public Diagram Diagram
		{
			get
			{
				return diagram;
			}
			private set
			{
				diagram = value;
				OnPropertyChanged("Diagram");
			}
		}

		/// <summary>
		/// This overload is used for reflection, use other overloads for standard 
		/// work.
		/// </summary>
		[Obsolete("Parameterless construtor is intended for reflection usage only. Use ViewHelper(Diagram) instead.")]
		protected ViewHelper()
		{
		}

		protected ViewHelper(Diagram diagram)
		{
			Diagram = diagram;
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(sender, e);
			}
		}

		public ViewHelper CreateCopy(Diagram diagram, IDictionary<Element, Element> modelMap)
		{
			ViewHelper copyElement = Clone(diagram);
			FillCopy(copyElement, modelMap);
			return copyElement;
		}

		public virtual ViewHelper Clone(Diagram diagram)
		{
			// TODO udelat neco lepsiho nez InvalidOperationException
			throw new NotImplementedException(string.Format("Clone is not implemented for type {0}.", this.GetType().Name));
		}

		public virtual void FillCopy(ViewHelper copy, IDictionary<Element, Element> modelMap)
		{
			// nothing to copy in the base class, subclasses ought to override the method
		}

		protected static Element Map(Element element, IDictionary<Element, Element> modelMap)
		{
			return modelMap == null ? element : modelMap[element];
		}
	}
}