using Amazon.S3.Model;
using med.common.api.library.model;
using med.common.library.model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.fileupload
{
    public interface IAWSFileUploader
    {
        Task<AWSFileDetail> UploadFile(FileUploadParameter parameter, string folder);
        Task<List<AWSFileDetail>> UploadFile(List<FileUploadParameter> parameters, string folder);
        Task<ListVersionsResponse> FilesList(string bucketName);
        Task<Stream> GetFile(string fileName, string bucketName);
        Task<List<S3Object>> GetFile(string bucketName);
        Task<FileDetail> UpdateFile(FileUploadParameter parameter, string folder, string fileName, string bucketName);
        Task<bool> DeleteFile(string fileName, string bucketName);
    }
    public class FileUploadParameter
    {
        public IFormFile File { get; set; }
        public Guid Id { get; set; }
    }
}
