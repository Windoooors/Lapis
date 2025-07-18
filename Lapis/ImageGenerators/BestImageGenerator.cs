﻿using System;
using System.IO;
using Lapis.Commands.GroupCommands;
using Lapis.Commands.GroupCommands.MaiCommands;
using Lapis.Operations.ApiOperation;
using Lapis.Operations.ImageOperation;

namespace Lapis.ImageGenerators;

public class BestImageGenerator
{
    public string Generate(BestDto best, string userId, bool usingHead, bool isCompressed)
    {
        Image head;
        if (usingHead)
        {
            head = ApiOperator.Instance.UrlToImage("https://q.qlogo.cn/g?b=qq&nk=" + userId + "&s=640");
        }
        else
        {
            head = new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/best50_userhead_background.png"));
            head.DrawText(best.Username.Substring(0, 1), new Color(0, 0.8f, 1, 0.1f), 36, FontWeight.Regular, 3,
                59);
        }

        head.Resize(65, 65);

        var image = GenerateBackground(best);
        var nameForm = new Image(500, 65);

        nameForm.DrawText(best.Username, Color.White, 36, FontWeight.Light, 2, 59);

        image.DrawImage(nameForm, 74, 25);

        nameForm.Dispose();

        if (best.Rating <= 999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_regular.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), Color.White, 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 999 and <= 1999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_blue.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0, 0.24f, 0.36f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 1999 and <= 3999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_green.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.25f, 0.38f, 0.09f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 3999 and <= 6999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_yellow.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.30f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 6999 and <= 9999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_red.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(1, 0.71f, 0.71f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 9999 and <= 11999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_purple.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.73f, 0.56f, 1f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 11999 and <= 12999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_bronze.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.18f, 0.07f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 12999 and <= 13999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_silver.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.1f, 0.1f, 0.1f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 13999 and <= 14499)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_gold.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.3f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating is > 14499 and <= 14999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_platinum.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.36f, 0.3f, 0.24f, 1), 36, FontWeight.Light, 74, 150);
        }
        else if (best.Rating > 14999)
        {
            using var ratingBackground =
                new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/score_background_rainbow.png"));
            image.DrawImage(ratingBackground, 73, 90);
            image.DrawText(best.Rating.ToString(), new Color(0.1f, 0f, 0.37f, 1), 36, FontWeight.Light, 74, 150);
        }

        image.DrawImage(head, 8, 25);

        head.Dispose();

        for (var i = 0; i < best.Charts.SdCharts.Length; i++)
        {
            var x = 0;
            var y = 0;
            if (i < 5)
            {
                y = 299;
                x = i * 350;
            }

            if (i is > 4 and < 10)
            {
                y = 399;
                x = (i - 5) * 350;
            }

            if (i is > 9 and < 15)
            {
                y = 499;
                x = (i - 10) * 350;
            }

            if (i is > 14 and < 20)
            {
                y = 599;
                x = (i - 15) * 350;
            }

            if (i is > 19 and < 25)
            {
                y = 699;
                x = (i - 20) * 350;
            }

            if (i is > 24 and < 30)
            {
                y = 799;
                x = (i - 25) * 350;
            }

            if (i is > 29 and < 35)
            {
                y = 899;
                x = (i - 30) * 350;
            }

            var item = GenerateItem(best.Charts.SdCharts[i], i + 1);

            image.DrawImage(item, x, y);

            item.Dispose();
        }

        for (var i = 0; i < best.Charts.DxCharts.Length; i++)
        {
            var x = 0;
            var y = 0;
            if (i < 5)
            {
                y = 1061;
                x = i * 350;
            }

            if (i is > 4 and < 10)
            {
                y = 1161;
                x = (i - 5) * 350;
            }

            if (i is > 9 and < 15)
            {
                y = 1261;
                x = (i - 10) * 350;
            }

            var item = GenerateItem(best.Charts.DxCharts[i], i + 1);

            image.DrawImage(item, x, y);

            item.Dispose();
        }

        var result = image.ToBase64(isCompressed);

        image.Dispose();

        return result;
    }

    private Image GenerateItem(BestDto.ScoreDto score, int rank)
    {
        var difficulty = string.Empty;
        var fontColor = new Color();

        var fcIndicatorText = score.Fc.Length > 2
            ? score.Fc.Remove(score.Fc.Length - 1).ToUpper() + "+"
            : score.Fc.ToUpper();

        var fsIndicatorText = score.Fs.Length > 2 ? score.Fs.Replace("p", "+").ToUpper() : score.Fs.ToUpper();


        switch (score.LevelIndex)
        {
            case 0:
                difficulty = "bas";
                fontColor = new Color(16128 / 65535f, 24832 / 65535f, 6144 / 65535f, 1);
                break;
            case 1:
                difficulty = "avd";
                fontColor = new Color(23296 / 65535f, 19968 / 65535f, 15872 / 65535f, 1);
                break;
            case 2:
                difficulty = "exp";
                fontColor = new Color(65535 / 65535f, 46848 / 65535f, 46848 / 65535f, 1);
                break;
            case 3:
                difficulty = "mas";
                fontColor = new Color(48128 / 65535f, 36864 / 65535f, 65535 / 65535f, 1);
                break;
            case 4:
                difficulty = "re";
                fontColor = new Color(34560 / 65535f, 29184 / 65535f, 46848 / 65535f, 1);
                break;
        }

        var foreground = new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/b50_item_foreground_" +
                                                                          difficulty + ".png"));

        Image background;
        if (File.Exists(Path.Combine(AppContext.BaseDirectory, "resource/covers/" + score.Id +
                                                               ".png")))
            background = new Image(Path.Combine(AppContext.BaseDirectory, "resource/covers/" +
                                                                          score.Id + ".png"));
        else
            background = new Image(Path.Combine(AppContext.BaseDirectory, "resource/covers/1000.png"));

        var foregroundColor = background.GetDominantColor();

        var foregroundColorImage = new Image(321, 100, foregroundColor);

        foregroundColorImage.DrawImage(foreground, 0, 0);

        foreground.Dispose();

        foreground = foregroundColorImage;

        using (var mask = new Image(Path.Combine(AppContext.BaseDirectory,
                   "resource/best50/b50_item_foreground_mask.png")))
        {
            foreground.FuseAlpha(mask);
        }

        background.Scale(100, 100);
        var image = new Image(350, 100);
        image.DrawImage(background, 0, 0);
        image.DrawImage(foreground, 29, 0);

        foreground.Dispose();

        var info = new Image(350, 100);

        info.DrawText(score.Type, fontColor, 13, FontWeight.Heavy, 100, 26);

        var isWhiteOnDark = background.isWhiteOnDark();

        fontColor = isWhiteOnDark ? Color.White : Color.Black;

        background.Dispose();

        info.DrawText(score.Title, fontColor, 16, FontWeight.Regular, 126, 28);
        info.DrawText(score.Achievements.ToString("0.0000") + "%", fontColor, 20, FontWeight.Heavy, 102, 50);
        info.DrawText(score.DifficultyFactor.ToString("0.0") + "·" + score.Rating, fontColor, 20, FontWeight.Heavy,
            102, 69);
        info.DrawText("#" + rank + "·ID " + score.Id, new Color(fontColor.R, fontColor.G, fontColor.B, 0.6f), 15,
            FontWeight.Regular, 102, 87);

        using var scoreLayerMask =
            new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/b50_item_score_layer_mask.png"));

        info.FuseAlpha(scoreLayerMask);

        var imagePath = "";
        switch (score.Rate)
        {
            case MaiCommandBase.Rate.Sss:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/sss.png");
                break;
            case MaiCommandBase.Rate.Sssp:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/sss_plus.png");
                break;
            case MaiCommandBase.Rate.Ss:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/ss.png");
                break;
            case MaiCommandBase.Rate.Ssp:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/ss_plus.png");
                break;
            case MaiCommandBase.Rate.S:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/s.png");
                break;
            case MaiCommandBase.Rate.Sp:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/s_plus.png");
                break;
            case MaiCommandBase.Rate.Aaa:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/aaa.png");
                break;
            case MaiCommandBase.Rate.Aa:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/aa.png");
                break;
            case MaiCommandBase.Rate.A:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/a.png");
                break;
            case MaiCommandBase.Rate.Bbb:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/bbb.png");
                break;
            case MaiCommandBase.Rate.Bb:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/bb.png");
                break;
            case MaiCommandBase.Rate.B:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/b.png");
                break;
            case MaiCommandBase.Rate.C:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/c.png");
                break;
            case MaiCommandBase.Rate.D:
                imagePath = Path.Combine(AppContext.BaseDirectory, "resource/ratings_hd/d.png");
                break;
        }

        var stars = 0;
        var starRate = (float)score.DxScore / score.MaxDxScore * 100;
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

        using var gradient = new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/gradient.png"));
        info.FuseAlpha(gradient);
        image.DrawImage(info, 0, 0);

        using var rateLayer = new Image(110, 100);
        using var rateImage = new Image(imagePath);

        var width = rateLayer.Width;
        rateLayer.DrawText("DX 分数", new Color(fontColor.R, fontColor.G, fontColor.B, 0.60f), 12, FontWeight.Heavy,
            HorizontalAlignment.Right, width - 10, 26);
        rateLayer.DrawText(score.DxScore + "/" + score.MaxDxScore, fontColor, 12,
            FontWeight.Regular, HorizontalAlignment.Right, width - 10, 39);
        var indicatorText = fcIndicatorText + " " + fsIndicatorText;
        if (indicatorText.Length != 0)
            indicatorText = indicatorText.TrimEnd();
        rateLayer.DrawText(indicatorText, fontColor, 10, FontWeight.Heavy,
            HorizontalAlignment.Right, width - 10, 51);
        rateImage.Scale(80, 80);
        rateLayer.DrawImage(rateImage, rateLayer.Width - rateImage.Width - 10,
            88 - rateImage.Height);

        using var starImage = isWhiteOnDark
            ? new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/star_white.png"))
            : new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/star.png"));

        for (var i = 0; i < stars; i++)
            rateLayer.DrawImage(starImage,
                rateLayer.Width - (int)(i * 11f) - 18,
                51
            );

        image.DrawImage(rateLayer, info.Width - rateLayer.Width,
            info.Height - rateLayer.Height
        );
        info.Dispose();

        return image;
    }

    private Image GenerateBackground(BestDto best)
    {
        var image = new Image(1750, 1440, Color.White);
        var background = new Image(1750, 1440);
        if (best.Charts.SdCharts.Length != 0)
        {
            background.Dispose();
            background = new Image(Path.Combine(AppContext.BaseDirectory, "resource/covers/" +
                                                                          best.Charts.SdCharts[0].Id + ".png"));
        }

        background.Scale(75, 75);
        background.GaussianBlur(10);
        background.Resize(1750, 1750);
        image.DrawImage(background, 0, 0);
        var foreground = new Image(Path.Combine(AppContext.BaseDirectory, "resource/best50/best50_background.png"));
        image.DrawImage(foreground, 0, 0);

        image.DrawText($"Generated by {BotConfiguration.Instance.BotName}", new Color(1f, 1f, 1f, 0.1f), 36,
            FontWeight.Regular, HorizontalAlignment.Right, 1722, 192);

        background.Dispose();
        foreground.Dispose();

        return image;
    }
}