using Microsoft.AspNetCore.Mvc;
using MiniBank.Models;
using MiniBank.Dependencies;

namespace MiniBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountTransactionController : ControllerBase
    {
        [HttpPost("[action]", Name = "CreateAccountTransaction")]
        public ActionResult<AccountTransaction> Create(AccountTransactionInOutput accountTransactionInput)
        {
            try
            {
                if (accountTransactionInput.Value == 0)
                    return BadRequest("The value must be must be non-zero");

                if (!ValidateTransactionType(accountTransactionInput.TransactionType))
                    return BadRequest($"Invalid operation (Type: {accountTransactionInput.TransactionType})");

                if (accountTransactionInput.AccountAgency < 1 || accountTransactionInput.AccountNumber < 1)
                    return BadRequest($"Number (ID: {accountTransactionInput.AccountNumber}) or Agency (ID: {accountTransactionInput.AccountAgency}) is invalid");

                double? currentBalance = GetCurrentAccountBalance(accountTransactionInput.AccountNumber, accountTransactionInput.AccountAgency);

                if (currentBalance is null)
                    return BadRequest($"Account (Number {accountTransactionInput.AccountNumber} and Agency {accountTransactionInput.AccountAgency}) not found");

                if ((currentBalance < 0 && accountTransactionInput.Value < 0) || currentBalance < 0 || currentBalance + accountTransactionInput.Value <= 0)
                    return BadRequest("Insufficient balance");

                string sql = @"INSERT INTO Account_Transaction (ACCOUNT_NUMBER, ACCOUNT_AGENCY, VALUE, TRANSACTION_TYPE, OBS) 
OUTPUT Inserted.ID
VALUES(@AccountNumber, @AccountAgency, @Value, @Type, @Obs)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "AccountNumber", accountTransactionInput.AccountNumber },
                    { "AccountAgency", accountTransactionInput.AccountAgency },
                    { "Value", accountTransactionInput.Value },
                    { "Type", accountTransactionInput.TransactionType ?? "" },
                    { "Obs", accountTransactionInput.Obs ?? "" },
                };

                SqlServerConnection conn = SqlServerConnection.GetInstance();

                int id = int.Parse(conn.OutputValue(sql, parameters));

                sql = @"UPDATE Account 
SET CURRENT_BALANCE = CURRENT_BALANCE + @Value
WHERE NUMBER = @AccountNumber AND AGENCY = @AccountAgency";

                conn.NonQuery(sql, parameters);

                return new AccountTransaction(id, DateTime.Now, accountTransactionInput.AccountNumber, accountTransactionInput.AccountAgency, accountTransactionInput.TransactionType, accountTransactionInput.Value, accountTransactionInput.Obs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region PrivateMethods

        private bool ValidateTransactionType(string transactionType)
        {
            string[] accountTypesAvaliable = { "DEP", "SAQ", "PIX", "TED" };

            return accountTypesAvaliable.Contains(transactionType);
        }

        private double? GetCurrentAccountBalance(int number, int agency)
        {
            string sql = $"SELECT CURRENT_BALANCE FROM Account WHERE NUMBER = {number} AND AGENCY = {agency}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            if (result.Count > 0)
                return null;

            return double.Parse(result[0]["CURRENT_BALANCE"]);
        }

        #endregion PrivateMethods

    }
}
