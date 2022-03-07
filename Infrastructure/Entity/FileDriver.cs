using med.common.library.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventAuthServer.Entity
{
    public class FileDriver: AuditableEntity
    {
        public string Title { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string BucketName { get; set; }
        public string UploadFrom { get; set; }
        public double ByteSize { get; set; }
        public int Download { get; set; }
    }
}
