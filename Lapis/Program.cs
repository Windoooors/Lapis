using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using Lapis.Commands;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.PrivateCommands;
using Lapis.Commands.UniversalCommands;
using Lapis.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Lapis;

public class BotConfiguration
{
    public static BotConfiguration Instance;
    [JsonProperty] public string AccessToken;

    [JsonProperty] public string Address;

    [JsonProperty] public long AdministratorQqNumber;

    [JsonProperty] public string AliasUrl;
    [JsonProperty] public string BotName;
    [JsonProperty] public long BotQqNumber;
    [JsonProperty] public string DeepSeekApiToken;
    [JsonProperty] public string DeepSeekUrl;
    [JsonProperty] public string DivingFishDevToken;
    [JsonProperty] public string DivingFishUrl;
    [JsonProperty] public string WahlapConnectiveKitsUrl;
}

public class Program
{
    public static readonly Command[] Commands =
    [
        new TaskHandleQueueCommand(),
        // new AbuseCommand(),
        new VocabularyCommand(),
        new StickerCommand(),
        new AboutCommand(),
        new PingCommand(),
        new GroupMemberCommand(),
        new MaiCommand(),
        new SettingsCommand(),
        new HelpCommand(),
        new StickerSavingCommand(),
        new QuitCommand(),
        new TooLongDontReadCommand()
    ];

    private static DateTime _lastDailyRefreshTime;

    private static DateTime _lastWeeklyRefreshTime;

    private static DateTime _lastHourlyRefreshTime;

    private static readonly ApiOperator ApiOperator = new();

    private static BotConfiguration _botConfiguration = new();

    public static CqWsSession Session;

    public static ILogger<Program> Logger;

    private static readonly List<Task> InitializationTasks = new();

    private static bool _initializationStateLogged;

    public static event EventHandler DateChanged;

    public static event EventHandler WeekChanged;

    public static event EventHandler HourChanged;

    public static event EventHandler TimeChanged;

    public static Task Main()
    {
        if (!Directory.Exists(AppContext.BaseDirectory + "/data"))
            Directory.CreateDirectory(AppContext.BaseDirectory + "/data");

        Logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
        Logger.LogInformation("Program has started.");

        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "config.json")))
        {
            _botConfiguration =
                JsonConvert.DeserializeObject<BotConfiguration>(
                    File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "config.json")).Result);
        }
        else
        {
            File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "config.json"),
                JsonConvert.SerializeObject(_botConfiguration));
            Console.WriteLine(
                $"Please set up the Lapis via editing \"{Path.Combine(AppContext.BaseDirectory, "config.json")}\"");
            return Task.CompletedTask;
        }

        BotConfiguration.Instance = _botConfiguration;
        ApiOperator.Instance = ApiOperator;

        Session = new CqWsSession(new CqWsSessionOptions
        {
            BaseUri = new Uri("ws://" + _botConfiguration.Address),
            AccessToken = _botConfiguration.AccessToken
        });

        Session.StartAsync();

        Console.CancelKeyPress += Console_CancelKeyPress;

        Logger.LogInformation("Initializing...");

        foreach (var command in Commands)
        {
            var task = new Task(() => command.StartInitializing());
            InitializationTasks.Add(task);
            task.Start();
        }

        var commandParser = new CommandParser();

        Session.UseGroupMessage(async (context, next) =>
        {
            commandParser.StartParsing(context);
            await next.Invoke();
        });

        Session.UsePrivateMessage(async (context, next) =>
        {
            commandParser.StartParsing(context);
            await next.Invoke();
        });

        Session.UseFriendRequest(async (context, next) =>
        {
            await Session.ApproveFriendRequestAsync(context.Flag, "");
            var thread = new Task(() => Welcome(context.UserId));
            thread.Start();
            await next.Invoke();
        });

        Session.UseGroupRequest(async (context, next) =>
        {
            await Session.ApproveGroupRequestAsync(context.Flag, context.GroupRequestType);
            await next.Invoke();
        });

        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data/date.json")))
            _lastDailyRefreshTime =
                JsonConvert.DeserializeObject<DateTime>(File
                    .ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "data/date.json")).Result);

        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "data/week.json")))
            _lastWeeklyRefreshTime =
                JsonConvert.DeserializeObject<DateTime>(File
                    .ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "data/week.json")).Result);

        var thread = new Thread(CountTime);
        thread.Start();

        Console.ReadLine();

        return Task.CompletedTask;
    }

    private static void Welcome(long userId)
    {
        Thread.Sleep(3000);
        Session.SendPrivateMessageAsync(userId,
        [
            new CqTextMsg(
                $"感谢使用！请邀请 {BotConfiguration.Instance.BotName} 进入您的群聊！\n若要保存表情，请直接将表情发送给 {BotConfiguration.Instance.BotName}")
        ]);
    }

    private static void SaveDate()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/date.json"),
            JsonConvert.SerializeObject(_lastDailyRefreshTime));
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/week.json"),
            JsonConvert.SerializeObject(_lastWeeklyRefreshTime));
        Logger.LogInformation("Date data have been saved.");
    }

    private static void CountTime()
    {
        try
        {
            while (true)
            {
                Thread.Sleep(1000);

                var completed = true;

                foreach (var task in InitializationTasks)
                    if (!task.IsCompleted)
                    {
                        completed = false;
                        break;
                    }

                if (!completed)
                    continue;

                if (!_initializationStateLogged)
                {
                    _initializationStateLogged = true;
                    Logger.LogInformation("Initialized.");
                }

                if (TimeChanged != null)
                    TimeChanged(new object(), EventArgs.Empty);

                if (_lastDailyRefreshTime.Date != DateTime.Now.Date)
                {
                    _lastDailyRefreshTime = DateTime.Now;
                    Logger.LogInformation("Date change detected, refreshing data...");
                    SaveDate();
                    DateChanged?.Invoke(new object(), EventArgs.Empty);
                }

                if (DateTime.Now.Day - _lastWeeklyRefreshTime.Day >= 7)
                {
                    _lastWeeklyRefreshTime = DateTime.Now;
                    Logger.LogInformation("Week changed.");
                    WeekChanged?.Invoke(new object(), EventArgs.Empty);
                }

                if (_lastHourlyRefreshTime.Hour != DateTime.Now.Hour)
                {
                    _lastHourlyRefreshTime = DateTime.Now;
                    HourChanged?.Invoke(new object(), EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Unknown error:\n\n" + ex.StackTrace + "\n\n" + ex.Message);
        }
    }

    private static void Console_CancelKeyPress(object sender, EventArgs e)
    {
        foreach (var command in Commands)
            command.StartUnloading();

        SaveDate();
    }
}