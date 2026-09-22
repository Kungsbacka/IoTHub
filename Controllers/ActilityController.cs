using IoTHub.Model;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IActilityEnvelopeDumper _envelopeDumper;

        public ActilityController(IPayloadRouter payloadRouter,
            IActilityTokenVerifier tokenVerifier,
            IAuthenticationKeySource keySource,
            IActilityEnvelopeDumper envelopeDumper)
        {
            _payloadRouter = payloadRouter ?? throw new ArgumentNullException(nameof(payloadRouter));
            _tokenVerifier = tokenVerifier ?? throw new ArgumentNullException(nameof(tokenVerifier));
            _envelopeDumper = envelopeDumper ?? throw new ArgumentNullException(nameof(envelopeDumper));
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

            if (HttpContext.Items.TryGetValue("ActilityEnvelope", out var envelope))
            {
                try
                {
                    await _envelopeDumper.Dump(envelope.ToString());
                }
                catch
                {
                    // Envelope dump must not affect payload routing
                }
            }

            await _payloadRouter.RoutePayload(data);

            await _payloadRouter.RoutePayload(data);
            return Ok();
        }
    }
}
