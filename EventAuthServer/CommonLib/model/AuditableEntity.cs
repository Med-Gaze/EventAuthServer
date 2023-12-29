using med.common.library.model.entity;
using System;

namespace med.common.library.model
{
    public abstract class AuditableEntity : EntityBase<Guid>
    {
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastModifiedBy { get; set; }
    }
 
}


