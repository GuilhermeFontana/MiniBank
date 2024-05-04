namespace MiniBank.Classes
{
    public class Customer
    {
        public int ID { get; set; }
        public string FirtsName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string? Address { get; set; }

        public Account? Account { get; set; }

        public Customer(int iD, string firtsName, string lastName, DateTime birthDate, string email, string? address)
        {
            ID = iD;
            FirtsName = firtsName;
            LastName = lastName;
            Email = email;
            BirthDate = birthDate;
            Address = address;
        }

        public Customer(int iD, string firtsName, string lastName, DateTime birthDate, string email, string? address, Account? account) 
            : this(iD, firtsName, lastName, birthDate, email, address)
        {
            Account = account;
        }
    }

    public class CustomerInput
    {
        public string? FirtsName { get; set; }
        public string? LastName { get; set; }
        public string? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public CustomerInput(string? firtsName, string? lastName, string? birthDate, string? email, string? address)
        {
            FirtsName = firtsName;
            LastName = lastName;
            Email = email;
            BirthDate = birthDate;
            Address = address;
        }
    }

}
