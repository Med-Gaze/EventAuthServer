using med.common.library.configuration.service;
using med.common.library.model;
using System.Threading.Tasks;

namespace med.common.library.configuration.service
{


    public interface IDomainEventService
    {
        Task Publish(DomainEvent domainEvent);
    }
}
