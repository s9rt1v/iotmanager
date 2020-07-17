using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using IoT.Application.GatewayAppService.DTO;
using IoT.Core;
using L._52ABP.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Entities;
using IoT.Core.Gateways;
using IoT.Core.Workshops;
using IoT.Core.Factories;
using IoT.Core.Cities;
using Microsoft.AspNetCore.Mvc;

namespace IoT.Application.GatewayAppService
{
    public class GatewayAppService:ApplicationService,IGatewayAppService
    {
        private readonly IRepository<GatewayType, int> _gatewayTypeRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IFactoryRepository _factoryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IGatewayManager _gatewayManager;

        public GatewayAppService(IRepository<GatewayType, int> gatewayTypeRepository,
            IGatewayRepository gatewayRepository,
            IWorkshopRepository workshopRepository,
            IFactoryRepository factoryRepository,
            ICityRepository cityRepository,
            IGatewayManager gatewayManager)
        {
            _gatewayTypeRepository = gatewayTypeRepository;
            _gatewayRepository = gatewayRepository;
            _workshopRepository = workshopRepository;
            _factoryRepository = factoryRepository;
            _cityRepository = cityRepository;
            _gatewayManager = gatewayManager;
        }


        public GatewayDto Get(EntityDto<int> input)
        {
            var query = _gatewayRepository.GetAll().Where(g => g.Id == input.Id)
                .Include(g => g.Workshop)
                .Include(g => g.Workshop.Factory)
                .Include(g => g.Workshop.Factory.City)
                .Include(g=>g.GatewayType);
            var entity = query.FirstOrDefault();
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            return ObjectMapper.Map<GatewayDto>(entity);
        }

        public GatewayDto GetByName(string gatewayName)
        {
            var query = _gatewayRepository.GetAll().Where(g => g.GatewayName.Contains(gatewayName)).Where(g=>g.IsDeleted==false)
                .Include(g => g.Workshop)
                .Include(g => g.Workshop.Factory)
                .Include(g => g.Workshop.Factory.City)
                .Include(g => g.GatewayType);
            var entity = query.FirstOrDefault();
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            return ObjectMapper.Map<GatewayDto>(entity);
        }

        public PagedResultDto<GatewayDto> GetAll(PagedSortedAndFilteredInputDto input)
        {
            var query=_gatewayRepository.GetAll().Where(g=>g.IsDeleted==false)
                .Include(g => g.Workshop)
                .Include(g => g.Workshop.Factory)
                .Include(g => g.Workshop.Factory.City)
                .Include(g=>g.GatewayType);
            var total = query.Count();
            var result = input.Sorting != null
                ? query.OrderBy(input.Sorting).AsNoTracking().PageBy(input).ToList()
                : query.PageBy(input).ToList();
            return new PagedResultDto<GatewayDto>(total, ObjectMapper.Map<List<GatewayDto>>(result));
        }

        [HttpGet]
        public PagedResultDto<GatewayDto> GetByCity(string CityName)
        {
            var cityQuery = _cityRepository.GetAll().Where(c => c.CityName == CityName).Where(g => g.IsDeleted == false);
            if (!cityQuery.Any())
            {
                throw new ApplicationException("城市不存在或已被删除");
            }
            var query = _gatewayRepository.GetAll().Where(d => d.IsDeleted == false).Where(g => g.Workshop.Factory.City.CityName == CityName)
               .Include(g => g.Workshop)
               .Include(g => g.Workshop.Factory)
               .Include(g => g.Workshop.Factory.City)
               .Include(g => g.GatewayType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<GatewayDto>(total, ObjectMapper.Map<List<GatewayDto>>(result));
        }

        [HttpGet]
        public PagedResultDto<GatewayDto> GetByFactory(string FactoryName)
        {
            var factoryQuery = _factoryRepository.GetAll().Where(f => f.FactoryName == FactoryName).Where(g => g.IsDeleted == false);
            if (!factoryQuery.Any())
            {
                throw new ApplicationException("factory不存在或已被删除");
            }
            var query = _gatewayRepository.GetAll().Where(d => d.IsDeleted == false).Where(g => g.Workshop.Factory.FactoryName == FactoryName)
               .Include(g => g.Workshop)
               .Include(g => g.Workshop.Factory)
               .Include(g => g.Workshop.Factory.City)
               .Include(g => g.GatewayType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<GatewayDto>(total, ObjectMapper.Map<List<GatewayDto>>(result));
        }

        [HttpGet]
        public PagedResultDto<GatewayDto> GetByWorkshop(string WorkshopName)
        {
            var workQuery = _workshopRepository.GetAll().Where(w => w.WorkshopName == WorkshopName).Where(g => g.IsDeleted == false);
            if (!workQuery.Any())
            {
                throw new ApplicationException("workshop不存在或已被删除");
            }
            var query = _gatewayRepository.GetAll().Where(d => d.IsDeleted == false).Where(g => g.Workshop.WorkshopName == WorkshopName)
               .Include(g => g.Workshop)
               .Include(g => g.Workshop.Factory)
               .Include(g => g.Workshop.Factory.City)
               .Include(g => g.GatewayType);
            var total = query.Count();
            var result = query.ToList();
            return new PagedResultDto<GatewayDto>(total, ObjectMapper.Map<List<GatewayDto>>(result));
        }

        [HttpGet]
        public long GetNumber()
        {
            var query = _gatewayRepository.GetAll().Where(g => g.IsDeleted == false);
            return query.Count();
        }

        public GatewayDto Create(CreateGatewayDto input)
        {
            var query = _gatewayRepository.GetAllIncluding().Where(g => g.HardwareId == input.HardwareId || g.GatewayName == input.GatewayName);
            var gateway_old = query.FirstOrDefault();
            if ((query.Any()) && (query.FirstOrDefault().IsDeleted == true))
            {
                
                gateway_old.IsDeleted = false;
                var result_old = _gatewayRepository.Update(gateway_old);
                CurrentUnitOfWork.SaveChanges();
                return ObjectMapper.Map<GatewayDto>(result_old);
            }

            if (query.Any()&&gateway_old.IsDeleted==false)
            {
                throw new ApplicationException("网关已存在");
            }

            var workshopQuery = _workshopRepository.GetAllIncluding().Where(w => w.WorkshopName == input.WorkshopName)
                .Where(w => w.Factory.FactoryName == input.FactoryName)
                .Where(w => w.Factory.City.CityName == input.CityName);
            var workshop = workshopQuery.FirstOrDefault();
            if (workshop==null)
            {
                throw new ApplicationException("Workshop不存在");
            }

            var gatewayTypeQuery = _gatewayTypeRepository.GetAll().Where(gt => gt.TypeName == input.GatewayTypeName);
            var gatewayType = gatewayTypeQuery.FirstOrDefault();
            if (gatewayType==null)
            {
                throw new ApplicationException("网关类型不存在");
            }
            var gateway = ObjectMapper.Map<Gateway>(input);
            gateway.Workshop = workshop;
            gateway.GatewayType = gatewayType;
            var result = _gatewayRepository.Insert(gateway);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<GatewayDto>(result);
        }

        public GatewayDto Update(UpdateGatewayDto input)
        {
            var entity = _gatewayRepository.Get(input.Id);
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
                    ObjectMapper.Map(input, entity);
                    entity.Workshop = workshop;
                }
            }
            entity.LastModificationTime = DateTime.Now;
            var result = _gatewayRepository.Update(entity);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<GatewayDto>(result);

        }

        public void Delete(EntityDto<int> input)
        {
            var entity = _gatewayRepository.Get(input.Id);
            if (entity.IsNullOrDeleted())
            {
                throw new ApplicationException("该设备不存在或已被删除");
            }
            _gatewayManager.Delete(entity);
        }

        [HttpDelete]
        public void BatchDelete(int[] inputs)
        {
            foreach (var input in inputs)
            {
                var entity = _gatewayRepository.Get(input);
                _gatewayManager.Delete(entity);
            }
        }
    }
}
