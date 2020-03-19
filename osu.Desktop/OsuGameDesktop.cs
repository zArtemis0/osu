// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Desktop.Overlays;
using osu.Framework.Platform;
using osu.Game;
using osuTK.Input;
using Microsoft.Win32;
using osu.Desktop.Updater;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Updater;

namespace osu.Desktop
{
    internal class OsuGameDesktop : OsuGame
    {
        private readonly bool noVersionOverlay;
        private VersionManager versionManager;

        public OsuGameDesktop(string[] args = null)
            : base(args)
        {
            noVersionOverlay = args?.Any(a => a == "--no-version-overlay") ?? false;
        }

        public override Storage GetStorageForStableInstall()
        {
            try
            {
                if (Host is DesktopGameHost desktopHost)
                    return new StableStorage(desktopHost);
            }
            catch (Exception)
            {
                Logger.Log("Could not find a stable install", LoggingTarget.Runtime, LogLevel.Important);
            }

            return null;
        }

        protected override UpdateManager CreateUpdateManager()
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    return new SquirrelUpdateManager();

                default:
                    return new SimpleUpdateManager();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!noVersionOverlay)
                LoadComponentAsync(versionManager = new VersionManager { Depth = int.MinValue }, Add);

            LoadComponentAsync(new DiscordRichPresence(), Add);
        }

        protected override void ScreenChanged(IScreen lastScreen, IScreen newScreen)
        {
            base.ScreenChanged(lastScreen, newScreen);

            switch (newScreen)
            {
                case IntroScreen _:
                case MainMenu _:
                    versionManager?.Show();
                    break;

                default:
                    versionManager?.Hide();
                    break;
            }
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            if (host.Window is DesktopGameWindow desktopWindow)
            {
                desktopWindow.CursorState |= CursorState.Hidden;

                desktopWindow.SetIconFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "lazer.ico"));
                desktopWindow.Title = Name;

                desktopWindow.FileDrop += fileDrop;
            }
        }

        private void fileDrop(object sender, FileDropEventArgs e)
        {
            var filePaths = e.FileNames;

            var firstExtension = Path.GetExtension(filePaths.First());

            if (filePaths.Any(f => Path.GetExtension(f) != firstExtension)) return;

            Task.Factory.StartNew(() => Import(filePaths), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// A method of accessing an osu-stable install in a controlled fashion.
        /// </summary>
        private class StableStorage : WindowsStorage
        {
            private const string default_songs_path = "Songs";

            private readonly string songsPath;
            private readonly DesktopGameHost host;

            public StableStorage(DesktopGameHost host)
                : base(string.Empty, host)
            {
                this.host = host;
                songsPath = locateSongsDirectory();
            }

            protected override string LocateBasePath()
            {
                static bool checkExists(string p) => File.Exists(Path.Combine(p, "osu!.exe"));

                string stableInstallPath;

                try
                {
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu"))
                        stableInstallPath = key?.OpenSubKey(@"shell\open\command")?.GetValue(string.Empty).ToString().Split('"')[1].Replace("osu!.exe", "");

                    if (checkExists(stableInstallPath))
                        return stableInstallPath;
                }
                catch
                {
                }

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"osu!");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                stableInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osu");
                if (checkExists(stableInstallPath))
                    return stableInstallPath;

                return null;
            }

            /// <summary>
            /// osu!stable <c>Songs</c> folder can be moved to another location by changing the <c>BeatmapDirectory</c> setting in the stable configuration file.
            /// This locates the <c>Songs</c> folder location for this stable installation and sets flags if the song folder location has been customized.
            /// </summary>
            private string locateSongsDirectory()
            {
                //we get the user config file.
                var configFile = GetStream(GetFiles(".", "osu!.*.cfg").First());
                var textReader = new StreamReader(configFile);

                while (!textReader.EndOfStream)
                {
                    var line = textReader.ReadLine();

                    if (line?.StartsWith("BeatmapDirectory") == true)
                        return line.Split('=')[1].TrimStart();
                }

                return default_songs_path;
            }

            public override Storage GetStorageForDirectory(string directory)
            {
                if (directory.Equals(default_songs_path, StringComparison.Ordinal) && !songsPath.Equals(default_songs_path, StringComparison.Ordinal))
                    return new StableSongsStorage(Path.GetFullPath(songsPath), host);

                return base.GetStorageForDirectory(directory);
            }

            private class StableSongsStorage : WindowsStorage
            {
                public StableSongsStorage(string basePath, DesktopGameHost host)
                    : base(string.Empty, host)
                {
                    BasePath = basePath;
                }
            }
        }
    }
}
