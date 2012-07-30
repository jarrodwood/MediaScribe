using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using JayDev.Notemaker.Common;
using System.Reflection;
using System.Collections.ObjectModel;

namespace JayDev.Notemaker
{
    [Serializable]
    public class Note : INotifyPropertyChanged, IEditableObject, ICloneable
    {
        public virtual int? ID { get; protected set; }

        public virtual int ParentCourseID { get; set; }
        public virtual Course ParentCourse { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private TrackTime _start;

        public delegate void ObjectChangeCommittedEventHandler(object sender, EventArgs e);

        public virtual event ObjectChangeCommittedEventHandler ChangeCommitted;

        [DataMember]
        public virtual TrackTime Start
        {
            get { return _start; }
            set
            {
                //has the value changed?
                bool isChanged = _start != value;

                //if it has changed... remove the event from the old object, replace the object, hook up the event handler to the new value (if new value is not null), then notify everyone of the change.
                if (null != _start && isChanged)
                {
                    (_start as INotifyPropertyChanged).PropertyChanged -= startPropertyChanged;
                }
                _start = value;
                if (null != _start)
                {
                    (_start as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(startPropertyChanged);
                }
                if (isChanged)
                {
                    OnPropertyChanged("Start");
                }
            }
        }

        void startPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Start");
        }

        public virtual bool IsDirty { get; set; }

        private TrackTime _end;

        [DataMember]
        public virtual TrackTime End
        {
            get { return _end; }
            set
            {
                //has the value changed?
                bool isChanged = _end != value;

                //if it has changed... remove the event from the old object, replace the object, hook up the event handler to the new value (if new value is not null), then notify everyone of the change.
                if (null != _end && isChanged)
                {
                    (_end as INotifyPropertyChanged).PropertyChanged -= endPropertyChanged;
                }
                _end = value;
                if (null != _end)
                {
                    (_end as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(endPropertyChanged);
                }
                if (isChanged)
                {
                    OnPropertyChanged("End");
                }
            }
        }

        void endPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("End");
        }
        private string _title;
        [DataMember]
        public virtual string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _body;
        [DataMember]
        public virtual string Body
        {
            get { return _body; }
            set
            {
                _body = value;
                OnPropertyChanged("Body");
            }
        }

        private int _rating;
        [DataMember]
        public virtual int Rating
        {
            get { return _rating; }
            set
            {
                _rating = value;
                OnPropertyChanged("Rating");
            }
        }

        public virtual string BodyStripped { get; set; }

        private ObservableCollection<Tag> _tags;
        [DataMember]
        public virtual ObservableCollection<Tag> Tags
        {
            get { return _tags; }
            set
            {
                if (null == value)
                {
                    throw new ApplicationException("Domain object's collection cannot be set to null");
                }
                //has the value changed?
                bool isChanged = _tags != value;
                if (null != _tags && _tags != value)
                {
                    _tags.CollectionChanged -= tags_CollectionChanged;
                }
                _tags = value;
                _tags.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(tags_CollectionChanged);
                OnPropertyChanged("Tags");
            }
        }

        void tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Tags");
        }

        public virtual string TitleBody
        {
            get
            {
                string result = string.Empty;
                if (false == string.IsNullOrEmpty(Title))
                {
                    result = Title += " - ";
                }
                if (false == string.IsNullOrEmpty(Body))
                {
                    result += Body;
                }
                return result;
            }
        }


        public Note()
        {
            Tags = new ObservableCollection<Tag>();
        }

        // Create the OnPropertyChanged method to raise the event
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        private Note _versionBeforeEdit = null;
        // Summary:
        //     Begins an edit on an object.
        public virtual void BeginEdit()
        {
            //JDW: DataGrid control calls BeginEdit twice. this is known datagrid behaviour. ref: http://stackoverflow.com/questions/4450878/wpf-datagrid-calls-beginedit-on-an-ieditableobject-two-times
            if (false == IsDirty)
            {
                IsDirty = true;
                _versionBeforeEdit = (Note)this.Clone();
            }
        }

        public virtual object Clone()
        {
            Note clone = new Note();
            DeepCopy(this, clone);
            return clone;
        }

        public virtual void CancelEdit()
        {
            //JDW: DataGrid control calls CancelEdit twice. this is known datagrid behaviour. ref: http://stackoverflow.com/questions/4450878/wpf-datagrid-calls-beginedit-on-an-ieditableobject-two-times
            if (IsDirty)
            {
                //revert all changes. use the tracked properties where possible, to raise PropertyChanged events.
                //NOTE: for reference members, this will always send out PropertyChanged events (since we cloned them, they're difference objects)
                DeepCopy(_versionBeforeEdit, this);

                _versionBeforeEdit = null;
                this.IsDirty = false;
            }
        }

        private void DeepCopy(Note source, Note destination)
        {
            //use properties where possible, to raise PropertyChanged events.
            destination.Start = null == source.Start ? null : (TrackTime)source.Start.Clone();
            destination.End = null == source.End ? null : (TrackTime)source.End.Clone();
            destination.Title = source.Title;
            destination.Body = source.Body;
            destination.Rating = source.Rating;
            destination.Tags = new ObservableCollection<Tag>(source.Tags);
        }

        //
        // Summary:
        //     Pushes changes since the last System.ComponentModel.IEditableObject.BeginEdit()
        //     or System.ComponentModel.IBindingList.AddNew() call into the underlying object.
        public virtual void EndEdit()
        {
            //JDW: DataGrid control calls CancelEdit twice. this is known datagrid behaviour. ref: http://stackoverflow.com/questions/4450878/wpf-datagrid-calls-beginedit-on-an-ieditableobject-two-times
            if (IsDirty)
            {
                if (null != ChangeCommitted)
                {
                    ChangeCommitted(this, new EventArgs());
                }
                _versionBeforeEdit = null;
                this.IsDirty = false;
            }
        }
    }
}
