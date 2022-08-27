using dogpixels_viewer.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dogpixels_viewer
{
    public class SQLiteController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SqliteConnection sqliteContext;

        private string profileName;

        // stopwatch time measurement
        Stopwatch stopwatch;

        public SQLiteController(string profileName)
        {
            this.profileName = profileName;
            stopwatch = new Stopwatch();
        }

        private SqliteCommand constructQuery(ImageData knownData, string query = "*", string operand = "AND")
        {
            SqliteCommand ret = new SqliteCommand();
            ret.Connection = sqliteContext;

            //log.Debug($"[sql] constructing query from known data: {{id:{knownData.id}, md5:'{knownData.md5}', path:'{knownData.path}', tags:'{knownData.tags}', source:'{knownData.source}'}}");

            List<string> queryParts = new List<string>();

            if (knownData.id > 0)
            {
                queryParts.Add("`id`=$knownDataId");
                ret.Parameters.AddWithValue("$knownDataId", knownData.id);
            }
            if (knownData.path != null)
            {
                queryParts.Add("`path`=$knownDataPath");
                ret.Parameters.AddWithValue("$knownDataPath", knownData.path);
            }
            if (knownData.md5 != null)
            {
                queryParts.Add("`md5`=$knownDataMd5");
                ret.Parameters.AddWithValue("$knownDataMd5", knownData.md5);
            }
            if (knownData.tags != null)
            {
                queryParts.Add("`tags`=$knownDataTags");
                ret.Parameters.AddWithValue("$knownDataTags", knownData.tags);
            }
            if (knownData.source != null)
            {
                queryParts.Add("`source`=$knownDataSource");
                ret.Parameters.AddWithValue("$knownDataSource", knownData.source);
            }

            ret.CommandText = string.Concat
            (
                $"SELECT {query} FROM `files`" + (queryParts.Count == 0 ? "" : " WHERE "),
                string.Join($" {operand} ", queryParts),
                ";"
            );

            //log.Debug($"[sql] result (including {ret.Parameters.Count} parameters): {ret.CommandText}");

            return ret;
        }

        private bool run(string sql, List<SqliteParameter>? parameters = null)
        {
            parameters = parameters ?? new List<SqliteParameter>();

            //log.Debug($"[sql] running the following statement with {parameters.Count} parameters:\n{sql}");

            try
            {
                SqliteCommand cmd = new SqliteCommand(sql, sqliteContext);
                cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                log.Fatal($"[sql] Database command failed: {ex}");
            }

            return false;
        }

        public bool Initialize()
        {
            string path;

            // prepare database file path

            try
            {
                path = Path.Combine
                (
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "dogpixels",
                    "viewer",
                    "data",
                    profileName + ".sqlite3"
                );
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            catch (Exception ex)
            {
                log.Fatal($"Failed to create database file path or directory:\n{ex}");
                return false;
            }

            // open database file

            log.Info($"Establishing database file connection, profile '{profileName}'...");
            log.Debug($"path: {path}");
            try
            {
                sqliteContext = new SqliteConnection($"Data Source=\"{path}\";");
                sqliteContext.Open();
            }
            catch(Exception ex)
            {
                log.Fatal("Database file connection failed:");
                log.Fatal(ex);
                return false;
            }
            log.Info("Database file connection established");

            // initialize tables

            log.Info("Initializing database tables...");

            return run
            (
                string.Concat
                (
                    "CREATE TABLE IF NOT EXISTS `files` (",
                        "`id` INTEGER PRIMARY KEY AUTOINCREMENT,",
                        "`path` TEXT DEFAULT '',",
                        "`md5` VARCHAR(32) DEFAULT '',",
                        "`tags` TEXT DEFAULT '',",
                        "`source` TEXT DEFAULT ''",
                    ");"
                )
            );
        }

        internal void InitializeFTS5()
        {
            stopwatch.Restart();

            if (!run("CREATE VIRTUAL TABLE IF NOT EXISTS `fts` USING FTS5(`path`, `md5`, `tags`);"))
            {
                log.Error("Error creating virtual FTS5 table.");
                return;
            }

            if (!run("INSERT INTO `fts` SELECT `path`, `md5`, `tags` FROM `files`;"))
            {
                log.Error("Error copying data from database to virtual FTS5 table");
                return;
            }

            stopwatch.Stop();

            log.Info($"FTS5 virtual table initialized in {stopwatch.ElapsedMilliseconds}ms.");
        }

        internal void DeinitializeFTS5()
        {
            if (!run("DROP TABLE IF EXISTS `fts`;"))
            {
                log.Error("Error deinitializing virtual FTS5 table. It might have remained in database.");
            }
        }
        public bool Insert(string filePath, string md5)
        {
            int count = Count(new ImageData { path = filePath, md5 = md5 });
            if (count != 0) 
            {
                log.Debug($"[sql] Entry already present, skipping: {{md5: '{md5}', path: '{filePath}'}}");
                return false;
            }

            return run
            (
                @"INSERT INTO `files` (`path`, `md5`)
                VALUES ($filePath, $md5);",
                new List<SqliteParameter>
                {
                    new SqliteParameter("$filePath", filePath),
                    new SqliteParameter("$md5", md5)
                }
            );
        }

        public List<ImageData> Get(string searchQuery)
        {
            List<ImageData> ret = new List<ImageData>();

            SqliteCommand cmd = new SqliteCommand(@"SELECT * FROM `fts` WHERE `tags` MATCH $searchQuery;", sqliteContext);
            cmd.Parameters.Add(new SqliteParameter("$searchQuery", searchQuery));

            SqliteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ImageData row = new ImageData
                {
                    path = reader.GetString(0),
                    md5 = reader.GetString(1),
                    tags = reader.GetString(2)
                };
                ret.Add(row);

                //log.Debug($"[sql] retrieved {{id:{row.id}, md5:'{row.md5}', path:'{row.path}', tags:'{row.tags}', source:'{row.source}'}}");
            }

            return ret;
        }

        public List<ImageData> Get(ImageData knownData)
        {
            List<ImageData> ret = new List<ImageData>();

            SqliteDataReader reader = constructQuery(knownData).ExecuteReader();

            while(reader.Read())
            {
                ImageData row = new ImageData
                {
                    id = reader.GetInt32(0),
                    path = reader.GetString(1),
                    md5 = reader.GetString(2),
                    tags = reader.GetString(3),
                    source = reader.GetString(4)
                };
                ret.Add(row);

                //log.Debug($"[sql] retrieved {{id:{row.id}, md5:'{row.md5}', path:'{row.path}', tags:'{row.tags}', source:'{row.source}'}}");
            }

            return ret;
        }

        public int Count(ImageData knownData)
        {
            SqliteCommand cmd = constructQuery(knownData, "COUNT(*)");
            SqliteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
                return reader.GetInt32(0);

            return 0;
        }

        public bool UpdateTags(string md5, string newTags)
        {
            return run
            (
                @"UPDATE `files` 
                SET `tags`=$newTags 
                WHERE `md5`=$md5;",
                new List<SqliteParameter>
                {
                    new SqliteParameter("$newTags", newTags),
                    new SqliteParameter("$md5", md5)
                }
            );
        }
    }
}
