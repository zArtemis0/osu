// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.Play
{
    public class SoloPlayer : SubmittingPlayer
    {
        private LLinGameplayLeaderboard leaderboard;

        public SoloPlayer()
            : this(null)
        {
        }

        protected SoloPlayer(PlayerConfiguration configuration = null)
            : base(configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponentAsync(leaderboard = new LLinGameplayLeaderboard
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft
            }, HUDOverlay.Add);
        }

        protected override APIRequest<APIScoreToken> CreateTokenRequest()
        {
            int beatmapId = Beatmap.Value.BeatmapInfo.OnlineID;
            int rulesetId = Ruleset.Value.OnlineID;

            if (beatmapId <= 0)
                return null;

            if (!Ruleset.Value.IsLegacyRuleset())
                return null;

            return new CreateSoloScoreRequest(beatmapId, rulesetId, Game.VersionHash);
        }

        protected override bool HandleTokenRetrievalFailure(Exception exception) => false;

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            IBeatmapInfo beatmap = score.ScoreInfo.BeatmapInfo;

            Debug.Assert(beatmap.OnlineID > 0);

            return new SubmitSoloScoreRequest(score.ScoreInfo, token, beatmap.OnlineID);
        }
    }
}
