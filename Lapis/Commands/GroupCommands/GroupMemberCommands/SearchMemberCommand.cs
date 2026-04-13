using System.Collections.Generic;
using System.Linq;
using System.Text;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Miscellaneous;
using Lapis.Operations.DatabaseOperation;
using Lapis.Settings;
using Microsoft.EntityFrameworkCore;

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
        var membersMatchedByAlias = new Dictionary<GroupMember, List<string>>();

        var splitWords = Searcher.Instance.GetSplitWords(keyWord.ToLower());
        var queryString = string.Join(" AND ", splitWords);

        using var db = DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetDb;

        var aliasList = db.MemberAliasesDataSet.Include(x => x.Aliases).ToList();

        var matchedAlias = aliasList.Where(x =>
            x.Aliases.Exists(y => y.Alias.ToLower().Contains(keyWord.ToLower())) && x.GroupId == groupId).ToList();

        if (matchedAlias.Count == 0)
            return new SearchResult([]);

        foreach (var aliasItem in matchedAlias)
        foreach (var alias in aliasItem.Aliases)
        {
            var groupMember =
                DatabaseHandler.Instance.GroupMemberDatabaseOperator.GetMember(aliasItem.QqId, groupId, db);

            var added = membersMatchedByAlias.TryAdd(groupMember, [alias.Alias]);
            if (!added && membersMatchedByAlias.TryGetValue(groupMember, out var value)) value.Add(alias.Alias);
        }

        if (findAgreedWithEula)
            membersMatchedByAlias = membersMatchedByAlias.Where(x => x.Key.AgreedEula).Select(x => x)
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
                if (!TryGetNickname(pair.Key.QqId, groupId, out var nickname)) continue;

                var aliasStringBuilder = new StringBuilder();
                foreach (var alias in pair.Value) aliasStringBuilder.AppendJoin(' ', $"\"{alias}\"");

                stringBuilder.AppendLine(pair.Key.QqId + " - " + nickname + $" （通过别称 {aliasStringBuilder} 匹配）");
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

    public class SearchResult(Dictionary<GroupMember, List<string>> membersMatchedByAlias)
    {
        public readonly Dictionary<GroupMember, List<string>> MembersMatchedByAlias =
            membersMatchedByAlias;
    }
}