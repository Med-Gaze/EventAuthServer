using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace med.common.library.model
{
    /// <summary>
    /// 
    /// </summary>

    public class CreateRoleViewModel
    {
        /// <summary>
        /// 
        /// </summary>

        public string Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Rank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CategoryId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<CreateRoleDocumentViewModel> DocumentList { get; set; }

    }
    /// <summary>
    /// 
    /// </summary>
    public class EditRoleViewModel
    {
        /// <summary>
        /// 
        /// </summary> public string Id { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Rank { get; set; }
        /// <summary>
        /// 
        /// </summary>public string Alias { get; set; }
        public string CategoryId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }

    }
    /// <summary>
    /// 
    /// </summary>

    public class RoleViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Rank { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>

    public class RoleActivateViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }

    }
    /// <summary>
    /// 
    /// </summary>
    public class RoleDocumentViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RoleId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>

    public class CreateRoleDocumentViewModel
    {
        /// <summary>
        /// 
        /// </summary>

        public string Title { get; set; }
    }
}
