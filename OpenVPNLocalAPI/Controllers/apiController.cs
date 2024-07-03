using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVPNLocalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class apiController : ControllerBase
    {
        public apiController()
        {
        }

        [HttpPost("maketrade")]
        public string maketrade()
        {
            return "test";
        }

        [HttpGet("getusers")]
        public string get()
        {
            return "Success";
        }

        [HttpGet("getlog")]
        public string getlog()
        {
            return OVPN.ReadLog();
        }
        [HttpGet("getdata")]
        public string getdata()
        {
            return OVPN.init(OVPN.ReadLog());
        }
    }
}