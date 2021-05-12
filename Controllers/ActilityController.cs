using IoTHub.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace IoTHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ActilityController : ControllerBase
    {
        private readonly IPayloadRouter _payloadRouter;
        private readonly IActilityTokenVerifier _tokenVerifier;
        private readonly string _authenticationKey;

        public ActilityController(IPayloadRouter payloadRouter, IActilityTokenVerifier tokenVerifier, IAuthenticationKeySource keySource)
        {
            _payloadRouter = payloadRouter ?? throw new ArgumentNullException(nameof(payloadRouter));
            _tokenVerifier = tokenVerifier ?? throw new ArgumentNullException(nameof(tokenVerifier));
            _authenticationKey = keySource.GetKey();
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Alive!");
        }

        [HttpPost]
        [Route("persist")]
        public async Task<ActionResult> Post([FromBody] ActilityUplinkData data)
        {
            if (!_tokenVerifier.IsTokenValid(data, Request.Query, _authenticationKey))
            {
                return BadRequest("Invalid token");
            }
            await _payloadRouter.RoutePayload(data);
            return Ok();
        }
    }
}
