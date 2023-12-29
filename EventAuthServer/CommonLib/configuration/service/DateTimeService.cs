using med.common.library.configuration.service;
using System;

namespace med.common.library.configuration.service
{

    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}