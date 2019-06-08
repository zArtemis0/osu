// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI;
using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Manage the animation to be applied when a player fails.
    /// Single file; automatically disposed after use.
    /// </summary>
    public class FailAnimation : Component
    {
        public Action OnComplete;

        private readonly DrawableRuleset drawableRuleset;

        private readonly BindableDouble trackFreq = new BindableDouble(1);

        private Track track;

        private const float duration = 2500;

        private SampleChannel failSample;

        public FailAnimation(DrawableRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IBindable<WorkingBeatmap> beatmap)
        {
            track = beatmap.Value.Track;
            failSample = audio.Samples.Get(@"Gameplay/failsound");
        }

        private bool started;

        /// <summary>
        /// Start the fail animation playing.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if started more than once.</exception>
        public void Start()
        {
            if (started) throw new InvalidOperationException("Animation cannot be started more than once.");

            started = true;

            failSample.Play();

            this.TransformBindableTo(trackFreq, 0, duration).OnComplete(_ =>
            {
                OnComplete?.Invoke();
                Expire();
            });

            track.AddAdjustment(AdjustableProperty.Frequency, trackFreq);

            drawableRuleset.Playfield.DisplayJudgements.Value = false;

            applyToPlayfield(drawableRuleset.Playfield);
            drawableRuleset.Playfield.HitObjectContainer.FlashColour(Color4.Red, 500);
            drawableRuleset.Playfield.HitObjectContainer.FadeOut(duration / 2);
        }

        protected override void Update()
        {
            base.Update();

            if (!started)
                return;

            applyToPlayfield(drawableRuleset.Playfield);
        }

        private void applyToPlayfield(Playfield playfield)
        {
            foreach (var nested in playfield.NestedPlayfields)
                applyToPlayfield(nested);

            playfield.Fail();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            track?.RemoveAdjustment(AdjustableProperty.Frequency, trackFreq);
        }
    }
}
