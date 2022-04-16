using System;
using System.Collections.Generic;
using System.Text;

namespace EventAuthServer.Entity
{
    public class MenuModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Class { get; set; }
        public string ParentId { get; set; }
        public int Rank { get; set; }
        public bool IsPublic { get; set; }
        public bool IsSeed { get; set; }
        public bool IsAdmin { get; set; }
        public string FaClass { get; set; } 
        public string RouteUrl { get; set; }
        public virtual MenuModel Parent { get; set; }
        public virtual ICollection<MenuModel> Children { get; set; }
    }
    
}
