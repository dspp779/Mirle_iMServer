using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    public class DeviceData
    {
        public static DeviceData Empty = new DeviceData();

        private ProjectData _project;
        private string _deviceName;
        private List<TagData> _tags;

        public ProjectData project
        {
            get { return _project; }
        }

        public string name
        {
            get { return _deviceName; }
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

        public DeviceData()
        {
            _project = ProjectData.Empty;
            _deviceName = "EmptyDevice";
            _tags = new List<TagData>{TagData.Empty};
        }

        public DeviceData(ProjectData project, string deviceName)
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
