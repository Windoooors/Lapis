using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Lapis.Operations.DatabaseOperation;

public class GroupMemberDatabaseOperator
{
    public readonly GroupMemberDatabaseContext Db;

    public GroupMemberDatabaseOperator()
    {
        Db = new GroupMemberDatabaseContext();
    }

    public GroupMemberDatabaseContext GetDb => new();

    ~GroupMemberDatabaseOperator()
    {
        Db.Dispose();
    }

    public void AddMember(long userId, long groupId, GroupMemberDatabaseContext db)
    {
        var findResult = db.GroupMembersDataSet.FirstOrDefault(x => x.GroupId == groupId && x.QqId == userId);
        if (findResult == null)
        {
            db.GroupMembersDataSet.Add(new GroupMember
            {
                GroupId = groupId,
                QqId = userId
            });

            db.SaveChanges();
        }
    }

    public bool AddAlias(string alias, long groupId, long qqId, GroupMemberDatabaseContext db)
    {
        var findResult = db.MemberAliasesDataSet.Include(m => m.Aliases)
            .FirstOrDefault(x => x.GroupId == groupId && x.QqId == qqId);

        if (findResult == null)
        {
            db.MemberAliasesDataSet.Add(new MemberAlias
            {
                GroupId = groupId,
                QqId = qqId,
                Aliases =
                [
                    new SingleMemberAlias
                    {
                        Alias = alias
                    }
                ]
            });

            db.SaveChanges();

            return true;
        }

        if (findResult.Aliases.Exists(x => x.Alias == alias)) return false;

        findResult.Aliases ??= [];

        findResult.Aliases.Add(new SingleMemberAlias
        {
            Alias = alias
        });

        db.SaveChanges();

        return true;
    }

    public void RemoveMember(long userId, long groupId, GroupMemberDatabaseContext db)
    {
        var findResult = db.GroupMembersDataSet.FirstOrDefault(x => x.GroupId == groupId && x.QqId == userId);
        if (findResult != null)
        {
            db.GroupMembersDataSet.Remove(findResult);
            db.SaveChanges();
        }
    }

    public GroupMember GetMember(long userId, long groupId, GroupMemberDatabaseContext db)
    {
        var findResult = db.GroupMembersDataSet.FirstOrDefault(x => x.GroupId == groupId && x.QqId == userId);
        return findResult;
    }
}