﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Track_Maker
{
    public enum TelemetryConsent
    { 
        /// <summary>
        /// Telemetry consent not completed
        /// </summary>
        NotDone = 0,
        
        /// <summary>
        /// Telemetry consent denied
        /// </summary>
        No = 1, 

        /// <summary>
        /// Telemetry consent approved
        /// </summary>
        Yes = 2
    };
}
