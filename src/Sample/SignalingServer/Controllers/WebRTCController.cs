using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTC.Shared;

namespace SignalingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebRTCController : ControllerBase
    {
        [HttpPost]
        public void Connect([FromBody] SignalingMessage message)
        {
            Console.WriteLine(message);
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "1", "2" };
        }
    }
}