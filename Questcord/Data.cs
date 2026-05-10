/* Handle data for each server. */

using System.Text.Json;

public class ServerEntry
{
    public ulong channelId { get; set; } = 0;
    public Dictionary<ulong, ulong> threads { get; set; } = new Dictionary<ulong, ulong>();
}

public static class Data
{
    private static Dictionary<ulong, ServerEntry> servers = new Dictionary<ulong, ServerEntry>();

    public static void Save()
    {
        string newContents = JsonSerializer.Serialize(servers);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "data.json"), newContents);
    }

    public static void Init()
    {
        /* Fetch the JSON and load it, if it exists. */
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "data.json");
        if (File.Exists(filePath))
        {
            string fileContents = File.ReadAllText(filePath);
            servers = JsonSerializer.Deserialize<Dictionary<ulong, ServerEntry>>(fileContents);

            if(servers == null) throw new Exception("Database file corrupt");
        } else Save();
    }

    public static ServerEntry GetEntry(ulong serverId)
    {
        ServerEntry entry;
        if(!servers.ContainsKey(serverId))
        {
            servers[serverId] = new ServerEntry();
            entry = servers[serverId];
            Save();
        } else entry = servers[serverId];
        return entry;
    }
}