using System;
using System.Collections.Generic;
using System.Text;

namespace med.common.library.model
{
    public class AppTenantProperty
    {
        public string Id { get; set; }
        public string ModuleId { get; set; }
        public string ModuleTitle { get; set; }
        public string ModuleAlias { get; set; }
        public string Domain { get; set; }
        public string TanentId { get; set; }
        public string ConnectionString { get; set; }
        public string OrgName { get; set; }
        public string OrgAddress { get; set; }
        public string OrgEmail { get; set; }
        public string OrgPhoneNo { get; set; }
        public string OrgFax { get; set; }
        public string LogoUrl { get; set; }
        public string DomainUrl { get; set; }
        public string Description { get; set; }
        public string ThemeImageUrl { get; set; }
        public bool UseSharedScheme { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
