using Microsoft.AspNetCore.Mvc;
using MiniBank.Models;
using MiniBank.Services;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        CustomerService Service = new CustomerService();

        [HttpPost("[action]", Name = "CreateCustomer")]
        public ActionResult<Customer> Create([FromBody] CustomerInput customerInput)
        {
            try
            {
                return Service.CreateCustomer(
                    customerInput.FirstName.Trim(),
                    customerInput.LastName.Trim(),
                    customerInput.BirthDate.Trim(),
                    customerInput.Email.Trim(),
                    customerInput.Address?.Trim()
                );
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("CUSTOMER_EMAIL_UK"))
                    return BadRequest("Email já cadastrado");

                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/[action]", Name = "UpdateCustomer")]
        public ActionResult<Customer> Update(int id, [FromBody] CustomerInput customerInput)
        {
            try
            {
                return Service.UpdateCustomer(
                  id,
                  customerInput.FirstName,
                  customerInput.LastName,
                  customerInput.BirthDate,
                  customerInput.Email,
                  customerInput.Address
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}