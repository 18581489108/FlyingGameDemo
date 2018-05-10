﻿using Kurisu.GameEditor.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kurisu.Game.Data
{
    /// <summary>
    /// 正常模式下的地图数据
    /// </summary>
    public class NormalModeMapData : MapData
    {
        /// <summary>
        /// 地图数据
        /// </summary>
        public MapPartData mapPart;

        public NormalModeMapData() : base(MapMode.NormalMode)
        {

        }
    }
}