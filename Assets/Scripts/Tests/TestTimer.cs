﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace C2M2
{
    namespace Tests
    {
        using Utilities;
        public class TestTimer : AwakeTest
        {
            public int tickErrorThreshold = 2;
            public override bool PreTest()
            {
                return true;
            }
            // Timer should take 0 seconds to run itself
            public override bool RunTest()
            {
                TimeUtilities.Timer timer = new TimeUtilities.Timer();
                timer.StartTimer();
                timer.StopTimer();
                bool succeeded = timer.timerNodes[0].Ticks < tickErrorThreshold;

                return succeeded;
            }
            public override bool PostTest()
            {
                return true;
            }
        }
    }   
}