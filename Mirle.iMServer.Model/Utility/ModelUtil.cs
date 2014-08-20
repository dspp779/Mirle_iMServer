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
        public static List<Device> getDeviceList(ProjectData project)
        {
            List<Device> dList = new List<Device>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM ProjectDevice WHERE project_id = @project_id");
                cmd.Parameters.AddWithValue("@project_id", project.id);
                cmd.Connection = conn as MySqlConnection;
                getDeviceList(cmd, project, dList);
            }
            return dList;
        }
        public static void getDeviceList(MySqlCommand cmd, ProjectData project, List<Device> dList)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get column values
                    Device d = new Device(project, reader.GetString("device"));
                    dList.Add(d);
                }
            }
        }
        #endregion

        #region -- get tag list --

        public static List<Tag> getTagList(Device device)
        {
            List<Tag> tList = new List<Tag>();
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
        public static void getTagList(MySqlCommand cmd, Device device, List<Tag> dList)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // get column values
                    dList.Add(
                        new Tag(reader.GetInt64("id"), reader.GetString("table"), reader.GetString("name"),
                            reader.GetString("logid"), reader.GetString("log"), reader.GetString("tag"),
                            reader.GetString("tag_memo"), reader.GetInt32("io_addr"), device)
                    );
                }
            }
        }
        #endregion

        public static float? getVal(Tag tag)
        {
            float? value = null;
            Dictionary<string, float?> trendTable = null;
            if (!trendTableManager.TryGetValue(tag.table, out trendTable))
            {
                trendTable = new Dictionary<string, float?>();
                getRowVal(tag.table, trendTable);
                trendTableManager.Add(tag.table, trendTable);
            }
            trendTable.TryGetValue(tag.log_id, out value);
            return value;
        }

        private static void getRowVal(string table, Dictionary<string, float?> trendTable)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                string cmdstr = string.Format("SELECT * FROM {0} LIMIT 1", table);
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
                            try
                            {
                                trendTable.Add(columnName, reader.GetFloat(columnName));
                            }
                            catch (Exception) { };
                        }
                    }
                }
            }
        }
    }
}
