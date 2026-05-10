/* Game logic. */

using System.ComponentModel;
using System.Data;
using NetCord;

public enum InteractableType
{
    Exit,
    Item,
    Interact
}

public class Interactable
{
    public string id {get; set;} = "";
    public string name {get; set;} = "";
    public InteractableType type {get; set;} = InteractableType.Interact;
    public int state {get; set;} = 0;

    public string[] abbrDescriptions {get; set;} = new string[0];
    public string[] descriptions {get; set;} = new string[0];

    public string[] interactText {get; set;} = new string[0];
    public int[] interactTransitions {get; set;} = new int[0];

    public Dictionary<string, string> itemText {get; set;} = new Dictionary<string, string>();
    public Dictionary<string, int> itemTransitions {get; set;} = new Dictionary<string, int>();

    public bool[] exitUsable {get; set;} = new bool[0];
}

public class Room
{
    public string name { get; set; } = "";
    public string description {get; set;} = "";
    public string firstDescription {get; set;} = "";
    public bool visited {get; set;} = false;
    public Interactable[] interactables {get; set;} = new Interactable[0];
}

public class GameState
{
    public Room[] rooms {get; set;} = new Room[0];
    public int currentRoom {get; set;} = 0;
    public Dictionary<string, Interactable> currentItems {get; set;} = new Dictionary<string, Interactable>();
}

public static class Game
{
    public static string GetRoomTitle(ulong serverId, ulong userId)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        return entry.state.rooms[entry.state.currentRoom].name;
    }

    public static string GetRoomDescription(ulong serverId, ulong userId)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        Room room = entry.state.rooms[entry.state.currentRoom];
        string description = room.description;

        if(room.interactables.Length != 0)
        {
            description += "\n";
            foreach(Interactable interactable in room.interactables)
            {
                string interactableType = "";

                if(interactable.type == InteractableType.Exit) interactableType = "Path";
                if(interactable.type == InteractableType.Item) interactableType = "Item";
                if(interactable.type == InteractableType.Interact) interactableType = "Interactable";

                description += $"\n- _[{interactableType}]_ " + interactable.abbrDescriptions[interactable.state];
            }
        }

        return description;
    }

    public static string VisitRoom(ulong serverId, ulong userId, int room)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        entry.state.currentRoom = room;

        if (entry.state.rooms[room].visited)
        {
            /* We've already been here. Give a normal description. */
            return $"# {entry.state.rooms[room].name}\n{GetRoomDescription(serverId, userId)}";
        } else
        {
            entry.state.rooms[room].visited = true;
            return $"# {entry.state.rooms[room].name}\n{entry.state.rooms[room].firstDescription}\n\n{GetRoomDescription(serverId, userId)}";
        }
    }

    public static int GetCurrentRoom(ulong serverId, ulong userId)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        return entry.state.currentRoom;
    }

    public static IEnumerable<Interactable> GetRoomItems(ulong serverId, ulong userId)
    {
        IEnumerable<Interactable> items = new Interactable[0];

        UserEntry entry = Data.GetUserEntry(serverId, userId);
        Room room = entry.state.rooms[entry.state.currentRoom];

        foreach(Interactable inter in room.interactables)
        {
            if(inter.type == InteractableType.Item) items = items.Append(inter);
        }

        return items;
    }

    public static string CollectItem(ulong serverId, ulong userId, string item)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        Room room = entry.state.rooms[entry.state.currentRoom];

        int i = 0;
        foreach(Interactable inter in room.interactables)
        {
            if(inter.id == item)
            {
                /* Bingo! */
                if(inter.type == InteractableType.Item)
                {
                    /* Add it to the inventory. */
                    entry.state.currentItems[item] = inter;

                    List<Interactable> newInters = new List<Interactable>(room.interactables);
                    newInters.RemoveAt(i);
                    room.interactables = newInters.ToArray();

                    return inter.interactText[inter.state];
                } else
                {
                    return "You really think you can pick that up?";
                }
            }
            i++;
        }

        return "You try but fail to find such an item.";
    }

    public static string DescribeInventory(ulong serverId, ulong userId)
    {
        UserEntry entry = Data.GetUserEntry(serverId, userId);
        if(entry.state.currentItems.Keys.Count == 0) return "You have nothing. No property to your name. Think about fixing that.";

        string result = "";

        foreach(Interactable inter in entry.state.currentItems.Values)
        {
            result += $"\n- **{inter.name}** - {inter.descriptions}";
        }

        return result;
    }
}