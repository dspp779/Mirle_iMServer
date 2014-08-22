using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class ProjectData
    {
        public static ProjectData Empty = new ProjectData();

        private long _id;
        private string _name;
        private string _alias;
        private string _addr;
        private double _lat;
        private double _lng;
        private List<DeviceData> _devices;

        public Int64 id
        {
            get { return _id; }
        }
        public string name
        {
            get { return _name; }
        }
        public string alias
        {
            get { return _alias; }
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
        public List<DeviceData> devices
        {
            get
            {
                if (_devices == null)
                {
                    reload();
                }
                return _devices;
            }
        }
        public List<TagData> tags
        {
            get
            {
                List<TagData> tagList = new List<TagData>();
                foreach(DeviceData device in devices)
                {
                    tagList.AddRange(device.tags);
                }
                return tagList;
            }
        }

        public ProjectData()
        {
            _lat = _lng = _id = 0;
            _name = _alias = _addr = "EmptyProject";
            _devices = new List<DeviceData> { DeviceData.Empty };
        }

        public ProjectData(Int64 id, string name, string alias, string addr, double lat, double lng)
        {
            this._id = id;
            this._name = name;
            this._alias = alias;
            this._addr = addr;
            this._lat = lat;
            this._lng = lng;
        }

        public bool containsKeyword(string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (name.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return name;
        }

        public void reload()
        {
            _devices = ModelUtil.getDeviceList(this);
        }
    }
}
