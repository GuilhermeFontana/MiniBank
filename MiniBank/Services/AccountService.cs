using MiniBank.Dependencies;
using MiniBank.Models;
using System.Globalization;

namespace MiniBank.Services
{
    public class AccountService
    {
        /// <summary>
        /// Create a new Account and link to Customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="agency"></param>
        /// <param name="accountType"></param>
        /// <param name="currentBalance"></param>
        /// <returns>Account</returns>
        /// <exception cref="Exception"></exception>
        public Account CreateAccount(int customerId, int agency, string accountType, double currentBalance)
        {
            if (customerId == 0)
                throw new Exception("Customer (ID: 0) not found");

            if (agency < 1)
                throw new Exception($"Agency (ID: {agency}) is invalid");

            if (!ValidateAccountType(accountType))
                throw new Exception($"{accountType} not is a account type valid");

            if (!ValidateCustomerWithAccount(customerId))
                throw new Exception($"Customer (ID: {customerId}) not found or already has a linked account");

            int accountNumber = new Random().Next(1, 99999);

            while (!ValidateNumberAgency(accountNumber, agency))
                accountNumber = new Random().Next(1, 99999);

            string sql = @"INSERT INTO Account (NUMBER, AGENCY, ACCOUNT_TYPE, CURRENT_BALANCE)
VALUES (@Number, @Agency, @AccountType, @CurrentBalance)";

            Dictionary<string, object> parameters = new()
            {
                { "Number", accountNumber },
                { "Agency", agency},
                { "AccountType", accountType},
                { "CurrentBalance", currentBalance},
            };

            SqlServerConnection conn = SqlServerConnection.GetInstance();

            conn.NonQuery(sql, parameters);

            ExecuteLinkAccountToCustomer(customerId, accountNumber, agency);

            return new Account(accountNumber, agency, accountType, currentBalance);
        }

        /// <summary>
        /// Link a existent Account to a existent Customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="number"></param>
        /// <param name="agency"></param>
        /// <returns>String</returns>
        /// <exception cref="Exception"></exception>
        public String LinkAccountToCustomer(int customerId, int number, int agency)
        {
            if (customerId == 0)
                throw new Exception("Customer (ID: 0) not found");

            if (agency < 1 || number < 1)
                throw new Exception($"Number (ID: {number}) or Agency (ID: {agency}) is invalid");

            if (!ValidateCustomerWithAccount(customerId))
                throw new Exception($"Customer (ID: {customerId}) not found or already has a linked account");

            if (ValidateNumberAgency(number, agency))
                throw new Exception($"Account (Number {number} and Agency {agency}) not found");

            ExecuteLinkAccountToCustomer(customerId, number, agency);

            return "Customer linked to account";
        }

        #region InternalMethods

        internal bool ValidateCustomerWithoutAccount(int customerID)
        {
            string sql = $"SELECT ACCOUNT_NUMBER FROM Customer WHERE ID = {customerID}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            if (result.Count == 0)
                return false;

            return !String.IsNullOrEmpty(result[0]["ACCOUNT_NUMBER"]);
        }

        #endregion InternalMethods

        #region PrivateMethods

        private bool ValidateAccountType(string accountType)
        {
            string[] accountTypesAvaliable = { "CC", "CP" };

            return accountTypesAvaliable.Contains(accountType);
        }

        private bool ValidateNumberAgency(int number, int agency)
        {
            string sql = $"SELECT 1 as TEST FROM Account WHERE NUMBER = {number} AND AGENCY = {agency}";

            SqlServerConnection conn = SqlServerConnection.GetInstance();
            var result = conn.Query(sql);

            return result.Count == 0;
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

        private void ExecuteLinkAccountToCustomer(int customerId, int number, int agency)
        {
            SqlServerConnection conn = SqlServerConnection.GetInstance();

            string sql = @"UPDATE Customer 
  SET [ACCOUNT_AGENCY] = @AccountAgency, [ACCOUNT_NUMBER] = @AccountNumber
WHERE ID = @CustomerId";

            Dictionary<string, object> parameters = new()
            {
                { "AccountNumber", number },
                { "AccountAgency", agency },
                { "CustomerId", customerId },
            };

            conn.NonQuery(sql, parameters);
        }

        #endregion PrivateMethods

    }
}

