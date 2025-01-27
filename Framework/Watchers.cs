using System.Reflection;

namespace pixel_lab;

public static class Watchers
{
    private static Dictionary<string, FileSystemWatcher> _watchers = new();

    public static void Watch(string filePath, Action<FileSystemEventArgs> callback)
    {
        filePath = FindFild(filePath);
        if (!_watchers.TryGetValue(filePath, out var watcher))
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null)
                throw new InvalidOperationException($"Directory '{directoryName}' not found.");
            watcher = new FileSystemWatcher(directoryName);
            watcher.EnableRaisingEvents = true;
            watcher.Filter = Path.GetFileName(filePath);
            watcher.Changed += (sender, args) => callback(args);
            _watchers[directoryName] = watcher;
        }
    }

    public static string FindFild(string file)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        while (true)
        {
            var test = Path.Combine(dir, file);
            if (File.Exists(test))
                return test;
            dir = Path.GetDirectoryName(dir);
            if (dir == null)
                throw new InvalidOperationException($"File '{file}' not found.");
        }
    }
}