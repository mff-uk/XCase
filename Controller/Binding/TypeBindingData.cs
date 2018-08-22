//#define bindingoff
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller.Commands;
using XCase.Controller.DynamicCompilation;

namespace XCase.Controller
{
	/// <summary>
	/// Used to hold type binding data (which properties of a 
	/// type are bound to which properties in model and view helper).
	/// Initialized automatically when the particular type is used. 
	/// Can be used for types implementing <see cref="IBindable"/>
	/// and the initialization is performed when the 
	/// <see cref="BindableSupport.StartBindings(IBindable)"/> is called for the
	/// first time.
	/// </summary>
	public class TypeBindingData
	{
		/// <summary>
		/// Map "source property" => "target property getter/setter" function (See: <see cref="SetHandler"/>, <see cref="GetHandler"/>). 
		/// </summary>
		private class BindingCollection : Hashtable
		{

		}

		/// <summary>
		/// Examined type
		/// </summary>
		private Type Type { get; set; }

		/// <summary>
		/// Possible sources of binding
		/// </summary>
		public enum EBindingSourceType
		{
			/// <summary>
			/// Binding o a model element (PIM_Class for example)
			/// </summary>
			Model,
			/// <summary>
			/// Binding to a View Helper
			/// </summary>
			View
		}

		/// <summary>
		/// Gets or sets the type of the model element
		/// (after the <see cref="Type"/> was examined
		/// in <see cref="PrepareBindingTo"/> call.
		/// </summary>
		/// <value>The type of the model element.</value>
		private Type ModelElementType { get; set; }

		/// <summary>
		/// Gets or sets the type of the view helper
		/// (after the <see cref="Type"/> was examined
		/// in <see cref="PrepareBindingTo"/> call.
		/// </summary>
		private Type ViewHelperType { get; set; }

		/// <summary>
		/// Dynamically compiled function that returns 
		/// property with attribute [ModelElement] declared
		/// for objects of the examined type (<see cref="Type"/>).
		/// </summary>
		/// <seealso cref="ModelElementAttribute"/>
		/// <value><see cref="GetHandler"/></value>
		private GetHandler ModelSourceGetter { get; set; }

		/// <summary>
		/// Dynamically compiled function that returns 
		/// property with attribute [ViewHelper] declared
		/// for objects of the examined type (<see cref="Type"/>)
		/// </summary>
		/// <seealso cref="ModelElementAttribute"/>
		/// <value><see cref="GetHandler"/></value>
		private GetHandler ViewSourceGetter { get; set; }

		/// <summary>
		/// This collection is a map that returns a function 
		/// that gets value of a property for the name of the property. 
		/// The collection is filled in <see cref="CreateBindingFunctions"/>
		/// by those properties of <see cref="ModelElementType"/> 
		/// to which some properies of <see cref="Type"/> or bound to
		/// via
		/// [ModelPropertyMappingAttribute] declaration
		/// </summary>
		/// <seealso cref="ModelPropertyMappingAttribute"/>
		/// <value><see cref="BindingCollection"/></value>
		private BindingCollection ModelGetterCollection { get; set; }

		/// <summary>
		/// This collection is a map that returns a function 
		/// that sets value of a property for the name of the property. 
		/// The collection is filled in <see cref="CreateBindingFunctions"/>
		/// by those properties of <see cref="ModelElementType"/> 
		/// to which some properies of <see cref="Type"/> or bound to
		/// via
		/// [ModelPropertyMappingAttribute] declaration
		/// </summary>
		/// <seealso cref="ModelPropertyMappingAttribute"/>
		/// <value><see cref="BindingCollection"/></value>
		private BindingCollection ModelSetterCollection { get; set; }

		/// <summary>
		/// This collection is a map that returns a function 
		/// that gets value of a property for the name of the property. 
		/// The collection is filled in <see cref="CreateBindingFunctions"/>
		/// by those properties of <see cref="ViewHelperType"/> 
		/// to which some properies of <see cref="Type"/> or bound to
		/// via
		/// [ViewHelperPropertyMappingAttribute] declaration
		/// </summary>
		/// <seealso cref="ViewHelperPropertyMappingAttribute"/>
		/// <value><see cref="BindingCollection"/></value>
		private BindingCollection ViewGetterCollection { get; set; }

		/// <summary>
		/// This collection is a map that returns a function 
		/// that sets value of a property for the name of the property. 
		/// The collection is filled in <see cref="CreateBindingFunctions"/>
		/// by those properties of <see cref="ModelElementType"/> 
		/// to which some properies of <see cref="Type"/> or bound to
		/// via
		/// [ViewHelperPropertyMappingAttribute] declaration
		/// </summary>
		/// <value><see cref="BindingCollection"/></value>
		private BindingCollection ViewSetterCollection { get; set; }

		/// <summary>
		/// Stores update handler for each instance of <see cref="Type"/> for which
		/// <see cref="StartBindings(IBindable)"/> was called. Update handler is 
		/// method bound to <see cref="INotifyPropertyChanged.PropertyChanged"/> of
		/// the source of binding (view helper element of type <see cref="ViewHelperType"/>)
		/// </summary>
		/// <remarks>
		/// Instance of <see cref="TypeBindingData"/> is created only once for 
		/// each type, but start <see cref="StartBindings(IBindable)"/> is called 
		/// for each instance of <see cref="Type"/>. 
		/// </remarks>
		private Hashtable ViewUpdaters { get; set; }

		/// <summary>
		/// Stores update handler for each instance of <see cref="Type"/> for which
		/// <see cref="StartBindings(IBindable)"/> was called. Update handler is 
		/// method bound to <see cref="INotifyPropertyChanged.PropertyChanged"/> of
		/// the source of binding (model element of type <see cref="ModelElementType"/>)
		/// </summary>
		/// <remarks>
		/// Instance of <see cref="TypeBindingData"/> is created only once for 
		/// each type, but start <see cref="StartBindings(IBindable)"/> is called 
		/// for each instance of <see cref="Type"/>. 
		/// </remarks>
		private Hashtable ModelUpdaters { get; set; }

		/// <summary>
		/// List of instances of <see cref="ViewHelperPropertyMappingAttribute"/>
		/// on the bound object.
		/// </summary>
		private List<ViewHelperPropertyMappingAttribute> ViewMappingDeclarations { get; set; }

		/// <summary>
		/// List of instances of <see cref="ModelPropertyMappingAttribute"/>
		/// on the bound object.
		/// </summary>
		private List<ModelPropertyMappingAttribute> ModelMappingDeclarations { get; set; }

		private static bool initializationInProgress = false;

		private static readonly object initializationLock = new object();

		/// <summary>
		/// Calls <paramref name="callDelegate"/>, this method can be used to 
		/// prefetch some type binding data before they are needed.
		/// </summary>
		/// <param name="callDelegate">The call delegate.</param>
		public static void PrefetchInitialization(EventHandler callDelegate)
		{
			initializationInProgress = true;
			lock (initializationLock)
			{
				callDelegate(null, null);
			}

			initializationInProgress = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeBindingData"/> class.
		/// </summary>
		/// <param name="type">The examined type. The type must implement 
		/// <see cref="IBindable"/></param>
		public TypeBindingData(Type type)
		{
			Type = type;
			ModelGetterCollection = new BindingCollection();
			ModelSetterCollection = new BindingCollection();
			ViewGetterCollection = new BindingCollection();
			ViewSetterCollection = new BindingCollection();
			ViewMappingDeclarations = new List<ViewHelperPropertyMappingAttribute>();
			ModelMappingDeclarations = new List<ModelPropertyMappingAttribute>();
			ViewUpdaters = new Hashtable();
			ModelUpdaters = new Hashtable();

			FindBindingAttributes(Type, ModelMappingDeclarations);
			FindBindingAttributes(Type, ViewMappingDeclarations);
		}

		/// <summary>
		/// Finds the binding attributes (used to fill <see cref="ViewMappingDeclarations"/>
		/// and <see cref="ModelMappingDeclarations"/> collections).
		/// </summary>
		/// <typeparam name="AttributeType">The type of the binding attribute type.</typeparam>
		/// <param name="type">The examined type.</param>
		/// <param name="collection">The collection where to put found declarations</param>
		private static void FindBindingAttributes<AttributeType>(Type type, ICollection<AttributeType> collection)
			where AttributeType : PropertyMappingAttribute
		{
			PropertyInfo[] typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance );
			foreach (PropertyInfo property in typeProperties)
			{
				if (!property.DeclaringType.Assembly.ToString().Contains("View"))
					continue;
				object[] propertyAttributes = Attribute.GetCustomAttributes(property, typeof(AttributeType), true);
				foreach (object attr in propertyAttributes)
				{
					AttributeType pa = (AttributeType)attr;
					pa.declaringProperty = property;
					collection.Add(pa);
				}
			}
			foreach (object attr in type.GetCustomAttributes(typeof(AttributeType), true))
			{
				AttributeType pa = (AttributeType)attr;
				PropertyInfo boundProperty = type.GetProperty(pa.TargetPropertyName);
				pa.declaringProperty = boundProperty;
				collection.Add(pa);
			}
		}

		/// <summary>
		/// Creates the binding functions (fills <see cref="ViewSetterCollection"/> and 
		/// <see cref="ViewGetterCollection"/> or <see cref="ModelSetterCollection"/> and 
		/// <see cref="ModelGetterCollection"/> with correct handlers). The handlers are
		/// dynamically compiled.
		/// </summary>
		/// <param name="sourceType">Type of the source to bound to</param>
		/// <param name="boundType">Type of the bound object</param>
		private void CreateBindingFunctions(EBindingSourceType sourceType, Type boundType)
		{
			if (sourceType == EBindingSourceType.View)
				foreach (ViewHelperPropertyMappingAttribute attribute in ViewMappingDeclarations)
				{
					if (!initializationInProgress)
						Debug.WriteLine("View binding: " + attribute.SourcePropertyName + " -> " + attribute.declaringProperty.Name);
					PropertyInfo sourceProp = boundType.GetProperty(attribute.SourcePropertyName);
					ViewGetterCollection[sourceProp.Name] =
						DynamicMethodCompiler.CreateGetHandler(boundType, sourceProp);
					ViewSetterCollection[sourceProp.Name] =
						DynamicMethodCompiler.CreateSetHandler(Type, attribute.declaringProperty);
				}

			if (sourceType == EBindingSourceType.Model)
				foreach (ModelPropertyMappingAttribute attribute in ModelMappingDeclarations)
				{
					if (!initializationInProgress)
						Debug.WriteLine("Model binding: " + attribute.SourcePropertyName + " -> " + attribute.declaringProperty.Name);
					PropertyInfo sourceProp = boundType.GetProperty(attribute.SourcePropertyName);
					ModelGetterCollection[sourceProp.Name] =
						DynamicMethodCompiler.CreateGetHandler(boundType, sourceProp);
					ModelSetterCollection[sourceProp.Name] =
						DynamicMethodCompiler.CreateSetHandler(Type, attribute.declaringProperty);
				}
		}

		/// <summary>
		/// Finds field for which a certain attribute declaration is used.
		/// </summary>
		/// <param name="type">The examined type</param>
		/// <param name="attributeType">Type of the attribute</param>
		/// <returns>PropertyInfo for the field if it was found</returns>
		private static PropertyInfo FindFieldByAttribute(Type type, Type attributeType)
		{
			Debug.Assert(attributeType.IsSubclassOf(typeof(Attribute)));
			PropertyInfo result = null;

			PropertyInfo[] properties = type.GetProperties();

			foreach (PropertyInfo info in properties)
			{
				if (info.IsDefined(attributeType, true))
				{
					if (result != null)
					{
						throw new InvalidOperationException(String.Format(CommandError.CMDERR_DUPLICATE_ATTRIBUTE, type, attributeType));
					}

					result = info;
				}
			}
			return result;
		}

		/// <summary>
		/// Updates the bound value of the bound property of <paramref name="boundObject"/> 
		/// when value of <paramref name="property"/> is changed in the binding source.
		/// </summary>
		/// <param name="boundObject">The bound object</param>
		/// <param name="property">The property that changed in model or viewhelper </param>
		/// <param name="sourceType">Type of binding (model/view helper).</param>
		private void UpdateValue(IBindable boundObject, string property, EBindingSourceType sourceType)
		{
			if (sourceType == EBindingSourceType.Model && ModelGetterCollection.ContainsKey(property))
			{
				object value = ((GetHandler)ModelGetterCollection[property])(ModelSourceGetter(boundObject));
				((SetHandler)ModelSetterCollection[property])(boundObject, value);
			}
			if (sourceType == EBindingSourceType.View && ViewGetterCollection.ContainsKey(property))
			{
				object value = ((GetHandler)ViewGetterCollection[property])(ViewSourceGetter(boundObject));
				((SetHandler)ViewSetterCollection[property])(boundObject, value);
			}
		}

		#region start and close bindings

		/// <summary>
		/// Starts bindings to properties of model element 
		/// and view helper of the object <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The bound object</param>
		/// <seealso cref="ModelElementAttribute" />
		/// <seealso cref="ViewHelperElementAttribute"/>
		public void StartBindings(IBindable target)
		{
			StartBindings(target, EBindingSourceType.View);
			StartBindings(target, EBindingSourceType.Model);

			PropertyChangedEventHandler viewUpdate = (sender, e) => UpdateValue(target, e.PropertyName, EBindingSourceType.View);
			PropertyChangedEventHandler modelUpdate = (sender, e) => UpdateValue(target, e.PropertyName, EBindingSourceType.Model);

			((INotifyPropertyChanged)ViewSourceGetter(target)).PropertyChanged += viewUpdate;
			((INotifyPropertyChanged)ModelSourceGetter(target)).PropertyChanged += modelUpdate;

			ViewUpdaters[target] = viewUpdate;
			ModelUpdaters[target] = modelUpdate;
		}

		/// <summary>
		/// Suspends the binding for the object <paramref name="target"/>.
		/// </summary>
		/// <param name="target">the bound object</param>
		public void CloseBindings(IBindable target)
		{
			CloseBindings(target, EBindingSourceType.View);
			CloseBindings(target, EBindingSourceType.Model);
		}

		/// <summary>
		/// Starts bindings to properties of model element
		/// or view helper of the object <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The bound object</param>
		/// <param name="sourceType">Type of binding (model/view helper).</param>
		/// <seealso cref="ModelElementAttribute"/>
		/// <seealso cref="ViewHelperElementAttribute"/>
		public void StartBindings(IBindable target, EBindingSourceType sourceType)
		{
			if (initializationInProgress) lock (initializationLock) { }

			bool isReady = false; 

			if (sourceType == EBindingSourceType.Model && ModelSourceGetter != null)
			{
				isReady = true; 
			}

			if (sourceType == EBindingSourceType.View && ViewSourceGetter != null)
			{
				isReady = true; 
			}
			if (!isReady)
				PrepareBindingTo(target, sourceType);

			if (!initializationInProgress)
			{
				BindingCollection getterCollection = (sourceType == EBindingSourceType.Model) ? ModelGetterCollection : ViewGetterCollection;
				foreach (string key in getterCollection.Keys)
				{
					UpdateValue(target, key, sourceType);
				}
			}
		}

		/// <summary>
		/// Suspends the binding for the object <paramref name="target"/>.
		/// </summary>
		/// <param name="target">The bound object</param>
		/// <param name="sourceType">Type of binding (model/view helper).</param>
		public void CloseBindings(IBindable target, EBindingSourceType sourceType)
		{
			if (sourceType == EBindingSourceType.View)
			{
				if (ViewUpdaters.ContainsKey(target))
				{
					((INotifyPropertyChanged)ViewSourceGetter(target)).PropertyChanged -=
						(PropertyChangedEventHandler)ViewUpdaters[target];
					ViewUpdaters.Remove(target);
				}
			}
			else
			{
				if (ModelUpdaters.ContainsKey(target))
				{
					((INotifyPropertyChanged)ModelSourceGetter(target)).PropertyChanged -=
						(PropertyChangedEventHandler)ModelUpdaters[target];
					ModelUpdaters.Remove(target);
				}
			}
		}

		/// <summary>
		/// Examines the type of the bound object and initializes all neccessary structures
		/// that are needed before binding can start. This methods to be called once for
		/// each type passed as <paramref name="target"/> (not for each object of that type).
		/// </summary>
		/// <param name="target">bound object</param>
		/// <param name="sourceType">Type of binding (model/view helper).</param>
		private void PrepareBindingTo(IBindable target, EBindingSourceType sourceType)
		{
			if (sourceType == EBindingSourceType.Model)
			{
				PropertyInfo modelProperty = FindFieldByAttribute(Type, typeof(ModelElementAttribute));
				ModelElementType = modelProperty.GetValue(target, null).GetType();
				if (ModelElementType == null)
					throw new InvalidOperationException(string.Format("Model property for type {0} is not assigned or the property is not marked with ModelElementAttribute", Type));
				if (!initializationInProgress)
					Debug.WriteLine("Model binding for type " + target.GetType().Name + " with model type " + ModelElementType.Name);
				ModelSourceGetter = DynamicMethodCompiler.CreateGetHandler(ModelElementType, modelProperty);
				CreateBindingFunctions(sourceType, ModelElementType);
			}

			if (sourceType == EBindingSourceType.View)
			{
				PropertyInfo viewHelperProperty = FindFieldByAttribute(Type, typeof(ViewHelperElementAttribute));
				ViewHelperType = viewHelperProperty.GetValue(target, null).GetType();
				if (ViewHelperType == null)
					throw new InvalidOperationException(string.Format("ViewHelper for type {0} is not assigned or the property is not marked with ViewHelperElementAttribute", Type));
				if (!initializationInProgress)
					Debug.WriteLine("View binding for type " + target.GetType().Name + " with view type " + ViewHelperType.Name);
				ViewSourceGetter = DynamicMethodCompiler.CreateGetHandler(ViewHelperType, viewHelperProperty);
				CreateBindingFunctions(sourceType, ViewHelperType);
			}
		}

		#endregion
	}
}