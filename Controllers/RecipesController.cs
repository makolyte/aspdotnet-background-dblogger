using BackgroundDatabaseLogger.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace BackgroundDatabaseLogger.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly ILoggerService Logger;
        public RecipesController(ILoggerService logger)
        {
            Logger = logger;
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            Logger.Log(LogLevel.Debug, $"GET /Recipes/{id}");
            return "recipe";
        }
    }
}
