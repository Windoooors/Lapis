using System;
using System.IO;

namespace Lapis;

public class ImageGenerator
{
    protected static string GetSongCoverPath(int id)
    {
        return Path.Combine(AppContext.BaseDirectory, "resource/covers/" +
                                                      (File.Exists(Path.Combine(AppContext.BaseDirectory,
                                                          "resource/covers/" + id + ".png"))
                                                          ? id
                                                          : 1000) + ".png");
    }

    protected static string GetHdSongCoverPath(int id)
    {
        return File.Exists(Path.Combine(AppContext.BaseDirectory, "resource/covers_hd/" + id + ".png"))
            ? Path.Combine(AppContext.BaseDirectory, "resource/covers_hd/" + id + ".png")
            : Path.Combine(AppContext.BaseDirectory, "resource/covers/1000.png");
    }
}