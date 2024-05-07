namespace MiniBank.Models
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
        public int TransactionId { get; set; }
        public double Value { get; set; }
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
        public DateTime Dthr { get; set; }
        public string? Obs { get; set; }

        public AccountTransactionClean(int transactionId, double value, string transactionType, DateTime dthr, string? obs)
        {
            TransactionId = transactionId;
            Value = value;
            TransactionType = transactionType;
            Dthr = dthr;
            Obs = obs;
        }
    }
}
