using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Miscellaneous;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.ImageOperation;
using HorizontalAlignment = Lapis.Operations.ImageOperation.HorizontalAlignment;

namespace Lapis.ImageGenerators;

public class InfoImageGenerator : ImageGenerator
{
    private static readonly Dictionary<MaiCommandBase.Rate, string> RateImagePaths =
        new ()
        {
            { MaiCommandBase.Rate.D, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/d.png") },
            { MaiCommandBase.Rate.C, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/c.png") },
            { MaiCommandBase.Rate.B, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/b.png") },
            { MaiCommandBase.Rate.Bb, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/bb.png") },
            { MaiCommandBase.Rate.Bbb, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/bbb.png") },
            { MaiCommandBase.Rate.A, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/a.png") },
            { MaiCommandBase.Rate.Aa, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/aa.png") },
            { MaiCommandBase.Rate.Aaa, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/aaa.png") },
            { MaiCommandBase.Rate.S, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/s.png") },
            { MaiCommandBase.Rate.Sp, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/s_plus.png") },
            { MaiCommandBase.Rate.Ss, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/ss.png") },
            { MaiCommandBase.Rate.Ssp, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/ss_plus.png") },
            { MaiCommandBase.Rate.Sss, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/sss.png") },
            { MaiCommandBase.Rate.Sssp, Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/sss_plus.png") },
        };

    private static readonly string BaseUtageImagePath = Path.Combine(AppContext.BaseDirectory, "resource/info/base_utage.png");
    private static readonly string BaseImagePath = Path.Combine(AppContext.BaseDirectory, "resource/info/base.png");
    private static readonly string CoverBaseImagePath = Path.Combine(AppContext.BaseDirectory, "resource/info/cover.png");
    private static readonly string NameBaseImagePath = Path.Combine(AppContext.BaseDirectory, "resource/info/name_background.png");
    private static readonly string InfoMaskImagePath = Path.Combine(AppContext.BaseDirectory, "resource/info/song_info_mask.png");

    private Image _maskImage;
    private Image _songAndChartInformationLayer;
    private Image _starImage;
    
    private Image GetBaseImage(SongDto song)
    {
        using var baseImage = new Image(song .Id >= 100000 ? BaseUtageImagePath : BaseImagePath);
        var blurredCover = _coverImage.Clone();
        
        blurredCover.Resize(75, 75);
        blurredCover.GaussianBlur(5);
        
        blurredCover.Resize(baseImage.Width, baseImage.Width);
        blurredCover.Crop(baseImage.Width, baseImage.Height);

        blurredCover.DrawImage(baseImage, 0, 0);
        
        return blurredCover;
    }

    private Image _coverImage;

    private void DrawSongInformationOnLayer(SongDto song, Image baseImage)
    {
        var typeText = song.Type.ToUpper();
        _songAndChartInformationLayer.DrawText(typeText, Color.White, 36, FontWeight.Heavy, HorizontalAlignment.Right, 230.4f, 77.1f);
        _songAndChartInformationLayer.DrawText($"ID {song.Id}", Color.White, 36, FontWeight.Regular, HorizontalAlignment.Left, 250.4f, 77.1f);

        _songAndChartInformationLayer.DrawText(song.BasicInfo.Artist, new Color(1f, 1f, 1f, 0.5f), 36, FontWeight.Regular,
            HorizontalAlignment.Left, 19.1f, 309.2f);
        _songAndChartInformationLayer.DrawText(song.Title, Color.White, 60, FontWeight.Regular, HorizontalAlignment.Left, 19.8f, 357.8f);

        _songAndChartInformationLayer.DrawText("BPM", new Color(1, 1, 1, 0.5f), 28, FontWeight.Regular, HorizontalAlignment.Left, 19.6f, 439f);
        _songAndChartInformationLayer.DrawText(song.BasicInfo.Bpm.ToString("0"), Color.White, 48, FontWeight.Regular, HorizontalAlignment.Left,
            19f, 479.6f);

        _songAndChartInformationLayer.DrawText("From Version", new Color(1, 1, 1, 0.5f), 28, FontWeight.Regular, HorizontalAlignment.Left,
            19.6f, 519.3f);

        var versionNames = SharedConsts.DxVersionToChineseVersionName
            .Where(x => x.Key == song.BasicInfo.Version)
            .Select(x => x.Value).ToArray();
        SharedConsts.VersionNameToPlateCharacters.TryGetValue(song.BasicInfo.Version, out var plateVersionNames);

        plateVersionNames ??= ["未知"];
        var versionText = (versionNames.Length > 0 ? versionNames[0] : song.BasicInfo.Version) +
                          "·" + (plateVersionNames.Count > 1
                              ? plateVersionNames[0] + "、" + plateVersionNames[1]
                              : plateVersionNames[0]);

        _songAndChartInformationLayer.DrawText(versionText, Color.White, 48, FontWeight.Regular, HorizontalAlignment.Left, 20.2f, 558.5f);

        using var coverShadow = new Image(CoverBaseImagePath);
        
        baseImage.DrawText(song.Title, new Color(1,1,1,0.03f), 320f, FontWeight.Heavy, HorizontalAlignment.Left, VerticalAlignment.Bottom, -84.2f, 585f);
        
        baseImage.DrawImage(coverShadow, 0, 0);
        
        baseImage.DrawImage(_coverImage, 981, 173);
    }

    private void DrawChartInformationOnLayer(SongDto song)
    {
        for (var i = 0; i < 5; i++)
        {
            var baseHeight = i * ItemHeight;

            var chart = i < song.Charts.Length ? song.Charts[i] : null;

            if (chart == null)
            {
                if (song.Id < 100000)
                    _songAndChartInformationLayer.DrawText("NaN", Color.White, 48, FontWeight.Regular, 97.1f,
                        705.5f + baseHeight);
                continue;
            }
            
            _songAndChartInformationLayer.DrawText($"by {(chart.Charter == "-" ? "未知谱作者" : chart.Charter)}", new Color(1, 1, 1, 0.5f), 28, FontWeight.Regular, 96.5f,
                663.6f + baseHeight);

            var ratingText = song.Ratings[i].ToString("0.0");
            var ratingTextLength = Image.MeasureString(ratingText, FontWeight.Regular, 48);
            
            _songAndChartInformationLayer.DrawText(ratingText, Color.White, 48, FontWeight.Regular, 97.1f,
                705.5f + baseHeight);
            _songAndChartInformationLayer.DrawText($"fit {song.FitRatings[i] :0.00}", Color.White, 36, FontWeight.Regular, 110.1f + ratingTextLength,
                705.5f + baseHeight);
        }
    }

    private const float ItemHeight = 137;

    private void DrawScoreInformation(SongDto song,InfoCommand.GetScore.ScoreData levels, Image baseImage)
    {
        if (levels == null)
            return;

        foreach (var level in levels.Levels)
        {
            var i = level.LevelIndex;
            var baseHeight = i * ItemHeight;

            var achievementText = $"{level.Achievement:0.0000}%·{level.Rating}";
            using var rateImage = new Image(RateImagePaths[level.Rate]);

            baseImage.DrawText(achievementText, Color.White, 36, FontWeight.Regular, HorizontalAlignment.Right, baseImage.Width - rateImage.Width - 25,
                709f + baseHeight);
            baseImage.DrawImage(rateImage, 1384, (int)(715 + baseHeight), -rateImage.Width, -rateImage.Height);

            var indicatorText =
                $"{(level.PlayCount == -1 ? "" : ("PC " + level.PlayCount + "·"))} {level.Fc} {level.Fs}".Trim().Trim('·').ToUpper();
            baseImage.DrawText(indicatorText, Color.White, 28, FontWeight.Regular, HorizontalAlignment.Right, 1384,
                675.4f + baseHeight);

            var dxScoreText = $"{level.DxScore}/{song.Charts[level.LevelIndex].MaxDxScore}";
            var dxScoreTextLength = Image.MeasureString(dxScoreText, FontWeight.Regular, 28);
            baseImage.DrawText(dxScoreText, Color.White, 28, FontWeight.Regular, HorizontalAlignment.Right, 1384.3f,
                646.4f + baseHeight);
            baseImage.DrawText("DX 分数", new  Color(1, 1, 1, 0.5f), 28, FontWeight.Regular,HorizontalAlignment.Right, 1375.3f - dxScoreTextLength, 644.4f + baseHeight);
            
            var stars = 0;
            var starRate = (float)level.DxScore / song.Charts[level.LevelIndex].MaxDxScore * 100;
            if (starRate is <= 100 and >= 97)
                stars = 5;
            if (starRate is < 97 and >= 95)
                stars = 4;
            if (starRate is < 95 and >= 93)
                stars = 3;
            if (starRate is < 93 and >= 90)
                stars = 2;
            if (starRate is < 90 and >= 85)
                stars = 1;
            if (starRate < 85)
                stars = 0;

            for (int j = 0; j < stars; j++)
            {
                var baseLeft = 1368;
                var baseTop = 609 + baseHeight;
                
                baseImage.DrawImage(_starImage, baseLeft - j * 18, (int)baseTop);
            }
        }
    }

    public string Generate(SongDto song, string title, InfoCommand.GetScore.ScoreData levels, long userId, bool usingHead, bool isCompressed)
    {
        _coverImage = new Image(GetHdSongCoverPath(song.Id));
        _maskImage = new Image(InfoMaskImagePath);
        _songAndChartInformationLayer = new Image(_maskImage.Width, _maskImage.Height);
        _starImage = new Image(Path.Combine(AppContext.BaseDirectory, "resource/info/star.png"));
        
        using var image = GetBaseImage(song);

        image.DrawText($"Generated by {BotConfiguration.Instance.BotName}", new Color(1, 1, 1, 0.13f), 28,
            FontWeight.Regular, HorizontalAlignment.Right, 1380.6f, 37.5f);
        
        image.DrawText("Powered by Lapis", new Color(1, 1, 1, 0.13f), 28f,
            FontWeight.Regular, HorizontalAlignment.Right, 1379.9f, 67.5f);
        
        image.DrawText(title, new Color(1, 1, 1, 0.13f), 76.4f,
            FontWeight.Regular, HorizontalAlignment.Right, 1381.9f, 125.5f);

        if (levels?.Song != null)
        {
            using var nameBackground = new Image(NameBaseImagePath);

            nameBackground.DrawText(levels.UserName, Color.White, 36, FontWeight.Regular, 99.1f, 78.3f);

            if (usingHead)
            {
                if (ApiOperator.Instance.TryUrlToImage("https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640",
                        out var head))
                {
                    head.Resize(92, 92);
                    nameBackground.DrawImage(head, 0, 0);

                    head.Dispose();
                }
            }
            else
            {
                nameBackground.DrawText(levels.UserName.Substring(0, 1), new Color(0, 0.8f, 1, 0.1f), 36, FontWeight.Regular, 3,
                    82);
            }
            
            image.DrawImage(nameBackground, 0, 92);
        }
        
        DrawSongInformationOnLayer(song, image);
        DrawChartInformationOnLayer(song);
        DrawScoreInformation(song, levels, image);
        
        _songAndChartInformationLayer.FuseAlpha(_maskImage);
        _maskImage.Dispose();
        image.DrawImage(_songAndChartInformationLayer, 0, 0);
        
        _starImage.Dispose();
        _songAndChartInformationLayer.Dispose();
        _coverImage.Dispose();
        
        return image.ToBase64(isCompressed);
    }
}