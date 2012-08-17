using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace JayDev.MediaScribe.Model
{
    public class SettingRepository : RepositorySqliteBase
    {
        private object _destructiveOperationLockToken = new object();

        public List<Hotkey> GetHotkeys()
        {
            PrepareConnection();
            List<Hotkey> hotkeys;
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                hotkeys = SqliteHelper.ReadAll<Hotkey>(connection);

                mytransaction.Commit();
            }

            return hotkeys;
        }

        public void PersistHotkeys(List<Hotkey> hotkeys)
        {
            PrepareConnection();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {

                List<Hotkey> savedhotkeys = GetHotkeys();

                List<Hotkey> toDelete = new List<Hotkey>();
                List<Hotkey> toCreate = new List<Hotkey>();
                List<Hotkey> unchanged = new List<Hotkey>();
                foreach (Hotkey hotkey in hotkeys)
                {
                    Hotkey existingMatch = savedhotkeys.FirstOrDefault(x => x.Equals(hotkey));
                    if (null == existingMatch)
                    {
                        toCreate.Add(hotkey);
                    }
                    else
                    {
                        unchanged.Add(hotkey);
                    }
                }
                foreach (Hotkey hotkey in savedhotkeys)
                {
                    if (false == unchanged.Contains(hotkey))
                    {
                        toDelete.Add(hotkey);
                    }
                }

                foreach (Hotkey hotkey in toDelete)
                {
                    SqliteHelper.Delete<Hotkey>(hotkey, connection);
                }
                foreach (Hotkey hotkey in toCreate)
                {
                    SqliteHelper.Save<Hotkey>(hotkey, connection);
                }

                mytransaction.Commit();
            }
        }
    }
}
