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
        #region -- get device list --
        public static List<DeviceData> getMapDeviceList()
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                //string cmdstr = "SELECT * FROM Device";
                string cmdstr = "SELECT * FROM device, "
                    + "(SELECT DISTINCT(device) FROM loginfo) as deviceinfo"
                    + " where deviceName = device";
                MySqlCommand cmd = new MySqlCommand(cmdstr);
                cmd.Connection = conn as MySqlConnection;
                getDeviceList(cmd, dList);
            }
            return dList;
        }

        public static List<DeviceData> getMapDeviceList(string keyword)
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Device WHERE alias LIKE @keyword");
                cmd.Parameters.AddWithValue("@keyword", '%' + keyword + '%');
                cmd.Connection = conn as MySqlConnection;
                getDeviceList(cmd, dList);
            }
            return dList;
        }

        public static List<DeviceData> getMapDeviceList(string[] keywords)
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                int i = 0;
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Device WHERE FALSE");
                cmd.Connection = conn as MySqlConnection;
                foreach (string keyword in keywords)
                {
                    cmd.CommandText += " OR alias LIKE @keyword" + i;
                    cmd.Parameters.AddWithValue("@keyword" + i, '%' + keyword + '%');
                    i++;
                }
                getDeviceList(cmd, dList);
            }
            return dList;
        }

        public static List<DeviceData> getDeviceList()
        {
            HashSet<DeviceData> deviceSet = new HashSet<DeviceData>(getRawDeviceList());
            HashSet<DeviceData> mapDeviceSet = new HashSet<DeviceData>(getMapDeviceList());
            //mapDeviceSet.IntersectWith(deviceSet);
            mapDeviceSet.UnionWith(deviceSet);
            return mapDeviceSet.ToList();
        }

        public static List<DeviceData> getRawDeviceList()
        {
            List<DeviceData> dList = new List<DeviceData>();
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(device) FROM loginfo");
                cmd.Connection = conn as MySqlConnection;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // get column values
                        dList.Add(new DeviceData(reader.GetString("device")));
                    }
                }
            }
            return dList;
        }

        private static void getDeviceList(MySqlCommand cmd, List<DeviceData> dList)
        {
            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // get column values
                        dList.Add(
                            new DeviceData(reader.GetInt64("id"), reader.GetString("alias"),
                                reader.GetString("deviceName"), reader.GetString("addr"),
                                reader.GetDouble("lat"), reader.GetDouble("lng"))
                        );
                    }
                }
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1146:
                        createDeviceTable();
                        break;
                    default:
                        break;
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
                cmd.Parameters.AddWithValue("@device", device.deviceName);
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

        #region -- device update methods --
        public static int insertDervice(DeviceData device)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            string insertCommand =
                @"INSERT INTO Device(alias,addr,lat,lng,deviceName)"
                + " VALUES (@alias,@addr,@lat,@lng,@deviceName);";

            MySqlCommand cmd = new MySqlCommand(insertCommand);
            cmd.Parameters.AddWithValue("@alias", device.alias);
            cmd.Parameters.AddWithValue("@addr", device.addr);
            cmd.Parameters.AddWithValue("@lat", device.lat);
            cmd.Parameters.AddWithValue("@lng", device.lng);
            cmd.Parameters.AddWithValue("@deviceName", device.deviceName);

            return db.execInsert(cmd);
        }

        public static int updateDervice(DeviceData device)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            string updateCommand = @"UPDATE Device SET alias=@alias,"
                + "addr=@addr,lat=@lat,lng=@lng,deviceName=@deviceName"
                + " WHERE id=@id;";

            MySqlCommand cmd = new MySqlCommand(updateCommand);
            cmd.Parameters.AddWithValue("@id", device.id);
            cmd.Parameters.AddWithValue("@alias", device.alias);
            cmd.Parameters.AddWithValue("@addr", device.addr);
            cmd.Parameters.AddWithValue("@lat", device.lat);
            cmd.Parameters.AddWithValue("@lng", device.lng);
            cmd.Parameters.AddWithValue("@deviceName", device.deviceName);

            return db.execUpdate(cmd);
        }
        #endregion

        #region -- create table --
        private static void createDeviceTable()
        {
            MySqlDbInterface db = new MySqlDbInterface();
            string cmd =
                "CREATE TABLE IF NOT EXISTS device"
                + "(id bigint(20) NOT NULL AUTO_INCREMENT PRIMARY KEY, "
                + "alias text, addr text, lat double, lng double, "
                + "deviceName varchar(64) NOT NULL) DEFAULT CHARSET=utf8";
            db.execUpdate(cmd);
        }
        #endregion
    }
}
