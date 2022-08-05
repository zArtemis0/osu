// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using Squirrel;
using Squirrel.SimpleSplat;

namespace osu.Desktop.Updater
{
    [SupportedOSPlatform("windows")]
    public class SquirrelUpdateManager : AbstractUpdateManager
    {
        private UpdateManager updateManager;
        private INotificationOverlay notificationOverlay;

        public override Task PrepareUpdateAsync() => UpdateManager.RestartAppWhenExited();

        private static readonly Logger logger = Logger.GetLogger("updater");

        /// <summary>
        /// Whether an update has been downloaded but not yet applied.
        /// </summary>
        private bool updatePending;

        private readonly SquirrelLogger squirrelLogger = new SquirrelLogger();

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications)
        {
            notificationOverlay = notifications;

            SquirrelLocator.CurrentMutable.Register(() => squirrelLogger, typeof(ILogger));
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(bool useDeltaPatching = true, UpdateProgressNotification notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            const string github_token = null; // TODO: populate.

            try
            {
                updateManager ??= new GithubUpdateManager(@"https://github.com/ppy/osu", false, github_token, @"osulazer");

                var info = await updateManager.CheckForUpdate(!useDeltaPatching).ConfigureAwait(false);

                if (info.ReleasesToApply.Count == 0)
                {
                    if (updatePending)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    // no updates available. bail and retry later.
                    return false;
                }

                scheduleRecheck = false;

                if (notification == null)
                {
                    notification = new UpdateProgressNotification(this) { State = ProgressNotificationState.Active };
                    Schedule(() => notificationOverlay.Post(notification));
                }

                notification.Progress = 0;
                notification.Text = @"Downloading update...";

                try
                {
                    await updateManager.DownloadReleases(info.ReleasesToApply, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.Progress = 0;
                    notification.Text = @"Installing update...";

                    await updateManager.ApplyReleases(info, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.State = ProgressNotificationState.Completed;
                    updatePending = true;
                }
                catch (Exception e)
                {
                    if (useDeltaPatching)
                    {
                        logger.Add(@"delta patching failed; will attempt full download!");

                        // could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                        // try again without deltas.
                        await checkForUpdateAsync(false, notification).ConfigureAwait(false);
                    }
                    else
                    {
                        // In the case of an error, a separate notification will be displayed.
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Close();

                        Logger.Error(e, @"update failed!");
                    }
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
            }
            finally
            {
                if (scheduleRecheck)
                {
                    // check again in 30 minutes.
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private class SquirrelLogger : ILogger, IDisposable
        {
            public Squirrel.SimpleSplat.LogLevel Level { get; set; } = Squirrel.SimpleSplat.LogLevel.Info;

            public void Write(string message, Squirrel.SimpleSplat.LogLevel logLevel)
            {
                if (logLevel < Level)
                    return;

                logger.Add(message);
            }

            public void Dispose()
            {
            }
        }
    }
}
