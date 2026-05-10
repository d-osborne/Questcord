/* Handle data for each server. */

using System.Text.Json;
using NetCord;

public class UserEntry
{
    public ulong threadId { get; set; } = 0;

    /* We need to save the game state. */
    public GameState state { get; set; } = new GameState();
}

public class ServerEntry
{
    public ulong channelId { get; set; } = 0;
    public Dictionary<ulong, UserEntry> threads { get; set; } = new Dictionary<ulong, UserEntry>();
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
            servers = JsonSerializer.Deserialize<Dictionary<ulong, ServerEntry>>(fileContents)!;

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

    public static UserEntry GetUserEntry(ulong serverId, ulong userId)
    {
        ServerEntry serverEntry = GetEntry(serverId);
        UserEntry userEntry;

        if (!serverEntry.threads.ContainsKey(userId))
        {
            /* We need to make a new state for the user. */
            userEntry = new UserEntry();
            
            /* Create a new game state. */
            string fileContents = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "game.json"));
            userEntry.state = JsonSerializer.Deserialize<GameState>(fileContents)!;

            serverEntry.threads[userId] = userEntry;
        } else
        {
            userEntry = serverEntry.threads[userId];
        }

        Data.Save();

        return userEntry;
    }
}