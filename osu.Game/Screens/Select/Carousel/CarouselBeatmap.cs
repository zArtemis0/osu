// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public readonly BeatmapInfo Beatmap;

        public CarouselBeatmap(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            State.Value = CarouselItemState.Collapsed;
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            bool match =
                criteria.Ruleset == null ||
                Beatmap.RulesetID == criteria.Ruleset.ID ||
                (Beatmap.RulesetID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            match &= criteria.StarDifficulty.IsTrue(Beatmap.StarDifficulty);
            match &= criteria.ApproachRate.IsTrue(Beatmap.BaseDifficulty.ApproachRate);
            match &= criteria.Length.IsTrue(Beatmap.OnlineInfo.Length);

            match &= criteria.BeatDivisor == Beatmap.BeatDivisor;

            foreach (var criteriaTerm in criteria.SearchTerms)
                match &=
                    Beatmap.Metadata.SearchableTerms.Any(term => term.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    Beatmap.Version.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0;

            Filtered.Value = !match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    var ruleset = Beatmap.RulesetID.CompareTo(otherBeatmap.Beatmap.RulesetID);
                    if (ruleset != 0) return ruleset;

                    return Beatmap.StarDifficulty.CompareTo(otherBeatmap.Beatmap.StarDifficulty);
            }
        }

        public override string ToString() => Beatmap.ToString();
    }
}
