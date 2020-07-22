using System;
using Abp.Domain.Entities.Auditing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace IoT.Core.MongoDb.Model
{
    public class AlarmInfoModel 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
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
        //public long? CreatorUserId { get ; set ; }
        //public DateTime CreationTime { get ; set ; }
        //public long? LastModifierUserId { get ; set ; }
        //public DateTime? LastModificationTime { get; set; }
        //public long? DeleterUserId { get; set; }
        //public DateTime? DeletionTime { get; set; }
        //public bool IsDeleted { get ; set; }
    }
}
