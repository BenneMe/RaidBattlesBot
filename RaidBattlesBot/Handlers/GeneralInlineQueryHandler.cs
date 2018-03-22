﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DelegateDecompiler.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using RaidBattlesBot.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;

namespace RaidBattlesBot.Handlers
{
  [InlineQueryHandler]
  public class GeneralInlineQueryHandler : IInlineQueryHandler
  {
    private readonly ITelegramBotClient myBot;
    private readonly IUrlHelper myUrlHelper;
    private readonly UserInfo myUserInfo;
    private readonly RaidBattlesContext myDb;
    private readonly IClock myClock;

    public GeneralInlineQueryHandler(ITelegramBotClient bot, IUrlHelper urlHelper, UserInfo userInfo, RaidBattlesContext db, IClock clock)
    {
      myBot = bot;
      myUrlHelper = urlHelper;
      myUserInfo = userInfo;
      myDb = db;
      myClock = clock;
    }

    public async Task<bool?> Handle(InlineQuery data, object context = default, CancellationToken cancellationToken = default)
    {
      InlineQueryResult[] inlineQueryResults;

      var query = data.Query;
      if (string.IsNullOrWhiteSpace(query))
      {
        var userId = data.From.Id;
        var now = myClock.GetCurrentInstant().ToDateTimeOffset();

        // active raids or polls
        var polls = await myDb.Polls
          .IncludeRelatedData()
          .Where(_ => _.EndTime > now) // live poll
          .Where(_ => _.Raid.EggRaidId == null) // no eggs if boss is already known
          .Where(_ => _.Owner == userId || _.Votes.Any(vote => vote.UserId == userId))
          .OrderBy(_ => _.EndTime)
          //.Take(10)
          .DecompileAsync()
          .ToArrayAsync(cancellationToken);

        inlineQueryResults = await
          Task.WhenAll(polls.Select(poll => poll.ClonePoll(myUrlHelper, myUserInfo, cancellationToken)));
      }
      else
      {
        inlineQueryResults = await
          Task.WhenAll(VoteEnumEx.AllowedVoteFormats
          .Select(_ => new Poll
          {
            Title = query,
            AllowedVotes = _
          })
          .Select(async fakePoll => new InlineQueryResultArticle
          {
            Id = $"create:{query.GetHashCode()}:{fakePoll.AllowedVotes:D}",
            Title = fakePoll.GetTitle(myUrlHelper),
            Description = fakePoll.AllowedVotes?.Format(new StringBuilder("Создать голосование ")).ToString(),
            HideUrl = true,
            ThumbUrl = fakePoll.GetThumbUrl(myUrlHelper).ToString(),
            InputMessageContent = new InputTextMessageContent
            {
              MessageText = (await fakePoll.GetMessageText(myUrlHelper, myUserInfo, RaidEx.ParseMode, cancellationToken)).ToString(),
              ParseMode = RaidEx.ParseMode,
              DisableWebPagePreview = fakePoll.GetRaidId() == null
            },
            ReplyMarkup = fakePoll.GetReplyMarkup()
          }));
      }

      return await myBot.AnswerInlineQueryAsync(data.Id, inlineQueryResults, cacheTime: 0, cancellationToken: cancellationToken);
    }
  }
}