using System;
using System.Collections.Generic;
using System.Text;

namespace med.common.library.configuration
{
    public class ModuleSettings
    {
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public bool LocalRun { get; set; }
        public int MaxApiExecutionTimeInMs { get; set; }
        public string BaseUrl { get; set; }
        public IList<ModuleFeatures> Features { get; set; }
    }
    public class ModuleFeatures
    {
        public string Title { get; set; }
        public string Alias { get; set; }
    }
    
}
