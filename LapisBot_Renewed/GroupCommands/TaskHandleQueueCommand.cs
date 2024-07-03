using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using EleCho.GoCqHttpSdk.Post;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;

namespace LapisBot_Renewed.GroupCommands
{
    public class TaskHandleQueueCommand : GroupCommand
    {
        public override Task Initialize()
        {
            HeadCommand = new Regex(@"^handle\s");
            DirectCommand = new Regex(@"^待处理\s");
            DefaultSettings.SettingsName = "待处理列表";

            return Task.CompletedTask;
        }

        public override Task Parse(string command, CqGroupMessagePostContext source)
        {
			if (command == "confirm")
			{
				if(TaskHandleQueue.Singleton.IsEmpty())
				{
					Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqTextMsg("没有待处理的消息！")
                        ]);
				}
				else
				{
					TaskHandleQueue.Singleton.HandleTask(true);
				}
			}
			else if(command == "cancel")
			{
				if(TaskHandleQueue.Singleton.IsEmpty())
				{
					Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqTextMsg("没有待处理的消息！")
                        ]);
				}
				else
				{
					TaskHandleQueue.Singleton.HandleTask(false);
				}
			}
			else
			{
				Program.Session.SendGroupMessageAsync(source.GroupId,
                        [
                            new CqTextMsg("命令错误！应为 \"confirm\" 或 \"cancel\"！")
                        ]);
			}
			
            return Task.CompletedTask;
        }

    }

	class TaskHandleQueue
	{
		List<HandlableTask> tasks = new();

		public bool AddTask(HandlableTask task)
		{
			if(IsFull())return false;
			tasks.Add(task);
			return true;
		}
		public bool IsFull() => tasks.Count >= maxTaskCount;
		public bool HandleTask(bool confirm = true,int index = 0)
		{
			if(tasks.Count >= index)return false;
			if(confirm)tasks[index].whenConfirm();
			else tasks[index].whenCancel();
			tasks.RemoveAt(0);
			return true;
		}
		public bool IsEmpty() => tasks.Count == 0;

		public int maxTaskCount = 1;


		public class HandlableTask
		{
			public Action whenConfirm = ()=>{};
			public Action whenCancel = ()=>{};
		}

		private TaskHandleQueue(){}

		public static TaskHandleQueue Singleton{get;} = new();
	}
}