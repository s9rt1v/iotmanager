﻿using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services;
using IoT.Application.GatewayAppService.DTO;
using L._52ABP.Application.Dtos;

namespace IoT.Application.GatewayAppService
{
    public interface IGatewayAppService:ICrudAppService<GatewayDto,int,PagedSortedAndFilteredInputDto,CreateGatewayDto,UpdateGatewayDto>
    {
    }
}
