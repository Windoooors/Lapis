using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Lapis.Operations.DatabaseOperation;

public class SongMetaDatabaseContext : DbContext
{
    public DbSet<SongMetaData> SongMetaDataSet { get; set; }
    public DbSet<ChartMetaData> ChartMetaDataSet { get; set; }
    public DbSet<SongAlias> SongAliasDataSet { get; set; }

    public DbSet<ChartScoreData> Scores { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(BotConfiguration.Instance.SqlServerConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var chartMetaEntityBuilder = modelBuilder.Entity<ChartMetaData>();
        chartMetaEntityBuilder.HasOne(a => a.SongMetaData)
            .WithMany(m => m.Charts)
            .HasForeignKey(a => new { a.SongId });
        chartMetaEntityBuilder.HasKey(x => x.ChartId);

        var songMetaEntityBuilder = modelBuilder.Entity<SongMetaData>();

        songMetaEntityBuilder
            .Property(s => s.SongId)
            .ValueGeneratedNever();

        songMetaEntityBuilder.HasKey(c => new { c.SongId });

        var aliasMetaEntityBuilder = modelBuilder.Entity<SongAlias>();

        aliasMetaEntityBuilder
            .Property(s => s.SimplifiedSongId)
            .ValueGeneratedNever();

        aliasMetaEntityBuilder.HasKey(s => s.SimplifiedSongId);

        var singleAliasEntityBuilder = modelBuilder.Entity<SingleSongAlias>();
        singleAliasEntityBuilder.HasOne(a => a.SongAlias)
            .WithMany(m => m.Aliases)
            .HasForeignKey(a => new { a.SongId });

        singleAliasEntityBuilder.HasKey(x => x.AliasId);

        var scoreEntityBuilder = modelBuilder.Entity<ChartScoreData>();

        scoreEntityBuilder
            .HasKey(c => new { c.QqId, c.SongId, c.LevelIndex });
        scoreEntityBuilder
            .Property(s => s.Fc);
        scoreEntityBuilder
            .Property(s => s.Fs);

        scoreEntityBuilder.HasOne(x => x.Song).WithMany().HasForeignKey(x => x.SongId);
    }
}

public class SongMetaData
{
    public int SongId { get; set; }

    [MaxLength(1024)] public string Title { get; set; }

    [MaxLength(1024)] public string Artist { get; set; }

    [MaxLength(32)] public string Version { get; set; }

    public float Bpm { get; set; }

    public List<ChartMetaData> Charts { get; set; } = [];

    public override bool Equals(object obj)
    {
        return obj is SongMetaData other && other.SongId == SongId;
    }

    public override int GetHashCode()
    {
        return SongId.GetHashCode();
    }
}

public class ChartMetaData
{
    public int ChartId { get; set; }

    public int SongId { get; set; }

    public SongMetaData SongMetaData { get; set; }

    public int LevelIndex { get; set; }
    public int TapCount { get; set; }
    public int HoldCount { get; set; }
    public int TouchCount { get; set; }
    public int SlideCount { get; set; }
    public int BreakCount { get; set; }
    public float Rating { get; set; }
    public float FitRating { get; set; }

    [MaxLength(1024)] public string CharterName { get; set; }

    public int MaxDxScore { get; set; }

    [MaxLength(1024)] public string LevelName { get; init; }
}

public class SongAlias
{
    public int SimplifiedSongId { get; init; }
    public List<SingleSongAlias> Aliases { get; set; } = [];
}

public class SingleSongAlias
{
    public int AliasId { get; }

    [MaxLength(1024)] public string Alias { get; init; }

    public int SongId { get; init; }

    public SongAlias SongAlias { get; init; }
}