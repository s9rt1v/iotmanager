using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IoT.Application.WorkshopAppService.DTO;
using L._52ABP.Application.Dtos;

namespace IoT.Application.WorkshopAppService
{
    public interface IWorkshopAppService:ICrudAppService<WorkshopDto,int,PagedSortedAndFilteredInputDto,CreateWorkshopDto>
    {
        void BatchDelete(int[] inputs);
        long GetNumber();
        PagedResultDto<WorkshopDto> GetByCity(string CityName);
        PagedResultDto<WorkshopDto> GetByFactory(string FactoryName);
        WorkshopDto GetByName(string workshopName);
        Object GetAffilateNumber(EntityDto<int> input);
    }
}

