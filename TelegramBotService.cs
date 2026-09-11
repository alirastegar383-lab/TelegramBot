using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WebApplication1;

public class TelegramBotService
{
    private readonly TelegramBotClient _bot;

    public TelegramBotService(IConfiguration configuration)
    {
        var token = configuration["TelegramBotToken"];

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Telegram bot token is missing.");

        _bot = new TelegramBotClient(token);
    }

    public async Task StartAsync()
    {
        var me = await _bot.GetMe();

        Console.WriteLine($"Bot started: @{me.Username}");

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync
        );
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("UPDATE RECEIVED!");

        if (update.CallbackQuery is not null)
        {
            var callback = update.CallbackQuery;

            await botClient.AnswerCallbackQuery(
                callback.Id,
                cancellationToken: cancellationToken
            );

            var chatId = callback.Message!.Chat.Id;

            string gameName = callback.Data switch
            {
                "gta" => "GTA",
                "god_of_war" => "God of War",
                "assassins_creed" => "Assassin's Creed",
                "last_of_us" => "The Last of Us",
                "resident_evil" => "Resident Evil",
                _ => "Unknown"
            };

            string imagePath = callback.Data switch
            {
                "gta" => "Images/GTA.jpg",
                "god_of_war" => "Images/GodOfWar.jpg",
                "assassins_creed" => "Images/AssassinsCreed.jpg",
                "last_of_us" => "Images/LastOfUs.jpg",
                "resident_evil" => "Images/ResidentEvil.jpg",
                _ => ""
            };

            string description = callback.Data switch
            {
                "gta" =>
                    "🎮 Grand Theft Auto (GTA)\n\nیک مجموعه معروف جهان‌باز از شرکت Rockstar Games است که روی داستان، رانندگی، مأموریت‌ها و آزادی عمل بازیکن تمرکز دارد.",

                "god_of_war" =>
                    "⚔️ God of War\n\nیک مجموعه اکشن و ماجراجویی معروف از Sony است که داستان Kratos و مبارزات او با خدایان و موجودات افسانه‌ای را دنبال می‌کند.",

                "assassins_creed" =>
                    "🗡️ Assassin's Creed\n\nیک مجموعه اکشن و ماجراجویی تاریخی از Ubisoft است که ترکیبی از مخفی‌کاری، مبارزه و کاوش در دوره‌های مختلف تاریخی را ارائه می‌دهد.",

                "last_of_us" =>
                    "🍄 The Last of Us\n\nیک بازی داستان‌محور و احساسی از Naughty Dog است که رابطه Joel و Ellie را در دنیایی گرفتارشده با یک بیماری مرموز روایت می‌کند.",

                "resident_evil" =>
                    "🧟 Resident Evil\n\nیک مجموعه ترسناک و بقا از Capcom است که با موجودات ترسناک، معماها و مبارزه برای زنده ماندن شناخته می‌شود.",

                _ => "بازی مورد نظر پیدا نشد."
            };

            if (File.Exists(imagePath))
            {
                await botClient.SendPhoto(
                    chatId,
                    InputFile.FromStream(
                        File.OpenRead(imagePath),
                        Path.GetFileName(imagePath)
                    ),
                    caption: description,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(
                    chatId,
                    $"❌ عکس بازی {gameName} پیدا نشد.",
                    cancellationToken: cancellationToken
                );
            }

            return;
        }

        if (update.Message?.Text is not string text)
            return;

        var messageChatId = update.Message.Chat.Id;
if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🎮 GTA", "gta"),
                    InlineKeyboardButton.WithCallbackData("⚔️ God of War", "god_of_war")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🗡️ Assassin's Creed", "assassins_creed"),
                    InlineKeyboardButton.WithCallbackData("🍄 The Last of Us", "last_of_us")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🧟 Resident Evil", "resident_evil")
                }
            });

            await botClient.SendMessage(
                messageChatId,
                "👋 سلام! خوش اومدی 🎮\n\nلطفاً یکی از بازی‌های زیر رو انتخاب کن:",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );

            return;
        }

        if (text.Equals("سلام", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                messageChatId,
                "سلام 👋😊\nخوش اومدی! حالت چطوره؟",
                cancellationToken: cancellationToken
            );

            return;
        }

        await botClient.SendMessage(
            messageChatId,
            $"شما گفتید: {text}",
            cancellationToken: cancellationToken
        );
    }

    private Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"BOT ERROR: {exception.Message}");

        return Task.CompletedTask;
    }
}
