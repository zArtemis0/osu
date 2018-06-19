﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenBeatmap : BackgroundScreen
    {
        private Background background;
        private readonly Box dimColourBox;

        private WorkingBeatmap beatmap;
        private Vector2 blurTarget;

        public WorkingBeatmap Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value && beatmap != null)
                    return;

                beatmap = value;

                Schedule(() =>
                {
                    LoadComponentAsync(new BeatmapBackground(beatmap), b =>
                    {
                        float newDepth = 0;
                        if (background != null)
                        {
                            newDepth = background.Depth + 1;
                            background.FinishTransforms();
                            background.FadeOut(250);
                            background.Expire();
                        }

                        b.Depth = newDepth;
                        Add(background = b);
                        background.BlurSigma = blurTarget;
                    });
                });
            }
        }

        public Color4 DimColour
        {
            set {
                if (dimColourBox != null) dimColourBox.Colour = value;
            }
        }

        public BackgroundScreenBeatmap(WorkingBeatmap beatmap = null)
        {
            Beatmap = beatmap;
            Add(dimColourBox = new Box
            {
                Depth = float.MaxValue,
                FillMode = FillMode.Fill,
                RelativeSizeAxes = Axes.Both
            });
        }

        public TransformSequence<Background> BlurTo(Vector2 sigma, double duration, Easing easing = Easing.None)
            => background?.BlurTo(blurTarget = sigma, duration, easing);

        public TransformSequence<Background> FadeTo(float newAlpha, double duration = 0, Easing easing = Easing.None)
            => background?.FadeTo(newAlpha, duration, easing);

        public override bool Equals(BackgroundScreen other)
        {
            var otherBeatmapBackground = other as BackgroundScreenBeatmap;
            if (otherBeatmapBackground == null) return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.Beatmap;
        }

        private class BeatmapBackground : Background
        {
            private readonly WorkingBeatmap beatmap;

            public BeatmapBackground(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Sprite.Texture = beatmap?.Background ?? textures.Get(@"Backgrounds/bg1");
            }
        }
    }
}
