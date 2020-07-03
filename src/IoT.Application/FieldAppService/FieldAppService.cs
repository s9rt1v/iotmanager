﻿using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using IoT.Core;
using L._52ABP.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using IoT.Application.FieldAppService.DTO;
using AutoMapper;

namespace IoT.Application.FieldAppService
{
    public class FieldAppService : ApplicationService, IFieldAppService
    {
        private readonly IRepository<Field, int> _fieldRepository;
        private readonly IRepository<Device, int> _deviceRepository;
        public FieldAppService(IRepository<Field, int> fieldRepository,IRepository<Device, int> deviceRepository)
        {
            _fieldRepository = fieldRepository;
            _deviceRepository = deviceRepository;
        }

        public FieldDto Get(EntityDto<int> input)
        {
           var query = _fieldRepository.GetAllIncluding(f => f.Device).Where(f => f.Id == input.Id);
            var entity = query.FirstOrDefault();
            return ObjectMapper.Map<FieldDto>(entity);
        }

        public PagedResultDto<FieldDto> GetAll(PagedSortedAndFilteredInputDto input)
        {
            var query = _fieldRepository.GetAll().Include(f=>f.Device);
            var total = query.Count();
            var result = input.Sorting != null
                ? query.OrderBy(input.Sorting).AsNoTracking().PageBy(input).ToList()
                : query.PageBy(input).ToList();
            return new PagedResultDto<FieldDto>(total, ObjectMapper.Map<List<FieldDto>>(result));
        }

        public FieldDto Create(FieldDto input)
        {
            var fieldQuery = _fieldRepository.GetAll().Where(f=>f.FieldName == input.FieldName);
            if(fieldQuery.Any())
            {
                throw new ApplicationException("field 已存在");
            }
            var deviceQuery = _deviceRepository.GetAll().Where(d => d.DeviceName == input.DeviceName);
            if(!deviceQuery.Any())
            {
                throw new ApplicationException("设备不存在");
            }
            
            var device = deviceQuery.FirstOrDefault();
            var field = new Field()
            {
                FieldName = input.FieldName,
                IndexId = input.IndexId,
                Device = device
            };
            
            var result = _fieldRepository.Insert(field);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<FieldDto>(result);
        }


        public FieldDto Update(FieldDto input)
        {
            var deviceQuery = _deviceRepository.GetAll().Where(d => d.DeviceName == input.DeviceName);
            if (!deviceQuery.Any())
            {
                throw new ApplicationException("设备不存在");
            }
            var device = deviceQuery.FirstOrDefault();
            var field = new Field() {
                FieldName = input.FieldName,
                IndexId = input.IndexId,
                Device = device,
                Id = input.Id
            } ;
            
            
            var result = _fieldRepository.Update(field);
            CurrentUnitOfWork.SaveChanges();
            return ObjectMapper.Map<FieldDto>(result);
        }

        public void Delete(EntityDto<int> input)
        {
            var entity = _fieldRepository.Get(input.Id);
            _fieldRepository.Delete(entity);
        }
    }
}
