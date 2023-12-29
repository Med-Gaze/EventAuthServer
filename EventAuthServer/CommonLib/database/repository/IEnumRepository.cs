
using med.common.library.model.entity;
using System;

namespace med.common.library.database.repository
{
    public interface IEnumRepository<T> : IRepositoryBase<T, Guid> where T : IEntityBase<Guid>
    {
    }
}
