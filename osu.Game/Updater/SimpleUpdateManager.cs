﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.
    /// Installation is left up to the user.
    /// </summary>
    public class SimpleUpdateManager : UpdateManager
    {
        private string version;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            version = game.Version;

            if (game.IsDeployedBuild)
                Schedule(() => Task.Run(checkForUpdateAsync));
        }

        private async void checkForUpdateAsync()
        {
            try
            {
                var releases = new OsuJsonWebRequest<GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await releases.PerformAsync();

                var latest = releases.ResponseObject;

                if (latest.TagName != version)
                {
                    Notifications.Post(new SimpleNotification
                    {
                        Text = "osu!lazer已有新版本可用!\n"
                                + $"你的版本{version}\n"
                                + $"最新版本{latest.TagName}.\n\n"
                                + "点击这里前往github下载",
                        Icon = FontAwesome.Solid.Upload,
                        Activated = () =>
                        {
                            host.OpenUrlExternally(getBestUrl(latest));
                            return true;
                        }
                    });
                }
            }
            catch
            {
                // we shouldn't crash on a web failure. or any failure for the matter.
            }
        }

        private string getBestUrl(GitHubRelease release)
        {
            GitHubAsset bestAsset = null;

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".exe"));
                    break;

                case RuntimeInfo.Platform.MacOsx:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".app.zip"));
                    break;

                case RuntimeInfo.Platform.Linux:
                    bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".tar.xz"));
                    break;

                case RuntimeInfo.Platform.Android:
                    // on our testing device this causes the download to magically disappear.
                    //bestAsset = release.Assets?.Find(f => f.Name.EndsWith(".apk"));
                    break;
            }

            return bestAsset?.BrowserDownloadUrl ?? release.HtmlUrl;
        }

        public class GitHubRelease
        {
            [JsonProperty("html_url")]
            public string HtmlUrl { get; set; }

            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("assets")]
            public List<GitHubAsset> Assets { get; set; }
        }

        public class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
