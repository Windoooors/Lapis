using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EleCho.GoCqHttpSdk.Post;
using System.Collections.Generic;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;

namespace LapisBot_Renewed.GroupCommands
{
    public class TaskHandleQueueCommand : GroupCommand
    {
	    public TaskHandleQueueCommand()
	    {
		    CommandHead = new Regex("^handle");
		    DirectCommandHead = new Regex("^待处理");
	    }

        public override Task ParseWithArgument(string command, CqGroupMessagePostContext source)
        {
			if (command == "confirm")
			{
				if(TaskHandleQueue.Singleton.IsEmpty())
				{
					Program.Session.SendGroupMessageAsync(source.GroupId,
						new CqMessage
						{
							new CqTextMsg("没有待处理的消息！")
						});
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
						new CqMessage
						{
							new CqTextMsg("没有待处理的消息！")
						});
				}
				else
				{
					TaskHandleQueue.Singleton.HandleTask(false);
				}
			}
			else
			{
				Program.Session.SendGroupMessageAsync(source.GroupId,
					new CqMessage
					{
						new CqTextMsg("参数错误！应为 \"confirm\" 或 \"cancel\"！")
					});
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
			if(tasks.Count <= index)return false;
			if(confirm)tasks[index].whenConfirm();
			else tasks[index].whenCancel();
			tasks.RemoveAt(index);
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