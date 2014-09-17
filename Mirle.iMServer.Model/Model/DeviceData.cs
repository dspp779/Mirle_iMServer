using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    /// <summary>
    /// 站位資料類別
    /// 此類別包含站位的詳細資訊：
    /// id: 獨一無二識別ID，已設定資訊之站位才有
    /// alias" 站位別名，方便識讀的站位名稱(可為中文)
    /// deviceNam" 站位名稱(不可為中文)
    /// addr: 站位所在地址
    /// lat: 站位的地理緯度
    /// lng: 站位的地理經度
    /// tags: 站位的點位
    /// </summary>
    public class DeviceData : AbstractData
    {
        public static DeviceData Empty = new DeviceData();

        protected string _alias;
        protected string _deviceName;
        protected string _addr;
        protected double _lat;
        protected double _lng;
        protected Dictionary<string, TagData> _tags;

        public string alias
        {
            get { return _alias; }
        }
        public string deviceName
        {
            get { return _deviceName; }
        }
        public string addr
        {
            get { return _addr; }
        }
        public double lat
        {
            get { return _lat; }
        }
        public double lng
        {
            get { return _lng; }
        }
        public List<TagData> TagList
        {
            get
            {
                return Tags.Values.ToList();
            }
        }
        public Dictionary<string, TagData> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = new Dictionary<string, TagData>();
                    reload();
                }
                return _tags;
            }
        }

        // 站位的顯示名稱
        public string DisplayName
        {
            get { return ToString(); }
        }

        #region -- Constructors --
        public DeviceData()
        {
            _lat = _lng = 0;
            _alias = _deviceName = _addr = "";
            Tags[TagData.Empty.log_id] = TagData.Empty;
        }

        public DeviceData(string deviceName)
        {
            _lat = _lng = 0;
            _alias = _addr = "";
            _deviceName = deviceName;
        }

        public DeviceData(string alias, string deviceName, string addr, double lat, double lng)
        {
            this._alias = alias;
            this._deviceName = deviceName;
            this._addr = addr;
            this._lat = lat;
            this._lng = lng;
        }
        #endregion

        #region -- 站位的資料設定 --
        public void apply(DeviceData device)
        {
            set(device.alias, device.deviceName, device.addr, device.lat, device.lng);
        }

        public void apply(Int64 id, DeviceData device)
        {
            set(device.alias, device.deviceName, device.addr, device.lat, device.lng);
        }

        private void set(string alias, string deviceName, string addr, double lat, double lng)
        {
            this._alias = alias;
            this._deviceName = deviceName;
            this._addr = addr;
            this._lat = lat;
            this._lng = lng;
            NotifyValueChanged("DisplayName");
        }
        #endregion

        /* 改寫搜尋包含關鍵字之方法
         * 此方法比對任一包含之關鍵字
         * */
        public override bool Contains(string[] keywords)
        {
            string str = String.Concat(alias, deviceName);
            foreach (string keyword in keywords)
            {
                // comparison ignore case
                if (Contains(str, keyword, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        
        // 改寫取得雜湊碼之方法
        public override int GetHashCode()
        {
            return _deviceName.GetHashCode();
        }

        // 改寫相等判定方法
        public override bool Equals(object obj)
        {
            return obj is DeviceData && Equals((DeviceData)obj);
        }
        public override bool Equals(AbstractData data)
        {
            return data is DeviceData && Equals((DeviceData)data);
        }
        public bool Equals(DeviceData d)
        {
            return d._deviceName == _deviceName;
        }

        // 改寫大小比較方法
        public override int CompareTo(object obj)
        {
            return (obj is DeviceData) ? CompareTo((DeviceData)obj) : 0;
        }
        public int CompareTo(DeviceData data)
        {
            return _deviceName.CompareTo(data._deviceName);
        }

        // 改寫 ToString 方法
        public override string ToString()
        {
            if (alias != null && alias.Length > 0)
            {
                return alias;
            }
            else
            {
                return deviceName;
            }
        }

        // 站位之點位資料更新方法
        public void reload()
        {
            foreach (TagData tag in ModelUtil.getTagList(this))
            {
                Tags[tag.log_id] = tag;
            }
        }

    }
}
