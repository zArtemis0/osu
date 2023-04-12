// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Taiko.Audio
{
    public class TaikoHitSampleInfo : HitSampleInfo
    {
        public const string TAIKO_STRONG_HIT = @"taiko-strong-hit";
        public const string TAIKO_STRONG_CLAP = @"taiko-strong-clap";
        public const string TAIKO_STRONG_FLOURISH = @"taiko-strong-flourish";

        public const int SAMPLE_VOLUME_THRESHOLD_HARD = 90;
        public const int SAMPLE_VOLUME_THRESHOLD_MEDIUM = 60;

        public TaikoHitSampleInfo(string name, string? bank = null, string? suffix = null, int volume = 0)
            : base(name, bank, suffix, volume)
        {
        }

        public override IEnumerable<string> LookupNames
        {
            get
            {
                List<string> lookupNames = new List<string>();

                // custom sample sets should still take priority
                if (!string.IsNullOrEmpty(Suffix))
                    lookupNames.Add($"Gameplay/{Bank}-{Name}{Suffix}");

                switch (Name)
                {
                    // flourish shouldn't fallback to anything, it should just not play if missing
                    case TAIKO_STRONG_FLOURISH:
                        lookupNames.Add(@$"Gameplay/{Name}");

                        return lookupNames;

                    case TAIKO_STRONG_HIT:
                    case TAIKO_STRONG_CLAP:
                        lookupNames.Add(@$"Gameplay/{Name}");

                        // fallback to a standard hit/clip (for non-Argon skins)
                        string? fallback = Name switch
                        {
                            TAIKO_STRONG_HIT => HIT_NORMAL,
                            TAIKO_STRONG_CLAP => HIT_CLAP,
                            _ => null
                        };

                        if (fallback != null)
                        {
                            lookupNames.Add($"Gameplay/taiko-{fallback}");
                            lookupNames.Add($"Gameplay/{Bank}-{fallback}");
                        }

                        break;

                    case HIT_NORMAL:
                    case HIT_CLAP:
                        string velocitySuffix = Volume switch
                        {
                            >= SAMPLE_VOLUME_THRESHOLD_HARD => "hard",
                            >= SAMPLE_VOLUME_THRESHOLD_MEDIUM => "medium",
                            _ => "soft"
                        };

                        lookupNames.Add($"Gameplay/taiko-{Name}-{velocitySuffix}");
                        lookupNames.Add($"Gameplay/taiko-{Name}");

                        break;
                }

                lookupNames.Add($"Gameplay/{Bank}-{Name}");
                return lookupNames;
            }
        }

        /// <inheritdoc />
        public override TaikoHitSampleInfo With(Optional<string> newName = default, Optional<string?> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
            => new TaikoHitSampleInfo(newName.GetOr(Name), newBank.GetOr(Bank), newSuffix.GetOr(Suffix), newVolume.GetOr(Volume));

        public override int GetHashCode() => HashCode.Combine(Name, Bank, Suffix, LookupNames);
    }
}
