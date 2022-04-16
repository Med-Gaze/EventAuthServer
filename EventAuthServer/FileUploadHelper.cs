using med.common.api.library.fileupload;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Amazon.S3;
using System.Threading.Tasks;
using med.common.api.library.model;
using EventAuthServer.Entity;

namespace EventAuthServer
{
    public static class FileUploadHelper
    {
        public static async Task<FileDriver> UploadFile(IFormFile file, string title, string uploadFrom, IAWSFileUploader _aWSFileUploader)
        {
            var generatedId = Guid.NewGuid();
            var entityFile = new FileDriver
            {
                Title = title,
                UploadFrom = uploadFrom,
                ByteSize = file.Length,
                Extension = Path.GetExtension(file.FileName),
            };
            var parameter = new FileUploadParameter
            {
                Id = generatedId,
                File = file
            };
            try
            {
                var awsFileDetail = await _aWSFileUploader.UploadFile(parameter, uploadFrom);
                entityFile.Path = awsFileDetail.FilePath;
                entityFile.FileName = awsFileDetail.FileName;
                entityFile.BucketName = awsFileDetail.BucketName;
            }
            catch (AmazonS3Exception)
            {
                var directoryFileDetail = await FileDirectoryUploadHelper.UploadFile(parameter, uploadFrom);
                entityFile.Path = directoryFileDetail.FilePath;
                entityFile.FileName = directoryFileDetail.FileName;
            }
            return entityFile;
        }

        public static async Task<FileDetail> UpdateFile(IFormFile file, IAWSFileUploader AWSFileUploader, string bucket, string path, string fileName, string uploadFrom)
        {

            var generatedId = Guid.NewGuid();
            var parameter = new FileUploadParameter
            {
                Id = generatedId,
                File = file
            };
            try
            {
                if (!string.IsNullOrEmpty(bucket))
                    return await AWSFileUploader.UpdateFile(parameter, uploadFrom, fileName, bucket);
                else
                    return await FileDirectoryUploadHelper.UpdateFile(path, fileName, parameter, uploadFrom);

            }
            catch (AmazonS3Exception)
            {
                return await FileDirectoryUploadHelper.UpdateFile(path, fileName, parameter, uploadFrom);
            }
            catch
            {
                throw;
            }
        }

    }
}
