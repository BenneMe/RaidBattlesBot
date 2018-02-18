﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using Microsoft.AspNetCore.Mvc;
using RaidBattlesBot.Model;
using Telegram.Bot.Types;

namespace RaidBattlesBot.Handlers
{
  public class ChosenInlineResultHandler : IChosenInlineResultHandler
  {
    private readonly RaidBattlesContext myContext;
    private readonly RaidService myRaidService;
    private readonly IUrlHelper myUrlHelper;

    public ChosenInlineResultHandler(RaidBattlesContext context, RaidService raidService, IUrlHelper urlHelper)
    {
      myContext = context;
      myRaidService = raidService;
      myUrlHelper = urlHelper;
    }

    public async Task<bool?> Handle(ChosenInlineResult data, object context = default, CancellationToken cancellationToken = default)
    {
      var resultParts = data.ResultId.Split(':');
      if (resultParts[0] == "poll" && int.TryParse(resultParts.ElementAtOrDefault(1) ?? "", out var pollId))
      {
        myContext.Messages.Add(new PollMessage
        {
          PollId = pollId,
          InlineMesssageId = data.InlineMessageId
        });
        return await myContext.SaveChangesAsync(cancellationToken) > 0;
      }

      if (resultParts[0] == "create")
      {
        return await myRaidService.AddPoll(data.Query, FlagEnums.TryParseFlags(resultParts.ElementAtOrDefault(2), out VoteEnum allowedVotes, EnumFormat.DecimalValue, EnumFormat.Name) ?
            allowedVotes : VoteEnum.Standard, new PollMessage(data), myUrlHelper, cancellationToken);
      }

      return null;
    }
  }
}