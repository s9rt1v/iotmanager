using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using IoT.Application.CityAppService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IoT.Application.CityAppService
{
    public interface ICityAppService:ICrudAppService<CityDto,int,CityPagedSortedAndFilteredDto,CreateCityDto,UpdateCityDto>
    {
        [HttpDelete]
        void  BatchDelete(int[] inputs);

        [HttpGet]
        long GetNumber();

        List<object> GetCityCascaderOptionsTilWorkshop();
        List<object> GetCityCascaderOptionsTilGateway();
        List<object> GetCityCascaderOptionsTilDevice();
        object GetThreeLevelMenu();
        List<Object> GetMapInfo();
        List<object> GetCityMapInfo(String cityName);
        object GetCityFactoryTree();
        List<object> GetCityOptions();
        Object GetAffiliateFactoryAndWorkshopNumber(EntityDto<int> input);
    }
}
