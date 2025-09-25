# Lapis
[中文](README.md) English

## Description
A QQ bot program written in C#, specializing in maimai DX related functions and compatible with go-cqhttp frameworks.

## Framework & Language
- .NET 9.0
- C# 13.0

## Functions
Lapis is now capable for
* B50 Acquisition
* Chart Information and Score Acquisition
* Plate Acquisition
* Random Song Recommendation Generation
* Song Nicknames Acquisition and Management
* Song Puzzles Based on the Music or Its Title
* Fuzzy Song Match
  
and other non-maimai-related tasks.
  
Notice that Wahlap Connective Kits are not and will not be open-sourced due to safety reasons, so that you may not be able to use score uploading functions.
  
If you want to build from project, please remove instantiations for both UpdateCommand and BindCommand in the constructor of MaiCommand.
  
It will look like this afterwards:
```
public MaiCommand()
{
    MaiCommandInstance = this;
    CommandHead = new Regex("^mai");
    SubCommands =
    [
        new RandomCommand(),
        new InfoCommand(),
        new AliasCommand(),
        new BestCommand(),
        new LettersCommand(),
        new GuessCommand(),
        new PlateCommand(),
        new SearchCommand()
    ];
}
```
  
## Depolyment
* Install FFmpeg by running (if you are on Debian or Debian based distros)  
  `sudo apt install ffmpeg` *Debian*  
  `winget install ffmpeg` *Windows Server*  
  `brew install ffmpeg` *macOS with Homebrew*  
  
* Install .NET Runtime 9.0.x by following instructions from [Microsoft](https://learn.microsoft.com/zh-cn/dotnet/core/install/)
  
* Set up a NapCat or LLOneBot or other go-cqhttp compatible framework instance (NapCat is recommended), log in with your bot QQ account, and create a WebSocket server listening on a preferred address.  
  You may get more instructions about this from [NapCat](https://napneko.github.io/guide/napcat) if you want it.
  
* Connect Lapis to the framework you just set up by editing `config.json`  
  `Address` should be the one that the WebSocket server is listening on. `AdministratorQqNumber` , `BotQqNumber` and `BotName` should be respectively your QQ number, your bot's QQ number and your bot's name.  
  eg. 
```
{
    "Address": "localhost:3000",
    "AdministratorQqNumber": 0,
    "BotQqNumber": 0,
    "BotName": "",
    "AliasUrl": "https://maimai.lxns.net/api/v0/maimai/alias/list",
    "DivingFishUrl": "https://www.diving-fish.com",
    "WahlapConnectiveKitsUrl": ""
}
```
* Before launching, please download resource files from [here](https://github.com/Windoooors/Lapis/releases/tag/resource-v1.0.0) and unarchive them, then put the `resource` folder into the directory that contains the executable file of Lapis.
  
* Launch Lapis   
  `./Lapis` *Linux and Darwin*  
  `.\Lapis.exe` *Windows with Powershell*  

## Libraries Used
* [NLog](https://github.com/NLog/NLog)
* [SixLabours.ImageSharp](https://github.com/SixLabors/ImageSharp)
* [SixLabours.ImageSharp.Drawing](https://github.com/SixLabors/ImageSharp.Drawing)
* [EleCho.GoCqHttpSdk](https://github.com/OrgEleCho/EleCho.GoCqHttpSdk)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [NLog.Extensions.Logging](https://github.com/NLog/NLog.Extensions.Logging)
* [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg)

## Contributors
- Setchin ([@Windoooors](https://github.com/Windoooors))
- 2750558108 ([@L2750558108](https://github.com/L2750558108))

## Contributing
Everyone is welcomed to contribute to this project. You can tell us some suggestions by **Issue**. Moreover, we will appreciate if you could contribute codes by **Pull Request**.
