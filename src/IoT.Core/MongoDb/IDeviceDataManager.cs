using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Services;
using IoT.Core.MongoDb.Model;
using MongoDB.Driver;

namespace IoT.Core.MongoDb
{
    public interface IDeviceDataManager:IDomainService
    {
        DeviceData GetDeviceDataByDeviceName(string deviceName);
        List<DeviceData> GetAllDeviceData(PagedResultRequestDto input);
        IMongoCollection<DeviceData> DeviceDatas { get; set; }
        IMongoCollection<AlarmInfoModel> AlarmInfo { get; set; }
        AlarmInfoModel GetAlarmInfoByDeviceId(string deviceId);
        List<AlarmInfoModel> GetAllAlarmInfo(PagedResultRequestDto input);
    }
}
