using System;
using JorisHoef.APIHelper.Models;

namespace JorisHoef.APIHelper.Core
{
    internal interface IApiErrorParser
    {
        ApiError Parse(ApiRequest request, ApiTransportResponse response, Exception exception = null);
    }
}
