﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TrackMaker.Core
{
    /// <summary>
    /// Settings that are specific to the Track Maker session.
    /// 
    /// Used to reduce MainWindow use
    /// </summary>
    public class VolatileSettings
    {
        public Point WindowSize { get; set; }
    }
}
