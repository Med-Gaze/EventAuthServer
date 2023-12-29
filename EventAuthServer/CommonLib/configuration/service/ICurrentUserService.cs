namespace med.common.library.configuration.service
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string Username { get; }
    }
}
