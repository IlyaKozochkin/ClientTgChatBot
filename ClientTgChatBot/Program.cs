using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TionitKozochkin.Models;


class Program
{
    // ПРИМЕЧАНИЕ: данные о пользователях и topic хранятся в оперативке, но при необходимости можно сохранять в формате log либо в БД(sqlite - оптимально)
    static List<Topic> topics = new List<Topic>();
    readonly static string token = "6265477851:AAHaQxx9TshTxE2DZEs6YTsj3YcZYFRscwM";
    readonly static long adminGroupId = -1001867545047;

    static async Task Main()
    {
        var botClient = new TelegramBotClient(token);
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;

        ReceiverOptions receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        botClient.StartReceiving(
            HandleUpdatesAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token);

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Бот начал работу | id бота: {me.Id}");
        Console.ReadLine();
        cts.Cancel();
    }

    async static Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandleMessage(botClient, update.Message);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Ошибка");
        }
    }

    async static Task HandleMessage(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine(message.Chat.Id);
        if (message.Type == MessageType.Text && message.Text != null)
        {
            Console.WriteLine(message.Chat.Id);
            if (message.Chat.Id == adminGroupId)
            {
                var topic = topics.FirstOrDefault(p => p.IdTopic == message.MessageThreadId.Value);
                await botClient.SendTextMessageAsync(topic.UserId, message.Text);
            } 
            else
            {
                bool topicWithUserNameExists = topics.Any(topic => topic.UserName == message.Chat.Username);

                if (topicWithUserNameExists)
                {
                    int idTopic = topics.FirstOrDefault(p => p.UserName == message.Chat.Username).IdTopic;
                    await botClient.SendTextMessageAsync(adminGroupId, replyToMessageId: idTopic, text: message.Text);
                }
                else
                {
                    ForumTopic topic = await botClient.CreateForumTopicAsync(chatId: adminGroupId, name: message.Chat.Username, Color.PinkColor);
                    Topic userTopic = new Topic(topic.MessageThreadId, topic.Name, message.Chat.Id);
                    topics.Add(userTopic);
                    int idTopic = topics.FirstOrDefault(p => p.UserName == message.Chat.Username).IdTopic;
                    await botClient.SendTextMessageAsync(adminGroupId, replyToMessageId: idTopic, text: message.Text);
                }
            }
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}