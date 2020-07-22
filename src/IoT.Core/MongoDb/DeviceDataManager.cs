using System.Collections.Generic;
using Abp.Application.Services.Dto;
using MongoDB.Driver;
using System.Linq;
using Abp.Domain.Services;
using Microsoft.Extensions.Options;
using IoTManager;
using IoT.Core.MongoDb.Model;

namespace IoT.Core.MongoDb
{
    public class DeviceDataManager:DomainService,IDeviceDataManager
    {
        
        //private readonly MongoDbConfiguration _mongoConfiguration;
        private readonly IMongoCollection<DeviceData> _deviceDatas;
        private readonly IOptions<ConnectionStrings> _connectionStrings;
        private readonly IMongoCollection<AlarmInfoModel> _alarmInfo;

        public DeviceDataManager(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings;
            //var mongoConfiguration = new MongoDbConfiguration();
            
            var client = new MongoClient(_connectionStrings.Value.MongoDb);
            
            var database = client.GetDatabase(MongoConsts.DbName);
            _deviceDatas = database.GetCollection<DeviceData>(MongoConsts.MonitorData);
            DeviceDatas = _deviceDatas;
            AlarmInfo = _alarmInfo;
            
        }

        public IMongoCollection<DeviceData> DeviceDatas { get; set; }
        public IMongoCollection<AlarmInfoModel> AlarmInfo { get; set; }

        public DeviceData GetByName(string deviceName)
        {
            var result= _deviceDatas.FindAsync(d => d.DeviceName == deviceName).Result.FirstOrDefault(); 
            return result;

        }

        public List<DeviceData> GetAll(PagedResultRequestDto input)
        {
            
            var query = _deviceDatas.AsQueryable();
            int total = query.Count();
            var result = query.OrderBy(dd => dd.Timestamp).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return result;
            
        }
    }
}
