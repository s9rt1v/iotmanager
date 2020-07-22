using System;
using System.Collections.Generic;
using Abp.Linq.Extensions;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using IoT.Application.DeviceAppService.DeviceService.Dto;
using IoT.Core;
using L._52ABP.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Entities;
using IoT.Core.Gateways;
using IoT.Core.Workshops;
using IoT.Core.Factories;
using IoT.Core.Cities;
using IoT.Core.Devices;
using Microsoft.AspNetCore.Mvc;
using IoT.Core.Fields;
using IoT.Application.FieldAppService.DTO;
using System.IO;
using Microsoft.Extensions.Logging;
using Abp.Extensions;

namespace IoT.Application.DeviceAppService.DeviceService
{
    public class DeviceAppService : ApplicationService, IDeviceAppService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IRepository<DeviceType> _deviceTypeRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IFactoryRepository _factoryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IDeviceManager _deviceManager;
        private readonly IFieldRepository _fieldRepository;

        public DeviceAppService(IDeviceRepository deviceRepository,
        IRepository<DeviceType> deviceTypeRepository,
        IGatewayRepository gatewayRepository,
        IWorkshopRepository workshopRepository,
        IFactoryRepository factoryRepository,
        ICityRepository cityRepository,
        IDeviceManager deviceManager,
        IFieldRepository fieldRepository
        )
        {
            _deviceRepository = deviceRepository;
            _deviceTypeRepository = deviceTypeRepository;
            _gatewayRepository = gatewayRepository;
            _workshopRepository = workshopRepository;
            _factoryRepository = factoryRepository;
            _cityRepository = cityRepository;
            _deviceManager = deviceManager;
            _fieldRepository = fieldRepository;
        }

        //得到单个设备
        public DeviceDto Get(EntityDto<int> input)
        {
            var query = _deviceRepository.GetAll().Where(d => d.Id == input.Id)
               .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.Gateway.GatewayType)
               .Include(d => d.DeviceType);
            var entity = query.FirstOrDefault();
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            return ObjectMapper.Map<DeviceDto>(entity);
        }

        //通过id模糊搜索
        public PagedResultDto<DeviceDto> GetByFuzzyId(EntityDto<int> input)
        {
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d=>d.Id.ToString().Contains(input.Id.ToString()))
               .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过名字模糊搜索
        public PagedResultDto<DeviceDto> GetByFuzzyName(string deviceName)
        {
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.DeviceName.Contains(deviceName))
               .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }


        //得到所有的设备
        public PagedResultDto<DeviceDto> GetAll(PagedSortedAndFilteredInputDto input)
        {
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).WhereIf(!input.FilterText.IsNullOrEmpty(), d =>d.DeviceName.Contains(input.FilterText))
               .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = input.Sorting != null
                ? query.OrderBy(input.Sorting).AsNoTracking().PageBy(input).ToList()
                : query.PageBy(input).ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过设备类型过滤设备
        [HttpGet]
        public PagedResultDto<DeviceDto> GetByType(string deviceType)
        {
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.DeviceType.TypeName == deviceType)
               .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过城市过滤设备
        [HttpGet]
        public PagedResultDto<DeviceDto> GetByCity(string CityName)
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.CityName == CityName).Where(g => g.IsDeleted == false);
            if (!cityQuery.Any())
            {
                throw new ApplicationException("城市不存在或已被删除");
            }
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.Gateway.Workshop.Factory.City.CityName == CityName)
                .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过网关过滤设备
        [HttpGet]
        public PagedResultDto<DeviceDto> GetByGateway(string GatewayName)
        {
            var gatewayQuery = _gatewayRepository.GetAll().Where(g => g.GatewayName == GatewayName).Where(g => g.IsDeleted == false);
            if (!gatewayQuery.Any())
            {
                throw new ApplicationException("gateway 不存在或已被删除");
            }
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.Gateway.GatewayName == GatewayName)
                .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过车间过滤设备
        [HttpGet]
        public PagedResultDto<DeviceDto> GetByWorkshop(string WorkshopName)
        {
            var workshopQuery = _workshopRepository.GetAll().Where(w => w.WorkshopName == WorkshopName).Where(g => g.IsDeleted == false);
            if (!workshopQuery.Any())
            {
                throw new ApplicationException("workshop 不存在或已被删除");
            }
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.Gateway.Workshop.WorkshopName == WorkshopName)
                .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }

        //通过工厂过滤设备
        [HttpGet]
        public PagedResultDto<DeviceDto> GetByFactory(string FactoryName)
        {
            var gatewayQuery = _factoryRepository.GetAll().Where(f => f.FactoryName == FactoryName).Where(g => g.IsDeleted == false);
            if (!gatewayQuery.Any())
            {
                throw new ApplicationException("factory 不存在或已被删除");
            }
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d => d.Gateway.Workshop.Factory.FactoryName == FactoryName)
                .Include(d => d.Gateway)
               .Include(d => d.Gateway.Workshop)
               .Include(d => d.Gateway.Workshop.Factory)
               .Include(d => d.Gateway.Workshop.Factory.City)
               .Include(d => d.DeviceType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<DeviceDto>(total, ObjectMapper.Map<List<DeviceDto>>(result));
        }
        //获得设备的数量
        [HttpGet]
        public long GetNumber()
        {
            var query = _deviceRepository.GetAll().Where(d => d.IsDeleted == false);
            return query.Count();
        }
        //新建设备
        public DeviceDto Create(CreateDeviceDto input)
        {
            var query = _deviceRepository.GetAllIncluding()
               .Where(d => d.HardwareId == input.HardwareId || d.DeviceName == input.DeviceName);
            if ((query.Any()) && (query.FirstOrDefault().IsDeleted == true))
            {
                var entity = query.FirstOrDefault();
                entity.IsDeleted = false;
                var result_old = _deviceRepository.Update(entity);
                CurrentUnitOfWork.SaveChanges();
                return ObjectMapper.Map<DeviceDto>(result_old);
            }
            if (query.Any())
            {
                throw new ApplicationException("设备已存在");
            }

            var workshopQuery = _workshopRepository.GetAllIncluding().Where(w => w.WorkshopName == input.WorkshopName)
                .Where(w => w.Factory.FactoryName == input.FactoryName)
                .Where(w => w.Factory.City.CityName == input.CityName);
            var workshop = workshopQuery.FirstOrDefault();
            if (workshop == null)
            {
                throw new ApplicationException("Workshop不存在");
            }

            var gatewayQuery = _gatewayRepository.GetAllIncluding().Where(g => g.Workshop.WorkshopName == input.WorkshopName)
                .Where(g => g.Workshop.Factory.FactoryName == input.FactoryName)
                .Where(g => g.Workshop.Factory.City.CityName == input.CityName)
                .Where(g => g.HardwareId == input.HardwareId || g.GatewayName == input.GatewayName);
            var gateway = gatewayQuery.FirstOrDefault();
            if (gateway == null)
            {
                throw new ApplicationException("网关不存在");
            }

            var deviceTypeQuery = _deviceTypeRepository.GetAll().Where(dt => dt.TypeName == input.DeviceTypeName);
            var deviceType = deviceTypeQuery.FirstOrDefault();
            if (deviceType == null)
            {
                throw new ApplicationException("设备类型不存在");
            }
            var device = ObjectMapper.Map<Device>(input);
            device.Gateway = gateway;
            device.DeviceType = deviceType;
            var result = _deviceRepository.Insert(device);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<DeviceDto>(result);
        }
        //更新设备
        public DeviceDto Update(CreateDeviceDto input)
        {
            var entity = _deviceRepository.Get(input.Id);
            var cityQuery = _cityRepository.GetAll().Where(c => c.CityName == input.CityName);
            if (!cityQuery.Any())
            {
                throw new ApplicationException("City不存在");
            }

            var factoryQuery = _factoryRepository.GetAll().Where(f => f.FactoryName == input.FactoryName);
            if (!factoryQuery.Any())
            {
                throw new ApplicationException("Factory不存在");
            }

            var factory = factoryQuery.FirstOrDefault();
            if (factory != null)
            {
                factory.City = cityQuery.FirstOrDefault();
                var workshopQuery = _workshopRepository.GetAll().Where(w => w.WorkshopName == input.WorkshopName);
                if (!workshopQuery.Any())
                {
                    throw new ApplicationException("Workshop不存在");
                }

                var workshop = workshopQuery.FirstOrDefault();
                if (workshop != null)
                {
                    workshop.Factory = factory;
                    var gatewayQuery = _gatewayRepository.GetAll().Where(g => g.GatewayName == input.GatewayName);

                    if (!gatewayQuery.Any())
                    {
                        throw new ApplicationException("Gateway不存在");
                    }

                    var gateway = gatewayQuery.FirstOrDefault();
                    if (gateway != null)
                    {
                        gateway.Workshop = workshop;
                        ObjectMapper.Map(input, entity);
                        entity.Gateway = gateway;
                        var deviceTypeQuery = _deviceTypeRepository.GetAll().Where(dt => dt.TypeName == input.DeviceTypeName);
                        if (!deviceTypeQuery.Any())
                        {
                            throw new ApplicationException("该设备类型不存在");
                        }

                        var deviceType = deviceTypeQuery.FirstOrDefault();
                        if (deviceType != null)
                        {
                            entity.DeviceType = deviceType;
                        }
                    }
                }


            }
            entity.LastModificationTime = DateTime.Now;
            var result = _deviceRepository.Update(entity);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<DeviceDto>(result);
        }
        //单个删除
        public void Delete(EntityDto<int> input)
        {
            var entity = _deviceRepository.Get(input.Id);
            if (entity.IsNullOrDeleted())
            {
                throw new ArgumentException("设备不存在或已删除");
            }
            _deviceManager.Delete(entity);
        }
        //批量删除
        [HttpDelete]
        public void BatchDelete(int[] inputs)
        {
            foreach (var input in inputs)
            {
                var entity = _deviceRepository.Get(input);
                _deviceManager.Delete(entity);
            }
        }
        //获得设备位置
        public Object GetDeviceLocationByDeviceId(EntityDto<int> input)
        {
            var device = _deviceRepository.Get(input.Id);
            if (device.IsNullOrDeleted())
            {
                throw new ApplicationException("device不存在或已被删除");
            }
            var gateway = _gatewayRepository.Get(device.GatewayId);
            var workshop = _workshopRepository.Get(gateway.WorkshopId);
            var factory = _factoryRepository.Get(workshop.FactoryId);
            var city = _cityRepository.Get(factory.CityId);
            if (city.IsNullOrDeleted())
            {
                throw new ApplicationException("city不存在或已被删除");
            }
            return new
            {
                cityName = city.CityName,
                longitude = city.Longitude,
                latitude = city.Latitude,
                factoryName = factory.FactoryName,
                workshopName = workshop.WorkshopName,
                gatewayName = gateway.GatewayName

            };
        }

        //获得设备属性选项
        public List<object> GetFieldOptions()
        {
            var deviceQuery = _deviceRepository.GetAll().Where(d => d.IsDeleted == false);
            List<Device> devices1 = deviceQuery.ToList();
            List<DeviceDto> devices = ObjectMapper.Map<List<DeviceDto>>(devices1);
            List<object> result = new List<object>();
            foreach (DeviceDto d in devices)
            {
                var fieldQuery = _fieldRepository.GetAll().Where(f => f.IsDeleted == false).Where(f => f.DeviceId == d.Id);
                List<Field> fields1 = fieldQuery.ToList();
                List<FieldDto> fields = ObjectMapper.Map<List<FieldDto>>(fields1);
                List<object> children = new List<object>();
                foreach (var f in fields)
                {
                    children.Add(new { value = f.IndexId, label = f.FieldName });
                }
                result.Add(new { value = d.HardwareId, label = d.DeviceName, children = children });
            }

            return result;
        }

        //上传base64图片数据
        [HttpPost]
        public String UploadPicture(string base64Image,int deviceId)
        {
            try
            {
                var device = _deviceRepository.Get(deviceId);
                device.Base64Image = base64Image;
                _deviceRepository.Update(device);
                CurrentUnitOfWork.SaveChanges();
                return "图片上传成功";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //获得base64图片数据
        public string GetPicture(int deviceId)
        {
            try
            {
                var device = _deviceRepository.Get(deviceId);
                return device.Base64Image;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        //更新连接时间
        [HttpPost]
        public DeviceDto UpdateLastConnectionTimeByDeviceId(int deviceId)
        {
            var device = _deviceRepository.Get(deviceId);
            device.LastConnectionTime = DateTime.Now;
            var result = _deviceRepository.Update(device);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<DeviceDto>(result);
        }

    }
}
