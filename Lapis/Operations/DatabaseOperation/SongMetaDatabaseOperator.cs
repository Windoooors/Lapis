using System;
using System.Linq;
using Lapis.Commands.GroupCommands;
using Microsoft.EntityFrameworkCore;

namespace Lapis.Operations.DatabaseOperation;

public class SongMetaDatabaseOperator
{
    public readonly SongMetaDatabaseContext Db;

    public SongMetaDatabaseOperator()
    {
        Db = new SongMetaDatabaseContext();
    }


    public SongMetaDatabaseContext GetDb => new();

    ~SongMetaDatabaseOperator()
    {
        Db.Dispose();
    }

    public void InsertSongs(SongDto[] songDtos, MaiCommandBase.AliasItemDto[] aliases, SongMetaDatabaseContext db)
    {
        db.SongMetaDataSet.ExecuteDelete();

        db.SongMetaDataSet.AddRange(songDtos.Select(x => new SongMetaData
        {
            SongId = x.Id,
            Title = x.Title,
            Bpm = x.BasicInfo.Bpm,
            Artist = x.BasicInfo.Artist,
            Version = x.BasicInfo.Version
        }));

        db.ChartMetaDataSet.ExecuteDelete();
        foreach (var songDto in songDtos)
            db.ChartMetaDataSet.AddRange(songDto.Charts.Select(x => new ChartMetaData
            {
                TapCount = x.Notes[0],
                HoldCount = x.Notes[1],
                SlideCount = x.Notes[2],
                TouchCount = songDto.Type == "DX" ? x.Notes[3] : 0,
                BreakCount = x.Notes[songDto.Type == "DX" ? 4 : 3],
                LevelIndex = songDto.Charts.IndexOf(x),
                SongId = songDto.Id,
                Rating = songDto.Ratings[songDto.Charts.IndexOf(x)],
                FitRating = songDto.FitRatings[songDto.Charts.IndexOf(x)],
                CharterName = x.Charter,
                MaxDxScore = 3 * (songDto.Type == "DX"
                    ? x.Notes[0] + x.Notes[1] + x.Notes[2] + x.Notes[3] + x.Notes[4]
                    : x.Notes[0] + x.Notes[1] + x.Notes[2] + x.Notes[3]),
                LevelName = songDto.Levels[songDto.Charts.IndexOf(x)]
            }));

        var aliasDbSet = db.SongAliasDataSet;
        var aliasList = aliasDbSet.Include(x => x.Aliases).ToList();

        foreach (var aliasItemDto in aliases)
        {
            if (!aliasList.Exists(x => x.SimplifiedSongId == aliasItemDto.Id))
            {
                aliasDbSet.Add(new SongAlias
                {
                    SimplifiedSongId = aliasItemDto.Id,
                    Aliases = aliasItemDto.Aliases.Select(x => new SingleSongAlias
                    {
                        Alias = x
                    }).ToList()
                });

                continue;
            }

            aliasItemDto.Aliases.ForEach(aliasString =>
            {
                var aliasItem = aliasDbSet.FirstOrDefault(x => x.SimplifiedSongId == aliasItemDto.Id);

                if (aliasItem == null || aliasItem.Aliases.Exists(x => x.Alias == aliasString))
                    return;

                aliasItem.Aliases ??= [];

                aliasItem.Aliases.Add(new SingleSongAlias
                {
                    Alias = aliasString
                });
            });
        }

        db.SaveChanges();
    }


    public ChartScoreData[] GetUserScores(long qqId, SongMetaDatabaseContext db)
    {
        var scores = db.Scores.Include(x => x.Song.Charts)
            .Where(x => x.QqId == qqId);

        return scores.ToArray();
    }

    public ChartScoreData[] GetScores(long qqId, int songId, SongMetaDatabaseContext db)
    {
        var scores =
            db.Scores.Include(x => x.Song.Charts)
                .Where(x => x.QqId == qqId && x.SongId == songId);

        return scores.ToArray();
    }

    public void DeleteScore(long qqId, SongMetaDatabaseContext db)
    {
        var scores = db.Scores.Where(x => x.QqId == qqId).ToList();

        db.Scores.RemoveRange(scores);

        db.SaveChanges();
    }

    public void UpsertScores(ChartScoreData[] input)
    {
        using var db = GetDb;

        var songs = db.SongMetaDataSet.ToList();

        foreach (var inputUserScore in input)
        {
            var findResult = db.Scores.FirstOrDefault
            (x => x.SongId == inputUserScore.SongId &&
                  x.LevelIndex == inputUserScore.LevelIndex && x.QqId == inputUserScore.QqId);

            if (findResult != null)
            {
                findResult.Achievements = Math.Max(findResult.Achievements, inputUserScore.Achievements);
                findResult.DxScore = Math.Max(findResult.DxScore, inputUserScore.DxScore);
                findResult.PlayCount = Math.Max(findResult.PlayCount, inputUserScore.PlayCount);

                findResult.Fc = GetFcIndex(findResult.Fc) > GetFcIndex(inputUserScore.Fc)
                    ? findResult.Fc
                    : inputUserScore.Fc;
                findResult.Fs = GetFsIndex(findResult.Fs) > GetFsIndex(inputUserScore.Fs)
                    ? findResult.Fs
                    : inputUserScore.Fs;
            }
            else
            {
                if (!songs.Exists(x => x.SongId == inputUserScore.SongId))
                    continue;

                db.Scores.Add(inputUserScore);
            }
        }

        db.SaveChanges();

        return;

        int GetFcIndex(string fcString)
        {
            return fcString switch
            {
                "" => 0,
                "fc" => 1,
                "fcp" => 2,
                "ap" => 3,
                "app" => 4,
                _ => 0
            };
        }

        int GetFsIndex(string fsString)
        {
            return fsString switch
            {
                "" => 0,
                "sync" => 5,
                "fs" => 1,
                "fsp" => 2,
                "fsd" => 3,
                "fsdp" => 4,
                _ => 0
            };
        }
    }
}