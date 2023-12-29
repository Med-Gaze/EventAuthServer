using med.common.api.library.model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.fileupload
{
    public static class FileDirectoryUploadHelper
    {
        public async static Task<FileDetail> UploadFile(FileUploadParameter parameter, string folder)
        {
            try
            {
                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), $"Upload\\files\\{folder}");
                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }
                var fileExtension = Path.GetExtension(parameter.File.FileName);
                var documentName = $"{parameter.Id}{fileExtension}";
                var path = Path.Combine(pathBuilt, documentName);
                if (parameter.File.Length > 0)
                {
                    using var stream = new FileStream(path, FileMode.Create);
                    await parameter.File.CopyToAsync(stream);
                    return await Task.FromResult(new FileDetail
                    {
                        FilePath = path,
                        FileName = documentName
                    });
                }
                else
                {
                    throw new Exception("No File");
                }
            }
            catch
            {
                throw;
            }
        }
        public static bool DeleteFile(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
                return true;

            }
            catch
            {
                throw;
            }
        }

        public static async Task<FileDetail> UpdateFile(string originalPath, string fileName, FileUploadParameter parameter, string folder)
        {
            try
            {
                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), $"Upload\\files\\{folder}");
                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }
                var path = Path.Combine(pathBuilt, fileName);
                if (parameter.File.Length > 0)
                {
                    using var stream = new FileStream(path, FileMode.Create);
                    await parameter.File.CopyToAsync(stream);
                }
               
                return await Task.FromResult(new FileDetail
                {
                    FilePath = path,
                    FileName = fileName
                });
            }
            catch
            {
                throw;
            }
        }
    }
}
