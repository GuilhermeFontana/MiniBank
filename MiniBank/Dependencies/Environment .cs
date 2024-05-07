namespace MiniBank.Dependencies
{
    public class Environment
    {
        private static Environment? _instance;
        private IConfiguration? _variables { get; set; }

        public static IConfiguration Variables
        {
            get
            {
                if (_instance is null)
                    _instance = new Environment();

                _instance._variables ??= _instance.Configure();

                return _instance._variables;
            }
        }

        public IConfiguration Configure()
        {
            return new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", false)
               .AddEnvironmentVariables()
               .Build();
        }
    }
}
