using EventAuthServer.Datum.Enum;
using EventAuthServer.Domains.ViewModels.Account;
using EventAuthServer.Domains.ViewModels.Identity;
using EventAuthServer.Entity;
using med.common.api.library.fileupload;
using med.common.library.configuration.service;
using med.common.library.constant;
using med.common.library.controller;
using med.common.library.Enum;
using med.common.library.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public UserController(
             IAWSFileUploader aWSFileUploader, AppDbContext appDbContext, UserManager<AppUserModel> userManager,
            SignInManager<AppUserModel> signInManager, IConfiguration configuration, IEmailService emailService
            )
        {
            _aWSFileUploader = aWSFileUploader;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost]
        [Authorize]
        [Route("UploadProfile")]
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
        [Route("GetUploadProfile")]
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
        [HttpGet]
        [Route("GetUser")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var joinResult = await (from user in _appDbContext.Users
                                    join userRole in _appDbContext.UserRoles
                                    on user.Id equals userRole.UserId
                                    join role in _appDbContext.Roles
                                    on userRole.RoleId equals role.Id
                                    select new
                                    {
                                        user,
                                        role
                                    }
                             ).ToListAsync();

            if (joinResult == null) return NotFound();

            var users = joinResult.Select(x => new GetEmployeeUserDetailResponse
            {
                Id = x.user.Id,
                Email = x.user.Email,
                UserName = x.user.UserName,
                FullName = x.user.FullName,
                PhoneNumber = x.user.PhoneNumber,
                RoleId = x.role.Id,
                RoleName = x.role.Name
            }).ToList();

            return Ok(users);
        }
        [HttpGet]
        [Route("GetUser/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string userId)
        {
            var joinResult = await (from user in _appDbContext.Users
                                    join userRole in _appDbContext.UserRoles
                                    on user.Id equals userRole.UserId
                                    join role in _appDbContext.Roles
                                    on userRole.RoleId equals role.Id
                                    where user.Id == userId
                                    select new
                                    {
                                        user,
                                        role
                                    }
                           ).ToListAsync();

            if (joinResult == null) return NotFound();

            var users = joinResult.Select(x => new GetEmployeeUserDetailResponse
            {
                Id = x.user.Id,
                Email = x.user.Email,
                UserName = x.user.UserName,
                FullName = x.user.FullName,
                PhoneNumber = x.user.PhoneNumber,
                RoleId = x.role.Id,
                RoleName = x.role.Name
            }).FirstOrDefault();

            return Ok(users);
        }

        [HttpPost]
        [Route("CreateEmployee")]
        [Authorize]
        public async Task<IActionResult> CreateEmployee([FromBody] RegisterEmployeeViewModel model)
        {
            bool usernameExists = await _userManager.FindByNameAsync(model.Email) != null;
            if (usernameExists)
            {
                return BadRequest("Username is taken.");
            }

            bool emailExists = await _userManager.FindByEmailAsync(model.Email) != null;
            if (emailExists)
            {
                return BadRequest("Email is in use.");
            }

            var applicationUser = new AppUserModel
            {
                UserName = model.Email,
                FullName = string.Join(" ", model.FirstName, model.MiddleName, model.LastName),
                NickName = model.CalledName,
                Email = model.Email,
                LockoutEnabled = true,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                Status = (int)AccountStatusEnum.Verified,
                EmailConfirmed = true
            };
            var userResult = await _userManager.CreateAsync(applicationUser);
            if (!userResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, userResult.Errors);
            }
            var roleResult = await _userManager.AddToRoleAsync(applicationUser, "Staff");
            if (!roleResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, roleResult.Errors);
            }

            var confirmationLink = $"Your account have created successfully. Please reset your password to login for the first time";

            var emailConfig = _configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();

            var emailModel = new EmailViewModel(emailTo: applicationUser.Email, subject: "Reset Password Link",
                  content: confirmationLink, contentType: (int)EmailContentTypeEnum.Html);

            try
            {
                await _emailService.SendMail(emailModel, emailConfig);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }


        [HttpPut]
        [Route("UpdateEmployee")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest updateEmployeeRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(updateEmployeeRequest.Id);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                user.FullName = updateEmployeeRequest.FullName;
                user.Email = updateEmployeeRequest.Email;
                user.PhoneNumber = updateEmployeeRequest.Phone;

                var identityResult = await _userManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, identityResult.Errors);
                }
                return Ok();
            }
            catch
            {
                throw;
            }
        }


        [HttpPost]
        [Route("UpdateEmployeePassword")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployeePassword([FromBody] UpdateEmployeePasswordRequest updateEmployeeRequest)
        {
            if (updateEmployeeRequest.NewPassword != updateEmployeeRequest.NewConfirmPassword)
                return BadRequest("New passwords didnot matched.");

            var user = await _userManager.FindByIdAsync(updateEmployeeRequest.Id);
            if (user == null)
                return BadRequest("User not found");

            var passwordChangeResult = await _userManager.ChangePasswordAsync(user, updateEmployeeRequest.Password, updateEmployeeRequest.NewPassword);

            if (passwordChangeResult.Succeeded) return Ok();

            return BadRequest("Error while changing password.");
        }
    }
}
