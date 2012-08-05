using System;
using System.Runtime;

namespace JayDev.Notemaker.Common
{
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
