using System.Collections.Generic;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Settings;

namespace Lapis.Commands.GroupCommands.GroupMemberCommands;

public class SearchMemberCommand : GroupMemberCommandBase
{
    public SearchMemberCommand()
    {
        CommandHead = "search";
        DirectCommandHead = "msearch|查群友";
        ActivationSettingsSettingsIdentifier = new SettingsIdentifierPair("msearch", "1");
        SearchMemberCommandInstance = this;
        IntendedArgumentCount = 1;
    }

    public static SearchMemberCommand SearchMemberCommandInstance { get; private set; }

    public SearchResult Search(string keyWord, long groupId, bool findAgreedWithEula = false)
    {
        var membersMatchedByAlias = new Dictionary<GroupMemberCommand.GroupMember, List<string>>();

        var group = GroupMemberCommandInstance.Groups.ToList().Find(x => x.GroupId == groupId);
        if (group == null || group.GroupId == 0)
            return new SearchResult(null);

        foreach (var member in group.Members)
        {
            var aliasObject = GroupMemberCommandInstance.GetAliasById(member.Id, groupId);

            if (aliasObject == null)
                continue;

            var aliases = aliasObject.Aliases;

            foreach (var alias in aliases)
                if (Searcher.Instance.IsMatch(keyWord, alias))
                {
                    if (!membersMatchedByAlias.ContainsKey(member))
                    {
                        membersMatchedByAlias.Add(member, [alias]);
                        continue;
                    }

                    membersMatchedByAlias[member].Add(alias);
                }
        }

        if (findAgreedWithEula)
            membersMatchedByAlias = membersMatchedByAlias.Where(x => x.Key.AgreedWithEula).Select(x => x)
                .ToDictionary();

        return new SearchResult
        (
            membersMatchedByAlias
        );
    }

    public StringBuilder GetMultiSearchResults(SearchResult searchResult, long groupId)
    {
        var memberAliasDict = searchResult.MembersMatchedByAlias;

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("找到了以下群友：");

        if (memberAliasDict.Count > 0)
            foreach (var pair in memberAliasDict)
            {
                if (!TryGetNickname(pair.Key.Id, groupId, out var nickname)) continue;

                var aliasStringBuilder = new StringBuilder();
                foreach (var alias in pair.Value) aliasStringBuilder.AppendJoin(' ', $"\"{alias}\"");

                stringBuilder.AppendLine(pair.Key.Id + " - " + nickname + $" （通过别称 {aliasStringBuilder} 匹配）");
            }

        return stringBuilder;
    }

    public override void ParseWithArgument(string[] arguments, string originalPlainMessage,
        CqGroupMessagePostContext source)
    {
        var searchResult = Search(arguments[0], source.GroupId);

        var stringBuilder = new StringBuilder();

        if (searchResult.MembersMatchedByAlias.Count >= 30)
        {
            stringBuilder.AppendLine("搜索结果过多，请提供更多关键词");
        }
        else
        {
            stringBuilder = GetMultiSearchResults(searchResult, source.GroupId);

            if (searchResult.MembersMatchedByAlias.Count == 0)
            {
                stringBuilder.Clear();
                stringBuilder = new StringBuilder("未找到该群友");
            }
        }

        SendMessage(source,
        [
            new CqReplyMsg(source.MessageId),
            new CqTextMsg(stringBuilder.ToString().Trim())
        ]);
    }

    public class SearchResult(Dictionary<GroupMemberCommand.GroupMember, List<string>> membersMatchedByAlias)
    {
        public readonly Dictionary<GroupMemberCommand.GroupMember, List<string>> MembersMatchedByAlias =
            membersMatchedByAlias;
    }
}