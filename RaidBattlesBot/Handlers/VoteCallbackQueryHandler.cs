﻿using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaidBattlesBot.Model;
using Telegram.Bot.Types;

namespace RaidBattlesBot.Handlers
{
  [CallbackQueryHandler(DataPrefix = "vote")]
  public class VoteCallbackQueryHandler : ICallbackQueryHandler
  {
    private readonly RaidBattlesContext myContext;
    private readonly RaidService myRaidService;

    public VoteCallbackQueryHandler(RaidBattlesContext context, RaidService raidService)
    {
      myContext = context;
      myRaidService = raidService;
    }

    public async Task<string> Handle(CallbackQuery data, object context = default, CancellationToken cancellationToken = default)
    {
      var callback = data.Data.Split(':');
      if (callback[0] != "vote")
        return null;
      
      if (!int.TryParse(callback.ElementAt(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var pollId))
        return null;

      var poll = await myContext
        .Polls
        .Where(_ => _.Id == pollId)
        .Include(_ => _.Votes)
        .Include(_ => _.Messages)
        .Include(_ => _.Raid)
        .FirstOrDefaultAsync(cancellationToken);

      if (poll == null)
        return null;

      var user = data.From;

      var vote = poll.Votes.SingleOrDefault(v => v.UserId == user.Id);
      if (vote == null)
      {
        poll.Votes.Add(vote = new Vote());
      }

      vote.User = user; // update firstname/lastname if necessary

      int? GetTeam(string team)
      {
        switch (team)
        {
          case "red": return 0;
          case "yellow": return 1;
          case "blue": return 2;
          case "none": return 3;
          case "cancel": return 4;
        }

        return null;
      }

      var teamStr = callback.ElementAt(2);
      vote.Team = GetTeam(teamStr);
      var changed = await myContext.SaveChangesAsync(cancellationToken) > 0;
      if (changed)
      {
        await myRaidService.UpdatePoll(poll, cancellationToken);
      }

      return changed ? $"You've voted for {teamStr}" : "You've already voted.";
    }
  }
}