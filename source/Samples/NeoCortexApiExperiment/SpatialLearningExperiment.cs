using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void Run ()
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
                StimulusThreshold = 0.5,


            };

            //Implementing a encoder for this experiment
            EncoderBase encoder = new ScalarEncoder(settings);

            //Creating 100 random variables as input
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            var sp = RunExperiment(cfg, encoder, inputValues);
        }


        private static SpatialPooler RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            var mem = new Connections(cfg);

            bool isInStableState = false;

            //Implementing New Method for Boosting
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) => {


                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        //This should usually not happen
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        //Entering in to Stable State
                        isInStableState = true;
                    }

                });


            //Creating the instance of Spatial Pooler Multithreaded version
            SpatialPooler sp = new SpatialPooler();

            //Initalizes the Spatial pooler
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            //Implementing Layers for Neocortex.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Encoder will receive the input and forward the encoded signal to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // This Module will use the Output From Encoder and Build Sparse Distributed Representation.
            cortexLayer.HtmModules.Add("sp", sp);

            double[] inputs = inputValues.ToArray();

            //Understanding the Input value in Array.
            foreach (double value in inputs)
            {
                Console.WriteLine("Inside For each");
                Console.WriteLine(value);
            }

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk - 1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //Initialize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            int maxSPLearningCycles = 1000;

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  * {cycle} * Stability: {isInStableState}");


                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
                    // 
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];

                    // This is a general way to get the SpatialPooler result from the layer.
                    var activeColumns = cortexLayer.GetResult("sp") as int[];

                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

                    //Creating a Dictionary to store Sdr values.
                    Dictionary<int, List<int>> SdrDictionary = new Dictionary<int, List<int>>();

                    //Converting the var int[] actcols to List<int>.
                    List<int> actColsintList = actCols.ToList();

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

            }

            return sp;
        }
    }

}

