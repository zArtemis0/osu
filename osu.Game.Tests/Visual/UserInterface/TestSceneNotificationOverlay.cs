﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNotificationOverlay : OsuManualInputManagerTestScene
    {
        private NotificationOverlay notificationOverlay;

        private readonly List<ProgressNotification> progressingNotifications = new List<ProgressNotification>();

        private SpriteText displayedCount;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            progressingNotifications.Clear();

            Content.Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    // move the overlay a bit to the bottom to avoid
                    // conflicting with the input priority overlay
                    Padding = new MarginPadding { Top = 100f },
                    Child = notificationOverlay = new NotificationOverlay
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                },
                displayedCount = new OsuSpriteText()
            };

            notificationOverlay.UnreadCount.ValueChanged += count => { displayedCount.Text = $"displayed count: {count.NewValue}"; };
        });

        [Test]
        public void TestBasicFlow()
        {
            setState(Visibility.Visible);
            AddStep(@"simple #1", sendHelloNotification);
            AddStep(@"simple #2", sendAmazingNotification);
            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);

            setState(Visibility.Hidden);

            AddRepeatStep(@"add many simple", sendManyNotifications, 3);

            waitForCompletion();

            AddStep(@"progress #3", sendUploadProgress);

            checkProgressingCount(1);

            checkDisplayedCount(33);

            waitForCompletion();
        }

        [Test]
        public void TestImportantWhileClosed()
        {
            AddStep(@"simple #1", sendHelloNotification);

            AddAssert("Is visible", () => notificationOverlay.State.Value == Visibility.Visible);

            checkDisplayedCount(1);

            AddStep(@"progress #1", sendUploadProgress);
            AddStep(@"progress #2", sendDownloadProgress);

            checkProgressingCount(2);
            checkDisplayedCount(3);
        }

        [Test]
        public void TestUnimportantWhileClosed()
        {
            AddStep(@"background #1", sendBackgroundNotification);

            AddAssert("Is not visible", () => notificationOverlay.State.Value == Visibility.Hidden);

            checkDisplayedCount(1);

            AddStep(@"background progress #1", sendBackgroundUploadProgress);

            checkProgressingCount(1);

            waitForCompletion();

            checkDisplayedCount(2);

            AddStep(@"simple #1", sendHelloNotification);

            checkDisplayedCount(3);
        }

        [Test]
        public void TestSpam()
        {
            setState(Visibility.Visible);
            AddRepeatStep("send barrage", sendBarrage, 10);
        }

        [Test]
        public void TestCancellable()
        {
            ProgressNotification notification = null;

            setState(Visibility.Visible);
            AddStep("send slow-progressing", () => notification = sendSlowProgressing());

            AddStep("disable user cancelling", () => notification.Cancellable = false);
            AddStep("click cancel button", clickNotificationCloseButton);
            AddAssert("notification still active", () => notification.State == ProgressNotificationState.Active);

            AddStep("re-enable user cancelling", () => notification.Cancellable = true);
            AddStep("click cancel button", clickNotificationCloseButton);
            AddAssert("notification cancelled", () => notification.State == ProgressNotificationState.Cancelled);

            void clickNotificationCloseButton()
            {
                InputManager.MoveMouseTo(notification.ChildrenOfType<Notification.NotificationCloseButton>().Single());
                InputManager.Click(MouseButton.Left);
            }
        }

        [Test]
        public void TestSwitchToCancelledOnDisabledUserCancel()
        {
            ProgressNotification notification = null;

            setState(Visibility.Visible);
            AddStep("send slow-progressing", () => notification = sendSlowProgressing());
            AddStep("disable user cancelling", () => notification.Cancellable = false);

            AddStep("switch to cancelled state", () => notification.State = ProgressNotificationState.Cancelled);
            AddAssert("cancelling enabled automatically", () => notification.Cancellable);

            bool exceptionThrown = false;
            AddStep("attempt disabling user cancelling", () =>
            {
                try
                {
                    notification.Cancellable = false;
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }
            });
            AddAssert("disabling cancel throws", () => exceptionThrown);
        }

        protected override void Update()
        {
            base.Update();

            progressingNotifications.RemoveAll(n => n.State == ProgressNotificationState.Completed);

            if (progressingNotifications.Count(n => n.State == ProgressNotificationState.Active) < 3)
            {
                var p = progressingNotifications.Find(n => n.State == ProgressNotificationState.Queued);

                if (p != null)
                    p.State = ProgressNotificationState.Active;
            }

            foreach (var n in progressingNotifications.FindAll(n => n.State == ProgressNotificationState.Active))
            {
                if (n.Progress < 1)
                {
                    if (n is SlowProgressingNotification)
                        n.Progress += (float)(Time.Elapsed / 10000);
                    else
                        n.Progress += (float)(Time.Elapsed / 400) * RNG.NextSingle();
                }
                else
                {
                    n.State = ProgressNotificationState.Completed;
                }
            }
        }

        private void checkDisplayedCount(int expected) =>
            AddAssert($"Displayed count is {expected}", () => notificationOverlay.UnreadCount.Value == expected);

        private void setState(Visibility state) => AddStep(state.ToString(), () => notificationOverlay.State.Value = state);

        private void checkProgressingCount(int expected) => AddAssert($"progressing count is {expected}", () => progressingNotifications.Count == expected);

        private void waitForCompletion() => AddUntilStep("wait for notification progress completion", () => progressingNotifications.Count == 0);

        private void sendBarrage()
        {
            switch (RNG.Next(0, 4))
            {
                case 0:
                    sendHelloNotification();
                    break;

                case 1:
                    sendAmazingNotification();
                    break;

                case 2:
                    sendUploadProgress();
                    break;

                case 3:
                    sendDownloadProgress();
                    break;
            }
        }

        private void sendAmazingNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"You are amazing" });
        }

        private void sendHelloNotification()
        {
            notificationOverlay.Post(new SimpleNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendBackgroundNotification()
        {
            notificationOverlay.Post(new BackgroundNotification { Text = @"Welcome to osu!. Enjoy your stay!" });
        }

        private void sendManyNotifications()
        {
            for (int i = 0; i < 10; i++)
                notificationOverlay.Post(new SimpleNotification { Text = @"Spam incoming!!" });
        }

        private void sendDownloadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Downloading Haitai...",
                CompletionText = "Downloaded Haitai!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendUploadProgress()
        {
            var n = new ProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private void sendBackgroundUploadProgress()
        {
            var n = new BackgroundProgressNotification
            {
                Text = @"Uploading to BSS...",
                CompletionText = "Uploaded to BSS!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
        }

        private SlowProgressingNotification sendSlowProgressing()
        {
            var n = new SlowProgressingNotification
            {
                Text = "Downloading too slowly...",
                CompletionText = "Finally made it there!",
            };
            notificationOverlay.Post(n);
            progressingNotifications.Add(n);
            return n;
        }

        private class BackgroundNotification : SimpleNotification
        {
            public override bool IsImportant => false;
        }

        private class BackgroundProgressNotification : ProgressNotification
        {
            public override bool IsImportant => false;
        }

        private class SlowProgressingNotification : ProgressNotification
        {
        }
    }
}
