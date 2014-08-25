using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Mirle.iMServer.Model;
using System.Data.Common;
using Mirle.iMServer.Model.Db;
using System.Data;

namespace Mirle.iMServer.Model
{
    public class ModelUtil
    {
        // 監測值資料快取
        private static Dictionary<string, Dictionary<string, float?>> trendTableManager
            = new Dictionary<string, Dictionary<string, float?>>();

        #region -- get project list --
        public static List<ProjectData> getProjectList()
        {
            List<ProjectData> pList = new List<ProjectData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Project");
                cmd.Connection = conn as MySqlConnection;
                getProjectList(cmd, pList);
            }
            return pList;
        }

        public static List<ProjectData> getProjectList(string keyword)
        {
            List<ProjectData> pList = new List<ProjectData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Project WHERE name LIKE @keyword");
                cmd.Parameters.AddWithValue("@keyword", '%' + keyword + '%');
                cmd.Connection = conn as MySqlConnection;
                getProjectList(cmd, pList);
            }
            return pList;
        }

        public static List<ProjectData> getProjectList(string[] keywords)
        {
            List<ProjectData> pList = new List<ProjectData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                int i = 0;
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Project WHERE FALSE");
                cmd.Connection = conn as MySqlConnection;
                foreach (string keyword in keywords)
                {
                    cmd.CommandText += " OR name LIKE @keyword" + i;
                    cmd.Parameters.AddWithValue("@keyword" + i, '%' + keyword + '%');
                    i++;
                }
                getProjectList(cmd, pList);
            }
            return pList;
        }

        public static void getProjectList(MySqlCommand cmd, List<ProjectData> pList)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get column values
                    pList.Add(
                        new ProjectData(reader.GetInt64("id"), reader.GetString("name"),
                            reader.GetString("alias"), reader.GetString("addr"),
                            reader.GetDouble("lat"), reader.GetDouble("lng"))
                    );
                }
            }
        }
        #endregion

        #region -- get device list --
        public static List<DeviceData> getDeviceList()
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(device) FROM loginfo");
                cmd.Connection = conn as MySqlConnection;
                getDeviceList(cmd, ProjectData.Empty, dList);
            }
            return dList;
        }
        public static List<DeviceData> getDeviceList(ProjectData project)
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM ProjectDevice WHERE log_name = @log_name");
                cmd.Parameters.AddWithValue("@log_name", project.alias);
                cmd.Connection = conn as MySqlConnection;
                getDeviceList(cmd, project, dList);
            }
            return dList;
        }
        public static void getDeviceList(MySqlCommand cmd, ProjectData project, List<DeviceData> dList)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get column values
                    DeviceData d = new DeviceData(project, reader.GetString("device"));
                    dList.Add(d);
                }
            }
        }
        #endregion

        #region -- get tag list --

        public static List<TagData> getTagList(DeviceData device)
        {
            List<TagData> tList = new List<TagData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM loginfo WHERE device = @device");
                cmd.Parameters.AddWithValue("@device", device.name);
                cmd.Connection = conn as MySqlConnection;
                getTagList(cmd, device, tList);
            }
            return tList;
        }
        public static void getTagList(MySqlCommand cmd, DeviceData device, List<TagData> dList)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get column values
                    dList.Add(
                        new TagData(reader.GetInt64("id"), reader.GetString("table"), reader.GetString("name"),
                            reader.GetString("logid"), reader.GetString("log"), reader.GetString("tag"),
                            reader.GetString("tag_memo"), reader.GetInt32("io_addr"), device)
                    );
                }
            }
        }
        #endregion

        public static float? getTagVal(TagData tag)
        {
            float? value = null;
            Dictionary<string, float?> trendTable = null;
            // 檢查監測資料是否已經讀取
            if (!trendTableManager.TryGetValue(tag.Table, out trendTable))
            {
                // 未讀取，新增字典供監測資料讀取
                trendTable = new Dictionary<string, float?>();
                // 監測資料讀取
                refreshRowVal(tag.Table, trendTable);
                // 存放在trenTableManager
                trendTableManager.Add(tag.Table, trendTable);
            }
            trendTable.TryGetValue(tag.log_id, out value);
            return value;
        }

        private static void refreshRowVal(string table, Dictionary<string, float?> trendTable)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                // get the newest row
                string cmdstr = string.Format("SELECT * FROM {0} ORDER BY 'datetime' LIMIT 1", table);
                MySqlCommand cmd = new MySqlCommand(cmdstr);
                //cmd.Parameters.AddWithValue("@table", tag.table);
                cmd.Connection = conn as MySqlConnection;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    DataTable schemaTable = reader.GetSchemaTable();
                    if (reader.Read())
                    {
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            string columnName = row[schemaTable.Columns[0]].ToString();
                            float? value = null;
                            try
                            {
                                value = reader.GetFloat(columnName);
                            }
                            catch (Exception) { };
                            // add or update
                            if (trendTable.ContainsKey(columnName))
                            {
                                trendTable[columnName] = value;
                            }
                            else
                            {
                                trendTable.Add(columnName, value);
                            }
                        }
                    }
                }
            }
        }

        private static void refreshAllCacheVal()
        {
            foreach (KeyValuePair<string, Dictionary<string, float?>> table in trendTableManager)
            {
                refreshRowVal(table.Key, table.Value);
            }
        }
    }
}
