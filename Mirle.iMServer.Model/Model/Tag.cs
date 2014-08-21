using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class Tag
    {
        private long _id;
        private string _table;
        private string _name;
        private string _log_id;
        private string _log_name;
        private string _tag;
        private string _tag_memo;
        private int _io_addr;
        private Device _device;

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
        public Device Device
        {
            get { return _device; }
        }
        public string DeviceName
        {
            get { return _device.name; }
        }
        public string Value
        {
            get
            {
                float? f = ModelUtil.getVal(this);
                return f != null ? f.ToString() : "null";
            }
        }

        public Tag(long id, string table, string name, string log_id, string log_name,
            string tag, string tag_memo, int io_addr, Device device)
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

        public bool containsKeyword(string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (!_log_name.Contains(keyword))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return _log_name;
        }
    }
}
