﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyCalculator : DifficultyCalculator
    {
        private const int section_length = 400;
        private const double difficulty_multiplier = 0.0675;

        public OsuDifficultyCalculator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public OsuDifficultyCalculator(IBeatmap beatmap, Mod[] mods)
            : base(beatmap, mods)
        {
        }

        public override double Calculate(Dictionary<string, double> categoryDifficulty = null)
        {
            OsuDifficultyBeatmap beatmap = new OsuDifficultyBeatmap((List<OsuHitObject>)Beatmap.HitObjects, TimeRate);
            Skill[] skills =
            {
                new Aim(),
                new Speed()
            };

            double sectionLength = section_length * TimeRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = 2 * sectionLength;

            foreach (OsuDifficultyHitObject h in beatmap)
            {
                while (h.BaseObject.StartTime > currentSectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(currentSectionEnd);
                    }

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;

            double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

            if (categoryDifficulty != null)
            {
                categoryDifficulty.Add("Aim", aimRating);
                categoryDifficulty.Add("Speed", speedRating);
            }

            return starRating;
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };
    }
}
