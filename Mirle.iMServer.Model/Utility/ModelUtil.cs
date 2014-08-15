using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Mirle.iMServer.Model;
using System.Data.Common;
using Mirle.iMServer.Model.Db;

namespace Mirle.iMServer.Model
{
    public class ModelUtil
    {
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
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Project WHERE name LIKE @keyword");
                cmd.Connection = conn as MySqlConnection;
                foreach (string keyword in keywords)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@keyword", '%' + keyword + '%');
                    getProjectList(cmd, pList);
                }
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
    }
}
