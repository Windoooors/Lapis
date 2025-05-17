using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace LapisBot.GroupCommands;

public class TaskHandleQueueCommand : GroupCommand
{
    public TaskHandleQueueCommand()
    {
        CommandHead = new Regex("^handle");
        DirectCommandHead = new Regex("^待处理");
    }

    public override void ParseWithArgument(string command, CqGroupMessagePostContext source)
    {
        if (command == "confirm")
        {
            if (TaskHandleQueue.Singleton.IsEmpty())
                SendMessage(source,
                    new CqMessage
                    {
                        new CqTextMsg("没有待处理的消息！")
                    });
            else
                TaskHandleQueue.Singleton.HandleTask();
        }
        else if (command == "cancel")
        {
            if (TaskHandleQueue.Singleton.IsEmpty())
                SendMessage(source,
                    new CqMessage
                    {
                        new CqTextMsg("没有待处理的消息！")
                    });
            else
                TaskHandleQueue.Singleton.HandleTask(false);
        }
        else
        {
            SendMessage(source,
                new CqMessage
                {
                    new CqTextMsg("参数错误！应为 \"confirm\" 或 \"cancel\"！")
                });
        }
    }
}

internal class TaskHandleQueue
{
    private readonly List<HandlableTask> tasks = new();

    public int maxTaskCount = 1;

    private TaskHandleQueue()
    {
    }

    public static TaskHandleQueue Singleton { get; } = new();

    public bool AddTask(HandlableTask task)
    {
        if (IsFull()) return false;
        tasks.Add(task);
        return true;
    }

    public bool IsFull()
    {
        return tasks.Count >= maxTaskCount;
    }

    public bool HandleTask(bool confirm = true, int index = 0)
    {
        if (tasks.Count <= index) return false;
        if (confirm) tasks[index].whenConfirm();
        else tasks[index].whenCancel();
        tasks.RemoveAt(index);
        return true;
    }

    public bool IsEmpty()
    {
        return tasks.Count == 0;
    }


    public class HandlableTask
    {
        public Action whenCancel = () => { };
        public Action whenConfirm = () => { };
    }
}