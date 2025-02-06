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

namespace LapisBot_Renewed
{
    
    public class BotSettings
    {
        public string Address;
        public bool IsDevelopingMode = false;
    }
    
    public class Program
    {
        public static List<GroupCommand> groupCommands = new List<GroupCommand>();

        public static List<PrivateCommand> privateCommands = new List<PrivateCommand>();

        public static HelpCommand helpCommand;

        public static BotSettingsCommand settingsCommand;

        private static DateTime lastDateTime;
        
        private static DateTime lastDateTimeHour;

        public static ApiOperator apiOperator = new ApiOperator(@"https://www.diving-fish.com");

        public static BotSettings BotSettings = new BotSettings();

        public static CqWsSession Session;

        public static event EventHandler DateChanged;
        
        public static event EventHandler HourChanged;

        public static event EventHandler TimeChanged;

        public static async Task Main()
        {
            if (System.IO.File.Exists(AppContext.BaseDirectory + "config.json"))
                BotSettings =
                    JsonConvert.DeserializeObject<BotSettings>(
                        System.IO.File.ReadAllText(AppContext.BaseDirectory + "config.json"));
            else
            {
                System.IO.File.WriteAllText(AppContext.BaseDirectory + "config.json",
                    JsonConvert.SerializeObject(BotSettings));
                Console.WriteLine("Please set up the Lapis Bot via editing \"" + AppContext.BaseDirectory + "config.json\"");
                return;
            }
            
            Session = new CqWsSession(new CqWsSessionOptions()
            {
                BaseUri = new Uri("ws://" + BotSettings.Address),  // WebSocket 地址
            });
            
            Session.Start();
            
            Console.CancelKeyPress += Console_CancelKeyPress;

            var _helpCommand = new HelpCommand();
            var _botSettingsCommand = new BotSettingsCommand();

            groupCommands.Add(new TaskHandleQueueCommand());
            groupCommands.Add(new RepeatCommand());
            groupCommands.Add(new AbuseCommand());
            groupCommands.Add(new VocabularyCommand());
            groupCommands.Add(new McPingCommand());
            groupCommands.Add(_helpCommand);
            groupCommands.Add(_botSettingsCommand);
            groupCommands.Add(new StickerCommand());
            groupCommands.Add(new AboutCommand());
            groupCommands.Add(new BangCommand());
            groupCommands.Add(new DoSomethingWithHimCommand());
            groupCommands.Add(new TaskHandleQueueCommand());
            groupCommands.Add(new MaiCommand());

            helpCommand = _helpCommand;
            settingsCommand = _botSettingsCommand;

            privateCommands.Add(new GetStickerImageCommand());
            privateCommands.Add(new GetGroupsCommand());
            privateCommands.Add(new UpdateMessageCommand());

            foreach (GroupCommand command in groupCommands)
                new Task(() => command.Initialize()).Start();

            foreach (PrivateCommand command in privateCommands)
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
                lastDateTime = JsonConvert.DeserializeObject<DateTime>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/date.json"));
            }
            Thread thread = new Thread(Reload);
            thread.Start();

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
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/date.json", JsonConvert.SerializeObject(lastDateTime));
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
                    if (lastDateTime.Date != DateTime.Now.Date)
                    {
                        lastDateTime = DateTime.Now;
                        SaveDate();
                        if (DateChanged != null)
                            DateChanged(new Object(), new EventArgs());
                    }

                    if (lastDateTimeHour.Hour != DateTime.Now.Hour)
                    {
                        lastDateTimeHour = DateTime.Now;
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
            foreach (GroupCommand _command in groupCommands)
            {
                await _command.Unload();
            }
            foreach (PrivateCommand _command in privateCommands)
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