using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoT.Core
{
    public class DeviceData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string GatewayId { get; set; }
        public string MonitorId { get; set; }
        public string MonitorName { get; set; }
        public string MonitorType { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public DateTime Timestamp { get; set; }
        public string IsScam { get; set; }
        public string Mark { get; set; }
        public string IsCheck { get; set; }
        //public long? CreatorUserId { get ; set ; }
        //public DateTime CreationTime { get ; set ; }
        //public long? LastModifierUserId { get ; set ; }
        //public DateTime? LastModificationTime { get ; set ; }
        //public long? DeleterUserId { get; set; }
        //public DateTime? DeletionTime { get; set; }
        //public bool IsDeleted { get; set; }
    }
}
