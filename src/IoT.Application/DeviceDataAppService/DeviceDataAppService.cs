using Abp.Application.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Uow;
using AutoMapper;
using IoT.Core;
using IoT.Core.MongoDb;
using MongoDB.Driver;

namespace IoT.Application
{
    public class DeviceDataAppService : ApplicationService
    {
        //private readonly IDeviceDataRepository _dataRepository;
        private readonly IDeviceDataManager _dataManager;
        private readonly IMongoCollection<DeviceData> _deviceDatas;

        public DeviceDataAppService( IDeviceDataManager dataManager)
        {
           // _dataRepository = dataRepository;
            _dataManager = dataManager;
            _deviceDatas = _dataManager.DeviceDatas;
        }


        public PagedResultDto<DeviceDataDto> GetAll(PagedResultRequestDto input)
        {
            
            var data= _dataManager.GetAllDeviceData(input);
            var total = data.Count;
            
  
            return new PagedResultDto<DeviceDataDto>(total,ObjectMapper.Map<List<DeviceDataDto>>(data));
        }

        public DeviceDataDto GetByName(string deviceName)
        {
            var data = _dataManager.GetDeviceDataByDeviceName(deviceName);
            return ObjectMapper.Map<DeviceDataDto>(data);
        }

        
    }
}
