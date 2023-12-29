using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.model
{
    public class AWSFileDetail
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string BucketName { get; set; }
    }
    public class FileDetail
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
