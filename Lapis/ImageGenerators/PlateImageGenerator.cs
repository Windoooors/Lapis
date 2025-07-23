using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.ImageOperation;

namespace Lapis.ImageGenerators;

public class PlateImageGenerator
{
    public string Generate(List<PlateCommand.SongToBeDisplayed> songsToBeDisplayed,
        List<PlateCommand.SongToBeDisplayed> allSongs, string username, MaiCommand maiCommand,
        PlateCommand.PlateCategories category, string userId, bool usingHead, int plateVersionIndex, bool isCompressed)
    {
        var difficulties = new Dictionary<string, List<PlateCommand.SongToBeDisplayed>>();
        foreach (var song in songsToBeDisplayed)
        {
            var rating = Math.Round(song.SongDto.Ratings[song.LevelIndex], 1);
            if ((rating > 13.5m) & (rating < 14.0m))
            {
                if (!difficulties.ContainsKey("13+"))
                {
                    difficulties.Add("13+", [song]);
                }
                else
                {
                    if (difficulties.TryGetValue("13+", out var songs))
                        songs.Add(song);
                }
            }

            if ((rating > 13.9m) & (rating < 14.6m))
            {
                if (!difficulties.ContainsKey("14"))
                {
                    difficulties.Add("14", [song]);
                }
                else
                {
                    if (difficulties.TryGetValue("14", out var songs))
                        songs.Add(song);
                }
            }

            if ((rating > 14.5m) & (rating < 15.0m))
            {
                if (!difficulties.ContainsKey("14+"))
                {
                    difficulties.Add("14+", [song]);
                }
                else
                {
                    if (difficulties.TryGetValue("14+", out var songs))
                        songs.Add(song);
                }
            }

            if (rating > 14.9m)
            {
                if (!difficulties.ContainsKey("15"))
                {
                    difficulties.Add("15", [song]);
                }
                else
                {
                    if (difficulties.TryGetValue("15", out var songs))
                        songs.Add(song);
                }
            }
        }

        var totalHeight = 171;

        if (difficulties.ContainsKey("15"))
            if (difficulties.TryGetValue("15", out var songs))
                totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;

        if (difficulties.ContainsKey("14+"))
            if (difficulties.TryGetValue("14+", out var songs))
                totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;

        if (difficulties.ContainsKey("14"))
            if (difficulties.TryGetValue("14", out var songs))
                totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;

        if (difficulties.ContainsKey("13+"))
            if (difficulties.TryGetValue("13+", out var songs))
                totalHeight += 100 * (int)Math.Ceiling((float)songs.Count / 7) + 75;

        var sortedDifficulties =
            difficulties.OrderByDescending(d =>
                    Array.IndexOf(maiCommand.LevelDictionary.Keys.ToArray(), d.Key))
                .ToDictionary();

        totalHeight += 104;

        using var image = new Image(700, totalHeight);

        using var backgroundImage = new Image(AppContext.BaseDirectory + @"resource/covers/" +
                                              sortedDifficulties.Values.ToArray()[0][0].SongDto.Id + ".png");

        backgroundImage.Resize(75, 75);

        backgroundImage.GaussianBlur(2.5f);

        if (totalHeight > 700)
        {
            backgroundImage.Resize(totalHeight, totalHeight);
            image.DrawImage(backgroundImage, (700 - totalHeight) / 2, 0);
        }
        else
        {
            backgroundImage.Resize(700, 700);
            image.DrawImage(backgroundImage, 0, (totalHeight - 700) / 2);
        }

        using var mask = new Image(AppContext.BaseDirectory + "resource/plate/mask.png");

        if (totalHeight > 700)
            mask.Resize(totalHeight, totalHeight);
        else
            mask.Resize(700, 700);

        image.DrawImage(mask, 0, 0);

        using var header = new Image(AppContext.BaseDirectory + @"resource/plate/header.png");

        using var nameFormImage = new Image(AppContext.BaseDirectory + @"resource/plate/name_form.png");

        var i = 0;
        var j = 0;

        using var itemsInFirstGroup = new Image(700, totalHeight);

        foreach (var song in sortedDifficulties.Values.ToArray()[0])
        {
            using var item = GenerateItem(song.SongDto, song.LevelIndex, song.ScoreDto, category);
            itemsInFirstGroup.DrawImage(item, i * 100, 171 + j * 100);
            i++;
            if (i % 7 == 0)
            {
                i = 0;
                j++;
            }
        }

        var compositingLeft = (image.Width - itemsInFirstGroup.Width) / 2;
        var compositingTop = (image.Height - itemsInFirstGroup.Height) / 2;

        image.DrawImage(itemsInFirstGroup, compositingLeft, compositingTop);

        image.DrawImage(header, 0, 0);

        Image head;
        if (usingHead)
        {
            head = ApiOperator.Instance.UrlToImage(
                "https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640");
        }
        else
        {
            head = new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/best50_userhead_background.png"));

            head.DrawText(username.Substring(0, 1),
                new Color(0f, 0.8f, 1f, 0.1f),
                36,
                FontWeight.Regular,
                3, 59);
        }

        head.Resize(65, 65);

        var path = AppContext.BaseDirectory + "resource/plate_images/" + plateVersionIndex + "/" +
                   (int)category + ".png";

        using var plateImage =
            new Image(File.Exists(path) ? path : AppContext.BaseDirectory + "resource/plate_images/0.png");

        plateImage.Resize(528, 85);

        using var plateShadow = new Image(AppContext.BaseDirectory + "resource/plate/plate_shadow.png");

        image.DrawImage(plateShadow, 0, 0);
        image.DrawImage(plateImage, 2, 15);
        image.DrawImage(nameFormImage, 0, 0);

        using var nameForm = new Image(500, 65);

        nameForm.DrawText(username, new Color(1, 1, 1, 1), 36, FontWeight.Light, 4, 59);

        image.DrawImage(nameForm, 74, 25);
        image.DrawImage(head, 12, 25);

        head.Dispose();

        image.DrawText(sortedDifficulties.Keys.ToArray()[0], new Color(1, 1, 1, 1), 20, FontWeight.Heavy, 6.3f, 162.3f);

        var top = (int)(171 + Math.Ceiling((float)sortedDifficulties.Values.ToArray()[0].Count / 7) * 100 + 75);
        for (var k = 1; k < sortedDifficulties.Values.ToArray().Length; k++)
        {
            i = 0;
            j = 0;
            var itemGroup = new Image(700, totalHeight);
            foreach (var song in sortedDifficulties.Values.ToArray()[k])
            {
                using var item = GenerateItem(song.SongDto, song.LevelIndex, song.ScoreDto, category);
                itemGroup.DrawImage(item, i * 100, top + j * 100);
                i++;
                if (i % 7 == 0)
                {
                    i = 0;
                    j++;
                }
            }

            if (sortedDifficulties.Values.ToArray()[k].Count % 7 == 0)
                j--;
            /*
            var shadow = itemGroup.Clone();

            shadow.Resize(new Percentage(5));

            shadow.Shadow(0, 0, 100, new Percentage(50), new MagickColor(0, 0, 0));

            shadow.Resize(new Percentage(2000));

            shadow.Composite(itemGroup, Gravity.Center, CompositeOperator.Blend);

            image.Composite(shadow, Gravity.Center, CompositeOperator.Atop);

            shadow.Dispose();*/

            compositingLeft = (image.Width - itemGroup.Width) / 2;
            compositingTop = (image.Height - itemGroup.Height) / 2;

            image.DrawImage(itemGroup, compositingLeft, compositingTop);

            itemGroup.Dispose();

            using var banner = new Image(AppContext.BaseDirectory + "resource/plate/banner.png");

            banner.DrawText(sortedDifficulties.Keys.ToArray()[k], new Color(1, 1, 1, 1), 20, FontWeight.Heavy, 6.3f,
                110.3f);

            image.DrawImage(banner, 0, top - 75 - 44);

            top += (j + 1) * 100 + 75;
        }

        var watermark = new Image(AppContext.BaseDirectory + "resource/plate/watermark.png");

        image.DrawImage(watermark, 17, totalHeight - 151);

        image.DrawText($"Generated by {BotConfiguration.Instance.BotName}", new Color(1f, 1f, 1f, 0.06f), 22,
            FontWeight.Regular, 15, totalHeight - 40);

        watermark.Dispose();

        var re = 0;
        var reAll = 0;
        var mas = 0;
        var masAll = 0;
        var exp = 0;
        var expAll = 0;
        var avd = 0;
        var avdAll = 0;
        var bas = 0;
        var basAll = 0;

        foreach (var song in allSongs)
            switch (song.LevelIndex)
            {
                case 0:
                    basAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.Bazhe)
                        bas++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.Jiang)
                        bas++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app") &&
                             category == PlateCommand.PlateCategories.Shen)
                        bas++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp" || song.ScoreDto.Fc == "ap" ||
                              song.ScoreDto.Fc == "app") && category == PlateCommand.PlateCategories.Ji)
                        bas++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp") &&
                             category == PlateCommand.PlateCategories.Wuwu)
                        bas++;
                    break;
                case 1:
                    avdAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.Bazhe)
                        avd++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.Jiang)
                        avd++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app") &&
                             category == PlateCommand.PlateCategories.Shen)
                        avd++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp" || song.ScoreDto.Fc == "ap" ||
                              song.ScoreDto.Fc == "app") && category == PlateCommand.PlateCategories.Ji)
                        avd++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp") &&
                             category == PlateCommand.PlateCategories.Wuwu)
                        avd++;
                    break;
                case 2:
                    expAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.Bazhe)
                        exp++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.Jiang)
                        exp++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app") &&
                             category == PlateCommand.PlateCategories.Shen)
                        exp++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp" || song.ScoreDto.Fc == "ap" ||
                              song.ScoreDto.Fc == "app") && category == PlateCommand.PlateCategories.Ji)
                        exp++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp") &&
                             category == PlateCommand.PlateCategories.Wuwu)
                        exp++;
                    break;
                case 3:
                    masAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.Bazhe)
                        mas++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.Jiang)
                        mas++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app") &&
                             category == PlateCommand.PlateCategories.Shen)
                        mas++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp" || song.ScoreDto.Fc == "ap" ||
                              song.ScoreDto.Fc == "app") && category == PlateCommand.PlateCategories.Ji)
                        mas++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp") &&
                             category == PlateCommand.PlateCategories.Wuwu)
                        mas++;
                    break;
                case 4:
                    reAll++;
                    if (song.ScoreDto.Achievements >= 80 && category == PlateCommand.PlateCategories.Bazhe)
                        re++;
                    else if (song.ScoreDto.Achievements >= 100 && category == PlateCommand.PlateCategories.Jiang)
                        re++;
                    else if ((song.ScoreDto.Fc == "ap" || song.ScoreDto.Fc == "app") &&
                             category == PlateCommand.PlateCategories.Shen)
                        re++;
                    else if ((song.ScoreDto.Fc == "fc" || song.ScoreDto.Fc == "fcp" || song.ScoreDto.Fc == "ap" ||
                              song.ScoreDto.Fc == "app") && category == PlateCommand.PlateCategories.Ji)
                        re++;
                    else if ((song.ScoreDto.Fs == "fsd" || song.ScoreDto.Fs == "fsdp") &&
                             category == PlateCommand.PlateCategories.Wuwu)
                        re++;
                    break;
            }

        if (reAll != 0)
            image.DrawText("Re:MASTER 完成度：" + re + "/" + reAll, new Color(1, 1, 1, 0.5f), 18, FontWeight.Regular,
                HorizontalAlignment.Right, 700 - 17f, totalHeight - 125);

        image.DrawText("MASTER 完成度：" + mas + "/" + masAll, new Color(1, 1, 1, 0.5f), 18, FontWeight.Regular,
            HorizontalAlignment.Right, 700 - 17f, totalHeight - 100);
        image.DrawText("EXPERT 完成度：" + exp + "/" + expAll, new Color(1, 1, 1, 0.5f), 18, FontWeight.Regular,
            HorizontalAlignment.Right, 700 - 17f, totalHeight - 75);
        image.DrawText("ADVANCED 完成度：" + avd + "/" + avdAll, new Color(1, 1, 1, 0.5f), 18, FontWeight.Regular,
            HorizontalAlignment.Right, 700 - 17f, totalHeight - 50);
        image.DrawText("BASIC 完成度：" + bas + "/" + basAll, new Color(1, 1, 1, 0.5f), 18, FontWeight.Regular,
            HorizontalAlignment.Right, 700 - 17f, totalHeight - 25);

        var result = image.ToBase64(isCompressed);
        return result;
    }

    private Image GenerateItem(SongDto songDto, int levelIndex, ScoresDto.ScoreDto scoreDto,
        PlateCommand.PlateCategories category)
    {
        var image = new Image(AppContext.BaseDirectory + "resource/covers/" + songDto.Id + ".png");
        var gradient = new Image(AppContext.BaseDirectory + "resource/plate/gradient.png");

        var dominantColor = image.GetDominantColor();

        var colorImage = new Image(gradient.Width, gradient.Height, dominantColor);

        colorImage.FuseAlpha(gradient);

        image.DrawImage(colorImage, 0, 0);

        gradient.Dispose();
        colorImage.Dispose();

        var whiteOnBlack = image.isWhiteOnDark();

        var textColor = whiteOnBlack ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);

        var textLayer = new Image(100, 100);

        var difficulty = "";

        switch (levelIndex)
        {
            case 0:
                difficulty = "BAS";
                break;
            case 1:
                difficulty = "AVD";
                break;
            case 2:
                difficulty = "EXP";
                break;
            case 3:
                difficulty = "MAS";
                break;
            case 4:
                difficulty = "RE";
                break;
        }

        var indicatorText = "";

        if (category == PlateCommand.PlateCategories.Bazhe && scoreDto.Achievements >= 80)
            if (scoreDto.Achievements >= 80)
                indicatorText = "Cleared";

        if (category == PlateCommand.PlateCategories.Jiang && scoreDto.Achievements >= 100)
            indicatorText = scoreDto.Achievements >= 100.5 ? "SSS+" : "SSS";

        if (category == PlateCommand.PlateCategories.Shen && (scoreDto.Fc == "ap" || scoreDto.Fc == "app"))
            if (scoreDto.Fc.Length > 2)
                indicatorText = scoreDto.Fc.Substring(0, scoreDto.Fc.Length - 1).ToUpper() + "+";
            else
                indicatorText = scoreDto.Fc.ToUpper();

        if (category == PlateCommand.PlateCategories.Ji && (scoreDto.Fc == "fc" || scoreDto.Fc == "fcp" ||
                                                            scoreDto.Fc == "ap" || scoreDto.Fc == "app"))
            if (scoreDto.Fc.Length > 2)
                indicatorText = scoreDto.Fc.Substring(0, scoreDto.Fc.Length - 1).ToUpper() + "+";
            else
                indicatorText = scoreDto.Fc.ToUpper();

        if (category == PlateCommand.PlateCategories.Wuwu && (scoreDto.Fs == "fsd" || scoreDto.Fs == "fsdp"))
            indicatorText = scoreDto.Fs.Length > 2 ? scoreDto.Fs.Replace("p", "+").ToUpper() : scoreDto.Fs.ToUpper();

        textLayer.DrawText(songDto.Title, textColor, 18, FontWeight.Regular, 7.6f, 37.2f);

        var gradientText = new Image(AppContext.BaseDirectory + "resource/plate/gradient_text.png");

        textLayer.FuseAlpha(gradientText);

        gradientText.Dispose();

        textLayer.DrawText("ID " + songDto.Id, new Color(textColor.R, textColor.G, textColor.B, textColor.A / 2), 10,
            FontWeight.Regular, 7.6f, 18.2f);
        textLayer.DrawText(songDto.Type + " " + difficulty, textColor, 10,
            FontWeight.Heavy, 7.6f, 50.2f);

        if (indicatorText != "")
            textLayer.DrawText(indicatorText, textColor, 10,
                FontWeight.Heavy, HorizontalAlignment.Right, 100 - 7.6f, 18.2f);

        image.DrawImage(textLayer, 0, 0);
        textLayer.Dispose();

        return image;
    }
}