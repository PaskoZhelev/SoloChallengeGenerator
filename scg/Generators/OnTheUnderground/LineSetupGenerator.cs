﻿using System;
using System.Collections.Generic;
using System.Linq;
using scg.Framework;
using scg.Utils;

namespace scg.Generators.OnTheUnderground
{
    public class LineSetupGenerator : TemplateGenerator
    {
        private readonly BuildingData _buildingData;
        private readonly GenerationResult _generationResult;
        private readonly LondonMapGenerator _londonMapGenerator;
        private readonly BerlinMapGenerator _berlinMapGenerator;
        private readonly GameOptions _gameOptions;

        public LineSetupGenerator(BuildingData buildingData, GenerationResult generationResult,
            LondonMapGenerator londonMapGenerator, BerlinMapGenerator berlinMapGenerator,
            GameOptions gameOptions)
        {
            _buildingData = buildingData;
            _generationResult = generationResult;
            _londonMapGenerator = londonMapGenerator;
            _berlinMapGenerator = berlinMapGenerator;
            _gameOptions = gameOptions;
        }

        public override string Token { get; } = "<<LINE_SETUP>>";

        [STAThread]
        public override string Apply(string template, string[] arguments)
        {
            var lines = _buildingData.GetAndSkipTakenBuildings("LINE", 2).ToList();

            var (line1, line2) = GenerateImage(lines);
            var targetScore = CalculateTargetScore(line1, line2);

            _generationResult.AddImage(new GeekImage("SETUP_IMAGE", "SETUP_IMAGE.png"));

            var setupText =
                $"Setup the board like the image, with the yellow ({line1.Value}) and pink ({line2.Value}) lines belonging to the Underground. " +
                $"The Underground starts at {targetScore} points and we start with [b]one[/b] branching tokens. " +
                "The game immediately ends once we overtake the Underground or the destination deck is empty.";

            return template.ReplaceFirst(Token, setupText);
        }

        private int CalculateTargetScore(Line line1, Line line2)
        {
            var baseScore = line1.Value + line2.Value;
            var expertScore = baseScore + 20;

            if (line1 is BerlinLine berlinLine1 && line2 is BerlinLine berlinLine2)
            {
                if (berlinLine1.Icon != BerlinLineIcon.None && berlinLine1.Icon == berlinLine2.Icon)
                {
                    return expertScore + 30;
                }
            }

            return expertScore;
        }

        private (Line line1, Line line2) GenerateImage(List<Building> lines)
        {
            var map = _gameOptions.Options["Map"];
            if (map == "London")
            {
                var _londonLines = LondonLinesFactory.Create();
                var line1 = _londonLines.First(p => p.Id == lines[0].Id);
                var line2 = _londonLines.First(p => p.Id == lines[1].Id);

                _londonMapGenerator.Generate(line1, line2);
                return (line1, line2);
            }

            if (_gameOptions.Options["Map"] == "Berlin")
            {
                var _berlinLines = BerlinLinesFactory.Create();
                var line1 = _berlinLines.First(p => p.Id == lines[0].Id);
                var line2 = _berlinLines.First(p => p.Id == lines[1].Id);

                _berlinMapGenerator.Generate(line1, line2);
                return (line1, line2);
            }

            throw new InvalidOperationException($"Map '{map}' is not supported.");
        }
    }
}