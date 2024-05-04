namespace MiniBank.Classes
{
    public class Account : AccountInput
    {
        public int Number { get; set; }

        public Account(int number, int agency, string accountType, double currentBalance)
            : base(agency, accountType, currentBalance)
        {
            Number = number;
        }
    }

    public class AccountInput
    {
        public int Agency { get; set; }
        private string _AccountType { get; set; }
        public string AccountType { 
            get 
            { 
                return _AccountType; 
            } 
            set 
            {
                _AccountType = value.ToUpper(); 
            } 
        }
        public double CurrentBalance { get; set; }

        public AccountInput(int agency, string accountType, double currentBalance)
        {
            Agency = agency;
            AccountType = accountType;
            CurrentBalance = currentBalance;
        }
    }
}
