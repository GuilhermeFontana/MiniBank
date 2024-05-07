namespace MiniBank.Models
{
    public class Statement
    {
        public int AccountNumber { get; set; }
        public int AccountAgency { get; set; }
        public double CurrentBalance { get; set; }
        public double TotalValue { get; set; }
        public List<AccountTransactionClean> Transactions { get; set; }

        public Statement(int accountNumber, int accountAgency, double currentBalance, double totalValue, List<AccountTransactionClean> transactions)
        {
            AccountNumber = accountNumber;
            AccountAgency = accountAgency;
            CurrentBalance = currentBalance;
            TotalValue = totalValue;
            Transactions = transactions;
        }
    }
}
