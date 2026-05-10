using System.Net.Http.Headers;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

public class CommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
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
        ServerEntry entry = Data.GetEntry(Context.Guild!.Id);
        if (entry.threads.ContainsKey(Context.User.Id))
        {
            /* We have one! */
            thread = entry.threads[Context.User.Id]; old = true;
        } else
        {
            /* Create a new thread. */
            GuildThreadProperties properties = new GuildThreadProperties(Context.User.Username);
            GuildThread newThread = await Context.Client.Rest.CreateGuildThreadAsync(entry.channelId, properties);

            thread = newThread.Id;
            entry.threads[Context.User.Id] = thread;
        }

        Data.Save();

        /* Send a welcome message. */
        MessageProperties msgProps = new MessageProperties();
        msgProps.Content = $"Welcome {((old == true) ? "back " : "")}to Questcord, <@{Context.User.Id}>!";
        await Context.Client.Rest.SendMessageAsync(thread, msgProps);

        return $"Go to your thread to play!";
    }
}