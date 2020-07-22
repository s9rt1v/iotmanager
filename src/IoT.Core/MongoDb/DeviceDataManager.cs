using System.Collections.Generic;
using Abp.Application.Services.Dto;
using MongoDB.Driver;
using System.Linq;
using Abp.Domain.Services;
using Microsoft.Extensions.Options;
using IoTManager;
using IoT.Core.MongoDb.Model;
using System;
using MongoDB.Bson;

namespace IoT.Core.MongoDb
{
    public class DeviceDataManager:DomainService,IDeviceDataManager
    {
        
        //private readonly MongoDbConfiguration _mongoConfiguration;
        private readonly IMongoCollection<DeviceData> _deviceDatas;
        private readonly IOptions<ConnectionStrings> _connectionStrings;
        private readonly IMongoCollection<AlarmInfoModel> _alarmInfo;
        public IMongoCollection<DeviceData> DeviceDatas { get; set; }
        public IMongoCollection<AlarmInfoModel> AlarmInfo { get; set; }

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

        

        public DeviceData GetDeviceDataByDeviceName(string deviceName)
        {
            var result= _deviceDatas.FindAsync(d => d.DeviceName == deviceName ).Result.FirstOrDefault(); 
            return result;

        }

        public List<DeviceData> GetAllDeviceData(PagedResultRequestDto input)
        {
            
            var query = _deviceDatas.AsQueryable()/*.Where(d=>d.IsDeleted==false)*/;
            int total = query.Count();
            var result = query.OrderBy(dd => dd.Timestamp).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return result;
            
        }

        public List<DeviceData> GetByDeviceName(String deviceName, int count)
        {
            var query = _deviceDatas.AsQueryable()
                .Where(dd => dd.DeviceName == deviceName)
                .OrderByDescending(dd => dd.Timestamp)
                .Take(count)
                .ToList();
            if (query.Count() != 0)
            {
                foreach (var d in query)
                {
                    d.Timestamp += TimeSpan.FromHours(8);
                }
            }
            return query;
        }

        public AlarmInfoModel GetAlarmInfoByDeviceId(string deviceId)
        {
            var result = _alarmInfo.FindAsync(a => a.DeviceId == deviceId).Result.FirstOrDefault();
            return result;
        }

        public List<AlarmInfoModel> GetAllAlarmInfo(PagedResultRequestDto input)
        {
            var query = _alarmInfo.AsQueryable();
            int total = query.Count();
            var result = query.OrderBy(dd => dd.Timestamp).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return result;
        }

        public int GetAmount()
        {
            var query = _deviceDatas.AsQueryable();
            return query.Count();
        }

        public Object GetLineChartData(string deviceId, string indexId)
        {
            List<string> chartValue = new List<string>();
            List<string> xAxises = new List<string>();

            var query = _deviceDatas.AsQueryable()
                .Where(dd => (dd.DeviceId == deviceId && dd.MonitorId == indexId))
                .OrderByDescending(dd => dd.Timestamp)
                .Take(12)
                .ToList();

            foreach (var dd in query)
            {
                chartValue.Add(dd.MonitorId);
                xAxises.Add(DateTime.Parse(dd.Timestamp.ToString()).ToLocalTime().GetDateTimeFormats()
                    .ToString());
            }

            xAxises.Reverse();
            chartValue.Reverse();

            return new { xAxis = xAxises, series = chartValue, indexId };
        }

        public object GetMultipleLineChartData(String deviceId, List<String> fields)
        {
            List<object> data = new List<object>();
            foreach (String f in fields)
            {
                data.Add(GetLineChartData(deviceId, f));
            }

            return data;
        }

        public Object GetDeviceDataInDevicePropertyByName(String deviceName)
        {
            /*
            DeviceModel device = this._deviceDao.GetByDeviceNamePrecise(deviceName);
            List<DeviceDataModel> deviceData = this._deviceDataDao.GetByDeviceId(device.HardwareDeviceId);
            
            List<DeviceDataModel> result = deviceData.AsQueryable()
                .OrderByDescending(dd => dd.Timestamp)
                .Take(10)
                .ToList();
            */
            /*zxin-修改：旧版获取所有设备数据后截取，修改后直接获取指定数量的设备数据*/
            List<DeviceData> result = GetByDeviceName(deviceName,10);
            foreach (var dd in result)
            {
                dd.DeviceId = dd.Timestamp.GetDateTimeFormats().ToString();
            }
            return result;
        }

        public DeviceData GetLastDataByDeviceId(string deviceId)
        {
            var query = _deviceDatas.AsQueryable()
                .Where(dd => dd.DeviceId == deviceId)
                .OrderByDescending(dd => dd.Timestamp)
                .Take(1).FirstOrDefault();
            query.Timestamp += TimeSpan.FromHours(8);
            return query;
        }

        public List<DeviceData> GetNotInspected()
        {
            List<DeviceData> deviceDataModels = _deviceDatas.Find<DeviceData>(dd => dd.IsScam == "false").ToList();
            var filter = Builders<DeviceData>.Filter.Eq("IsScam", "false");
            var update = Builders<DeviceData>.Update.Set("IsScam", "true");
            var result = _deviceDatas.UpdateMany(filter, update);
            return deviceDataModels;
        }

        public String Delete(String id)
        {
            var filter = Builders<DeviceData>.Filter.Eq("_id", new ObjectId(id));
            var result = _deviceDatas.DeleteOne(filter);
            return result.DeletedCount == 1 ? "success" : "error";
        }

        public int BatchDelete(List<String> ids)
        {
            int num = 0;
            foreach (String id in ids)
            {
                var filter = Builders<DeviceData>.Filter.Eq("_id", new ObjectId(id));
                var result = _deviceDatas.DeleteOne(filter);
                num = num + 1;
            }
            return num;
        }

        public string Create(DeviceData deviceData)
        {
            try
            {
                _deviceDatas.InsertOne(deviceData);
                return "success";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
