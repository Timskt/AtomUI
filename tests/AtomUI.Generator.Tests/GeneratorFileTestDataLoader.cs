namespace AtomUI.Generator.Tests;

internal static class GeneratorFileTestDataLoader
{
    private static readonly string TestDataPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "TestData");
    
    public static string LoadInput(string filename)
    {
        var filePath = Path.Combine(TestDataPath, filename);
        return File.ReadAllText(filePath);
    }
    
    public static string LoadExpected(string filename)
    {
        var filePath = Path.Combine(TestDataPath, filename);
        return File.ReadAllText(filePath);
    }
}