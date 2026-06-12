using System;
using Deucarian.API.Models;

namespace Deucarian.API.Core
{
    internal interface IApiErrorParser
    {
        ApiError Parse(ApiRequest request, ApiTransportResponse response, Exception exception = null);
    }
}
