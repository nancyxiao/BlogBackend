using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BlogViewModels
{
    public class RolesViewModel
    {
        public Roles role { get; set; }

        public string TreeIds { get; set; }

        public List<RoleSelect> RolesList { get; set; }
    }

    public class RoleSelect
    {
        public string RoleID { get; set; }
        public string Platform { get; set; }
        //前/後台_ID_名稱
        public string RoleDes { get; set; }
    }
}
