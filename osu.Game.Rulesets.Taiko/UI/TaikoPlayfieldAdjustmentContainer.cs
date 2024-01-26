// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;

        private const float maximum_aspect = 16f / 9f;
        private const float minimum_aspect = 5f / 4f;

        public readonly IBindable<bool> LockPlayfieldAspectRange = new BindableBool(true);

        private static double aspectRatioToTimeRange(float aspectRatio)
        {
            const float default_aspect = 16f / 9f;

            // This value is taken by visual comparison with stable
            const float default_time_range = 7000 / TaikoBeatmapConverter.VELOCITY_MULTIPLIER;

            // This is the fraction of the width that should not contribute to time range.
            const float non_playable_portion = 380f / 1366f;

            return (aspectRatio - non_playable_portion) / (default_aspect - non_playable_portion) * default_time_range;
        }

        public double ComputeTimeRange()
        {
            float aspectRatio = Math.Clamp(Parent!.ChildSize.X / Parent!.ChildSize.Y, minimum_aspect, maximum_aspect);
            return aspectRatioToTimeRange(aspectRatio);
        }

        protected override void Update()
        {
            base.Update();

            float height = default_relative_height;

            // Players coming from stable expect to be able to change the aspect ratio regardless of the window size.
            // We originally wanted to limit this more, but there was considerable pushback from the community.
            //
            // As a middle-ground, the aspect ratio can still be adjusted in the downwards direction but has a maximum limit.
            // This is still a bit weird, because readability changes with window size, but it is what it is.
            if (LockPlayfieldAspectRange.Value)
            {
                float currentAspect = Parent!.ChildSize.X / Parent!.ChildSize.Y;

                if (currentAspect > maximum_aspect)
                    height *= currentAspect / maximum_aspect;
                else if (currentAspect < minimum_aspect)
                    height *= currentAspect / minimum_aspect;
            }

            // Limit the maximum relative height of the playfield to one-third of available area to avoid it masking out on extreme resolutions.
            height = Math.Min(height, 1f / 3f);
            Height = height;

            // Position the taiko playfield exactly one playfield from the top of the screen, if there is enough space for it.
            // Note that the relative height cannot exceed one-third - if that limit is hit, the playfield will be exactly centered.
            RelativePositionAxes = Axes.Y;
            Y = height;
        }
    }
}
