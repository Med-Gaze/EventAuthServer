using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using med.common.api.library.configuration;
using med.common.api.library.model;
using med.common.library.model;
using med.common.library.security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.fileupload
{
    public class AWSFileUploader : IAWSFileUploader
    {
        private readonly IConfiguration configuration;
        private readonly ICryptography cryptography;
        private readonly AWSConfigSetting awsConfig;
        public AWSFileUploader(IConfiguration configuration, ICryptography cryptography)
        {
            this.configuration = configuration;
            this.cryptography = cryptography;
            awsConfig = this.configuration.GetSection("AWSConfig:Storage").Get<AWSConfigSetting>();
        }
        public async Task<AWSFileDetail> UploadFile(FileUploadParameter parameter, string folder)
        {

            var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
            var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);
            var folderPath = (!string.IsNullOrWhiteSpace(folder)
                    ? awsConfig.Bucket + @"/" + awsConfig.Folder + @"/" + folder
                    : awsConfig.Bucket + @"/" + awsConfig.Folder) ?? awsConfig.Bucket;

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint),
            };
            using var client = new AmazonS3Client(credentials, config);
            await using var newMemoryStream = new MemoryStream();

            await parameter.File.CopyToAsync(newMemoryStream);
            var fileExtension = Path.GetExtension(parameter.File.FileName);
            var documentName = $"{parameter.Id}{fileExtension}";
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = documentName,
                BucketName = folderPath,
                CannedACL = S3CannedACL.PublicRead
            };
            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);
            return await Task.FromResult(new AWSFileDetail
            {
                BucketName = folderPath,
                FilePath = $"https://{awsConfig.Bucket}.s3.{awsConfig.RegionPoint}.amazonaws.com/{awsConfig.Folder}/{folder}/{documentName}",
                FileName = documentName
            });

        }
        public async Task<ListVersionsResponse> FilesList(string bucketName)
        {
            try
            {
                var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
                var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint)
                };
                using var client = new AmazonS3Client(credentials, config);
                var fileTransferUtility = new TransferUtility(client);
                var objectResponse = await fileTransferUtility.S3Client.ListVersionsAsync(bucketName);

                return objectResponse;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        public async Task<Stream> GetFile(string fileName, string bucketName)
        {
            try
            {
                var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
                var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint)
                };
                using var client = new AmazonS3Client(credentials, config);
                var fileTransferUtility = new TransferUtility(client);

                var objectResponse = await fileTransferUtility.S3Client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileName
                });

                return objectResponse.ResponseStream;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        public async Task<List<S3Object>> GetFile( string bucketName)
        {
            try
            {
                var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
                var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint)
                };
                using var client = new AmazonS3Client(credentials, config);

                var fileTransferUtility = new TransferUtility(client);

                var objectResponse = await fileTransferUtility.S3Client.ListObjectsAsync(new ListObjectsRequest()
                {
                    BucketName = "medical-3391201",
                    Prefix = "event-ms/media"
                });

                return objectResponse.S3Objects;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        public async Task<bool> DeleteFile(string fileName, string bucketName)
        {
            try
            {
                var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
                var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint)
                };
                using var client = new AmazonS3Client(credentials, config);
                var fileTransferUtility = new TransferUtility(client);
                var objectResponse = await fileTransferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileName
                });

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        public async Task<FileDetail> UpdateFile(FileUploadParameter parameter, string folder, string fileName, string bucketName)
        {
            try
            {
                var accessKey = this.cryptography.Decrypt(awsConfig.AccessKey);
                var secretKey = this.cryptography.Decrypt(awsConfig.SecretKey);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig.RegionPoint)
                };
                var requestedExtension = Path.GetExtension(parameter.File.FileName);
                fileName = fileName.Split(".").FirstOrDefault();
                fileName = string.Concat(fileName, requestedExtension);
                using var client = new AmazonS3Client(credentials, config);
                await using var newMemoryStream = new MemoryStream();
                await parameter.File.CopyToAsync(newMemoryStream);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = fileName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };
                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);
                return await Task.FromResult(new FileDetail
                {
                    FileName = fileName,
                    FilePath = $"https://{awsConfig.Bucket}.s3.{awsConfig.RegionPoint}.amazonaws.com/{awsConfig.Folder}/{folder}/{fileName}"
                });
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }

        public Task<List<AWSFileDetail>> UploadFile(List<FileUploadParameter> parameters, string folder)
        {

            throw new NotImplementedException();
        }
    }
}
