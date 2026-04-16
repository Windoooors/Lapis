using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using Lapis.Commands.GroupCommands;
using Lapis.Operations.ApiOperation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lapis.Commands.PrivateCommands;

public class UpdateMaiDataCommand : PrivateCommand
{
    public UpdateMaiDataCommand()
    {
        CommandHead = "update-mai-data";
        DirectCommandHead = "update-mai-data";
    }

    public override void Parse(string originalPlainMessage, CqPrivateMessagePostContext source)
    {
        if (source.Sender.UserId != BotConfiguration.Instance.AdministratorQqNumber)
            return;
        
        SendMessage(source, [new CqReplyMsg(source.MessageId), "更新中..."]);

        try
        {
            UpdateSongDatabase();
        }
        catch(Exception e)
        {
            SendMessage(source, [new CqReplyMsg(source.MessageId), "更新失败"]);

            Program.Logger.LogError($"Failed to update maimai metadata. Message: {e.Message} StackTrace: {e.StackTrace}");
            return;
        }

        SendMessage(source, [new CqReplyMsg(source.MessageId), "更新成功"]);
    }

    private void UpdateSongDatabase()
    {
        var chartStatsResponse = ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
            "api/maimaidxprober/chart_stats", 60);

        if (chartStatsResponse.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {chartStatsResponse.StatusCode}", null,
                chartStatsResponse.StatusCode);

        var chartStatisticsResult =
            JsonConvert.DeserializeObject<ChartStatisticsDto>(chartStatsResponse.Result);

        var songMetaResult = ApiOperator.Instance.Get(BotConfiguration.Instance.DivingFishUrl,
            "api/maimaidxprober/music_data", 60);

        if (songMetaResult.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {songMetaResult.StatusCode}", null,
                songMetaResult.StatusCode);

        var songs = (SongDto[])JsonConvert.DeserializeObject(songMetaResult.Result, typeof(SongDto[]));

        if (songs != null)
            foreach (var song in songs)
            {
                foreach (var chart in song.Charts)
                foreach (var notes in chart.Notes)
                    chart.MaxDxScore += notes * 3;

                chartStatisticsResult.Charts.TryGetValue(song.Id.ToString(), out var chartStatistics);
                List<float> fitRatings = new();
                if (chartStatistics != null)
                    foreach (var chartStatistic in chartStatistics)
                        fitRatings.Add(chartStatistic.FitRating);
                else
                    foreach (var rating in song.Ratings)
                        fitRatings.Add(rating);

                song.FitRatings = fitRatings.ToArray();

                if (song.Id < 100000)
                    continue;
                for (var i = 0; i < song.Levels.Length; i++)
                    if (!song.Levels[i].Contains('?'))
                        song.Levels[i] += '?';
            }

        var aliasResponseContent = ApiOperator.Instance.Get(BotConfiguration.Instance.AliasUrl, 60);

        if (aliasResponseContent.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected status code: {aliasResponseContent.StatusCode}", null,
                aliasResponseContent.StatusCode);

        var aliasDto =
            JsonConvert.DeserializeObject<AliasDto>(
                aliasResponseContent.Result);

        using var db = DatabaseHandler.Instance.SongMetaDatabaseOperator.GetDb;

        DatabaseHandler.Instance.SongMetaDatabaseOperator.InsertSongs(songs, aliasDto.Content, db);
    }
}