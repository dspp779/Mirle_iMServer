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

        public long id
        {
            get { return _id; }
        }
        public string table
        {
            get { return _table; }
        }
        public string name
        {
            get { return _name; }
        }
        public string log_id
        {
            get { return _log_id; }
        }
        public string log_name
        {
            get { return _log_name; }
        }
        public string tag
        {
            get { return _tag; }
        }
        public string tag_memo
        {
            get { return _tag_memo; }
        }
        public int io_addr
        {
            get { return _io_addr; }
        }
        public Device device
        {
            get { return _device; }
        }
        public string Value
        {
            get {
                int? i = ModelUtil.getVal(this);
                return i!=null ? i.ToString():"null";
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

    }
}
