using Microsoft.AspNetCore.Mvc;
using MiniBank.Models;
using MiniBank.Dependencies;
using System.Globalization;
using MiniBank.Services;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        AccountService Service = new AccountService();

        [HttpPost("{customerId}/[action]", Name = "CreateAccount")]
        public ActionResult<Account> Create(int customerId, [FromBody] AccountInput accountInput)
        {
            try
            {
                return Service.CreateAccount(customerId, accountInput.Agency, accountInput.AccountType, accountInput.CurrentBalance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{customerId}/[action]", Name = "LinkAccountToCustomer")]
        public ActionResult<string> LinkCustomer(int customerId, AccountHeader accountHeader)
        {
            try
            {
                return Service.LinkAccountToCustomer(customerId, accountHeader.Number, accountHeader.Agency);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region PrivateMethods

        private bool ValidateAccountType(string accountType)
        {
            string[] accountTypesAvaliable = { "CC", "CP" };

            return accountTypesAvaliable.Contains(accountType);
        }

        private bool ValidateCustomerWithAccount(int customerID)
        {
            string sql = $"SELECT ACCOUNT_NUMBER FROM Customer WHERE ID = {customerID}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            if (result.Count == 0)
                return false;

            return String.IsNullOrEmpty(result[0]["ACCOUNT_NUMBER"]);
        }

        private bool ValidateCustomerWithoutAccount(int customerID)
        {
            string sql = $"SELECT ACCOUNT_NUMBER FROM Customer WHERE ID = {customerID}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            if (result.Count == 0)
                return false;

            return !String.IsNullOrEmpty(result[0]["ACCOUNT_NUMBER"]);
        }

        private bool ValidateNumberAgency(int number, int agency)
        {
            string sql = $"SELECT 1 as TEST FROM Account WHERE NUMBER = {number} AND AGENCY = {agency}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            return result.Count == 0;
        }

        #endregion PrivateMethods

    }
}
