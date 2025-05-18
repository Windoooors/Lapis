# Lapis Bot
中文 [English](README.en.md)

## 描述
用 C# 实现的 QQ 机器人，专注于舞萌 DX 相关的功能，支持 go-cqhttp

## 框架与语言
- .NET 9.0
- C# 13.0

## 功能
* B50 查询
* 谱面信息和玩家分数查询
* 牌子查询
* 随机歌曲
* 添加和查询歌曲别名
* 猜歌名和歌名开字母
* 歌名模糊匹配
  
和更多非舞萌相关功能
  
请注意，由于安全因素，WahlapConnectiveKits 没有也不会被开源
  
如果你想从工程构建 Lapis Bot，请从 MaiCommand 的构造函数中取消 UpdateCommand 和 BindCommand 的实例化
  
就像这样：
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
  
## 部署
* 安装 FFmpeg（如果你使用 Debian 或基于它的发行版）  
  `sudo apt install ffmpeg`
  
* 根据[微软](https://learn.microsoft.com/zh-cn/dotnet/core/install/)的文档安装 .NET 运行时 9.0.x
  
* 部署 NapCat、LLOneBot 或其他兼容 go-cqhttp 的 QQ 机器人框架并用机器人的 QQ 账户登录, 然后创建一个监听在任意有效地址的 WebSocket 服务器.  
  如果你使用 NapCat，可以跟着[这里](https://napneko.github.io/guide/napcat)完成框架部署
  
* 编辑 `config.json` 以使 Lapis Bot 连接到机器人框架  
  应该将上文 WebSocket 服务器监听的地址填入 Address 一栏  
  eg. 
```{
    "Address": "localhost:3000",
    "AdministratorQqNumber": 2794813909,
    "BotQqNumber": 3064967438,
    "AliasUrl": "https://api.yuzuchan.moe/maimaidx/maimaidxalias",
    "DivingFishUrl": "https://www.diving-fish.com",
    "WahlapConnectiveKitsUrl": ""
}
```

## 引用的库
* NLog (https://github.com/NLog/NLog)
* SixLabours.ImageSharp (https://github.com/SixLabors/ImageSharp)
* SixLabours.ImageSharp.Drawing (https://github.com/SixLabors/ImageSharp.Drawing)
* EleCho.GoCqHttpSdk (https://github.com/OrgEleCho/EleCho.GoCqHttpSdk)
* Newtonsoft.Json (https://github.com/JamesNK/Newtonsoft.Json)
* NLog.Extensions.Logging (https://github.com/NLog/NLog.Extensions.Logging)
* Raffinert.FuzzySharp (https://github.com/Raffinert/FuzzySharp)
* Xabe.FFmpeg (https://github.com/tomaszzmuda/Xabe.FFmpeg)

## 贡献者
- Setchin ([@Windoooors](https://github.com/Windoooors))
- 2750558108 ([@L2750558108](https://github.com/L2750558108))

## 贡献
欢迎各位参与本项目的开发，大家如果觉得该应用有不足之处，可以提出 Issue 告诉我们，有能力的当然希望能够通过 Pull Request 给我们更有价值的代码建议。
