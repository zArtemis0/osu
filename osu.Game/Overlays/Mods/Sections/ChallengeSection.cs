// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Sections
{
    public class ChallengeSection : ModSection
    {
        protected override Key[] ToggleKeys => null;
        public override ModType ModType => ModType.Challenge;

        public ChallengeSection()
        {
            Header = "Challenge";
        }
    }
}
