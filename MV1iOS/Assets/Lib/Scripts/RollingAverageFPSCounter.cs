// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicLeap.XR.XRKit.Sample
{
    class RollingAverageFPSCounter
    {
        // Number of seconds to average
        public int Seconds;

        private Queue<int> framesPerSecond;
        private float startTime;
        private int numFramesThisSecond = 0;

        public RollingAverageFPSCounter(int seconds = 5)
        {
            Seconds = seconds;
            framesPerSecond = new Queue<int>(Seconds);
        }

        public void StartCollecting(float time)
        {
            startTime = time;
        }

        public void AddFrame(float time)
        {
            numFramesThisSecond++;

            if (time - startTime > 1.0f)
            {
                framesPerSecond.Enqueue(numFramesThisSecond);
                // There's already 1 frame in the new second, this one
                numFramesThisSecond = 1;
                startTime = time;

                if (framesPerSecond.Count > Seconds)
                {
                    framesPerSecond.Dequeue();
                }
            }
        }

        public float InstFPS(float time) => numFramesThisSecond / (time - startTime);
        public float AvgFPS()
        {
            float denominator = 1.0f;
            if (framesPerSecond.Count != 0)
            {
                Debug.Log(framesPerSecond.Count);
                denominator = framesPerSecond.Count;
            }
            return framesPerSecond.Sum() / denominator;
        }
    }
}