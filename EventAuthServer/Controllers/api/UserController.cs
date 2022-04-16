using EventAuthServer.Entity;
using med.common.api.library.fileupload;
using med.common.library.constant;
using med.common.library.controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace EventAuthServer.Controllers.api
{
    [Authorize(LocalApi.PolicyName)]
    public class UserController : BaseController
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAWSFileUploader _aWSFileUploader;
        public UserController(
             IAWSFileUploader aWSFileUploader, AppDbContext appDbContext
            )
        {
            _aWSFileUploader = aWSFileUploader;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadProfile([FromForm] IFormFile profileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entity = _appDbContext.Users.FirstOrDefault(x => x.Id == userId);
            var fileDetail = (from user in _appDbContext.Users
                              join fileDriverTemp in _appDbContext.FileDriver on user.FileId equals fileDriverTemp.Id into Temp
                              from fileDriver in Temp.DefaultIfEmpty()
                              select new
                              {
                                  user.FileId,
                                  FilePath = fileDriver.Path,
                                  fileDriver.BucketName,
                                  fileDriver.FileName
                              }).Where(x => !x.FileId.Equals(null) && x.FileId == entity.FileId).FirstOrDefault();
            try
            {
                if (entity.FileId.HasValue)
                {
                    var entityFile = await FileUploadHelper.UpdateFile(profileImage, _aWSFileUploader, fileDetail.BucketName, fileDetail.FilePath, fileDetail.FileName, "User");
                    return Ok();
                }
                else
                {
                    var entityFile = await FileUploadHelper.UploadFile(profileImage, "Logo", "User", _aWSFileUploader);
                    var fileDriverData = _appDbContext.FileDriver.Add(entityFile);
                    await _appDbContext.SaveChangesAsync();
                    return Ok();
                }
            }
            catch
            {
                if (!string.IsNullOrEmpty(fileDetail.FilePath))
                {
                    if (!string.IsNullOrEmpty(fileDetail.BucketName))
                        await _aWSFileUploader.DeleteFile(fileDetail.FileName, fileDetail.BucketName);
                    else
                        FileDirectoryUploadHelper.DeleteFile(fileDetail.FilePath);
                }
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUploadProfile(Guid? fileId)
        {
            string base64 = string.Empty;
            var fileDetail = (from user in _appDbContext.Users
                              join fileDriverTemp in _appDbContext.FileDriver on user.FileId equals fileDriverTemp.Id into Temp
                              from fileDriver in Temp.DefaultIfEmpty()
                              select new
                              {
                                  user.FileId,
                                  FilePath = fileDriver.Path,
                                  fileDriver.BucketName,
                                  fileDriver.FileName
                              }).Where(x => !x.FileId.Equals(null) && x.FileId == fileId).FirstOrDefault();

            if (fileDetail != null)
            {
                var stream = await _aWSFileUploader.GetFile(fileDetail.FileName, fileDetail.BucketName);
                byte[] bytes;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
                base64 = Convert.ToBase64String(bytes);
            }

            return Ok(base64);

        }
    }
}
