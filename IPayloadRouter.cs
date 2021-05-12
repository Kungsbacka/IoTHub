using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTHub.Model;

namespace IoTHub
{
    public interface IPayloadRouter
    {
        public Task RoutePayload(ActilityUplinkData payload);
    }
}
