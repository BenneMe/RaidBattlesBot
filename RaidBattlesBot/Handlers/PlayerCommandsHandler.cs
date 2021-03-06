using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaidBattlesBot.Model;
using Team23.TelegramSkeleton;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RaidBattlesBot.Handlers
{
  [MessageEntityType(EntityType = MessageEntityType.BotCommand)]
  public class PlayerCommandsHandler : IMessageEntityHandler
  {
    private readonly RaidBattlesContext myContext;
    private readonly Message myMessage;

    public PlayerCommandsHandler(RaidBattlesContext context, Message message, ITelegramBotClient telegramBotClient)
    {
      myContext = context;
      myMessage = message;
    }

    public async Task<bool?> Handle(MessageEntityEx entity, PollMessage context = default, CancellationToken cancellationToken = default)
    {
      if (entity.Message.Chat.Type != ChatType.Private)
        return false;
      
      var commandText = entity.AfterValue.Trim();
      switch (entity.Command.ToString().ToLowerInvariant())
      {
        case "/ign":
        case "/nick":
        case "/nickname":
          var author = entity.Message.From;
          var player = await myContext.Set<Player>().Where(p => p.UserId == author.Id).FirstOrDefaultAsync(cancellationToken);
          var nickname = commandText.ToString();
          if (string.IsNullOrEmpty(nickname))
          {
            if (player != null)
            {
              myContext.Remove(player);
            }
          }
          else
          {
            if (player == null)
            {
              player = new Player
              {
                UserId = author.Id
              };
              myContext.Add(player);
            }

            player.Nickname = nickname;
          }
          await myContext.SaveChangesAsync(cancellationToken);
          return true;

        default:
          return false;
      }
    }
  }
}