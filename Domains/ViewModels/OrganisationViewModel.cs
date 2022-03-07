using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Domains.ViewModels
{
    public class OrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public string ImgBase64 { get; set; }
        public string FileName { get; set; }
        public IFormFile LogoFile { get; set; }
    }
}
