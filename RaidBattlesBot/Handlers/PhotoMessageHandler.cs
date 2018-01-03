﻿using System.Threading;
using System.Threading.Tasks;
using RaidBattlesBot.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RaidBattlesBot.Handlers
{
  [MessageType(MessageType = MessageType.PhotoMessage)]
  public class PhotoMessageHandler : IMessageHandler
  {
    public async Task<bool?> Handle(Message data, PollMessage pollMessage = default, CancellationToken cancellationToken = default)
    {
      return null;
    }
  }
}