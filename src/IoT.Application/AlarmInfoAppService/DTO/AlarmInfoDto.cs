using System;
using Abp.AutoMapper;
using IoT.Core.MongoDb.Model;

namespace IoT.Application.AlarmInfoAppService.DTO
{
    [AutoMapFrom(typeof(AlarmInfoModel))]
    public class AlarmInfoDto
    {
        public String Id { get; set; }

        public String AlarmInfo { get; set; }

        public String DeviceId { get; set; }

        public String IndexId { get; set; }

        public String IndexName { get; set; }

        public Double IndexValue { get; set; }

        public Double ThresholdValue { get; set; }

        public DateTime Timestamp { get; set; }
        public String Severity { get; set; }
        public String Processed { get; set; }
    }
}
