﻿using System.Collections.Generic;
using System.Linq;
using scg.Utils;

namespace scg.Windows.OnTheUnderground
{
    public class DestinationDeck
    {
        public DestinationDeck(IEnumerable<DestinationCard> destinations)
        {
            Destinations = destinations.ToList();
            Destinations.Shuffle();
        }

        public List<DestinationCard> Destinations { get; }
    }
}