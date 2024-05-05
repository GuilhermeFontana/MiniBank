using Microsoft.AspNetCore.Mvc;
using MiniBank.Classes;
using MiniBank.Resources;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpPost("{customerId}/[action]", Name = "CreateAccount")]
        public ActionResult<Account> Create(int customerId, [FromBody] AccountInput accountInput)
        {
            try
            {
                if (customerId == 0)
                    return BadRequest($"Customer (ID: {customerId}) not found");

                if (accountInput.Agency < 1)
                    return BadRequest($"Agency (ID: {accountInput.Agency}) is invalid");

                if (!ValidateAccountType(accountInput.AccountType))
                    BadRequest($"{accountInput.AccountType} not is a account type valid");

                if (!ValidateCustomer(customerId))
                    return BadRequest($"Customer (ID: {customerId}) not found or already has a linked account");

                int accountNumber = new Random().Next(1, 99999);

                while (!ValidateNumberAgency(accountNumber, accountInput.Agency))
                    accountNumber = new Random().Next(1, 99999);

                string sql = @"INSERT INTO Account (NUMBER, AGENCY, ACCOUNT_TYPE, CURRENT_BALANCE)
VALUES (@Number, @Agency, @AccountType, @CurrentBalance)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "Number", accountNumber },
                    { "Agency", accountInput.Agency},
                    { "AccountType", accountInput.AccountType},
                    { "CurrentBalance", accountInput.CurrentBalance},
                };

                SqlServerConnection conn = SqlServerConnection.GetInstance();

                conn.OutputValue(sql, parameters);

                sql = $@"UPDATE Customer 
  SET [ACCOUNT_AGENCY] = @AccountAgency, [ACCOUNT_NUMBER] = @AccountNumber
WHERE ID = {customerId}";

                parameters = new Dictionary<string, object>
                {
                    { "AccountNumber", accountNumber },
                    { "AccountAgency", accountInput.Agency},
                };

                conn.OutputValue(sql, parameters);

                return new Account(accountNumber, accountInput.Agency, accountInput.AccountType, accountInput.CurrentBalance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{customerId}/[action]", Name = "LinkCustomer")]
        public ActionResult<string> LinkCustomer(int customerId, AccountHeader accountHeader)
        {
            try
            {
                if (customerId == 0)
                    return BadRequest($"Customer (ID: {customerId}) not found");

                if (accountHeader.Agency < 1 || accountHeader.Number < 1)
                    return BadRequest($"Number (ID: {accountHeader.Number}) or Agency (ID: {accountHeader.Agency}) is invalid");

                if (!ValidateCustomer(customerId))
                    return BadRequest($"Customer (ID: {customerId}) not found or already has a linked account");

                if (ValidateNumberAgency(accountHeader.Number, accountHeader.Agency))
                    return BadRequest($"Account (Number {accountHeader.Number} and Agency {accountHeader.Agency}) not found");

                SqlServerConnection conn = SqlServerConnection.GetInstance();

                string sql = $@"UPDATE Customer 
  SET [ACCOUNT_AGENCY] = @AccountAgency, [ACCOUNT_NUMBER] = @AccountNumber
WHERE ID = {customerId}";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "AccountNumber", accountHeader.Number },
                    { "AccountAgency", accountHeader.Agency},
                };

                conn.OutputValue(sql, parameters);

                return "Customer linked to account";
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

        private bool ValidateCustomer(int customerID)
        {
            string sql = $"SELECT ACCOUNT_NUMBER FROM Customer WHERE ID = {customerID}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            if (result.Count == 0)
                return false;

            return String.IsNullOrEmpty(result[0]["ACCOUNT_NUMBER"]);
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
