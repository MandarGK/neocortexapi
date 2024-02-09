using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiExperiment
{
    /// <summary>
    /// Implementng a new experiment that demonstrates Spatial pattern learining.
    /// </summary>
    public class SpatialLearningExperiment
    {
        public void Run()
        {
            Console.WriteLine($"NeocortexApi! New Spatial Learning Experiment {nameof(SpatialLearningExperiment)}");
            int inputBits = 200;

            double max = 100;


            // Adding some parameter boosting Parameters 

            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // Using the Mini Columns we will create a slice of neocortex.
            int numColumns = 1024;


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

            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,


                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,

                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,


            };

            //Implementing a encoder for this experiment
            EncoderBase encoder = new ScalarEncoder(settings);

            //Creating 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }
        }
    }
}