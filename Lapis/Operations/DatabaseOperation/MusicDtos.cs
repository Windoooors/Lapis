using System;
using System.ComponentModel.DataAnnotations;
using Lapis.Commands.GroupCommands;
using Newtonsoft.Json;

namespace Lapis.Operations.DatabaseOperation;

public class WckMusicDataResponseItemDto
{
    [JsonProperty] public int Id { get; set; }
    [JsonProperty] public int Level { get; set; }
    [JsonProperty] public int Achievement { get; set; }
    [JsonProperty] public int DxScore { get; set; }
    [JsonProperty] public int ComboStatus { get; set; }
    [JsonProperty] public int SyncStatus { get; set; }
    [JsonProperty] public int PlayCount { get; set; }
}

public class ChartScoreData
{
    public ChartScoreData(DivingFishMusicDataDto divingFishMusicDataDto, SongMetaData song, long qqId)
    {
        Achievements = divingFishMusicDataDto.Achievements;
        DxScore = divingFishMusicDataDto.DxScore;
        Fc = divingFishMusicDataDto.Fc;
        Fs = divingFishMusicDataDto.Fs;
        LevelIndex = divingFishMusicDataDto.LevelIndex;
        PlayCount = -1;
        SongId = song.SongId;
        QqId = qqId;

        Song = song;
    }

    public ChartScoreData()
    {
    }

    public ChartScoreData(WckMusicDataResponseItemDto wckMusicDataDto, long qqId)
    {
        Achievements = wckMusicDataDto.Achievement / 10000f;
        DxScore = wckMusicDataDto.DxScore;
        Fc = wckMusicDataDto.ComboStatus switch
        {
            0 => "", 1 => "fc", 2 => "fcp", 3 => "ap", 4 => "app", _ => ""
        };
        Fs = wckMusicDataDto.SyncStatus switch
        {
            0 => "",
            5 => "sync",
            1 => "fs",
            2 => "fsp",
            3 => "fsd",
            4 => "fsdp",
            _ => ""
        };
        LevelIndex = wckMusicDataDto.Id < 100000 ? wckMusicDataDto.Level : wckMusicDataDto.Level - 10;
        PlayCount = wckMusicDataDto.PlayCount;
        SongId = wckMusicDataDto.Id;
        QqId = qqId;
    }

    public int SongId { get; }
    public float Achievements { get; set; }
    public int DxScore { get; set; }

    [MaxLength(8)] public string Fc { get; set; }

    [MaxLength(8)] public string Fs { get; set; }

    public int LevelIndex { get; }
    public int PlayCount { get; set; }
    public long QqId { get; init; }

    public SongMetaData Song { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ChartScoreData other && SongId == other.SongId && LevelIndex == other.LevelIndex &&
               QqId == other.QqId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SongId, LevelIndex, QqId);
    }

    public DivingFishMusicDataDto ToDivingFishDto()
    {
        return new DivingFishMusicDataDto
        {
            Achievements = Achievements,
            DxScore = DxScore,
            Fc = Fc,
            Fs = Fs,
            LevelIndex = LevelIndex,
            Title = Song.Title,
            Type = MaiCommandBase.GetSongType(SongId).ToString()
        };
    }
}

public class DivingFishMusicDataDto
{
    [JsonProperty("achievements")] public float Achievements;
    [JsonProperty("dxScore")] public int DxScore;
    [JsonProperty("fc")] public string Fc;
    [JsonProperty("fs")] public string Fs;
    [JsonProperty("level_index")] public int LevelIndex;
    [JsonProperty("title")] public string Title;
    [JsonProperty("type")] public string Type;
}