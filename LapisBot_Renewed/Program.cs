using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using LapisBot_Renewed.GroupCommands;
using System.Threading;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using LapisBot_Renewed.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace LapisBot_Renewed
{
    
    public class BotConfiguration
    {
        public static BotConfiguration Instance;
        
        public string Address;

        public string AliasUrl;
        public string DivingFishUrl;
        public string WahlapConnectiveKitsUrl;
        public string AircadeUrl;
        
        public long BotQqNumber;
        public long AdministratorQqNumber;
    }
    
    public class Program
    {
        public static readonly List<GroupCommand> GroupCommands = [];

        private static DateTime _lastDateTime;
        
        private static DateTime _lastDateTimeHour;

        private static readonly ApiOperator ApiOperator = new ();

        private static BotConfiguration _botConfiguration = new ();

        public static CqWsSession Session;

        public static event EventHandler DateChanged;
        
        public static event EventHandler HourChanged;

        public static event EventHandler TimeChanged;

        public static ILogger<Program> Logger; 
        
        private static readonly List<Task> InitializationTasks = new List<Task>();

        public static Task Main()
        {
            Logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
            Logger.LogInformation("Program has started.");
            
            if (File.Exists(AppContext.BaseDirectory + "config.json"))
                _botConfiguration =
                    JsonConvert.DeserializeObject<BotConfiguration>(
                        File.ReadAllTextAsync(AppContext.BaseDirectory + "config.json").Result);
            else
            {
                File.WriteAllTextAsync(AppContext.BaseDirectory + "config.json",
                    JsonConvert.SerializeObject(_botConfiguration));
                Console.WriteLine(
                    $"Please set up the Lapis Bot via editing \" {AppContext.BaseDirectory} config.json\"");
                return Task.CompletedTask;
            }
            
            BotConfiguration.Instance = _botConfiguration;
            ApiOperator.Instance = ApiOperator;
            
            Session = new CqWsSession(new CqWsSessionOptions()
            {
                BaseUri = new Uri("ws://" + _botConfiguration.Address),  // WebSocket 地址
            });
            
            Session.StartAsync();
            
            Console.CancelKeyPress += Console_CancelKeyPress;

            GroupCommands.Add(new TaskHandleQueueCommand());
            GroupCommands.Add(new AbuseCommand());
            GroupCommands.Add(new VocabularyCommand());
            GroupCommands.Add(new HelpCommand());
            GroupCommands.Add(new StickerCommand());
            GroupCommands.Add(new AboutCommand());
            GroupCommands.Add(new TaskHandleQueueCommand());
            GroupCommands.Add(new MaiCommand());
            GroupCommands.Add(new RepeatCommand());
            GroupCommands.Add(new SettingsCommand());

            foreach (GroupCommand command in GroupCommands)
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

            if (File.Exists(Environment.CurrentDirectory + "/date.json"))
            {
                _lastDateTime = JsonConvert.DeserializeObject<DateTime>(File.ReadAllTextAsync(Environment.CurrentDirectory + "/date.json").Result);
            }
            Thread thread = new Thread(Reload);
            thread.Start();
            
            Console.ReadLine();
            
            return Task.CompletedTask;
        }

        static void Welcome(long userId)
        { 
            Thread.Sleep(3000);
            Session.SendPrivateMessageAsync(userId, new CqMessage
                { new CqTextMsg("感谢使用！请邀请 Lapis Bot 进入您的群聊！\n若要保存表情，请直接将表情发送给 Lapis Bot") });
        }

        static void SaveDate()
        {
            File.WriteAllText(Environment.CurrentDirectory + "/date.json", JsonConvert.SerializeObject(_lastDateTime));
            Logger.LogInformation("Date data has been saved.");
        }

        static void Reload()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    var completed = true;
                    
                    foreach (var task in InitializationTasks)
                    {
                        if (!task.IsCompleted)
                        {
                            completed = false;
                            break;
                        }
                    }
                    
                    if (!completed)
                        continue;
                    
                    if (TimeChanged != null)
                        TimeChanged(new Object(), EventArgs.Empty);

                    if (_lastDateTime.Date != DateTime.Now.Date)
                    {
                        _lastDateTime = DateTime.Now;
                        Logger.LogInformation("Date change detected, reinitializing.");
                        SaveDate();
                        if (DateChanged != null)
                            DateChanged(new Object(),EventArgs.Empty);
                    }

                    if (_lastDateTimeHour.Hour != DateTime.Now.Hour)
                    {
                        _lastDateTimeHour = DateTime.Now;
                        if (HourChanged != null)
                            HourChanged(new Object(), new EventArgs());
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("Unknown error:\n\n" + ex.StackTrace + "\n\n" + ex.Message);
            }
        }

        private static void Console_CancelKeyPress(object sender, EventArgs e)
        {
            foreach (GroupCommand command in GroupCommands)
                command.Unload();
            
            SaveDate();
        }
    }
}