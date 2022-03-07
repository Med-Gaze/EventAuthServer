using EventAuthServer.Datum.Constant.Menu;
using System.Collections.Generic;

namespace EventAuthServer.Datum.Constant
{
    public static class MenuConstant
    {
        public static List<MenuSeedModel> MenuList()
        {
            return new List<MenuSeedModel>()
            {
                new MenuSeedModel
                {
                    Id = "3c3d7057-5135-416d-9ada-4a1d6c7016a0",
                    Title = AuthMenuConstant.Company.Key,
                    Rank = 1,
                    RouteUrl = "company",
                },
                 new MenuSeedModel
                {
                    Id = "5c3d7057-5135-416d-9ada-4a1d6c7016a0",
                    Title = AuthMenuConstant.Role.Key,
                    Rank = 3,
                    RouteUrl = "role",
                },
                 new MenuSeedModel
                {
                    Id = "6c3d7057-5135-416d-9ada-4a1d6c7016a0",
                    Title = AuthMenuConstant.Menu.Key,
                    Rank = 4,
                    RouteUrl = "menu",
                },
                 new MenuSeedModel
                {
                    Id = "9c3d7057-5135-416d-9ada-4a1d6c7016a0",
                    Title = AuthMenuConstant.User.Key,
                    Rank = 7,
                    RouteUrl = "useraccount",
                }
            };
        }
    }
    public class MenuSeedModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Rank { get; set; }
        public string RouteUrl { get; set; }
    }

}
