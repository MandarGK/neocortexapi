using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiExperiment
{
    /// <summary>
    /// Implementng a new experiment which demonstrates Spatial pattern learining.
    /// </summary>
    internal class SpatialLearningExperiment
    {
        /// We will use 200 bits to represent an input vector (pattern).
        int inputBits = 200;

        double max = 100;

        /// <summary>
        /// Implementing New Parameter in Dictionary for Scalar Encoder. 
        /// </summary>
        Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 5},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };
    }
}
