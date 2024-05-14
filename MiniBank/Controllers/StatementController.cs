using Microsoft.AspNetCore.Mvc;
using MiniBank.Models;
using MiniBank.Models.Others;
using MiniBank.Services;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatementController : ControllerBase
    {
        StatementService Service = new StatementService();

        [HttpGet("{customerId}", Name = "GetStatement")]
        public ActionResult<Statement> GetStatement(int customerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                return Service.GetStatement(customerId, startDate, endDate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{customerId}/[action]", Name = "SendStatementByEmail")]
        public ActionResult<string> SendByEmail(int customerId, [FromBody ] DateIntervalnput dateIntervalnput)
        {
            try
            {
                return Service.SendStatementByEmail(customerId, dateIntervalnput.StartDate, dateIntervalnput.EndDate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
