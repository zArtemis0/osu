// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModPitchShift : Mod, IUpdatableByPlayfield, IApplicableToTrack
    {
        public override string Name => "Pitch Shift";
        public override string Acronym => "PS";
        public override IconUsage? Icon => FontAwesome.Solid.WaveSquare;
        public override ModType Type => ModType.Conversion;
        public override string Description => "Raise or lower the track's pitch.";
        public override double ScoreMultiplier => 1;

        [SettingSource("Pitch shift", "Raise or lower the track's pitch")]
        public BindableNumber<double> PitchChange { get; } = new BindableDouble()
        {
            MinValue = 0.5,
            MaxValue = 2.0,
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        [SettingSource("Match tempo", "Match pitch with the tempo")]
        public BindableBool MatchTempo { get; } = new BindableBool();

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        private ITrack track;

        public ModPitchShift()
        {
            PitchChange.BindValueChanged(val =>
                setPitch(val.NewValue), true);

            MatchTempo.BindValueChanged(matchTempoChanged);
        }

        public void ApplyToTrack(ITrack track)
        {
            this.track = track;

            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
        }

        public void Update(Playfield playfield)
        {
            if (MatchTempo.Value)
                setPitch(getMatchTempoPitchAdjustment());
        }

        private void setPitch(double pitch)
        {
            tempoAdjust.Value = 1.0 / pitch;
            freqAdjust.Value = pitch;
        }

        private void matchTempoChanged(ValueChangedEvent<bool> val)
        {
            if (val.NewValue)
                PitchChange.Value = getMatchTempoPitchAdjustment();

            PitchChange.Disabled = val.NewValue;
        }

        private double getMatchTempoPitchAdjustment()
            => track != null ? track.AggregateTempo.Value / tempoAdjust.Value : 1.0;
    }
}
