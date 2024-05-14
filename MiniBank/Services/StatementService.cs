using MiniBank.Dependencies;
using MiniBank.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MiniBank.Services
{
    public class StatementService
    {

        /// <summary>
        /// Return the Statement Account
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Statement GetStatement(int customerId, DateTime startDate, DateTime endDate)
        {
            if (customerId == 0)
                throw new Exception("Customer (ID: 0) not found");

            if (startDate > endDate)
                throw new Exception("Start date greater than end date");

            AccountService accountService = new AccountService();
            if (!accountService.ValidateCustomerWithoutAccount(customerId))
                throw new Exception($"Customer (ID: {customerId}) not found or already has a linked account");

            string sql = @$"SELECT a.*,
	at.ID AS TRANSACTION_ID,
	at.VALUE,
	at.TRANSACTION_TYPE,
	at.DTHR,
	at.OBS,
	COALESCE(SUM(at.VALUE) OVER(), 0) TOTAL_VALUE 	
FROM Account a
LEFT JOIN Account_Transaction at
	ON a.NUMBER = at.ACCOUNT_NUMBER 
	AND a.AGENCY = at.ACCOUNT_AGENCY
WHERE (at.DTHR IS NULL OR at.DTHR BETWEEN @StartDate AND @EndDate)
AND EXISTS (SELECT 1 
	FROM Customer c 
	WHERE c.ACCOUNT_NUMBER = a.NUMBER 
	AND c.ACCOUNT_AGENCY  = a.AGENCY
	AND c.ID = @CustomerId)";

            Dictionary<string, object> parameters = new()
            {
                { "CustomerId", customerId },
                { "StartDate", startDate },
                { "EndDate", endDate },
            };

            SqlServerConnection conn = SqlServerConnection.GetInstance();

            var result = conn.Query(sql, parameters);

            List<AccountTransactionClean> transactions = new List<AccountTransactionClean>();

            if (!string.IsNullOrEmpty(result[0]["TRANSACTION_ID"]))
                transactions = result.Select(x => new AccountTransactionClean(
                    int.Parse(x["TRANSACTION_ID"]),
                    double.Parse(x["VALUE"]),
                    x["TRANSACTION_TYPE"],
                    DateTime.Parse(x["DTHR"], CultureInfo.InvariantCulture),
                    x["OBS"]
                )).ToList();

            return new Statement(
                int.Parse(result[0]["NUMBER"]),
                int.Parse(result[0]["AGENCY"]),
                double.Parse(result[0]["CURRENT_BALANCE"]),
                double.Parse(result[0]["TOTAL_VALUE"]),
                transactions
            );
        }

        public string SendStatementByEmail(int customerId, DateTime startDate, DateTime endDate)
        {
            Customer customer = new CustomerService().GetCustomerById(customerId);
            Statement statement = GetStatement(customerId, startDate, endDate);

            var csvHash = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"{DateTime.Now} {customerId}"));
            string csvHashValue = (BitConverter.ToUInt32(csvHash, 0) % 10000000000).ToString();

            CommaSeparatedValues csv = new CommaSeparatedValues();
            csv.CreateFile<Statement>(csvHashValue.ToString(), statement);

            Mail mail = new Mail();
            mail.SendMail(customer.Email, "Your Statemet", "StatementTemplate", $"{csvHashValue}.csv");

            return "Statement sent to registered email";
        }
    }
}
