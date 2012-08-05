using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JayDev.Notemaker.Core;
using NHibernate;
using NHibernate.Criterion;

namespace JayDev.Notemaker.Model
{
    public class SettingRepository
    {
        private object _destructiveOperationLockToken = new object();

        public List<Hotkey> GetHotkeys()
        {
            IList<Hotkey> result;
            using (ISession session = NHibernateHelper.OpenSession())
            {
                result = session.CreateCriteria<Hotkey>().List<Hotkey>();
            }

            return result as List<Hotkey>;
        }

        //public Course GetCourse(int courseID)
        //{
        //    Course result;
        //    using (ISession session = NHibernateHelper.OpenSession())
        //    {
        //        result = session.Get<Course>(courseID);
        //    }

        //    foreach (Note note in result.Notes)
        //    {
        //        if (null != note.Start)
        //            note.Start.ParentCourse = result;
        //        if (null != note.End)
        //            note.End.ParentCourse = result;
        //    }

        //    return result;
        //}

        public void SaveHotkey(Hotkey hotkey)
        {
            lock (_destructiveOperationLockToken)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(hotkey);

                    transaction.Commit();
                }
            }
        }

        public void DeleteHotkey(Hotkey hotkey)
        {
            lock (_destructiveOperationLockToken)
            {
                using (ISession session = NHibernateHelper.OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Delete(hotkey);

                    transaction.Commit();
                }
            }
        }
    }
}
