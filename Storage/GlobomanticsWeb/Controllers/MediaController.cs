using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GlobomanticsWeb.Controllers
{
    public class MediaController : Controller
    {
        private readonly IConfiguration config;

        public MediaController(IConfiguration appConfig)
        {
            config = appConfig;
        }
    }
}