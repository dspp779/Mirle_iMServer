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
            try
            {
                List<ProjectData> pList = new List<ProjectData>();
                MySqlDbInterface db = new MySqlDbInterface();
                using (DbConnection conn = db.getConnection())
                {
                    conn.Open();
                    DbCommand cmd = new MySqlCommand("SELECT * FROM Project");
                    cmd.Connection = conn;
                    using (MySqlDataReader reader = cmd.ExecuteReader() as MySqlDataReader)
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
                return pList;
            }
            catch (Exception)
            {
                return new List<ProjectData>();
            }
        }
    }
}
