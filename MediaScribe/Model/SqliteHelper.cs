using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Windows.Media;
using System.Reflection;
using JayDev.MediaScribe.Common;

namespace JayDev.MediaScribe.Model
{
    public class SqliteDataMapping
    {
        public PropertyInfo PropertyInfo { get; set; }
        public string ColumnName { get; set; }
        public DataType PropertyDataType { get; set; }
        public bool PrimaryKey { get; set; }
    }
    public enum DataType { String, Int, DateTime, TimeSpan, Double, Boolean, Float, Long, Colour }

    static class SqliteHelper
    {
        static Dictionary<Type, List<SqliteDataMapping>> MappingsByType = new Dictionary<Type, List<SqliteDataMapping>>();
        static Dictionary<Type, string> TableNamesByType = new Dictionary<Type,string>();

        static SqliteHelper()
        {
            TableNamesByType.Add(typeof(Course), "Courses");
            PropertyInfo[] courseProperties = typeof(Course).GetProperties();
            List<SqliteDataMapping> courseMappings = new List<SqliteDataMapping>();
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "CourseID", PropertyDataType = DataType.Int, PropertyInfo = courseProperties.First(x => x.Name == "ID"), PrimaryKey = true });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "Name", PropertyDataType = DataType.String, PropertyInfo = courseProperties.First(x => x.Name == "Name") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "LastPlayedTrackID", PropertyDataType = DataType.Int, PropertyInfo = courseProperties.First(x => x.Name == "LastPlayedTrackID") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "LastPlayedTrackPosition", PropertyDataType = DataType.TimeSpan, PropertyInfo = courseProperties.First(x => x.Name == "LastPlayedTrackPosition") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "EmbeddedVideoWidth", PropertyDataType = DataType.Double, PropertyInfo = courseProperties.First(x => x.Name == "EmbeddedVideoWidth") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "EmbeddedVideoHeight", PropertyDataType = DataType.Double, PropertyInfo = courseProperties.First(x => x.Name == "EmbeddedVideoHeight") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "DateCreated", PropertyDataType = DataType.DateTime, PropertyInfo = courseProperties.First(x => x.Name == "DateCreated") });
            courseMappings.Add(new SqliteDataMapping() { ColumnName = "DateViewed", PropertyDataType = DataType.DateTime, PropertyInfo = courseProperties.First(x => x.Name == "DateViewed") });
            MappingsByType.Add(typeof(Course), courseMappings);

            TableNamesByType.Add(typeof(Note), "Notes");
            PropertyInfo[] noteProperties = typeof(Note).GetProperties();
            List<SqliteDataMapping> noteMappings = new List<SqliteDataMapping>();
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "NoteID", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "ID"), PrimaryKey = true });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "CourseID", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "ParentCourseID") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "Body", PropertyDataType = DataType.String, PropertyInfo = noteProperties.First(x => x.Name == "Body") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "StartTrackTimeID", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "StartTrackTimeID") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "EndTrackTimeID", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "EndTrackTimeID") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "Rating", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "Rating") });
            MappingsByType.Add(typeof(Note), noteMappings);
            
            TableNamesByType.Add(typeof(TrackTime), "TrackTimes");
            PropertyInfo[] trackTimeProperties = typeof(TrackTime).GetProperties();
            List<SqliteDataMapping> trackTimeMappings = new List<SqliteDataMapping>();
            trackTimeMappings.Add(new SqliteDataMapping() { ColumnName = "TrackTimeID", PropertyDataType = DataType.Int, PropertyInfo = trackTimeProperties.First(x => x.Name == "ID"), PrimaryKey = true });
            trackTimeMappings.Add(new SqliteDataMapping() { ColumnName = "TrackID", PropertyDataType = DataType.Int, PropertyInfo = trackTimeProperties.First(x => x.Name == "TrackID") });
            trackTimeMappings.Add(new SqliteDataMapping() { ColumnName = "Time", PropertyDataType = DataType.TimeSpan, PropertyInfo = trackTimeProperties.First(x => x.Name == "Time") });
            MappingsByType.Add(typeof(TrackTime), trackTimeMappings);

            TableNamesByType.Add(typeof(Track), "Tracks");
            PropertyInfo[] trackProperties = typeof(Track).GetProperties();
            List<SqliteDataMapping> trackMappings = new List<SqliteDataMapping>();
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "TrackID", PropertyDataType = DataType.Int, PropertyInfo = trackProperties.First(x => x.Name == "ID"), PrimaryKey = true });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "CourseID", PropertyDataType = DataType.Int, PropertyInfo = trackProperties.First(x => x.Name == "ParentCourseID") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "FilePath", PropertyDataType = DataType.String, PropertyInfo = trackProperties.First(x => x.Name == "FilePath") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "Title", PropertyDataType = DataType.String, PropertyInfo = trackProperties.First(x => x.Name == "Title") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "Length", PropertyDataType = DataType.TimeSpan, PropertyInfo = trackProperties.First(x => x.Name == "Length") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "IsVideo", PropertyDataType = DataType.Boolean, PropertyInfo = trackProperties.First(x => x.Name == "IsVideo") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "AspectRatio", PropertyDataType = DataType.Float, PropertyInfo = trackProperties.First(x => x.Name == "AspectRatio") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "OrderNumber", PropertyDataType = DataType.Int, PropertyInfo = trackProperties.First(x => x.Name == "OrderNumber") });
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "FileSize", PropertyDataType = DataType.Long, PropertyInfo = trackProperties.First(x => x.Name == "FileSize") });
            MappingsByType.Add(typeof(Track), trackMappings);

            TableNamesByType.Add(typeof(Hotkey), "Hotkeys");
            PropertyInfo[] hotkeyProperties = typeof(Hotkey).GetProperties();
            List<SqliteDataMapping> hotkeyMappings = new List<SqliteDataMapping>();
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "HotkeyID", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "ID"), PrimaryKey = true });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "Function", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "Function") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "ModifierKey", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "ModifierKey") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "Key", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "Key") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "Colour", PropertyDataType = DataType.Colour, PropertyInfo = hotkeyProperties.First(x => x.Name == "Colour") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "SeekDirection", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "SeekDirection") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "SeekSeconds", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "SeekSeconds") });
            hotkeyMappings.Add(new SqliteDataMapping() { ColumnName = "Rating", PropertyDataType = DataType.Int, PropertyInfo = hotkeyProperties.First(x => x.Name == "Rating") });
            MappingsByType.Add(typeof(Hotkey), hotkeyMappings);
        }

        static Dictionary<Type, Dictionary<string, int>> OrdinalsByType = new Dictionary<Type, Dictionary<string, int>>();
        static Dictionary<string, int> GetOrdinals<T>(SQLiteDataReader reader, List<SqliteDataMapping> mappings)
        {
            Dictionary<string, int> ordinals;
            OrdinalsByType.TryGetValue(typeof(T), out ordinals);
            if (null != ordinals)
            {
                return ordinals;
            }

            ordinals = new Dictionary<string, int>();
            foreach (SqliteDataMapping mapping in mappings)
            {
                var ordinal = reader.GetOrdinal(mapping.ColumnName);
                ordinals[mapping.ColumnName] = ordinal;
            }

            OrdinalsByType.Add(typeof(T), ordinals);

            return ordinals;
        }

        public static List<T> LoadFromReader<T>(SQLiteDataReader reader) where T : new()
        {
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];
            Dictionary<string, int> ordinals = GetOrdinals<T>(reader, mappings);
            List<T> items = new List<T>();
            while (reader.Read())
            {
                T item = new T();
                foreach (SqliteDataMapping mapping in mappings)
                {
                    int ordinal = ordinals[mapping.ColumnName];
                    if (reader[ordinal] != DBNull.Value)
                    {
                        switch (mapping.PropertyDataType)
                        {
                            case DataType.DateTime:
                                {
                                    string colVal = (string)reader[ordinal];
                                    DateTime convertedVal = DateTime.Parse(colVal);
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                            case DataType.Double:
                                {
                                    string colVal = (string)reader[ordinal];
                                    double convertedVal = double.Parse(colVal);
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                            case DataType.Int:
                                {
                                    long colVal = (long)reader[ordinal];
                                    int convertedVal = (int)colVal;
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                            case DataType.Long:
                                {
                                    long colVal = (long)reader[ordinal];
                                    mapping.PropertyInfo.SetValue(item, colVal, null);
                                }
                                break;
                            case DataType.String:
                                {
                                    string colVal = (string)reader[ordinal];
                                    mapping.PropertyInfo.SetValue(item, colVal, null);
                                }
                                break;
                            case DataType.TimeSpan:
                                {
                                    string colVal = (string)reader[ordinal];
                                    if (colVal.IndexOf(':') != -1)
                                    {
                                        TimeSpan convertedVal = TimeSpan.Parse(colVal);
                                        mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                    }
                                    else
                                    {
                                        string trimmed = colVal.Substring(0, colVal.Length - 7);
                                        TimeSpan convertedVal = new TimeSpan(0, 0, Int32.Parse(trimmed));
                                        mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                    }
                                }
                                break;
                            case DataType.Boolean:
                                {
                                    long colVal = (long)reader[ordinal];
                                    bool convertedVal = colVal == 0 ? false : true;
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                            case DataType.Float:
                                {
                                    string colVal = (string)reader[ordinal];
                                    float convertedVal = float.Parse(colVal);
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                            case DataType.Colour:
                                {
                                    string colVal = (string)reader[ordinal];
                                    Color convertedVal = (Color)ColorConverter.ConvertFromString(colVal);
                                    mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                }
                                break;
                        }
                    }
                }
                items.Add(item);
            }
            return items;
        }

        static readonly Dictionary<Type, string> UpdateQueriesByType = new Dictionary<Type, string>();
        static readonly Dictionary<Type, string> InsertQueriesByType = new Dictionary<Type, string>();

        public static string CreateUpdateQuery<T>()
        {
            string updateQuery;
            UpdateQueriesByType.TryGetValue(typeof(T), out updateQuery);
            if (null != updateQuery)
            {
                return updateQuery;
            }

            StringBuilder builder = new StringBuilder();
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];

            builder.Append(string.Format("UPDATE [{0}] SET ", TableNamesByType[typeof(T)]));

            bool isFirst = true;
            foreach (var mapping in mappings)
            {
                if (false == mapping.PrimaryKey)
                {
                    if (false == isFirst)
                    {
                        builder.Append(", ");
                    }
                    isFirst = false;

                    builder.Append(string.Format("[{0}] = @{0}", mapping.ColumnName));
                }
            }

            SqliteDataMapping primaryKeyMapping = mappings.First(x => true == x.PrimaryKey);
            builder.Append(string.Format(" WHERE [{0}] = @{0}", primaryKeyMapping.ColumnName));

            updateQuery = builder.ToString();
            UpdateQueriesByType.Add(typeof(T), updateQuery);
            return updateQuery;
        }

        public static string CreateInsertQuery<T>()
        {
            string insertQuery;
            InsertQueriesByType.TryGetValue(typeof(T), out insertQuery);
            if (null != insertQuery)
            {
                return insertQuery;
            }
            
            string tableName = TableNamesByType[typeof(T)];

            StringBuilder builder = new StringBuilder();
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];

            builder.Append(string.Format("INSERT INTO [{0}] (", tableName));

            bool isFirst = true;
            foreach (var mapping in mappings)
            {
                if (false == mapping.PrimaryKey)
                {
                    if (false == isFirst)
                    {
                        builder.Append(", ");
                    }
                    isFirst = false;

                    builder.Append(string.Format("[{0}]", mapping.ColumnName));
                }
            }

            builder.Append(string.Format(") VALUES (", tableName));
            isFirst = true;
            foreach (var mapping in mappings)
            {
                if (false == mapping.PrimaryKey)
                {
                    if (false == isFirst)
                    {
                        builder.Append(", ");
                    }
                    isFirst = false;

                    builder.Append(string.Format("@{0}", mapping.ColumnName));
                }
            }

            builder.Append(")");

            insertQuery = builder.ToString();
            InsertQueriesByType.Add(typeof(T), insertQuery);
            return insertQuery;
        }

        public static void SetParameterValues<T>(T inputToSave, SQLiteCommand command)
        {
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];

            command.Parameters.Clear();
            foreach (var mapping in mappings)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = string.Format("@{0}", mapping.ColumnName);
                object value = mapping.PropertyInfo.GetValue(inputToSave, null);
                if (null == value)
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = value;
                }
                command.Parameters.Add(parameter);
            }
        }


        public static void Save<T>(IEnumerable<T> itemsToSave, SQLiteConnection connection)
        {
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];
            SqliteDataMapping primaryKeyMapping = mappings.First(x => true == x.PrimaryKey);
            string tableName = TableNamesByType[typeof(T)];

            using (SQLiteCommand getLastInsertIDCommand = new SQLiteCommand(connection))
            {
                getLastInsertIDCommand.CommandText = "SELECT last_insert_rowid()";

                using (SQLiteCommand updateCommand = new SQLiteCommand(connection))
                {
                    using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                    {
                        updateCommand.CommandText = SqliteHelper.CreateUpdateQuery<T>();
                        insertCommand.CommandText = SqliteHelper.CreateInsertQuery<T>();
                        foreach (T item in itemsToSave)
                        {
                            object primaryKey = primaryKeyMapping.PropertyInfo.GetValue(item, null);
                            if (null == primaryKey)
                            {
                                SqliteHelper.SetParameterValues<T>(item, insertCommand);

                                int id = insertCommand.ExecuteNonQuery();
                                if (id >= 0)
                                {
                                    int newID = Convert.ToInt32(getLastInsertIDCommand.ExecuteScalar());
                                    primaryKeyMapping.PropertyInfo.SetValue(item, newID, null);
                                }
                                else
                                {
                                    throw new ApplicationException("no ID returned?");
                                }
                            }
                            else
                            {
                                SqliteHelper.SetParameterValues<T>(item, updateCommand);

                                int affectedCount = updateCommand.ExecuteNonQuery();
                                if (affectedCount == 0)
                                {
                                    throw new ApplicationException("affected rows should be at least 1");
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Save<T>(T itemToSave, SQLiteConnection connection)
        {
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];
            SqliteDataMapping primaryKeyMapping = mappings.First(x => true == x.PrimaryKey);

            object primaryKey = primaryKeyMapping.PropertyInfo.GetValue(itemToSave, null);
            
            string tableName = TableNamesByType[typeof(T)];

            if (null == primaryKey)
            {
                using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                {
                    insertCommand.CommandText = SqliteHelper.CreateInsertQuery<T>();
                    SqliteHelper.SetParameterValues<T>(itemToSave, insertCommand);

                    int affectedCount = insertCommand.ExecuteNonQuery();
                    if (affectedCount >= 0)
                    {
                        using (SQLiteCommand getLastInsertIDCommand = new SQLiteCommand(connection))
                        {
                            getLastInsertIDCommand.CommandText = "SELECT last_insert_rowid()";
                            int newID = Convert.ToInt32(getLastInsertIDCommand.ExecuteScalar());
                            primaryKeyMapping.PropertyInfo.SetValue(itemToSave, newID, null);
                        }
                    }
                    else
                    {
                        throw new ApplicationException("no ID returned?");
                    }
                }
            }
            else
            {
                using (SQLiteCommand updateCommand = new SQLiteCommand(connection))
                {
                    updateCommand.CommandText = SqliteHelper.CreateUpdateQuery<T>();
                    SqliteHelper.SetParameterValues<T>(itemToSave, updateCommand);

                    int affectedCount = updateCommand.ExecuteNonQuery();
                    if (affectedCount == 0)
                    {
                        throw new ApplicationException("affected rows should be at least 1");
                    }
                }
            }
        }

        public static List<T> ReadAll<T>(SQLiteConnection connection) where T : new()
        {
            string tableName = TableNamesByType[typeof(T)];
            List<T> result = new List<T>();
            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = string.Format("SELECT * FROM [{0}]", tableName);
                using (SQLiteDataReader reader = mycommand.ExecuteReader())
                {
                    result = SqliteHelper.LoadFromReader<T>(reader);
                }
            }

            return result;
        }

        public static void Delete<T>(T itemToDelete, SQLiteConnection connection)
        {
            string tableName = TableNamesByType[typeof(T)];
            List<SqliteDataMapping> mappings = MappingsByType[typeof(T)];
            SqliteDataMapping primaryKeyMapping = mappings.First(x => true == x.PrimaryKey);

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                object primaryKey = primaryKeyMapping.PropertyInfo.GetValue(itemToDelete, null);
                if (null == primaryKey)
                {
                    throw new ApplicationException("error: cannot delete an object with no ID...");
                }

                mycommand.CommandText = string.Format("DELETE FROM [{0}] WHERE [{1}] = @{1}", tableName, primaryKeyMapping.ColumnName);
                var parameter = mycommand.CreateParameter();
                parameter.ParameterName = string.Format("@{0}", primaryKeyMapping.ColumnName);
                parameter.Value = primaryKey;
                mycommand.Parameters.Add(parameter);

                var affectedCount = mycommand.ExecuteNonQuery();
                if (affectedCount == 0)
                {
                    throw new ApplicationException("affected rows should be at least 1");
                }
            }
        }

    }
}