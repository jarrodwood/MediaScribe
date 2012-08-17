//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using JayDev.MediaScribe.Core;
//using NHibernate;
//using NHibernate.Criterion;

//namespace JayDev.MediaScribe.Model
//{
//    public class SettingRepository
//    {
//        private object _destructiveOperationLockToken = new object();

//        public List<Hotkey> GetHotkeys()
//        {
//            IList<Hotkey> result;
//            using (ISession session = NHibernateHelper.OpenSession())
//            {
//                //result = session.CreateCriteria<Hotkey>().AddOrder(Order.Desc("Function")).List<Hotkey>();
//                result = session.CreateCriteria<Hotkey>().List<Hotkey>();
//            }

//            //order by orderWeight (it's metadata, so we can't do it at the DB level)
//            result = result.OrderBy(x => x.OrderWeight).ToList();
//            return result as List<Hotkey>;
//        }

//        //public Course GetCourse(int courseID)
//        //{
//        //    Course result;
//        //    using (ISession session = NHibernateHelper.OpenSession())
//        //    {
//        //        result = session.Get<Course>(courseID);
//        //    }

//        //    foreach (Note note in result.Notes)
//        //    {
//        //        if (null != note.Start)
//        //            note.Start.ParentCourse = result;
//        //        if (null != note.End)
//        //            note.End.ParentCourse = result;
//        //    }

//        //    return result;
//        //}

//        public void PersistHotkeys(List<Hotkey> hotkeys)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    List<Hotkey> savedhotkeys = GetHotkeys();

//                    List<Hotkey> toDelete = new List<Hotkey>();
//                    List<Hotkey> toCreate = new List<Hotkey>();
//                    List<Hotkey> unchanged = new List<Hotkey>();
//                    foreach (Hotkey hotkey in hotkeys)
//                    {
//                        Hotkey existingMatch = savedhotkeys.FirstOrDefault(x => x.Equals(hotkey));
//                        if (null == existingMatch)
//                        {
//                            toCreate.Add(hotkey);
//                        }
//                        else
//                        {
//                            unchanged.Add(hotkey);
//                        }
//                    }
//                    foreach (Hotkey hotkey in savedhotkeys)
//                    {
//                        if (false == unchanged.Contains(hotkey))
//                        {
//                            toDelete.Add(hotkey);
//                        }
//                    }


//                    foreach (Hotkey hotkey in toDelete)
//                    {
//                        session.Delete(hotkey);
//                    }
//                    for(int i = 0; i < toCreate.Count; i++)
//                    {
//                        Hotkey hotkey = toCreate[i];
//                        if (null == hotkey.ID)
//                        {
//                            session.SaveOrUpdate(hotkey);
//                        }
//                        else
//                        {
//                            hotkey = (Hotkey)session.Merge(hotkey);
//                        }
//                    }

//                    transaction.Commit();
//                }
//            }
//        }

//        public void SaveHotkey(Hotkey hotkey)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    session.SaveOrUpdate(hotkey);

//                    transaction.Commit();
//                }
//            }
//        }

//        public void DeleteHotkey(Hotkey hotkey)
//        {
//            lock (_destructiveOperationLockToken)
//            {
//                using (ISession session = NHibernateHelper.OpenSession())
//                using (ITransaction transaction = session.BeginTransaction())
//                {
//                    session.Delete(hotkey);

//                    transaction.Commit();
//                }
//            }
//        }
//    }
//}
