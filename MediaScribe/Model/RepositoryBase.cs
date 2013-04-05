using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Windows.Media;
using System.Reflection;
using JayDev.MediaScribe.Common;
using System.Configuration;
using System.IO;
using JayDev.MediaScribe.Core;

namespace JayDev.MediaScribe.Model
{
    /// <summary>
    /// The base class for all SQLite repositories.
    /// </summary>
    public class RepositoryBase
    {
        #region Private Static Fields

        /// <summary>
        /// Stores the database table names that are mapped to given Domain object types. This is populated in the static constructor.
        /// </summary>
        private static Dictionary<Type, string> TableNamesByType = new Dictionary<Type, string>();

        /// <summary>
        /// Stores database mapping information for given Domain object types. This is populated in the static constructor.
        /// </summary>
        private static Dictionary<Type, List<SqliteDataMapping>> MappingsByType = new Dictionary<Type, List<SqliteDataMapping>>();

        /// <summary>
        /// When returning SQLite database table records, each column is returned in a specific position. This dictionary will cache that
        /// position information for each column, for each table. As such, this is populated as different tables' contents are read from the
        /// database.
        /// </summary>
        private static Dictionary<Type, Dictionary<string, int>> OrdinalsByType = new Dictionary<Type, Dictionary<string, int>>();

        /// <summary>
        /// Stores cached 'UPDATE' SQL commands for a given Domain object type. This is populated and used solely in the CreateUpdateQuery
        /// method.
        /// </summary>
        private static readonly Dictionary<Type, string> UpdateQueriesByType = new Dictionary<Type, string>();

        /// <summary>
        /// Stores cached 'INSERT' SQL commands for a given Domain object type. This is populated and used solely in the CreateInsertQuery
        /// method.
        /// </summary>
        private static readonly Dictionary<Type, string> InsertQueriesByType = new Dictionary<Type, string>();
        
        private static readonly string DatabaseFileName = "MediaScribe.db";
        private static readonly string InitialDatabaseScriptFileName = "JayDev.MediaScribe.Resources.create_db.sql";

        #endregion

        #region Static Constructor

        /// <summary>
        /// Static constructor, configuring the mapping of the Domain objects to the SQLite tables. This populates the TableNamesByType and
        /// MappingsByType static dictionaries.
        /// </summary>
        static RepositoryBase()
        {
            /***********************
             * Prepare database mappings
             ***********************/

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
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "StartTrackNumber", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "StartTrackNumber") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "StartTime", PropertyDataType = DataType.TimeSpan, PropertyInfo = noteProperties.First(x => x.Name == "StartTime") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "EndTrackNumber", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "EndTrackNumber") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "EndTime", PropertyDataType = DataType.TimeSpan, PropertyInfo = noteProperties.First(x => x.Name == "EndTime") });
            noteMappings.Add(new SqliteDataMapping() { ColumnName = "Rating", PropertyDataType = DataType.Int, PropertyInfo = noteProperties.First(x => x.Name == "Rating") });
            MappingsByType.Add(typeof(Note), noteMappings);
            
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
            trackMappings.Add(new SqliteDataMapping() { ColumnName = "TrackNumber", PropertyDataType = DataType.Int, PropertyInfo = trackProperties.First(x => x.Name == "TrackNumber") });
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

            /***********************
             * Prepare database upgrade scripts
             ***********************/

        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Saves a collection of Domain objects to the database. This is an efficient method of saving in bulk.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemsToSave"></param>
        /// <param name="connection"></param>
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
                        updateCommand.CommandText = CreateUpdateQuery<T>();
                        insertCommand.CommandText = CreateInsertQuery<T>();
                        foreach (T item in itemsToSave)
                        {
                            object primaryKey = primaryKeyMapping.PropertyInfo.GetValue(item, null);
                            if (null == primaryKey)
                            {
                                SetParameterValues<T>(item, insertCommand);

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
                                SetParameterValues<T>(item, updateCommand);

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

        /// <summary>
        /// Saves a Domain object to the database. This can be used to both create a new record, and update an existing record (the
        /// difference is determined depending on the existance of a primary key value)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToSave"></param>
        /// <param name="connection"></param>
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
                    insertCommand.CommandText = CreateInsertQuery<T>();
                    SetParameterValues<T>(itemToSave, insertCommand);

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
                    updateCommand.CommandText = CreateUpdateQuery<T>();
                    SetParameterValues<T>(itemToSave, updateCommand);

                    int affectedCount = updateCommand.ExecuteNonQuery();
                    if (affectedCount == 0)
                    {
                        throw new ApplicationException("affected rows should be at least 1");
                    }
                }
            }
        }

        /// <summary>
        /// Reads all records of a specified Domain object type from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToDelete"></param>
        /// <param name="connection"></param>
        public static List<T> ReadAll<T>(SQLiteConnection connection) where T : new()
        {
            string tableName = TableNamesByType[typeof(T)];
            List<T> result = new List<T>();
            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = string.Format("SELECT * FROM [{0}]", tableName);
                using (SQLiteDataReader reader = mycommand.ExecuteReader())
                {
                    result = LoadFromReader<T>(reader);
                }
            }

            return result;
        }

        /// <summary>
        /// Delete a Domain object from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToDelete"></param>
        /// <param name="connection"></param>
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

        #endregion

        #region Private Static Methods (Helper methods)

        /// <summary>
        /// Provided a populated SQLiteDataReader and the appropriate mapping metadata for it, populate the OrdinalsByType dictionary with
        /// the ordinal information for each column in the SQLiteDataReader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private static Dictionary<string, int> GetOrdinals<T>(SQLiteDataReader reader, List<SqliteDataMapping> mappings)
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

        /// <summary>
        /// Provided a SQLiteDataReader, construct the reader's contents into a list of Domain objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static List<T> LoadFromReader<T>(SQLiteDataReader reader) where T : new()
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
                                    if (colVal != string.Empty)
                                    {
                                        DateTime convertedVal = DateTime.Parse(colVal);
                                        mapping.PropertyInfo.SetValue(item, convertedVal, null);
                                    }
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
                                    if (colVal != string.Empty)
                                    {
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

        /// <summary>
        /// Creates an up 'UPDATE' SQL command for the given Domain object type. This command is then cached in the UpdateQueriesByType
        /// static dictionary for faster future use.
        /// TODO: change to accept typeof(T) as parameter, instead of using a generic method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string CreateUpdateQuery<T>()
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

        /// <summary>
        /// Creates an up 'INSERT' SQL command for the given Domain object type. This command is then cached in the InsertQueriesByType
        /// static dictionary for faster future use.
        /// TODO: change to accept typeof(T) as parameter, instead of using a generic method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string CreateInsertQuery<T>()
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

        /// <summary>
        /// Helper method: given a SQLiteCommand and a Domain object, populate the command with the appropriate property values from the
        /// domain object. This is required to allow the command to successfully save the Domain object's state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputToSave"></param>
        /// <param name="command"></param>
        private static void SetParameterValues<T>(T inputToSave, SQLiteCommand command)
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

        #endregion

        #region Private Sub-Classes & Enums

        /// <summary>
        /// An enumerator of the different supported database types
        /// </summary>
        private enum DataType { String, Int, DateTime, TimeSpan, Double, Boolean, Float, Long, Colour }

        /// <summary>
        /// Describes a mapping between a SQLite database table field, and a Domain object's field PropertyInfo.
        /// </summary>
        private class SqliteDataMapping
        {
            public PropertyInfo PropertyInfo { get; set; }
            public string ColumnName { get; set; }
            public DataType PropertyDataType { get; set; }
            public bool PrimaryKey { get; set; }
        }

        #endregion

        protected SQLiteConnection connection = null;

        protected static bool isDatabaseReady = false;
        protected static object isDatabaseReadyLock = new object();

        /// <summary>
        /// Prepares a connection to the MediaScribe database.
        /// </summary>
        protected void PrepareConnection()
        {
            if (null == connection)
            {
                CreateDatabaseIfNotExists();
               

                connection = CreateOpenConnection();
            }
        }

        protected SQLiteConnection CreateOpenConnection()
        {
            SQLiteFactory factory = new SQLiteFactory();
            var result = factory.CreateConnection() as SQLiteConnection;
            var connString = ConfigurationManager.ConnectionStrings["MediaScribeDB"].ConnectionString.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            result.ConnectionString = connString;
            result.Open();
            return result;
        }

        protected void CreateDatabaseIfNotExists()
        {
            if (false == isDatabaseReady)
            {
                lock (isDatabaseReadyLock)
                {
                    //because of the lock, if we got into here /after/ a previous thread's finished, return.
                    if (isDatabaseReady)
                        return;

                    string databaseFilePath = string.Format(Constants.ApplicationGenericFilePath, DatabaseFileName);

                    if (false == File.Exists(databaseFilePath))
                    {
                        if (false == Directory.Exists(Constants.ApplicationFolderPath))
                            Directory.CreateDirectory(Constants.ApplicationFolderPath);

                        try
                        {
                            string initialDBScript = null;
                            Assembly _assembly = Assembly.GetExecutingAssembly();
                            using (var reader = new StreamReader(_assembly.GetManifestResourceStream(InitialDatabaseScriptFileName)))
                            {
                                initialDBScript = reader.ReadToEnd();
                            }

                            SQLiteConnection.CreateFile(databaseFilePath);

                            using (var tempConnection = CreateOpenConnection())
                            {
                                using (SQLiteCommand mycommand = new SQLiteCommand(tempConnection))
                                {

                                    mycommand.CommandText = initialDBScript;
                                    var affectedCount = mycommand.ExecuteNonQuery();
                                    if (affectedCount <= 0)
                                    {
                                        throw new Exception("database creation failed?");
                                    }
                                }
                            }
                        }
                        catch
                        {
                            //if there was an exception, ensure that the (empty) database is removed
                            if (File.Exists(databaseFilePath))
                                File.Delete(databaseFilePath);

                            throw;
                        }
                    }

                    //flag that the database is ready for use.
                    isDatabaseReady = true;
                }
            }
        }
    }
}
