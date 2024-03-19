using NeoCortexApi;
using NeoCortexApi.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace NeoCortexApiExperiment
{
    class Program
    {
        /// <summary>
        /// This sample shows a typical experiment code for SP and TM.
        /// You must start this code in debugger to follow the trace.
        /// and TM.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //
            // Starts experiment that demonstrates the implementation of new Spatial Pooler Learning Experminent and  how to learn spatial patterns.
            SpatialLearningExperiment experiment = new SpatialLearningExperiment();
            experiment.Run();

         
        }

    }

}
