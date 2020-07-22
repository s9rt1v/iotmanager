using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Services;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using IoT.Application.CityAppService.DTO;
using IoT.Core.Cities;
using IoT.Core;
using IoT.Core.MongoDb;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NUglify.Helpers;
using System.Linq.Dynamic.Core;
using System.Linq;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using IoT.Application.FactoryAppService.DTO;
using IoT.Core.Gateways;
using IoT.Core.Workshops;
using IoT.Core.Factories;
using IoT.Application.WorkshopAppService.DTO;
using IoT.Application.GatewayAppService.DTO;
using IoT.Core.Devices;
using IoT.Application.DeviceAppService.DeviceService.Dto;
using IoT.Core.Regions.Entity;

namespace IoT.Application.CityAppService
{
    public class CityAppService:ApplicationService,ICityAppService
    {
        private readonly ICityManager _cityManager;
        private readonly ICityRepository _cityRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IFactoryRepository _factoryRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IRepository<Region, int> _regionRepository;

        public CityAppService(ICityManager cityManager, ICityRepository cityRepository,
        IGatewayRepository gatewayRepository,
        IWorkshopRepository workshopRepository,
        IFactoryRepository factoryRepository,
        IDeviceRepository deviceRepository,
        IRepository<Region, int> regionRepository)
        {
            _cityManager = cityManager;
            _cityRepository = cityRepository;
            _gatewayRepository = gatewayRepository;
            _workshopRepository = workshopRepository;
            _factoryRepository = factoryRepository;
            _deviceRepository = deviceRepository;
            _regionRepository = regionRepository;
        }


        public CityDto Get(EntityDto<int> input)
        {
            var entity = _cityRepository.Get(input.Id);
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            return entity.MapTo<CityDto>();
        }

        //通过名字获得城市
        public CityDto GetByName(string cityName)
        {
            var query = _cityRepository.GetAll().Where(c=>c.CityName.Contains(cityName)).Where(c=>c.IsDeleted==false);
            var entity = query.FirstOrDefault();
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            return entity.MapTo<CityDto>();
        }

        public PagedResultDto<CityDto> GetAll(CityPagedSortedAndFilteredDto input)
        {
            var query = _cityRepository.GetAll().Where(c=>c.IsDeleted==false).WhereIf(!input.FilterText.IsNullOrEmpty(),c=>c.CityName.Contains(input.FilterText));
            
            var total = query.Count();
            var result = input.Sorting != null
                ? query.OrderBy(input.Sorting).AsNoTracking().PageBy(input).ToList()
                : query.PageBy(input).ToList();
            return new PagedResultDto<CityDto>(total,ObjectMapper.Map<List<CityDto>>(result));

        }
        public Object GetAffiliateFactoryAndWorkshopNumber(EntityDto<int> input)
        {
            var city = _cityRepository.Get(input.Id);
            if (city.IsNullOrDeleted())
            {
                throw new ApplicationException("city不存在或已被删除");
            }
            var workshop = _workshopRepository.GetAll().Where(w=>w.Factory.CityId==input.Id).Where(w=>w.IsDeleted==false);
            var factory = _factoryRepository.GetAll().Where(f=>f.CityId==input.Id).Where(f=>f.IsDeleted==false);
            
            return new
            {
                factoryNumber = factory.Count(),
                workshopName = workshop.Count()

            };
        }

        //获得城市数量
        [HttpGet]
        public long GetNumber()
        {
            var query = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            return query.Count();
        }
        /*
        public PagedResultDto<CityDto> GetAll(PagedResultRequestDto input)
        {
            var query = _cityRepository.GetAll();
            int total = query.Count();
            var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList<City>();
            return new PagedResultDto<CityDto>(total,new List<CityDto>(
                ObjectMapper.Map<List<CityDto>>(result)));
        }
        */

        //新建城市
        public CityDto Create(CreateCityDto input)
        {
            string requestUrl= "https://restapi.amap.com/v3/geocode/geo?address=" + input.CityName +
                               "&key=c6d99b34598e3721a00fb609eb4a4c1b";
            CityGeoDto cityGeo=new CityGeoDto();
            using (HttpClient client=new HttpClient())
            {
                HttpResponseMessage responseMessage = client.GetAsync(requestUrl).Result;
                cityGeo =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<CityGeoDto>(responseMessage.Content.ReadAsStringAsync().Result);
            }
            if (!cityGeo.geocodes.Any())
            {
                throw new ArgumentException("Error Input");
            }
            GeocodeModel geoModel = cityGeo.geocodes[0];
            var location = geoModel.location.Split(",");
            var entity = new City()
            {
                CityName = geoModel.city,
                CityCode = geoModel.citycode,
                Longitude = decimal.Parse(location[0]),
                Latitude = decimal.Parse(location[1])
            };
            var city = _cityRepository.GetAll().Where(c => c.CityCode == entity.CityCode);
            if (city.Any()&&(city.FirstOrDefault().IsDeleted==true))
            {
                var city_old = city.FirstOrDefault();
                city_old.IsDeleted = false;
                var result_old = _cityRepository.Update(city_old);
                CurrentUnitOfWork.SaveChanges();
                return result_old.MapTo<CityDto>();
            }
            if (city.Any())
            {
                throw new ApplicationException("城市已存在！");
            }
            var result = _cityRepository.Create(entity);
            CurrentUnitOfWork.SaveChanges();
            return result.MapTo<CityDto>();
        }

        //更新城市信息
        public CityDto Update(UpdateCityDto input)
        {
            var city = _cityRepository.Get(input.Id);
            if (city.IsNullOrDeleted())
            {
                throw new ArgumentException("Error Input");
            }

            city.Remark = input.Remark;
            city.LastModificationTime = DateTime.Now;
            var result= _cityRepository.Update(city);
            CurrentUnitOfWork.SaveChanges();
            return result.MapTo<CityDto>();
        }

        //删除单个城市同时删除附属工厂到设备属性
        public void Delete(EntityDto<int> input)
        {
            /*删除城市，城市的实验楼提示是否删除*/
            var city = _cityRepository.Get(input.Id);
            
            if (city.IsNullOrDeleted())
            {
                throw new ArgumentException("城市不存在或已删除");
            }
            _cityManager.Delete(city);
        }

        //批量删除城市
        [HttpDelete]
        public void BatchDelete(int[] inputs)
        {
            foreach (var input in inputs)
            {
                var entity = _cityRepository.Get(input);
                _cityManager.Delete(entity);
            }
        }

        //获得城市下拉选项直到车间
        public List<object> GetCityCascaderOptionsTilWorkshop()
        {
            var cityQuery = _cityRepository.GetAll().Where(c=>c.IsDeleted==false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            List<object> result = new List<object>();
            result.Add(new { value = "全部", label = "全部" });
            foreach (CityDto c in cities)
            {
                List<object> children = new List<object>();
                var factoryQuery = _factoryRepository.GetAll().Where(f=>f.IsDeleted==false).Where(f=>f.City.CityName==c.CityName);
                List<Factory> factories1 = factoryQuery.ToList(); 
                List<FactoryDto> factories = ObjectMapper.Map<List<FactoryDto>>(factories1);
                children.Add(new { value = "全部", label = "全部" });
                foreach (FactoryDto f in factories)
                {
                    List<object> subchildren = new List<object>();
                    var workshopQuery = _workshopRepository.GetAll().Where(w=>w.IsDeleted==false).Where(w=>w.Factory.FactoryName==f.FactoryName);
                    List<Workshop> workshops1 = workshopQuery.ToList();
                    List<WorkshopDto> workshops = ObjectMapper.Map<List<WorkshopDto>>(workshops1);
                    subchildren.Add(new { value = "全部", label = "全部" });
                    foreach (WorkshopDto w in workshops)
                    {
                        subchildren.Add(new { value = w.WorkshopName, label = w.WorkshopName });
                    }
                    children.Add(new { value = f.FactoryName, label = f.FactoryName, Workshop = subchildren });
                }
                result.Add(new { value = c.CityName, label = c.CityName, Factory = children });
            }

            return result;
        }

        //获得城市下拉选项直到网关
        public List<object> GetCityCascaderOptionsTilGateway()
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            List<object> result = new List<object>();
            result.Add(new { value = "全部", label = "全部" });
            foreach (CityDto c in cities)
            {
                List<object> children = new List<object>();
                var factoryQuery = _factoryRepository.GetAll().Where(f => f.IsDeleted == false).Where(f => f.City.CityName == c.CityName);
                List<Factory> factories1 = factoryQuery.ToList();
                List<FactoryDto> factories = ObjectMapper.Map<List<FactoryDto>>(factories1);
                children.Add(new { value = "全部", label = "全部" });
                foreach (FactoryDto f in factories)
                {
                    List<object> subchildren = new List<object>();
                    var workshopQuery = _workshopRepository.GetAll().Where(w => w.IsDeleted == false).Where(w => w.Factory.FactoryName == f.FactoryName);
                    List<Workshop> workshops1 = workshopQuery.ToList();
                    List<WorkshopDto> workshops = ObjectMapper.Map<List<WorkshopDto>>(workshops1);
                    subchildren.Add(new { value = "全部", label = "全部" });
                    foreach (WorkshopDto w in workshops)
                    {
                        List<object> subchildren_2 = new List<object>();
                        var gatewayQuery = _gatewayRepository.GetAll().Where(g => g.IsDeleted == false).Where(g=>g.Workshop.WorkshopName==w.WorkshopName);
                        List<Gateway> gateways1 = gatewayQuery.ToList();
                        List<GatewayDto> gateways = ObjectMapper.Map<List<GatewayDto>>(gateways1);
                        subchildren_2.Add(new { value = "全部", label = "全部" });
                        foreach (GatewayDto g in gateways)
                        {
                            subchildren_2.Add(new { varlue = g.GatewayName, label = g.GatewayName});
                        }
                        subchildren.Add(new { value = w.WorkshopName, label = w.WorkshopName, Gateway = subchildren_2 });
                    }
                    children.Add(new { value = f.FactoryName, label = f.FactoryName, Workshop = subchildren });
                }
                result.Add(new { value = c.CityName, label = c.CityName, Factory = children });
            }

            return result;
        }

        //获得城市下拉选项直到设备
        public List<object> GetCityCascaderOptionsTilDevice()
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            List<object> result = new List<object>();
            result.Add(new { value = "全部", label = "全部" });
            foreach (CityDto c in cities)
            {
                List<object> children = new List<object>();
                var factoryQuery = _factoryRepository.GetAll().Where(f => f.IsDeleted == false).Where(f => f.City.CityName == c.CityName);
                List<Factory> factories1 = factoryQuery.ToList();
                List<FactoryDto> factories = ObjectMapper.Map<List<FactoryDto>>(factories1);
                children.Add(new { value = "全部", label = "全部" });
                foreach (FactoryDto f in factories)
                {
                    List<object> subchildren = new List<object>();
                    var workshopQuery = _workshopRepository.GetAll().Where(w => w.IsDeleted == false).Where(w => w.Factory.FactoryName == f.FactoryName);
                    List<Workshop> workshops1 = workshopQuery.ToList();
                    List<WorkshopDto> workshops = ObjectMapper.Map<List<WorkshopDto>>(workshops1);
                    subchildren.Add(new { value = "全部", label = "全部" });
                    foreach (WorkshopDto w in workshops)
                    {
                        List<object> subchildren_2 = new List<object>();
                        var gatewayQuery = _gatewayRepository.GetAll().Where(g => g.IsDeleted == false).Where(g => g.Workshop.WorkshopName == w.WorkshopName);
                        List<Gateway> gateways1 = gatewayQuery.ToList();
                        List<GatewayDto> gateways = ObjectMapper.Map<List<GatewayDto>>(gateways1);
                        subchildren_2.Add(new { value = "全部", label = "全部" });
                        foreach (GatewayDto g in gateways)
                        {
                            List<object> subchildren_3 = new List<object>();
                            var deviceQuery = _deviceRepository.GetAll().Where(d => d.IsDeleted == false).Where(d=>d.Gateway.GatewayName==g.GatewayName);
                            List<Device> devices1 = deviceQuery.ToList();
                            List<DeviceDto> devices = ObjectMapper.Map<List<DeviceDto>>(devices1);
                            subchildren_3.Add(new { value = "全部", label = "全部" });
                            foreach (DeviceDto d in devices)
                            {
                                subchildren_3.Add(new { value = d.DeviceName,label = d.DeviceName});
                            }
                            subchildren_2.Add(new { value = g.GatewayName, label = g.GatewayName, Device = subchildren_3 });
                        }
                        subchildren.Add(new { value = w.WorkshopName, label = w.WorkshopName, Gateway = subchildren_2 });
                    }
                    children.Add(new { value = f.FactoryName, label = f.FactoryName, Workshop = subchildren });
                }
                result.Add(new { value = c.CityName, label = c.CityName, Factory = children });
            }

            return result;
        }

        //获得三级菜单
        public object GetThreeLevelMenu()
        {            
           var query = _regionRepository.GetAll().Where(r=>r.IsDeleted==false);
            Dictionary<String, String> result = new Dictionary<string, string>();
           foreach (var q in query)
            {
                result.Add(q.Level, q.RegionName);
            }

            return result;
            
        }

        //获得地图设备分布信息
        public List<Object> GetMapInfo()
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            var deviceQuery = _deviceRepository.GetAll().Where(d => d.IsDeleted == false);
            List<Device> devices1 = deviceQuery.ToList();
            List<DeviceDto> devices = ObjectMapper.Map<List<DeviceDto>>(devices1);
            List<object> result = new List<object>();
            foreach (CityDto city in cities)
            {
                int offlineNum = devices.AsQueryable()
                    .Where(d => d.CityName == city.CityName && d.IsOnline==0)
                    .ToList().Count;
                int onlineNum = devices.AsQueryable()
                    .Where(d => d.CityName == city.CityName && d.IsOnline ==1)
                    .ToList().Count;
                List<object> info = new List<object>();
                info.Add(city.Longitude);
                info.Add(city.Latitude);
                info.Add("在线: " + onlineNum.ToString() + "; 离线: " + offlineNum.ToString());
                result.Add(new { name = city.CityName, value = info });
            }

            return result;
        }

        //获得特定城市设备分布
        public List<object> GetCityMapInfo(string cityName)
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false).Where(c=>c.CityName==cityName);
            if (!cityQuery.Any())
            {
                throw new ApplicationException("城市不存在或已被删除");
            }
            var entity = cityQuery.FirstOrDefault();
            var city = entity.MapTo<CityDto>();

            var deviceQuery = _deviceRepository.GetAll().Where(d => d.IsDeleted == false);
            List<Device> devices1 = deviceQuery.ToList();
            List<DeviceDto> devices = ObjectMapper.Map<List<DeviceDto>>(devices1);
            var offlineQuery = devices.AsQueryable()
                .Where(d => d.CityName == cityName && d.IsOnline == 0)
                .ToList().Count;
            var onlineQuery = devices.AsQueryable()
                .Where(d => d.CityName == cityName && d.IsOnline == 1)
                .ToList().Count;
            List<object> result = new List<object>();
            List<object> info = new List<object>();
            info.Add(city.Longitude);
            info.Add(city.Latitude);
            info.Add("在线: " + onlineQuery.ToString() + "; 离线: " + offlineQuery.ToString());
            result.Add(new { name = cityName, value = info });
            return result;
        }

        //获得城市工厂树状图
        public object GetCityFactoryTree()
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            List<object> result = new List<object>();
            foreach (CityDto c in cities)
            {
                var factoryQuery = _factoryRepository.GetAll().Where(f => f.IsDeleted == false).Where(f => f.City.CityName == c.CityName);
                List<Factory> factories1 = factoryQuery.ToList();
                List<FactoryDto> factories = ObjectMapper.Map<List<FactoryDto>>(factories1);
                List<object> children = new List<object>();
                foreach (FactoryDto f in factories)
                {
                    children.Add(new { value = f.FactoryName, label = f.FactoryName, id = f.Id, factoryName = f.FactoryName, cityName = c.CityName });
                }
                result.Add(new { value = c.CityName, label = c.CityName, Factory = children });
            }

            return result;
        }

        //获得城市选项
        public List<object> GetCityOptions()
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.IsDeleted == false);
            List<City> cities1 = cityQuery.ToList();
            List<CityDto> cities = ObjectMapper.Map<List<CityDto>>(cities1);
            List<object> result = new List<object>();
            foreach (CityDto c in cities)
            {
                result.Add(new { ValueTuple = c.CityName, label = c.CityName });
            }
            return result;
        }
    }
}
