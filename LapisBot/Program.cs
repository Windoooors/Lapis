using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using LapisBot.GroupCommands;
using LapisBot.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace LapisBot;

public class BotConfiguration
{
    public static BotConfiguration Instance;

    public string Address;
    public long AdministratorQqNumber;
    public long BotQqNumber;
    public string AliasUrl;
    public string DivingFishUrl;
    public string WahlapConnectiveKitsUrl;
}

public class Program
{
    public static readonly List<GroupCommand> GroupCommands =
    [
        new TaskHandleQueueCommand(),
        new AbuseCommand(),
        new VocabularyCommand(),
        new HelpCommand(),
        new StickerCommand(),
        new AboutCommand(),
        new MaiCommand(),
        new RepeatCommand(),
        new GroupMemberCommand(),
        new SettingsCommand()
    ];

    private static DateTime _lastDateTime;

    private static DateTime _lastDateTimeHour;

    private static readonly ApiOperator ApiOperator = new();

    private static BotConfiguration _botConfiguration = new();

    public static CqWsSession Session;

    public static ILogger<Program> Logger;

    private static readonly List<Task> InitializationTasks = new();

    private static bool _initializationStateLogged;

    public static event EventHandler DateChanged;

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
                $"Please set up the Lapis Bot via editing \"{Path.Combine(AppContext.BaseDirectory, "config.json")}\"");
            return Task.CompletedTask;
        }

        BotConfiguration.Instance = _botConfiguration;
        ApiOperator.Instance = ApiOperator;

        Session = new CqWsSession(new CqWsSessionOptions
        {
            BaseUri = new Uri("ws://" + _botConfiguration.Address) // WebSocket 地址
        });

        Session.StartAsync();

        Console.CancelKeyPress += Console_CancelKeyPress;

        Logger.LogInformation("Initializing...");

        foreach (var command in GroupCommands)
        {
            var task = new Task(() => command.Initialize());
            InitializationTasks.Add(task);
            task.Start();
        }

        var commandParser = new CommandParser();

        Session.UseGroupMessage(async (context, next) =>
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
            _lastDateTime =
                JsonConvert.DeserializeObject<DateTime>(File
                    .ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "data/date.json")).Result);
        var thread = new Thread(Reload);
        thread.Start();

        Console.ReadLine();

        return Task.CompletedTask;
    }

    private static void Welcome(long userId)
    {
        Thread.Sleep(3000);
        Session.SendPrivateMessageAsync(userId, new CqMessage
            { new CqTextMsg("感谢使用！请邀请 Lapis Bot 进入您的群聊！\n若要保存表情，请直接将表情发送给 Lapis Bot") });
    }

    private static void SaveDate()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "data/date.json"),
            JsonConvert.SerializeObject(_lastDateTime));
        Logger.LogInformation("Date data has been saved.");
    }

    private static void Reload()
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

                if (_lastDateTime.Date != DateTime.Now.Date)
                {
                    _lastDateTime = DateTime.Now;
                    Logger.LogInformation("Date change detected, refreshing data...");
                    SaveDate();
                    if (DateChanged != null)
                        DateChanged(new object(), EventArgs.Empty);
                }

                if (_lastDateTimeHour.Hour != DateTime.Now.Hour)
                {
                    _lastDateTimeHour = DateTime.Now;
                    if (HourChanged != null)
                        HourChanged(new object(), new EventArgs());
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
        foreach (var command in GroupCommands)
            command.Unload();

        SaveDate();
    }
}