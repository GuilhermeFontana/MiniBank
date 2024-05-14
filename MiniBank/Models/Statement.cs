using MiniBank.AbstractClasses;

namespace MiniBank.Models
{
    public class Statement : AToCSV
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

        public override List<string> ToCsvContent()
        {
            List<string> statementDictionary = new()
            {
                { $"AccountNumber; {AccountNumber};" },
                { $"AccountAgency;{AccountAgency};" },
                { $"CurrentBalance;{CurrentBalance};" },
                { $"TotalValue;{TotalValue};" },
                { "" },
                { $"Transactions;" },
            };

            if (Transactions.Count > 0)
            {
                statementDictionary.Add("Value;TransactionType;Dthr;Obs;");

                foreach (AccountTransactionClean transaction in Transactions)
                    statementDictionary.Add($"{transaction.TransactionType};{transaction.Value};{transaction.Dthr};{transaction.Obs};");
            }

            return statementDictionary;
        }
    }
}
