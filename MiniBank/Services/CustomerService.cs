using MiniBank.Dependencies;
using MiniBank.Models;
using System.Globalization;

namespace MiniBank.Services
{
    public class CustomerService
    {
        /// <summary>
        /// Create a new Customer
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="birthDate"></param>
        /// <param name="email"></param>
        /// <param name="address"></param>
        /// <returns>Customer</returns>
        /// <exception cref="Exception"></exception>
        public Customer CreateCustomer(string firstName, string lastName, string birthDate,string email, string? address)
        {
            string _firstName = firstName.Trim(), _lastName = lastName, _birthDate = birthDate.Trim(), _email = email.Trim();
            string? _address = address?.Trim();

            if (String.IsNullOrEmpty(_firstName))
                throw new Exception("FirstName field is required");
            if (String.IsNullOrEmpty(_lastName))
                throw new Exception("LastName field is required");
            if (String.IsNullOrEmpty(_birthDate))
                throw new Exception("BirthDate field is required");
            if (String.IsNullOrEmpty(_email))
                throw new Exception("Email field is required");

            string sql = @"INSERT INTO Customer (FIRST_NAME, LAST_NAME, BIRTH_DATE, EMAIL, ADDRESS) 
OUTPUT Inserted.ID
VALUES(@First_Name, @Last_Name, @Birth_Date, @Email, @Address)";

            Dictionary<string, object> parameters = new()
            {
                { "First_Name", _firstName },
                { "Last_Name", _lastName },
                { "Birth_Date", _birthDate },
                { "Email", _email },
                { "Address", address?.Trim() ?? "" }
            };

            SqlServerConnection conn = SqlServerConnection.GetInstance();

            int id = int.Parse(conn.OutputValue(sql, parameters));

            return new Customer(
                id,
                _firstName,
                _lastName,
                DateTime.Parse(birthDate, CultureInfo.InvariantCulture),
                _email,
                _address
            );
        }

        /// <summary>
        /// Update a existent Customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="birthDate"></param>
        /// <param name="email"></param>
        /// <param name="address"></param>
        /// <returns>Customer</returns>
        /// <exception cref="Exception"></exception>
        public Customer UpdateCustomer(int customerId, string? firstName, string? lastName, string? birthDate, string? email, string? address)
        {
            if (customerId == 0)
                throw new Exception("Customer (ID: 0) not found");
            
            string? _firstName = firstName?.Trim(), _lastName = lastName?.Trim(), _birthDate = birthDate?.Trim(), _email = email?.Trim(), _address = address?.Trim(); ;

            Customer customer = GetCustomerById(customerId);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (!String.IsNullOrEmpty(_firstName) && !customer.FirtsName.Equals(_firstName))
            {
                customer.FirtsName = _firstName;
                parameters.Add("First_Name", _firstName);
            }
            if (!String.IsNullOrEmpty(_lastName) && !customer.LastName.Equals(_lastName))
            {
                customer.LastName = _lastName;
                parameters.Add("Last_Name", _lastName);
            }
            if (!String.IsNullOrEmpty(_birthDate) && !customer.BirthDate.Equals(DateTime.Parse(_birthDate, CultureInfo.InvariantCulture)))
            {
                customer.BirthDate = DateTime.Parse(_birthDate, CultureInfo.InvariantCulture);
                parameters.Add("Birth_Date", _birthDate);
            }
            if (!String.IsNullOrEmpty(_email) && !customer.Email.Equals(_email))
            {
                customer.Email = _email;
                parameters.Add("Email", _email);
            }
            if ((String.IsNullOrEmpty(_address) && !String.IsNullOrEmpty(customer.Address)) 
                || (!String.IsNullOrEmpty(_address) && String.IsNullOrEmpty(customer.Address))
                || !customer.Address.Equals(_address))
            {
                customer.Address = _address;
                parameters.Add("Address", _address);
            }

            if (parameters.Count > 0)
            {
                SqlServerConnection conn = SqlServerConnection.GetInstance();

                string sql = $@"UPDATE Customer 
  SET {String.Join(", ", parameters.Select(x => $"{x.Key} = @{x.Key}").ToList())}
WHERE ID = {customerId}";

                conn.NonQuery(sql, parameters);
            }

            return customer;
        }

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

        private Customer GetCustomerById(int customerId)
        {
            string sql = $"SELECT * FROM Customer WHERE ID = @Id";

            SqlServerConnection conn = SqlServerConnection.GetInstance();

            List<Dictionary<string, string>> result = conn.Query(
                sql, 
                new Dictionary<string, object> { { "Id", customerId } }
            );

            if (result.Count == 0)
                throw new Exception($"Customer (ID: {customerId}) not found");

            return new Customer(
                int.Parse(result[0]["ID"]),
                result[0]["FIRST_NAME"],
                result[0]["LAST_NAME"],
                DateTime.Parse(result[0]["BIRTH_DATE"], CultureInfo.InvariantCulture),
                result[0]["EMAIL"],
                result[0]["ADDRESS"]
            );
        }
    }
}
