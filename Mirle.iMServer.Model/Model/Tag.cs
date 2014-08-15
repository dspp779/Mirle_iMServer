using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class Tag
    {
        private long _id;
        private int _addr;
        private string _alias;
        public long id
        {
            get { return _id; }
        }
        public int addr
        {
            get { return _addr; }
        }
        public string alias
        {
            get { return _alias; }
        }
        public Tag(long id, string alias, int addr, string format, string unit, long plcid)
        {
            this._id = id;
            this._addr = addr;
            this._alias = alias;
        }

    }
}
