using Mirle.iMServer.Model.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    /// <summary>
    /// 點位資料類別
    /// 此類別包含點位的詳細資料
    /// id: 點位在資料庫的識別碼
    /// table: 點位歷史監測資料所在的資料表
    /// name: 點位所屬的趨勢表名
    /// log_id: 點位在歷史資料表的欄位名稱
    /// log_name: 點位的別名
    /// tag: 點位的Tag名
    /// tag_memo: 點位備註
    /// io_addr: 點位的IO位址
    /// device: 點位所屬的Device
    /// </summary>
    public class TagData : AbstractData
    {
        public static TagData Empty = new TagData();

        private long _id;
        private string _table;
        private string _name;
        private string _log_id;
        private string _log_name;
        private string _tag;
        private string _tag_memo;
        private int _io_addr;
        private DeviceData _device;

        public long ID
        {
            get { return _id; }
        }
        public string Table
        {
            get { return _table; }
        }
        public string Name
        {
            get { return _name; }
        }
        public string log_id
        {
            get { return _log_id; }
        }
        public string LogName
        {
            get { return _log_name; }
        }
        public string TagName
        {
            get { return _tag; }
        }
        public string TagMemo
        {
            get { return _tag_memo; }
        }
        public int io_addr
        {
            get { return _io_addr; }
        }
        public DeviceData Device
        {
            get { return _device; }
        }

        // 點位所屬站位的名稱
        public string DeviceName
        {
            get { return _device.ToString(); }
        }
        // 點位包含站位的全名
        public string FullName
        {
            get
            {
                return this + @" @ " + DeviceName;
            }
        }
        // 點位目前監測值
        public string Value
        {
            get
            {
                return getTextVal();
            }
        }

        #region -- Constructors --
        public TagData()
        {
            _id = _io_addr = -1;
            _table = _name = _log_id = _log_name = _tag = _tag_memo = "";
            _device = DeviceData.Empty;
        }

        public TagData(long id, string table, string name, string log_id, string log_name,
            string tag, string tag_memo, int io_addr, DeviceData device)
        {
            this._id = id;
            this._table = table;
            this._name = name;
            this._log_id = log_id;
            this._log_name = log_name;
            this._tag = tag;
            this._tag_memo = tag_memo;
            this._io_addr = io_addr;
            this._device = device;
        }
        #endregion

        /* check if tag satisfy keyword rule
         * note: AND rule is used int this method
         * 點位搜尋的檢查方法
         * 注意！此方法比對點位是否包含所有關鍵字
         * */
        public override bool Contains(string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (!Contains(_log_name, keyword, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        // 改寫取得雜湊碼之方法
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        // 改寫相等判定方法
        public override bool Equals(object obj)
        {
            return obj is TagData && Equals((TagData)obj);
        }
        public override bool Equals(AbstractData data)
        {
            return data is TagData && Equals((TagData)data);
        }
        public bool Equals(TagData t)
        {
            return _id == t._id;
        }

        // 改寫大小比較方法
        public override int CompareTo(object obj)
        {
            return (obj is TagData) ? CompareTo((TagData)obj) : 0;
        }
        public int CompareTo(TagData data)
        {
            return _id.CompareTo(data._id);
        }

        // 改寫 ToString 方法
        public override string ToString()
        {
            return _log_name;
        }

        // 取得最新點位監測值
        private float? getNumericVal()
        {
            if (this.Equals(TagData.Empty))
                return null;
            float? f = (float?)TrendDataManager.getTagVal(this);
            return f != null ? f : null;
        }

        // 取得最新點位監測值，並以字串形式回傳
        private string getTextVal()
        {
            if (this.Equals(TagData.Empty))
                return "null";
            float? f = (float?)TrendDataManager.getTagVal(this);
            return f != null ? f.ToString() : "null";
        }

        // 通知內容變更方法
        public void NotifyValueChanged()
        {
            base.NotifyValueChanged("Value");
        }

    }
}
