using System;

namespace XCase.Model.Serialization
{
    public struct XCaseGuid : IEquatable<XCaseGuid>
    {
        private XCaseGuid(Guid guid)
            : this()
        {
            Value = guid.ToString();
        }

        private String Value { get; set; }

        public static XCaseGuid Parse(string value)
        {
            return new XCaseGuid { Value = value };
        }

        public static XCaseGuid NewGuid()
        {
            return new XCaseGuid(Guid.NewGuid());
        }

        public override string ToString()
        {
            return Value;
        }

        public bool IsEmpty
        {
            get { return String.IsNullOrEmpty(Value); }
        }

        #region equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(XCaseGuid other)
        {
            return true;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(XCaseGuid)) return false;
            return Equals((XCaseGuid)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(XCaseGuid left, XCaseGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(XCaseGuid left, XCaseGuid right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}