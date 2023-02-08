# 如何构建本项目:

```bash
#!/bin/bash
mkdir mfosu
cd mfosu

git clone https://github.com/MATRIX-feather/osu.git
git clone https://github.com/MATRIX-feather/osu-framework.git

cd osu-framework

# !!! 重要 !!!
# 这里通过git checkout来确保Framework在正确的分支上
# mfosu默认克隆下来是stable分支，因此Framework也使用对应的stable
# 如果你打算用daily分支，请记得把Framework也切换到daily上
git checkout stable
cd ../osu

dotnet run --project osu.Desktop -c Release
```

# 歌词插件的“用户定义”使用方法
**目前尚未完全实现在线功能，所以您需要先创建一个本地文件来体验。**

以你在设置中点“打开osu!文件夹”的地方为出发点，在`custom/lyrics`文件夹中创建`definition.json`文件。

里面的内容格式应该像这样：
```
{
  "LastUpdate": -1,
  "Data":
  [
    {
        "Target": <某首歌的网易云ID>,
        "Beatmaps":[<图1的在线ID>, <图2的在线ID>]
    },
    {
        "Target": <另外一首歌的网易云ID>,
        "Beatmaps":[<图3的在线ID>, <图4的在线ID>]
    }
  ]
}
```

**某张图的在线ID可以打开歌词界面查看顶上面的“ID:xxxxx”来获取**

<p align="center">
  <img width="500" alt="osu! logo" src="assets/lazer.png">
</p>

# osu!

[![Build status](https://github.com/ppy/osu/actions/workflows/ci.yml/badge.svg?branch=master&event=push)](https://github.com/ppy/osu/actions/workflows/ci.yml)
[![GitHub release](https://img.shields.io/github/release/ppy/osu.svg)](https://github.com/ppy/osu/releases/latest)
[![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu)
[![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)
[![Crowdin](https://d322cqt584bo4o.cloudfront.net/osu-web/localized.svg)](https://crowdin.com/project/osu-web)


[![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu/badge)](https://www.codefactor.io/repository/github/ppy/osu)CodeFactor(ppy/osu)

[![CodeFactor](https://www.codefactor.io/repository/github/matrix-feather/osu/badge/daily)](https://www.codefactor.io/repository/github/matrix-feather/osu/overview/daily)CodeFactor(matrix-feather/osu)

A free-to-win rhythm game. Rhythm is just a *click* away!

The future of [osu!](https://osu.ppy.sh) and the beginning of an open era! Currently known by and released under the release codename "*lazer*". As in sharper than cutting-edge.

## Status

This project is under heavy development, but is in a stable state. Users are encouraged to try it out and keep it installed alongside the stable *osu!* client. It will continue to evolve to the point of eventually replacing the existing stable client as an update.

**IMPORTANT:** Gameplay mechanics (and other features which you may have come to know and love) are in a constant state of flux. Game balance and final quality-of-life passes come at the end of development, preceded by experimentation and changes which may potentially **reduce playability or usability**. This is done in order to allow us to move forward as developers and designers more efficiently. If this offends you, please consider sticking to the stable releases of osu! (found on the website). We are not yet open to heated discussion over game mechanics and will not be using github as a forum for such discussions just yet.

We are accepting bug reports (please report with as much detail as possible and follow the existing issue templates). Feature requests are also welcome, but understand that our focus is on completing the game to feature parity before adding new features. A few resources are available as starting points to getting involved and understanding the project:

- Detailed release changelogs are available on the [official osu! site](https://osu.ppy.sh/home/changelog/lazer).
- You can learn more about our approach to [project management](https://github.com/ppy/osu/wiki/Project-management).
- Read peppy's [blog post](https://blog.ppy.sh/a-definitive-lazer-faq/) exploring where the project is currently and the roadmap going forward.

## Running osu!

If you are looking to install or test osu! without setting up a development environment, you can consume our [binary releases](https://github.com/ppy/osu/releases). Handy links below will download the latest version for your operating system of choice:

**Latest build:**

| [Windows 8.1+ (x64)](https://github.com/ppy/osu/releases/latest/download/install.exe) | macOS 10.15+ ([Intel](https://github.com/ppy/osu/releases/latest/download/osu.app.Intel.zip), [Apple Silicon](https://github.com/ppy/osu/releases/latest/download/osu.app.Apple.Silicon.zip)) | [Linux (x64)](https://github.com/ppy/osu/releases/latest/download/osu.AppImage) | [iOS 13.4+](https://osu.ppy.sh/home/testflight) | [Android 5+](https://github.com/ppy/osu/releases/latest/download/sh.ppy.osulazer.apk) |
| ------------- | ------------- | ------------- | ------------- | ------------- |

- The iOS testflight link may fill up (Apple has a hard limit of 10,000 users). We reset it occasionally when this happens. Please do not ask about this. Check back regularly for link resets or follow [peppy](https://twitter.com/ppy) on twitter for announcements of link resets.

If your platform is not listed above, there is still a chance you can manually build it by following the instructions below.

## Developing a custom ruleset

osu! is designed to have extensible modular gameplay modes, called "rulesets". Building one of these allows a developer to harness the power of osu! for their own game style. To get started working on a ruleset, we have some templates available [here](https://github.com/ppy/osu/tree/master/Templates).

You can see some examples of custom rulesets by visiting the [custom ruleset directory](https://github.com/ppy/osu/discussions/13096).

## Developing osu!

Please make sure you have the following prerequisites:

- A desktop platform with the [.NET 6.0 SDK](https://dotnet.microsoft.com/download) installed.
- When developing with mobile, [Xamarin](https://docs.microsoft.com/en-us/xamarin/) is required, which is shipped together with Visual Studio or [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/).
- When working with the codebase, we recommend using an IDE with intelligent code completion and syntax highlighting, such as the latest version of [Visual Studio](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio Code](https://code.visualstudio.com/).
- When running on Linux, please have a system-wide FFmpeg installation available to support video decoding.

### Downloading the source code

Clone the repository:

```shell
git clone https://github.com/ppy/osu
cd osu
```

To update the source code to the latest commit, run the following command inside the `osu` directory:

```shell
git pull
```

### Building

Build configurations for the recommended IDEs (listed above) are included. You should use the provided Build/Run functionality of your IDE to get things going. When testing or building new components, it's highly encouraged you use the `VisualTests` project/configuration. More information on this is provided [below](#contributing).

- Visual Studio / Rider users should load the project via one of the platform-specific `.slnf` files, rather than the main `.sln`. This will allow access to template run configurations.

You can also build and run *osu!* from the command-line with a single command:

```shell
dotnet run --project osu.Desktop
```

If you are not interested in debugging *osu!*, you can add `-c Release` to gain performance. In this case, you must replace `Debug` with `Release` in any commands mentioned in this document.

If the build fails, try to restore NuGet packages with `dotnet restore`.

_Due to a historical feature gap between .NET Core and Xamarin, running `dotnet` CLI from the root directory will not work for most commands. This can be resolved by specifying a target `.csproj` or the helper project at `build/Desktop.proj`. Configurations have been provided to work around this issue for all supported IDEs mentioned above._

### Testing with resource/framework modifications

Sometimes it may be necessary to cross-test changes in [osu-resources](https://github.com/ppy/osu-resources) or [osu-framework](https://github.com/ppy/osu-framework). This can be achieved by running some commands as documented on the [osu-resources](https://github.com/ppy/osu-resources/wiki/Testing-local-resources-checkout-with-other-projects) and [osu-framework](https://github.com/ppy/osu-framework/wiki/Testing-local-framework-checkout-with-other-projects) wiki pages.

### Code analysis

Before committing your code, please run a code formatter. This can be achieved by running `dotnet format` in the command line, or using the `Format code` command in your IDE.

We have adopted some cross-platform, compiler integrated analyzers. They can provide warnings when you are editing, building inside IDE or from command line, as-if they are provided by the compiler itself.

JetBrains ReSharper InspectCode is also used for wider rule sets. You can run it from PowerShell with `.\InspectCode.ps1`. Alternatively, you can install ReSharper or use Rider to get inline support in your IDE of choice.

## Contributing

When it comes to contributing to the project, the two main things you can do to help out are reporting issues and submitting pull requests. Please refer to the [contributing guidelines](CONTRIBUTING.md) to understand how to help in the most effective way possible.

If you wish to help with localisation efforts, head over to [crowdin](https://crowdin.com/project/osu-web).

For those interested, we love to reward quality contributions via [bounties](https://docs.google.com/spreadsheets/d/1jNXfj_S3Pb5PErA-czDdC9DUu4IgUbe1Lt8E7CYUJuE/view?&rm=minimal#gid=523803337), paid out via PayPal or osu!supporter tags. Don't hesitate to [request a bounty](https://docs.google.com/forms/d/e/1FAIpQLSet_8iFAgPMG526pBZ2Kic6HSh7XPM3fE8xPcnWNkMzINDdYg/viewform) for your work on this project.

## Licence

*osu!*'s code and framework are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.
