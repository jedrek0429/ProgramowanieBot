﻿using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using ProgramowanieBot.Data;
using ProgramowanieBot.Handlers.InteractionHandlerModules.PreconditionAttributes;

namespace ProgramowanieBot.Handlers.InteractionHandlerModules.Commands.SlashCommands;

public class ResolveCommand(IServiceProvider serviceProvider, ConfigService config) : ApplicationCommandModule<SlashCommandContext>
{
    [RequireThreadOwnerOfHelpChannel<SlashCommandContext>]
    [SlashCommand("resolve", "Closes your post and specifies who helped you", NameTranslationsProviderType = typeof(NameTranslationsProvider), DescriptionTranslationsProviderType = typeof(DescriptionTranslationsProvider))]
    public async Task<InteractionCallback> ResolveAsync(
        [SlashCommandParameter(NameTranslationsProviderType = typeof(HelperNameTranslationsProvider), Description = "User who helped you", DescriptionTranslationsProviderType = typeof(HelperDescriptionTranslationsProvider))]
        [NoBot<SlashCommandContext>]
        User helper,
        [SlashCommandParameter(Name = "second_helper", NameTranslationsProviderType = typeof(Helper2NameTranslationsProvider), Description = "Another user who helped you", DescriptionTranslationsProviderType = typeof(Helper2DescriptionTranslationsProvider))]
        [NoBot<SlashCommandContext>]
        User? helper2 = null)
    {
        var channelId = Context.Interaction.Channel.Id;
        await using (var context = serviceProvider.GetRequiredService<DataContext>())
        {
            if (await context.Posts.AnyAsync(p => p.PostId == channelId && p.IsResolved))
                throw new(config.Interaction.PostAlreadyResolvedResponse);
        }

        var isHelper2 = helper2 != null && helper != helper2;
        var user = Context.User;
        return InteractionCallback.Message(new()
        {
            Content = $"**{config.Emojis.Success} {(isHelper2 ? string.Format(config.Interaction.WaitingForApprovalWith2HelpersResponse, helper, helper2) : string.Format(config.Interaction.WaitingForApprovalResponse, helper))}**",
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"approve:{helper.Id}:{helper != user}:{(isHelper2 ? helper2!.Id : null)}:{(isHelper2 ? helper2 != user : null)}", config.Interaction.ApproveButtonLabel, ButtonStyle.Success),
                }),
            },
            AllowedMentions = AllowedMentionsProperties.None,
        });
    }

    public class NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "rozwiązane" },
        };
    }

    public class DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Zamyka twojego posta i wskazuje kto Ci pomógł" },
        };
    }

    public class HelperNameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "pomocnik" },
        };
    }

    public class HelperDescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Osoba, która Ci pomogła" },
        };
    }

    public class Helper2NameTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "drugi_pomocnik" },
        };
    }

    public class Helper2DescriptionTranslationsProvider : ITranslationsProvider
    {
        public IReadOnlyDictionary<CultureInfo, string>? Translations => new Dictionary<CultureInfo, string>()
        {
            { new("pl"), "Kolejna osoba, która Ci pomogła" },
        };
    }
}
