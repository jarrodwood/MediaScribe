using System;
using System.Runtime;
using System.Reflection;
using System.Linq;

namespace JayDev.Notemaker.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the DescriptionAttribute valud of an enum value, if none are found uses the string version of the specified value
        /// </summary>
        public static InformationAttribute GetInformation(this Enum value)
        {
            Type type = value.GetType();

            return GetEnumInformation(value.ToString(), type);
        }

        public static InformationAttribute GetInformationForEnum(this Type type, object value)
        {
            return GetEnumInformation(value.ToString(), type);
        }

        private static InformationAttribute GetEnumInformation(string value, Type type)
        {
            MemberInfo[] memberInfo = type.GetMember(value);

            if (memberInfo != null && memberInfo.Length > 0)
            {
                // default to the first member info, it's for the specific enum value
                var info = memberInfo.First().GetCustomAttributes(typeof(InformationAttribute), false).FirstOrDefault();

                if (info != null)
                    return ((InformationAttribute)info);
            }

            // no description - return the string value of the enum
            return null;
        }
    }

    /// <summary>
    /// Custom attribute, allowing the specifying of a Label, Description, and 'Applicable When' text to an attribute. This is intended to
    /// be used on Hotkeys, to provide not only a user-readable label, but also tooltip information.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class InformationAttribute : Attribute
    {
        public static readonly InformationAttribute Default = new InformationAttribute();
        private string _label;
        private string _description;
        private string _applicableWhen;

        public InformationAttribute()
        {
        }

        public InformationAttribute(string label, string description, string applicableWhen)
        {
            this._label = label;
            this._description = description;
            this._applicableWhen = applicableWhen;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            InformationAttribute attribute = obj as InformationAttribute;
            return ((attribute != null) && (attribute._label == this._label) && (attribute._description == this._description));
        }

        public override int GetHashCode()
        {
            return this.Description.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public virtual string Description
        {
            get
            {
                return this.DescriptionValue;
            }
        }

        protected string DescriptionValue
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        public virtual string Label
        {
            get
            {
                return this.LabelValue;
            }
        }

        protected string LabelValue
        {
            get
            {
                return this._label;
            }
            set
            {
                this._label = value;
            }
        }

        public virtual string ApplicableWhen
        {
            get
            {
                return this.ApplicableWhenValue;
            }
        }

        protected string ApplicableWhenValue
        {
            get
            {
                return this._applicableWhen;
            }
            set
            {
                this._applicableWhen = value;
            }
        }
    }
}
