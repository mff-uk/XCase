using System;
using System.Runtime.Serialization;

namespace XCase.Model.Serialization
{
	/// <summary>
	///  Exception caused by bad references during the restoration
	/// </summary>
	public class DeserializationException : SystemException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.SystemException"/> class.
		/// </summary>
		public DeserializationException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.SystemException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error. </param>
		public DeserializationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.SystemException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
		public DeserializationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.SystemException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data. </param><param name="context">The contextual information about the source or destination. </param>
		protected DeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}