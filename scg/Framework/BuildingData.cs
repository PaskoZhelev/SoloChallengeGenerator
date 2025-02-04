﻿using System;
using System.Collections.Generic;
using System.Linq;
using scg.Utils;

namespace scg.Framework
{
    public class BuildingData
    {
        private readonly FileRepository _repository;
        private readonly List<Building> _buildings = new();
        private readonly List<Building> _takenBuildings = new();
        private readonly List<List<int>> _illegalBuildingCombinations = new();
        private readonly FlagsDictionary _flags;

        public BuildingData(FileRepository repository, FlagsDictionary flags)
        {
            _repository = repository;
            _flags = flags;
            Load();
        }

        public IEnumerable<Building> GetAndSkipTakenBuildings(string category, int number)
        {
            return GetAndSkipTakenBuildings(category, number, _ => true);
        }

        public IEnumerable<Building> GetAndSkipTakenBuildings(string category, IEnumerable<string> subcategories, int number)
        {
            return GetAndSkipTakenBuildings(category, number,
                p => subcategories.Any(s => string.Equals(s, p.Subcategory, StringComparison.InvariantCultureIgnoreCase)));
        }

        public void ResetTakenBuildings()
        {
            _takenBuildings.Clear();
        }

        public IEnumerable<Building> GetAll(string category)
        {
            return _buildings.Where(p => string.Equals(p.Category, category, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        
        private void Load()
        {
            InitializeBuildings();
            InitializeTranslations();
            InitializeIllegalCombinations();
            RandomizeBuildings();
        }

        private void InitializeBuildings()
        {
            var buildingData = _repository.ReadAllLines(File.Buildings, false);
            foreach (var line in buildingData)
            {
                var splits = line.Split(",");
                var category = splits[0].Trim();
                var subcategory = splits.Length > 1 ? splits[1].Trim() : string.Empty;
                Add(new Building(category, subcategory,  _flags));
            }
        }

        private void InitializeTranslations()
        {
            var translationFiles = _repository.GetFiles("Translations", ".lang");
            foreach (var translationFile in translationFiles)
            {
                AddTranslation(translationFile);
            }
        }

        private void InitializeIllegalCombinations()
        {
            var illegalCombinations = _repository.ReadAllLines(File.IllegalCombinations, false);
            foreach (var illegalCombination in illegalCombinations)
            {
                var ids = illegalCombination.Split(";").Select(int.Parse).ToList();
                _illegalBuildingCombinations.Add(ids);
            }
        }

        private void RandomizeBuildings()
        {
            _buildings.Shuffle();
        }

        private void AddTranslation(string translationFile)
        {
            var lines = _repository.ReadAllLines(translationFile, false);
            if (lines.Length != Count)
            {
                Console.WriteLine($"Translation file '{translationFile}' has an invalid number of entries. Expected: {Count}.");
                return;
            }

            var languageName = translationFile.Replace("Translations.", "").Replace(".lang", "");

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var building = _buildings[i];

                building.AddTranslation(languageName, line);
            }
        }

        private int Count => _buildings.Count;

        private IEnumerable<Building> GetAndSkipTakenBuildings(string category, int number, Func<Building, bool> additionalFilter)
        {
            var chosenBuildings = new List<Building>();
            while (chosenBuildings.Count < number)
            {
                var candidate = _buildings.Except(_takenBuildings)
                    .Where(p => string.Equals(p.Category, category, StringComparison.InvariantCultureIgnoreCase))
                    .Where(additionalFilter)
                    .First();
                
                _takenBuildings.Add(candidate);

                if (_illegalBuildingCombinations.Any(illegalBuildingCombination =>
                        illegalBuildingCombination.Contains(candidate.Id) &&
                        illegalBuildingCombination.All(id => _takenBuildings.Any(b => b.Id == id))))
                {
                    continue;
                }

                chosenBuildings.Add(candidate);
            }
            
            return chosenBuildings;
        }
        
        private void Add(Building building)
        {
            _buildings.Add(building);
        }
    }
}