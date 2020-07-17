using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IoT.Core;

namespace IoT.Application.CityAppService.DTO
{
    public class CityDto:EntityDto<int>
    {
        public string CityName { get; set; }
        public string CityCode { get; set; }
        public string Remark { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
