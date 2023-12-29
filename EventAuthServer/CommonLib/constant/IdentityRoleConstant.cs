
using med.common.library.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace med.common.library.constant
{
    /// <summary>
    /// 
    /// </summary>

    public static class IdentityRoleConstant
    {
        /// <summary>
        /// 
        /// </summary>
        public const string SuperAdmin = nameof(SuperAdmin);
        /// <summary>
        /// 
        /// </summary>
        public const string Admin = nameof(Admin);
        /// <summary>
        /// 
        /// </summary>
        public const string Default = nameof(Default);
        /// <summary>
        /// 
        /// </summary>
        public const string Staff = nameof(Staff);

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, RoleViewModel> RoleList()
        {
            return new Dictionary<string, RoleViewModel>()
            {
               {SuperAdmin, new RoleViewModel(){ Id = "82c457a8-ecf4-40a5-8293-91fd55e43a31", Rank = 1 }},
               {Admin, new RoleViewModel(){ Id = "82c457a8-ecf4-40a5-8293-91fd55e43a32", Rank = 2}},
               {Default, new RoleViewModel(){ Id = "60c3f3c0-6801-4170-b8c8-0bca49c092bb", Rank = 0 }},
               {Staff, new RoleViewModel(){ Id = "8DD28606-8EC3-4F6A-9021-2D29BB8F3ABE", Rank = 3 }},
            };
        }
    }
}
