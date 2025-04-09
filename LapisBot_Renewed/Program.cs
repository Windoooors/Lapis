using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LapisBot_Renewed.GroupCommands;
using System.Threading;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using LapisBot_Renewed.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using NLog.Extensions.Logging;

namespace LapisBot_Renewed
{
    
    public class BotSettings
    {
        public static BotSettings Instance;
        
        public string Address;
        public bool IsDevelopingMode = false;

        public string AliasUrl;
        public string DivingFishUrl;
        public string WahlapConnectiveKitsUrl;
        public string AircadeUrl;
    }
    
    public class Program
    {
        
        public static readonly List<GroupCommand> GroupCommands = new List<GroupCommand>();

        public static readonly List<PrivateCommand> PrivateCommands = new List<PrivateCommand>();

        public static HelpCommand HelpCommand;

        public static BotSettingsCommand SettingsCommand;

        private static DateTime _lastDateTime;
        
        private static DateTime _lastDateTimeHour;

        private static ApiOperator _apiOperator = new ApiOperator();

        private static BotSettings _botSettings = new BotSettings();

        public static CqWsSession Session;
        
        public static FontFamily FontLight =
            new FontCollection().Add(Path.Combine(Environment.CurrentDirectory, "resource/font-light.otf"));
        public static FontFamily FontRegular = 
            new FontCollection().Add(Path.Combine(Environment.CurrentDirectory, "resource/font.otf"));
        public static FontFamily FontHeavy =
            new FontCollection().Add(Path.Combine(Environment.CurrentDirectory, "resource/font-heavy.otf"));

        public static event EventHandler DateChanged;
        
        public static event EventHandler HourChanged;

        public static event EventHandler TimeChanged;

        public static ILogger<Program> logger; 

        public static async Task Main()
        {
            logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
            logger.LogInformation("Program has started.");
            
            if (System.IO.File.Exists(AppContext.BaseDirectory + "config.json"))
                _botSettings =
                    JsonConvert.DeserializeObject<BotSettings>(
                        System.IO.File.ReadAllText(AppContext.BaseDirectory + "config.json"));
            else
            {
                System.IO.File.WriteAllText(AppContext.BaseDirectory + "config.json",
                    JsonConvert.SerializeObject(_botSettings));
                Console.WriteLine("Please set up the Lapis Bot via editing \"" + AppContext.BaseDirectory + "config.json\"");
                return;
            }
            
            Session = new CqWsSession(new CqWsSessionOptions()
            {
                BaseUri = new Uri("ws://" + _botSettings.Address),  // WebSocket 地址
            });
            
            await Session.StartAsync();
            
            Console.CancelKeyPress += Console_CancelKeyPress;

            var _helpCommand = new HelpCommand();
            var _botSettingsCommand = new BotSettingsCommand();

            GroupCommands.Add(new TaskHandleQueueCommand());
            GroupCommands.Add(new AbuseCommand());
            GroupCommands.Add(new VocabularyCommand());
            GroupCommands.Add(new McPingCommand());
            GroupCommands.Add(_helpCommand);
            GroupCommands.Add(_botSettingsCommand);
            GroupCommands.Add(new StickerCommand());
            GroupCommands.Add(new AboutCommand());
            GroupCommands.Add(new BangCommand());
            GroupCommands.Add(new DoSomethingWithHimCommand());
            GroupCommands.Add(new TaskHandleQueueCommand());
            GroupCommands.Add(new MaiCommand());
            GroupCommands.Add(new RepeatCommand());

            HelpCommand = _helpCommand;
            SettingsCommand = _botSettingsCommand;

            PrivateCommands.Add(new GetStickerImageCommand());
            PrivateCommands.Add(new GetGroupsCommand());
            PrivateCommands.Add(new UpdateMessageCommand());

            foreach (GroupCommand command in GroupCommands)
                new Task(() => command.Initialize()).Start();

            foreach (PrivateCommand command in PrivateCommands)
                new Task(() => command.Initialize()).Start();

            var commandParser = new CommandParser();
            
            Session.UseGroupMessage(async (context, next) =>
            {
                commandParser.MainParse(context);
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
            
            Session.UsePrivateMessage(async (context, next) =>
            {
                commandParser.Parse(context);
                await next.Invoke();
            });

            //bot.MessageReceived.OfType<FriendMessageReceiver>().Subscribe(commandParser.Parse);

            if (System.IO.File.Exists(Environment.CurrentDirectory + "/date.json"))
            {
                _lastDateTime = JsonConvert.DeserializeObject<DateTime>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/date.json"));
            }
            Thread thread = new Thread(Reload);
            thread.Start();
            
            BotSettings.Instance = _botSettings;
            Operations.ApiOperation.ApiOperator.Instance = _apiOperator;

            Console.ReadLine();
        }

        static void Welcome(long userId)
        { 
            Thread.Sleep(3000);
            Session.SendPrivateMessageAsync(userId, new CqMessage
                { new CqTextMsg("感谢使用！请邀请 Lapis Bot 进入您的群聊！\n若要保存表情，请直接将表情发送给 Lapis Bot") });
        }

        static void SaveDate()
        {
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/date.json", JsonConvert.SerializeObject(_lastDateTime));
            Console.WriteLine("Date data has been saved.");
        }

        static void Reload()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (TimeChanged != null)
                        TimeChanged(new Object(), new EventArgs());
                    //Console.WriteLine(DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second);
                    if (_lastDateTime.Date != DateTime.Now.Date)
                    {
                        _lastDateTime = DateTime.Now;
                        SaveDate();
                        if (DateChanged != null)
                            DateChanged(new Object(), new EventArgs());
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
                Console.WriteLine("Unknown error:\n\n" + ex.StackTrace + "\n\n" + ex.Message);
            }
        }

        private static async void Console_CancelKeyPress(object sender, EventArgs e)
        {
            foreach (GroupCommand _command in GroupCommands)
            {
                await _command.Unload();
            }
            foreach (PrivateCommand _command in PrivateCommands)
            {
                await _command.Unload();
            }
            SaveDate();
        }

        private static void FriendRequested(object sender, EventArgs e)
        {
        }
    }
}