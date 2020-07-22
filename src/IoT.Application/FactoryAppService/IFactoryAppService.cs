using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IoT.Application.FactoryAppService.DTO;
using L._52ABP.Application.Dtos;

namespace IoT.Application.FactoryAppService
{
    public interface IFactoryAppService:ICrudAppService<FactoryDto,int, PagedSortedAndFilteredInputDto, CreateFactoryDto>
    {
        void BatchDelete(int[] inputs);
        long GetNumber();
        PagedResultDto<FactoryDto> GetByCity(string CityName);
        FactoryDto GetByName(string factoryName);
        Object GetAffiliateNumber(EntityDto<int> input);
    }
}

