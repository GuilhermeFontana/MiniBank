namespace MiniBank.Classes
{
    public class AccountTransaction : AccountTransactionInOutput
    {
        public int ID { get; set; }
        public DateTime Dthr { get; set; }

        public AccountTransaction(int iD, DateTime dthr, int accountNumber, int accountAgency, string transactionType, double value, string? obs)
            : base(accountNumber, accountAgency, transactionType, value, obs)
        {
            ID = iD;
            Dthr = dthr;
        }
    }

    public class AccountTransactionInOutput
    {
        public int AccountNumber { get; set; }
        public int AccountAgency { get; set; }
        private string _TransactionType { get; set; }
        public string TransactionType
        {
            get
            {
                return _TransactionType;
            }
            set
            {
                _TransactionType = value.ToUpper();
            }
        }
        public double Value { get; set; }
        public string? Obs { get; set; }

        public AccountTransactionInOutput(int accountNumber, int accountAgency, string transactionType, double value, string? obs)
        {
            AccountNumber = accountNumber;
            AccountAgency = accountAgency;
            TransactionType = transactionType;
            Value = value;
            Obs = obs;
        }
    }

    public class AccountTransactionClean
    {
        public int ID { get; set; }
        public DateTime Dthr { get; set; }
        private string _TransactionType { get; set; }
        public string TransactionType
        {
            get
            {
                return _TransactionType;
            }
            set
            {
                _TransactionType = value.ToUpper();
            }
        }
        public double Value { get; set; }
        public string? Obs { get; set; }

        public AccountTransactionClean(int iD, DateTime dthr, string transactionType, double value, string? obs)
        {
            ID = iD;
            Dthr = dthr;
            TransactionType = transactionType;
            Value = value;
            Obs = obs;
        }
    }
}
