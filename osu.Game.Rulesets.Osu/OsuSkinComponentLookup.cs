// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu
{
    public class OsuSkinComponentLookup : GameplaySkinComponentLookup<OsuSkinComponents>
    {
        public OsuSkinComponentLookup(OsuSkinComponents component)
            : base(component)
        {
        }

        public override RulesetInfo Ruleset => new OsuRuleset().RulesetInfo;
    }
}
