using Microsoft.AspNetCore.Mvc;
using MiniBank.Classes;
using MiniBank.Resources;
using System.Globalization;

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

                if (!ValidateCustomerWithAccount(customerId))
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

        [HttpPost("{customerId}/[action]", Name = "LinkAccountToCustomer")]
        public ActionResult<string> LinkCustomer(int customerId, AccountHeader accountHeader)
        {
            try
            {
                if (customerId == 0)
                    return BadRequest($"Customer (ID: {customerId}) not found");

                if (accountHeader.Agency < 1 || accountHeader.Number < 1)
                    return BadRequest($"Number (ID: {accountHeader.Number}) or Agency (ID: {accountHeader.Agency}) is invalid");

                if (!ValidateCustomerWithAccount(customerId))
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

        [HttpGet("{customerId}/[action]", Name = "GetStatement")]
        public ActionResult<Statement> Statement(int customerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("Start date greater than end date");

            if (!ValidateCustomerWithoutAccount(customerId))
                return BadRequest($"Customer (ID: {customerId}) not found or already has a linked account");

            string sql = @$"SELECT at.*, 
	SUM(at.VALUE) OVER() as TOTAL_VALUE,
	(SELECT a.CURRENT_BALANCE
	FROM Account a
	WHERE a.NUMBER = at.ACCOUNT_NUMBER 
	AND a.AGENCY = at.ACCOUNT_AGENCY) AS ACCOUNT_CURRENT_BALANCE
FROM Account_Transaction at
WHERE EXISTS (SELECT 1 
	FROM Customer c 
	WHERE c.ACCOUNT_NUMBER = at.ACCOUNT_NUMBER 
	AND c.ACCOUNT_AGENCY  = at.ACCOUNT_AGENCY 
	AND c.ID = {customerId})";

            SqlServerConnection conn = SqlServerConnection.GetInstance();

            var result = conn.Query(sql);

            List<AccountTransactionClean> transactions = result.Select(x => new AccountTransactionClean(
                int.Parse(x["ID"]),
                DateTime.Parse(x["DTHR"], CultureInfo.InvariantCulture),
                x["TRANSACTION_TYPE"],
                double.Parse(x["VALUE"]),
                x["OBS"]
            )).ToList();

            return new Statement(
                int.Parse(result[0]["ACCOUNT_NUMBER"]),
                int.Parse(result[0]["ACCOUNT_AGENCY"]),
                double.Parse(result[0]["ACCOUNT_CURRENT_BALANCE"]),
                double.Parse(result[0]["TOTAL_VALUE"]),
                transactions
            );
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
