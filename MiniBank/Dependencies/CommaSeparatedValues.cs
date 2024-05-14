using MiniBank.AbstractClasses;

namespace MiniBank.Dependencies
{
    public class CommaSeparatedValues
    {
        public void CreateFile<T>(string fileName, T data) where T : AToCSV
        {
            List<string> lines = data.ToCsvContent();

            string filePath = $"{Directory.GetCurrentDirectory()}{Environment.Variables.GetValue<string>("TempFilesPath")}";

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, $"{fileName}.csv")))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
        }
    }
}
