using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace Task_9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(5);

        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine(semaphore.CurrentCount);

            if (Console.ReadLine() == "123")
            {
                semaphore.Release();
            }

            Console.WriteLine(semaphore.CurrentCount);

            if (!semaphore.Wait(0))
            {
                return BadRequest();
            }

            

            try
            {
                return Ok();
            }
            finally 
            { 

            }
            
        }
    }
}
