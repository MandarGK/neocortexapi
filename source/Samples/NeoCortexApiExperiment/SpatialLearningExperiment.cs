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
                { "W", 15},
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


            int counter = 0;

            // Creating a Dictionary to store SDR values.
            Dictionary<int, List<List<int>>> SdrDictionary = new Dictionary<int, List<List<int>>>();

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

                    // Check if the input key already exists in the dictionary.
                    if (!SdrDictionary.ContainsKey(Convert.ToInt32(input)))
                    {
                        // If not, add a new list to store SDR values for this input.
                        SdrDictionary[Convert.ToInt32(input)] = new List<List<int>>();
                    }

                    // Add the current SDR to the list for this input.
                    SdrDictionary[Convert.ToInt32(input)].Add(actCols.ToList());

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

                if (isInStableState)
                {
                    counter++;
                    Debug.WriteLine($"Counter Value: {counter}");

                    if (counter == inputs.Length)
                    {
                        // Print the last 100 stable cycles.
                        int startCycle = Math.Max(0, cycle - 99); // Adjusted start cycle
                        Debug.WriteLine($"Cycle:{cycle} , StartCycle: {startCycle}");
                        int endCycle = cycle; // End cycle is the current cycle
                        Debug.WriteLine($"Cycle:{cycle} , EndCycle: {endCycle}");


                        for (int i = startCycle; i <= endCycle; i++)
                        {
                            Debug.WriteLine($"Cycle ** {i} **:");

                            foreach (var kvp in SdrDictionary)
                            {
                                Debug.WriteLine($"Iteration: {kvp.Key}");

                                // Check if the key exists in the dictionary and has the required number of iterations.
                                if (SdrDictionary.ContainsKey(kvp.Key) && i - startCycle < SdrDictionary[kvp.Key].Count)
                                {
                                    // Get the SDRs for the current input key and cycle.
                                    List<int> sdrsForInput = SdrDictionary[kvp.Key][i - startCycle];

                                    // Print the SDRs in a formatted way.
                                    Debug.WriteLine($"  SDR {i - startCycle + 1}: {Helpers.StringifyVector(sdrsForInput.ToArray())}");
                                }

                            }

                            Debug.WriteLine("");  // New line for the next cycle.
                        }

                        // Break after printing the last 100 stable cycles.
                        break;
                    }

                }
                else
                {
                    SdrDictionary.Clear();
                    counter = 0;
                    Debug.WriteLine($"Counter is set to Zero Stability is not yet Reached");
                }


            }


                return sp;
        }
    }

}

