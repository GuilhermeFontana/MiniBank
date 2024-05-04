namespace MiniBank.Classes
{
    public enum AccountTypes
    {
        C,
        P
    }

    public class Account : AccountInput
    {
        public Customer Customer { get; set; }

        public Account(int number, int agency, AccountTypes accountType, double currentBalance, Customer customer) 
            : base(number, agency, accountType, currentBalance)
        {
            Customer = customer;
        }

    }

    public class AccountInput
    {
        public int Number { get; set; }
        public int Agency { get; set; }
        public AccountTypes AccountType { get; set; }
        public double CurrentBalance { get; set; }

        public AccountInput(int number, int agency, AccountTypes accountType, double currentBalance)
        {
            Number = number;
            Agency = agency;
            AccountType = accountType;
            CurrentBalance = currentBalance;
        }
    }
}
