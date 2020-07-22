using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IoT.Application.GatewayAppService.DTO;
using L._52ABP.Application.Dtos;

namespace IoT.Application.GatewayAppService
{
    public interface IGatewayAppService:ICrudAppService<GatewayDto,int,PagedSortedAndFilteredInputDto,CreateGatewayDto,UpdateGatewayDto>
    {
        void BatchDelete(int[] inputs);
        long GetNumber();
        PagedResultDto<GatewayDto> GetByCity(string CityName);
        PagedResultDto<GatewayDto> GetByFactory(string FactoryName);
        PagedResultDto<GatewayDto> GetByWorkshop(string WorkshopName);
        GatewayDto GetByName(string gatewayName);
        Object GetAffiliateNumber(EntityDto<int> input);
    }
}

