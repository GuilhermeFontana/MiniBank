using Microsoft.AspNetCore.Mvc;
using MiniBank.Classes;
using MiniBank.Resources;
using System.Globalization;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        [HttpPost("[action]", Name = "CreateCustomer")]
        public ActionResult<Customer> Create([FromBody] CustomerInput customerInput)
        {
            if (String.IsNullOrEmpty(customerInput.FirtsName))
                return BadRequest("FirstName field is required");
            if (String.IsNullOrEmpty(customerInput.LastName))
                return BadRequest("LastName field is required");
            if (String.IsNullOrEmpty(customerInput.BirthDate))
                return BadRequest("BirthDate field is required");
            if (String.IsNullOrEmpty(customerInput.Email))
                return BadRequest("Email field is required");

            string sql = @"INSERT INTO Customer (FIRTS_NAME, LAST_NAME, BIRTH_DATE, EMAIL, ADDRESS) 
OUTPUT Inserted.ID
VALUES(@First_Name, @Last_Name, @Birth_Date, @Email, @Address)";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "First_Name", customerInput.FirtsName.Trim() },
                { "Last_Name", customerInput.LastName.Trim() },
                { "Birth_Date", customerInput.BirthDate },
                { "Email", customerInput.Email.Trim() },
                { "Address", customerInput?.Address?.Trim() ?? "" }
            };

            try
            {
                SqlServerConnection conn = SqlServerConnection.GetInstance();

                int id = int.Parse(conn.OutputValue(sql, parameters));

                return new Customer(
                    id,
                    customerInput.FirtsName,
                    customerInput.LastName,
                    DateTime.Parse(customerInput.BirthDate, CultureInfo.InvariantCulture),
                    customerInput.Email,
                    customerInput.Address
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
            string sql = $"SELECT * FROM Customer WHERE ID = {id}";

            try
            {
                SqlServerConnection conn = SqlServerConnection.GetInstance();

                List<Dictionary<string, string>> result = conn.Query(sql);

                if (result.Count == 0)
                    return BadRequest($"Customer (ID: {id}) not found");

                Customer curCustomer = new Customer(
                    int.Parse(result[0]["ID"]),
                    result[0]["FIRST_NAME"],
                    result[0]["LAST_NAME"],
                    DateTime.Parse(result[0]["BIRTH_DATE"], CultureInfo.InvariantCulture),
                    result[0]["EMAIL"],
                    result[0]["ADDRESS"]
                );

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                if (!String.IsNullOrEmpty(customerInput.FirtsName) && !curCustomer.FirtsName.Equals(customerInput.FirtsName.Trim()))
                {
                    curCustomer.FirtsName = customerInput.FirtsName.Trim();
                    parameters.Add("First_Name", customerInput.FirtsName.Trim());
                }
                if (!String.IsNullOrEmpty(customerInput.LastName) && !curCustomer.LastName.Equals(customerInput.LastName.Trim()))
                {
                    curCustomer.LastName = customerInput.LastName.Trim();
                    parameters.Add("Last_Name", customerInput.LastName.Trim());
                }
                if (!String.IsNullOrEmpty(customerInput.BirthDate) && !curCustomer.BirthDate.Equals(DateTime.Parse(customerInput.BirthDate, CultureInfo.InvariantCulture)))
                {
                    curCustomer.BirthDate = DateTime.Parse(customerInput.BirthDate, CultureInfo.InvariantCulture);
                    parameters.Add("Birth_Date", customerInput.BirthDate);
                }
                if (!String.IsNullOrEmpty(customerInput.Email) && !curCustomer.Email.Equals(customerInput.Email.Trim()))
                {
                    curCustomer.Email = customerInput.Email.Trim();
                    parameters.Add("Email", customerInput.Email.Trim());
                }
                if (!String.IsNullOrEmpty(customerInput.Address) && !curCustomer.Address.Equals(customerInput.Address.Trim()))
                {
                    curCustomer.Address = customerInput.Address.Trim();
                    parameters.Add("Address", customerInput.Address.Trim());
                }

                if (parameters.Count > 0)
                {
                    sql = $@"UPDATE MyBank.dbo.Customer 
  SET {String.Join(", ", parameters.Select(x => $"{x.Key} = @{x.Key}").ToList())}
WHERE ID = {id}";

                    conn.NonQuery(sql, parameters);
                }

                return curCustomer;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}