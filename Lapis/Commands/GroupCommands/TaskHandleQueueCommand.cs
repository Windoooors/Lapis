using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace Lapis.Commands.GroupCommands;

public class TaskHandleQueueCommand : GroupCommand
{
    private readonly Regex _argumentRegex = new("^(确定|取消|confirm|cancel)$", RegexOptions.IgnoreCase);

    public TaskHandleQueueCommand()
    {
        CommandHead = "handle";
        DirectCommandHead = "待处理";
        IntendedArgumentCount = 1;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (_argumentRegex.IsMatch(command) && !TaskHandleQueue.Instance.IsEmpty(source.GroupId))
            ParseWithArgument([command], source);
    }

    public override void ParseWithArgument(string[] arguments, CqGroupMessagePostContext source)
    {
        if (!_argumentRegex.IsMatch(arguments[0]))
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("参数错误！应为 \"confirm\" 或 \"cancel\"！")
            ]);
            return;
        }

        if (TaskHandleQueue.Instance.IsEmpty(source.GroupId))
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("没有待处理的消息！")
            ]);
            return;
        }

        var confirm = arguments[0].Equals("confirm") || arguments[0].Equals("确定");

        TaskHandleQueue.Instance.HandleTask(source.GroupId, confirm);
    }
}

internal class TaskHandleQueue
{
    private const int MaxTaskCount = 1;
    private readonly HashSet<TaskList> _taskLists = new();

    private TaskHandleQueue()
    {
    }

    public static TaskHandleQueue Instance { get; } = new();

    public bool AddTask(HandleableTask task, long groupId)
    {
        if (!FindTaskList(groupId, out var taskList))
        {
            AddTaskList(groupId);
            return AddTask(task, groupId);
        }

        if (IsFull(taskList)) return false;

        taskList.Tasks.Add(task);

        return true;
    }

    private bool IsFull(TaskList taskList)
    {
        return taskList.Tasks.Count >= MaxTaskCount;
    }

    private void AddTaskList(long groupId)
    {
        _taskLists.Add(new TaskList(groupId));
    }

    private bool FindTaskList(long groupId, out TaskList taskList)
    {
        taskList = _taskLists.ToList().Find(x => x.GroupId == groupId);

        if (taskList is null || taskList.GroupId == 0) return false;
        return true;
    }

    public bool HandleTask(long groupId, bool confirm = true, int index = 0)
    {
        if (!FindTaskList(groupId, out var taskList))
            return false;

        if (taskList.Tasks.Count <= index) return false;
        if (confirm) taskList.Tasks[index].WhenConfirm();
        else taskList.Tasks[index].WhenCancel();
        taskList.Tasks.RemoveAt(index);
        return true;
    }

    public bool IsEmpty(long groupId)
    {
        if (!FindTaskList(groupId, out var taskList))
            return true;

        return taskList.Tasks.Count == 0;
    }

    private class TaskList(long groupId)
    {
        public readonly long GroupId = groupId;
        public readonly List<HandleableTask> Tasks = new();

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TaskList other && GroupId == other.GroupId;
        }
    }


    public class HandleableTask
    {
        public Action WhenCancel = () => { };
        public Action WhenConfirm = () => { };
    }
}