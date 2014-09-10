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
     * */
    public class TrendDataManager
    {
        private static int pollingRate;

        // 監測值資料快取
        private static Dictionary<string, Dictionary<string, double?>> instantValueDictionary
            = new Dictionary<string, Dictionary<string, double?>>();

        // 監測值更新背景工作執行序
        private static BackgroundWorker TagValRefreshWorker = new BackgroundWorker();

        // 監測值最後更新時間
        private static DateTime _lastRefreshTime;
        public static DateTime lastRefreshTime
        {
            get { return _lastRefreshTime; }
        }

        // 需更新資料之tag
        private static List<TagData> tagsToRefresh;
        // 目前的趨勢資料表名稱
        private static string trendTableName;
        // 目前趨勢資料之查詢Dictionary
        private static Dictionary<string, double?> trendTable = null;

        // 資料更新資訊同步的資料鎖
        private static object workerDataLock = new object();

        // initialize trend data manager
        static TrendDataManager()
        {
            // polling rate
            pollingRate = 1000;

            // 初始化監測值更新背景工作執行序
            TagValRefreshWorker.WorkerSupportsCancellation = true;
            TagValRefreshWorker.DoWork += new DoWorkEventHandler(TagValRefreshWorker_DoWork);
        }

        public TrendDataManager(int pollingRate)
        {
            // polling rate
            TrendDataManager.pollingRate = pollingRate;
        }

        // 監測值更新背景工作
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

        /// <summary>
        /// refresh the newest values of specific trend table
        /// </summary>
        /// <param name="table">trend table to refresh</param>
        /// <param name="trendTable">the newest value of the trend table</param>
        private static void refreshRowVal(string table, Dictionary<string, double?> trendTable)
        {
            MySqlDbInterface db = new MySqlDbInterface();
            using (DbConnection conn = db.getConnection())
            {
                conn.Open();
                // get the newest row in the trend table
                string cmdstr = string.Format("SELECT * FROM {0} ORDER BY datetime DESC LIMIT 1", table);
                MySqlCommand cmd = new MySqlCommand(cmdstr);
                cmd.Connection = conn as MySqlConnection;

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    /* get schema table of the trend table
                     * so that we can get schema information
                     * */
                    DataTable schemaTable = reader.GetSchemaTable();

                    // read the first row of trend table
                    if (reader.Read())
                    {
                        /* traverse all columns of trend table by 
                         * traversing the rows of schema table
                         * */
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            // column name
                            string columnName = row[schemaTable.Columns[0]].ToString();
                            double? value = null;
                            // try get float value
                            try
                            {
                                value = reader.GetFloat(columnName);
                            }
                            catch (Exception)
                            {
                                value = null;
                            }
                            // add or update the trend table dictionary
                            trendTable[columnName] = value;
                        }
                        // refresh last refresh time
                        _lastRefreshTime = reader.GetDateTime("datetime");
                    }
                }
            }
        }

    }
}
