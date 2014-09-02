using Mirle.iMServer.Model.Db;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mirle.iMServer.Model.Utility
{
    /* TrendDataManager is responsible for Tag monitoring value refreshing.
     * It maintains a Dictionary (or Hashtable) of trend history (trend table in database))
     * 
     * */
    public class TrendDataManager
    {
        private static int pollingRate;

        // 監測值資料快取
        private static Dictionary<string, Dictionary<string, double?>> instantValueDictionary
            = new Dictionary<string, Dictionary<string, double?>>();

        private static BackgroundWorker TagValRefreshWorker = new BackgroundWorker();

        private static DateTime _lastRefreshTime;
        public static DateTime lastRefreshTime
        {
            get { return _lastRefreshTime; }
        }

        // data needed for refresh worker
        private static List<TagData> tagsToRefresh;
        private static string trendTableName;
        private static Dictionary<string, double?> trendTable = null;

        // data lock for worker data
        private static object workerDataLock = new object();

        // initialize trend data manager
        static TrendDataManager()
        {
            // polling rate
            pollingRate = 1000;

            // initialize tag value refresh worker
            TagValRefreshWorker.WorkerSupportsCancellation = true;
            TagValRefreshWorker.DoWork += new DoWorkEventHandler(TagValRefreshWorker_DoWork);
        }

        public TrendDataManager(int pollingRate)
        {
            // polling rate
            TrendDataManager.pollingRate = pollingRate;
        }

        private static void TagValRefreshWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 監測資料定期更新
            while (!TagValRefreshWorker.CancellationPending)
            {
                string tableName;
                Dictionary<string, double?> trend;
                List<TagData> tags;

                lock (workerDataLock)
                {
                    tableName = trendTableName;
                    trend = trendTable;
                    tags = tagsToRefresh;
                }

                refreshRowVal(tableName, trend);
                foreach (TagData tag in tags)
                {
                    tag.NotifyValueChanged();
                }
                SpinWait.SpinUntil(() => false, pollingRate);
            }
        }

        public static void registerDeviceTagRefresh(List<TagData> tags)
        {
            if (tags.Count > 0)
            {
                string tableName = tags[0].Table;
                Dictionary<string, double?> trend = null;
                // 檢查監測資料是否已經讀取
                if (!instantValueDictionary.TryGetValue(tableName, out trend))
                {
                    // 未讀取，新增字典供監測資料讀取
                    trend = new Dictionary<string, double?>();
                    // 存放在trenTableManager
                    instantValueDictionary.Add(tableName, trend);
                }

                lock (workerDataLock)
                {
                    tagsToRefresh = tags;
                    trendTableName = tableName;
                    trendTable = trend;
                }
            }

            if (!TagValRefreshWorker.IsBusy)
            {
                TagValRefreshWorker.RunWorkerAsync();
            }
        }

        public static void cancelDeviceTagRefresh()
        {
            TagValRefreshWorker.CancelAsync();
        }

        public static double? getTagVal(TagData tag)
        {
            double? value = null;
            Dictionary<string, double?> trendTable = null;
            // 檢查監測資料是否已經讀取
            if (!instantValueDictionary.TryGetValue(tag.Table, out trendTable))
            {
                // 未讀取，新增字典供監測資料讀取
                trendTable = new Dictionary<string, double?>();
                // 監測資料讀取
                refreshRowVal(tag.Table, trendTable);
                // 存放在trenTableManager
                instantValueDictionary.Add(tag.Table, trendTable);
            }
            trendTable.TryGetValue(tag.log_id, out value);
            return value;
        }

        private static void refreshRowVal(string table, Dictionary<string, double?> trendTable)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                // get the newest row
                string cmdstr = string.Format("SELECT * FROM {0} ORDER BY datetime DESC LIMIT 1", table);
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
                            double? value = null;
                            try
                            {
                                value = reader.GetFloat(columnName);
                            }
                            catch (Exception)
                            {
                                value = null;
                            }
                            // add or update
                            trendTable[columnName] = value;
                        }
                        _lastRefreshTime = reader.GetDateTime("datetime");
                    }
                }
            }
        }

        private static void refreshAllCacheTagVal()
        {
            foreach (KeyValuePair<string, Dictionary<string, double?>> table in instantValueDictionary)
            {
                refreshRowVal(table.Key, table.Value);
            }
        }
    }
}
