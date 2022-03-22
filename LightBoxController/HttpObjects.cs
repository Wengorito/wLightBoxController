using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxController
{
    public class HttpObjects
    {
        public class Device
        {
            public string deviceName { get; set; }
            public string product { get; set; }
            public string apiLevel { get; set; }
        }
        public class RootDevice
        {
            public Device device { get; set; }
        }
        public class DurationsMs
        {
            public int colorFade { get; set; }
        }
        public class Rgbw
        {
            public int effectID { get; set; }
            public string desiredColor { get; set; }
            public string currentColor { get; set; }
            public string lastOnColor { get; set; }
            public DurationsMs durationsMs { get; set; }
        }
        public class RootDeviceStateGet
        {
            public Rgbw rgbw { get; set; }
        }
        public class RootDeviceStateSet
        {
            public Rgbw rgbw { get; set; }
        }
    }
}
