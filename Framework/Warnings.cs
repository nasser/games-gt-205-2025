namespace pixel_lab;

public static class Warnings
{
    private static readonly HashSet<string> Messages = [];

    public static void WriteOnce(string message)
    {
        if (Messages.Add(message))
            Console.WriteLine(message);
    }

    public static void ClearCache() => 
        Messages.Clear();
}