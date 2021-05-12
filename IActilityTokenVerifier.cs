using IoTHub.Model;
using Microsoft.AspNetCore.Http;

namespace IoTHub
{
    public interface IActilityTokenVerifier
    {
        public bool IsTokenValid(ActilityUplinkData data, IQueryCollection query, string authenticationKey);
    }
}
