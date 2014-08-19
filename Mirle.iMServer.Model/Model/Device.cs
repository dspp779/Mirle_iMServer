using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class Device
    {
        private ProjectData _project;
        private string _deviceName;
        private List<Tag> _tags;

        public ProjectData project
        {
            get { return _project; }
        }

        public string name
        {
            get { return _deviceName; }
        }

        public List<Tag> tags
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

        public Device(ProjectData project, string deviceName)
        {
            this._project = project;
            this._deviceName = deviceName;
        }

        public override string ToString()
        {
            return _deviceName;
        }

        public void reload()
        {
            _tags = ModelUtil.getTagList(this);
        }

    }
}
