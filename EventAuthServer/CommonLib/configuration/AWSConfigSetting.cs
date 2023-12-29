using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.configuration
{
    public class AWSConfigSetting
    {
        public string Bucket { get; set; }
        public string Folder { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string RegionPoint { get; set; }
    }
}
