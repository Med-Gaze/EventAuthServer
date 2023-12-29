namespace med.common.library.model.entity
{
    public interface IEntityBase<TId>
    {
        TId Id { get; }
    }
}
