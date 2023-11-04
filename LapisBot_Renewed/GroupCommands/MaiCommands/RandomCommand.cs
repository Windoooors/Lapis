using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using ImageMagick;
using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using Manganese.Text;

namespace LapisBot_Renewed
{
    public class RandomCommand : MaiCommand
    {
        public override void Initialize()
        {
            headCommand = new Regex(@"^random\s");
        }

        public override void Parse(string command, GroupMessageReceiver source)
        {
            int i;
            if (!levelDictionary.ContainsKey(command))
                MessageManager.SendGroupMessageAsync(source.GroupId, "你随牛魔酬宾");
            else
            {
                levelDictionary.TryGetValue(command, out i);
                if (i == levelDictionary.Count - 1)
                {
                    MessageManager.SendGroupMessageAsync(source.GroupId, "潘你妈");
                }
                if (i == 6)
                {
                    return;
                }
                SongDto[] _songs = levels[i].ToArray();

                Random random = new Random();
                int j = random.Next(0, _songs.Length);

                var _image = new ImageMessage
                {
                    Base64 = InfoImageGenerator.Generate(j, _songs, "随机歌曲", null).ToBase64(),
                };

                MessageManager.SendGroupMessageAsync(source.GroupId, new MessageChain() { new AtMessage(source.Sender.Id), _image });
            }
        }
    }
}
