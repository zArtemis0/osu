﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays.Settings.Sections.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        private Background background;

        private int currentDisplay;
        private const int background_count = 7;

        private string backgroundName => $@"Menu/menu-background-{currentDisplay % background_count + 1}";

        private Bindable<Skin> skin;
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private readonly Bindable<MainMenuBackgroundMode> backgroundMode = new Bindable<MainMenuBackgroundMode>();

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager, OsuConfigManager config, Bindable<WorkingBeatmap> workingBeatmap)
        {
            skin = skinManager.CurrentSkin.GetBoundCopy();
            beatmap.BindTo(workingBeatmap);
            config.BindWith(OsuSetting.MenuBackgroundMode, backgroundMode);

            skin.ValueChanged += _ => Next();
            backgroundMode.ValueChanged += _ => Next();
            beatmap.ValueChanged += _ => Next();

            currentDisplay = RNG.Next(0, background_count);

            display(createBackground());
        }

        private void display(Background newBackground)
        {
            background?.FadeOut(800, Easing.InOutSine);
            background?.Expire();

            AddInternal(background = newBackground);
            currentDisplay++;
        }

        private ScheduledDelegate nextTask;

        public void Next()
        {
            nextTask?.Cancel();
            nextTask = Scheduler.AddDelayed(() => { LoadComponentAsync(createBackground(), display); }, 100);
        }

        private Background createBackground()
        {
            Background newBackground;

            switch (backgroundMode.Value)
            {
                default:
                case MainMenuBackgroundMode.Default:
                    newBackground = new Background(backgroundName);
                    break;

                case MainMenuBackgroundMode.Skin:
                    newBackground = new SkinnedBackground(skin.Value, backgroundName);
                    break;

                case MainMenuBackgroundMode.Beatmap:
                    newBackground = new BeatmapBackground(beatmap.Value, backgroundName);
                    break;
            }

            newBackground.Depth = currentDisplay;

            return newBackground;
        }

        private class SkinnedBackground : Background
        {
            private readonly Skin skin;

            public SkinnedBackground(Skin skin, string fallbackTextureName)
                : base(fallbackTextureName)
            {
                this.skin = skin;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Sprite.Texture = skin.GetTexture("menu-background") ?? Sprite.Texture;
            }
        }

        private class BeatmapBackground : Background
        {
            private readonly WorkingBeatmap beatmap;

            public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName)
                : base(fallbackTextureName)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Sprite.Texture = beatmap?.Background ?? Sprite.Texture;
            }
        }
    }
}
