﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaidBattlesBot.Model;
using Team23.TelegramSkeleton;
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
      switch (resultParts[0])
      {
        case PollEx.InlineIdPrefix:
          if (!PollEx.TryGetPollId(resultParts.ElementAtOrDefault(1), out var pollId, out var format))
            return null;

          return (await myRaidService.GetOrCreatePollAndMessage(new PollMessage(data) { PollId = pollId }, myUrlHelper, format, cancellationToken)) != null;
      }

      return null;
    }
  }
}