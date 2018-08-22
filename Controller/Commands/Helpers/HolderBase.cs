namespace XCase.Controller.Commands.Helpers
{
	/// <summary>
	/// Holder class for elements of certain type.
	/// </summary>
	/// <typeparam name="Type">The type of the element to hold.</typeparam>
	public class HolderBase<Type> : IHolder
		where Type : class
	{
		public HolderBase(Type element)
		{
			this.Element = element;
		}

		public HolderBase()
		{
			Element = null;
		}

		public Type Element { get; set; }

		public virtual bool HasValue
		{
			get
			{
				return Element != null;
			}
		}

		object IHolder.WrappedValue
		{
			get
			{
				return Element;
			}
		}

		
	}

	/// <summary>
	/// Interface for object wrapping a value (not type-safe)
	/// </summary>
	public interface IHolder
	{
		object WrappedValue { get; }
	}
}