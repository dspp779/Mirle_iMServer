﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class DeviceData : IEquatable<DeviceData>, INotifyPropertyChanged
    {
        public static DeviceData Empty = new DeviceData();

        public event PropertyChangedEventHandler PropertyChanged;

        protected long _id;
        protected string _alias;
        protected string _deviceName;
        protected string _addr;
        protected double _lat;
        protected double _lng;
        protected List<TagData> _tags;

        public Int64 id
        {
            get { return _id; }
        }
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
        public List<TagData> tags
        {
            get
            {
                if (_tags == null)
                {
                    reload();
                }
                return _tags;
            }
        }

        public string DisplayName
        {
            get { return ToString(); }
        }

        public DeviceData()
        {
            _lat = _lng = _id = 0;
            _alias = _deviceName = _addr = "";
            _tags = new List<TagData> { TagData.Empty };
        }

        public DeviceData(string deviceName)
        {
            _lat = _lng = _id = 0;
            _alias = _addr = "";
            _deviceName = deviceName;
        }

        public DeviceData(Int64 id, string alias, string deviceName, string addr, double lat, double lng)
        {
            this._id = id;
            this._alias = alias;
            this._deviceName = deviceName;
            this._addr = addr;
            this._lat = lat;
            this._lng = lng;
        }

        public void apply(DeviceData device)
        {
            set(device.id, device.alias, device.deviceName, device.addr, device.lat, device.lng);
        }

        public void apply(Int64 id, DeviceData device)
        {
            set(id, device.alias, device.deviceName, device.addr, device.lat, device.lng);
        }

        private void set(Int64 id, string alias, string deviceName, string addr, double lat, double lng)
        {
            this._id = id;
            this._alias = alias;
            this._deviceName = deviceName;
            this._addr = addr;
            this._lat = lat;
            this._lng = lng;
            NotifyValueChanged("DisplayName");
        }

        public bool Contains(string[] keywords)
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

        private static bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public override int GetHashCode()
        {
            return _deviceName.GetHashCode() ;
        }

        public override bool Equals(object obj)
        {
            return obj is DeviceData && Equals((DeviceData)obj);
        }

        public bool Equals(DeviceData d)
        {
            return d._deviceName == _deviceName;
        }

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

        public void reload()
        {
            _tags = ModelUtil.getTagList(this);
        }

        public void NotifyValueChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
