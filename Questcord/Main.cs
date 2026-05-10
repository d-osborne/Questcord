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

/* Add the commands. */
ApplicationCommandService<ApplicationCommandContext, AutocompleteInteractionContext> applicationCommandService = new();
applicationCommandService.AddModules(typeof(Program).Assembly);

/* Handle the commands. */
client.InteractionCreate += async interaction =>
{
    if(interaction is not ApplicationCommandInteraction applicationCommandInteraction){
        if(interaction is AutocompleteInteraction autocompleteInteraction)
        {
            var autoResult = await applicationCommandService.ExecuteAutocompleteAsync(new AutocompleteInteractionContext(autocompleteInteraction, client));
            if(autoResult is not IFailResult autoFailResult)
                return;
            
            await interaction.SendResponseAsync(InteractionCallback.Message(autoFailResult.Message));
            return;
        }
        else return;
    };
    
    var result = await applicationCommandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, client));

    if(result is not IFailResult failResult)
        return;
    
    await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
};

await applicationCommandService.RegisterCommandsAsync(client.Rest, client.Id);
await client.StartAsync();
await Task.Delay(-1);