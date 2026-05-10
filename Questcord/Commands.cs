using System.Net;
using System.Net.Http.Headers;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

public class CommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private class CollectAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            IEnumerable<ApplicationCommandOptionChoiceProperties> choices = new ApplicationCommandOptionChoiceProperties[0];

            /* Show all the items in the room. */
            IEnumerable<Interactable> items = Game.GetRoomItems(context.Interaction.Guild!.Id, context.Interaction.User.Id);
            foreach(Interactable item in items)
            {
                ApplicationCommandOptionChoiceProperties newProps = new ApplicationCommandOptionChoiceProperties(item.name, item.id);
                choices = choices.Append(newProps);
            }

            return new ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
        }
    }

    [SlashCommand("init", "Initialise the bot with a channel to create threads under.")]
    public string ChannelInit(Channel channel)
    {
        ulong test = Context.Guild!.Id;

        ServerEntry entry = Data.GetEntry(Context.Guild!.Id);
        entry.channelId = channel.Id;
        Data.Save();

        return $"Registered {channel.Id} as the target channel.";
    }

    [SlashCommand("play", "Start a new game or continue an existing one.")]
    public async Task<string> PlayGame()
    {
        ulong thread; bool old = false;

        /* Let's see if we have a thread already. */
        ServerEntry serverEntry = Data.GetEntry(Context.Guild!.Id);
        UserEntry userEntry = Data.GetUserEntry(Context.Guild!.Id, Context.User.Id);

        if (userEntry.threadId != 0)
        {
            /* We have one! */
            thread = userEntry.threadId; old = true;
        } else
        {
            /* Create a new thread. */
            GuildThreadProperties properties = new GuildThreadProperties(Context.User.Username);
            GuildThread newThread = await Context.Client.Rest.CreateGuildThreadAsync(serverEntry.channelId, properties);

            thread = newThread.Id;
            userEntry.threadId = thread;
        }

        Data.Save();

        /* Send a welcome message. */
        string roomDescription = Game.GetRoomDescription(Context.Guild!.Id, Context.User.Id);

        MessageProperties msgProps = new MessageProperties();
        msgProps.Content = $"Welcome {((old == true) ? "back " : "")}to Questcord, <@{Context.User.Id}>!\nType **/look** to look around and /items to view your inventory.\n\n{Game.VisitRoom(Context.Guild!.Id, Context.User.Id, Game.GetCurrentRoom(Context.Guild!.Id, Context.User.Id))}";
        await Context.Client.Rest.SendMessageAsync(thread, msgProps);

        return $"Go to your thread to play!";
    }

    /* Now, the actual game commands. */
    [SlashCommand("look", "Look around the current room.")]
    public string GameLook()
    {
        UserEntry userEntry = Data.GetUserEntry(Context.Guild!.Id, Context.User.Id);
        if(Context.Channel.Id != userEntry.threadId) throw new Exception("Game commands should be run within your personal thread.");

        return Game.GetRoomDescription(Context.Guild!.Id, Context.User.Id);
    }



    [SlashCommand("collect", "Pick up an item.")]
    public string GameCollect([SlashCommandParameter(AutocompleteProviderType = typeof(CollectAutocompleteProvider))] string item)
    {
        return Game.CollectItem(Context.Guild!.Id, Context.User.Id, item);
    }
}