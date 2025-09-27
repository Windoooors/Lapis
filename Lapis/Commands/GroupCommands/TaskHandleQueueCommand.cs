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

    public override void Initialize()
    {
        Program.TimeChanged += TaskHandleQueue.Instance.DueCheck;
    }

    public override void RespondWithoutParsingCommand(string command, CqGroupMessagePostContext source)
    {
        if (!(_argumentRegex.IsMatch(command) &&
              !TaskHandleQueue.Instance.IsEmpty(source.GroupId, source.Sender.UserId)))
            return;

        var originalCommandString = command;
        ParseWithArgument([command], originalCommandString, source);
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
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

        if (TaskHandleQueue.Instance.IsEmpty(source.GroupId, source.Sender.UserId))
        {
            SendMessage(source,
            [
                new CqReplyMsg(source.MessageId),
                new CqTextMsg("没有待处理的消息！")
            ]);
            return;
        }

        var confirm = arguments[0].Equals("confirm") || arguments[0].Equals("确定");

        var done = TaskHandleQueue.Instance.HandleTask(source.GroupId, confirm, source.Sender.UserId);

        if (done)
            return;

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg("处理失败！")
        ]);
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

    public bool AddTask(HandleableTask task, long groupId, long userToBeAsked)
    {
        if (!FindTaskList(groupId, out var taskList))
        {
            AddTaskList(groupId);
            return AddTask(task, groupId, userToBeAsked);
        }

        if (IsFull(taskList, userToBeAsked)) return false;

        taskList.Tasks.Add(task);

        return true;
    }

    private bool IsFull(TaskList taskList, long userToBeAsked)
    {
        return taskList.Tasks.Where(x => x.UserToBeAsked == userToBeAsked).Select(x => x).ToArray().Length >=
               MaxTaskCount;
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

    public bool HandleTask(long groupId, bool confirm, long userToBeAsked)
    {
        if (!FindTaskList(groupId, out var taskList))
            return false;

        var targetedTasks = taskList.Tasks.Where(x => x.UserToBeAsked == userToBeAsked).Select(x => x).ToArray();

        if (targetedTasks.Length == 0) return false;
        if (confirm) targetedTasks[0].WhenConfirm();
        else targetedTasks[0].WhenCancel();
        taskList.Tasks.Remove(targetedTasks[0]);
        return true;
    }

    public bool IsEmpty(long groupId, long userToBeAsked)
    {
        if (!FindTaskList(groupId, out var taskList))
            return true;

        return taskList.Tasks.Where(x => x.UserToBeAsked == userToBeAsked).Select(x => x).ToArray().Length <
               MaxTaskCount;
    }

    public void DueCheck(object sender, EventArgs e)
    {
        foreach (var taskList in _taskLists)
        {
            var dueTasks = taskList.Tasks.Where(task => task.DueTime <= DateTime.Now);

            foreach (var handleableTask in dueTasks.ToList())
                HandleTask(taskList.GroupId, false, handleableTask.UserToBeAsked);
        }
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


    public class HandleableTask(long userToBeAsked, Action whenCancel, Action whenConfirm)
    {
        public readonly DateTime DueTime = DateTime.Now + TimeSpan.FromMinutes(5);
        public readonly long UserToBeAsked = userToBeAsked;
        public readonly Action WhenCancel = whenCancel;
        public readonly Action WhenConfirm = whenConfirm;
    }
}