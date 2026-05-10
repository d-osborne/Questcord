using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

string tokenStr = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "VERY_IMPORTANT.txt"));
BotToken token = new BotToken(tokenStr);

/* Load the saved data. */
Data.Init();

/* Create the necessary state. */
GatewayClient client = new GatewayClient(token, new GatewayClientConfiguration
    {
        Logger = new ConsoleLogger(),
    });

/* Add the init command. */
ApplicationCommandService<ApplicationCommandContext> applicationCommandService = new();
applicationCommandService.AddModules(typeof(Program).Assembly);
//applicationCommandService.AddSlashCommand(new SlashCommandBuilder("init", "Initialise the bot with a channel to create threads under.", (Channel channel) => "Registered " + channel.Id.ToString() + " as the channel."));
/*MessageProperties message = "Hello, world!";
await client.Rest.SendMessageAsync(1502745018383863819, message);*/

/* Handle the init command. */
client.InteractionCreate += async interaction =>
{
    if(interaction is not ApplicationCommandInteraction applicationCommandInteraction)
        return;
    
    var result = await applicationCommandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, client));

    /* Get the entry for the server. */
    
    
    if(result is not IFailResult failResult)
        return;
    
    await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
};

await applicationCommandService.RegisterCommandsAsync(client.Rest, client.Id);
await client.StartAsync();
await Task.Delay(-1);