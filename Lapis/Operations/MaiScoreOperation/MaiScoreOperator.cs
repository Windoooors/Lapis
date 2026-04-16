using System;
using System.Collections;
using System.Linq;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Operations.DatabaseOperation;

namespace Lapis.Operations.MaiScoreOperation;

public class MaiScoreOperator
{
    private const string CurrentVersion = "maimai でらっくす PRiSM";

    private static float GetRatingFactor(float achievement)
    {
        return achievement switch
        {
            79.9999f => 12.8f,
            96.9999f => 17.6f,
            98.9999f => 20.6f,
            99.9999f => 21.4f,
            100.4999f => 22.2f,

            >= 10 and < 20 => 1.6f,
            >= 20 and < 30 => 3.2f,
            >= 30 and < 40 => 4.8f,
            >= 40 and < 50 => 6.4f,
            >= 50 and < 60 => 8f,
            >= 60 and < 70 => 9.6f,
            >= 70 and < 75 => 11.2f,
            >= 75 and < 80 => 12f,
            >= 80 and < 90 => 13.6f,
            >= 90 and < 94 => 15.2f,
            >= 94 and < 97 => 16.8f,
            >= 97 and < 98 => 20f,
            >= 98 and < 99 => 20.3f,
            >= 99 and < 99.5f => 20.8f,
            >= 99.5f and < 100 => 21.1f,
            >= 100 and < 100.5f => 21.6f,
            >= 100.5f => 22.4f,

            _ => 0f
        };
    }

    private static int GetRating(float chartRating, float achievement)
    {
        return (int)(chartRating * GetRatingFactor(achievement) * (achievement > 100.5 ? 100.5 : achievement) / 100f);
    }

    public bool TryGetInformationFromLapis(long qqId, int songId, out InfoCommand.GetScore.ScoreData result)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        var scores =
            DatabaseHandler.Instance.SongMetaDatabaseOperator.GetScores(qqId, songId, db).ToList();
        if (scores.Count == 0)
        {
            result = null;
            return false;
        }

        var songDto = MaiCommandBase.MaiCommandInstance.ToSongDto(scores[0].Song);

        result = new InfoCommand.GetScore.ScoreData(scores.Select(x => new InfoCommand.GetScore.LevelDto
        {
            LevelIndex = x.LevelIndex,
            Achievement = x.Achievements,
            DxScore = x.DxScore,
            Fc = x.Fc,
            Fs = x.Fs,
            PlayCount = x.PlayCount,
            Rating = GetRating(x.Song.Charts.FirstOrDefault(y => y.LevelIndex == x.LevelIndex)?.Rating ?? 0,
                x.Achievements),
            Rate = MaiCommandBase.GetRate(x.Achievements)
        }).ToArray(), songDto);

        return true;
    }

    public bool UserDataCached(long qqId)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        return db.Scores.FirstOrDefault(x => x.QqId == qqId) != null;
    }

    public ChartScoreData[] GetScoreByVersionFromLapis(string[] versions, long qqId)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;
        return DatabaseHandler.Instance.SongMetaDatabaseOperator.GetScoresByVersions(versions, db, qqId)
            .ToArray();
    }

    private bool TryGetB50Core(long qqId, out BestDto result, Comparison<BestItem> comparison)
    {
        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        var previousVersionScores =
            DatabaseHandler.Instance.SongMetaDatabaseOperator.GetUserScores(qqId, db).ToList();

        if (previousVersionScores.Count == 0)
        {
            result = null;
            return false;
        }

        var currentVersionScores = previousVersionScores.Where(x => x.Song.Version == CurrentVersion).ToList();
        previousVersionScores.RemoveAll(currentVersionScores.Contains);

        var currentBestItems = currentVersionScores.Select(x =>
            new BestItem(
                x,
                db.ChartMetaDataSet.FirstOrDefault(y =>
                    y.LevelIndex == x.LevelIndex && y.SongId == x.SongId),
                x.Song
            )).ToList();

        var previousBestItems = previousVersionScores.Select(x =>
            new BestItem(
                x,
                db.ChartMetaDataSet.FirstOrDefault(y =>
                    y.LevelIndex == x.LevelIndex && y.SongId == x.SongId),
                x.Song
            )).ToList();

        currentBestItems.Sort((x,y) => -comparison(x, y));
        previousBestItems.Sort((x,y) => -comparison(x, y));

        var currentVersionBestItemsRanked = currentBestItems.GetRange(0, 15);
        var previousVersionBestItemsRanked = previousBestItems.GetRange(0, 35);

        var hasName = GroupMemberCommandBase.GroupMemberCommandInstance.TryGetNickname(qqId, out var nickname);

        nickname = hasName ? nickname : qqId.ToString();

        result = new BestDto
        {
            Rating = currentVersionBestItemsRanked.Sum(x => x.Rating) +
                     previousVersionBestItemsRanked.Sum(x => x.Rating),
            Username = nickname,

            Charts = new BestDto.ChartsDto
            {
                DxCharts = currentVersionBestItemsRanked.Select(x => new BestDto.ScoreDto
                {
                    Achievements = x.ChartScore.Achievements,
                    Rate = MaiCommandBase.GetRate(x.ChartScore.Achievements),
                    DifficultyFactor = x.ChartMetaData.Rating,
                    DxScore = x.ChartScore.DxScore,
                    Fc = x.ChartScore.Fc,
                    Fs = x.ChartScore.Fs,
                    Id = x.ChartScore.SongId,
                    Type = MaiCommandBase.GetSongType(x.SongMetaData.SongId).ToString(),
                    LevelIndex = x.ChartScore.LevelIndex,
                    Title = x.SongMetaData.Title,
                    MaxDxScore = x.ChartMetaData.MaxDxScore,
                    Rating = x.Rating,
                    PlayCount = x.ChartScore.PlayCount
                }).ToArray(),

                SdCharts = previousVersionBestItemsRanked.Select(x => new BestDto.ScoreDto
                {
                    Achievements = x.ChartScore.Achievements,
                    Rate = MaiCommandBase.GetRate(x.ChartScore.Achievements),
                    DifficultyFactor = x.ChartMetaData.Rating,
                    DxScore = x.ChartScore.DxScore,
                    Fc = x.ChartScore.Fc,
                    Fs = x.ChartScore.Fs,
                    Id = x.ChartScore.SongId,
                    Type = MaiCommandBase.GetSongType(x.SongMetaData.SongId).ToString(),
                    LevelIndex = x.ChartScore.LevelIndex,
                    Title = x.SongMetaData.Title,
                    MaxDxScore = x.ChartMetaData.MaxDxScore,
                    Rating = x.Rating,
                    PlayCount = x.ChartScore.PlayCount
                }).ToArray()
            }
        };

        return true;
    }

    public bool TryGetB50FromLapis(long qqId, out BestDto result)
    {
        return TryGetB50Core(qqId, out result, (x, y) => x.Rating.CompareTo(y.Rating));
    }

    public bool TryGetPc50(long qqId, out BestDto result)
    {
        return TryGetB50Core(qqId, out result, (x, y) => 
            x.ChartScore.PlayCount.CompareTo(y.ChartScore.PlayCount));
    }

    private class BestItem
    {
        public readonly ChartMetaData ChartMetaData;
        public readonly ChartScoreData ChartScore;
        public readonly int Rating;
        public readonly SongMetaData SongMetaData;

        public BestItem(ChartScoreData chartScore, ChartMetaData chartMetaData, SongMetaData songMetaData)
        {
            ChartScore = chartScore;
            ChartMetaData = chartMetaData;
            SongMetaData = songMetaData;
            Rating = GetRating(ChartMetaData.Rating, ChartScore.Achievements);
        }
    }
}