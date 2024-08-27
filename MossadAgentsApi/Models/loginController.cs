using Microsoft.AspNetCore.Mvc;
using MossadAgentsApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MossadAgentsApi.Models
{

    [Route("[controller]")]
    [ApiController]
    public class loginController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> PostLogin(string id)
        {
            if (id == "SimulationServer"/* || id == ""*/)
            {
              id = Guid.NewGuid().ToString();
            
                return StatusCode(StatusCodes.Status201Created, new { tokn = "ag123456" }); 
            }
            return BadRequest();
        }
    }
}
