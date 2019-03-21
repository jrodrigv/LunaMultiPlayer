﻿using System.Collections.Generic;

namespace LmpClient.Systems.Warp
{
    public class SubspaceDisplayEntry
    {
        public int SubspaceId { get; set; }
        public double SubspaceTime { get; set; }
        public List<string> Players { get; set; }
    }
}