using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Utils.Internal;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Data.Events.Concretes.Request;
using Mirai.Net.Data.Shared;
using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json;
using System.Threading;

namespace LapisBot_Renewed
{
    public class Program
    {
        public static List<GroupCommand> groupCommands = new List<GroupCommand>();

        public static List<PrivateCommand> privateCommands = new List<PrivateCommand>();

        private static DateTime lastDateTime;

        public static ApiOperator apiOperator = new ApiOperator(@"https://www.diving-fish.com");

        public static MiraiBot bot = new MiraiBot
        {
            Address = "10.45.72.126:8080",
            QQ = "3064967438",
            //QQ = "3180904635",
            //QQ = "3279357760",
            VerifyKey = "1234567890"
        };

        public static event EventHandler DateChanged;

        public static async Task Main()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            await bot.LaunchAsync();

            bot.EventReceived.OfType<NewFriendRequestedEvent>().Subscribe(async e =>
            {
                await RequestManager.HandleNewFriendRequestedAsync(e, NewFriendRequestHandlers.Approve);
            });

            bot.EventReceived
                .OfType<NewInvitationRequestedEvent>()
                .Subscribe(async e =>
                {
                    //同意入群
                    await RequestManager.HandleNewInvitationRequestedAsync(e, NewInvitationRequestHandlers.Approve, "");
                });

            groupCommands.Add(new MaiCommand());
            groupCommands.Add(new RepeatCommand());
            groupCommands.Add(new AbuseCommand());
            groupCommands.Add(new McPingCommand());
            groupCommands.Add(new HelpCommand());
            groupCommands.Add(new StickerCommand());
            groupCommands.Add(new DoSomethingWithHimCommand());

            privateCommands.Add(new GetGroupsCommand());

            foreach (GroupCommand _command in groupCommands)
                await _command.Initialize();


            foreach (PrivateCommand _command in privateCommands)
                await _command.Initialize();

            var commandParser = new CommandParser();

            bot.MessageReceived
                .OfType<GroupMessageReceiver>()
                .Subscribe(commandParser.Parse);

            bot.MessageReceived.OfType<FriendMessageReceiver>().Subscribe(commandParser.Parse);

            if (System.IO.File.Exists(Environment.CurrentDirectory + "/date.json"))
            {
                lastDateTime = JsonConvert.DeserializeObject<DateTime>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/date.json"));
            }
            Thread thread = new Thread(Reload);
            thread.Start();

            Console.ReadLine();
        }

        static void SaveDate()
        {
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "/date.json", JsonConvert.SerializeObject(lastDateTime));
            Console.WriteLine("Date data has been saved.");
        }

        static void Reload()
        {
            while (true)
            {
                Thread.Sleep(1000);
                //Console.WriteLine(DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second);
                if (lastDateTime.Date != DateTime.Now.Date)
                {
                    lastDateTime = DateTime.Now;
                    SaveDate();
                    DateChanged(new Object(), new EventArgs());
                }
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