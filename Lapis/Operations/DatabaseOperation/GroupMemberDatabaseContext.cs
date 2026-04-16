using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Lapis.Operations.DatabaseOperation;

public class GroupMemberDatabaseContext : DbContext
{
    public DbSet<MemberAlias> MemberAliasesDataSet { get; set; }
    public DbSet<GroupMember> GroupMembersDataSet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(BotConfiguration.Instance.SqlConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var aliasEntityBuilder = modelBuilder.Entity<MemberAlias>();

        aliasEntityBuilder.HasKey(c => new { c.GroupId, c.QqId });

        var groupMemberEntityBuilder = modelBuilder.Entity<GroupMember>();
        groupMemberEntityBuilder.HasKey(c => new { c.GroupId, c.QqId });

        var singleAliasEntityBuilder = modelBuilder.Entity<SingleMemberAlias>();
        singleAliasEntityBuilder
            .HasOne(a => a.Member)
            .WithMany(m => m.Aliases)
            .HasForeignKey(a => new { a.GroupId, a.MemberQqId });

        singleAliasEntityBuilder.HasKey(x => x.AliasId);
    }
}

public class MemberAlias
{
    public long GroupId { get; set; }
    public long QqId { get; set; }

    public List<SingleMemberAlias> Aliases { get; set; } = [];
}

public class SingleMemberAlias
{
    public int AliasId { get; }

    [MaxLength(1024)] public string Alias { get; set; }

    public long GroupId { get; set; }
    public long MemberQqId { get; set; }

    public MemberAlias Member { get; set; }
}

public class GroupMember
{
    public long GroupId { get; set; }
    public long QqId { get; set; }
    public bool AgreedEula { get; set; }
    public int RapedTimes { get; set; }
}