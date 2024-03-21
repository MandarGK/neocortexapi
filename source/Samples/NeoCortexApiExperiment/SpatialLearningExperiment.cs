using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Drawing;
using System.IO;
using System.Linq;
using static NPOI.HSSF.Util.HSSFColor;
using NPOI.SS.Formula.Functions;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace NeoCortexApiExperiment
{
    /// <summary>
    /// Implements an experiment that demonstrates how to learn spatial patterns.
    /// SP will learn every presented input in multiple iterations.
    /// </summary>
    public class SpatialLearningExperiment
    {
        public void Run()
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(SpatialLearningExperiment)}");

            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 200;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 1024;

            //
            // This is a set of configuration parameters used in the experiment.
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
                StimulusThreshold = 10
            };

            double max = 100;

            //
            // This dictionary defines a set of typical encoder parameters.
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


            EncoderBase encoder = new ScalarEncoder(settings);

            //
            // We create here 100 random input values.
            List<double> inputValues = new List<double>();

            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            var sp = RunExperiment(cfg, encoder, inputValues, numColumns);

            
        }

        /// <summary>
        /// Implements the experiment.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="encoder"></param>
        /// <param name="inputValues"></param>
        /// <returns>The trained bersion of the SP.</returns>
        private static SpatialPooler RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues, int numColumns)
        {
            // Creates the htm memory.
            var mem = new Connections(cfg);

            bool isInStableState = false;

            
            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    // Event should only be fired when entering the stable state.
                    // Ideal SP should never enter unstable state after stable state.
                    if (isStable == false)
                    {
                        Debug.WriteLine($"INSTABLE STATE");
                        // This should usually not happen.
                        isInStableState = false;
                    }
                    else
                    {
                        Debug.WriteLine($"STABLE STATE");
                        // Here you can perform any action if required.
                        isInStableState = true;
                    }
                });

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPooler(hpa);
            //sp = new SpatialPoolerMT(hpa);

            // Initializes the patial pooler
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            double[] inputs = inputValues.ToArray();

            // Will hold the SDR of every inputs.
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

            // Will hold the similarity of SDKk and SDRk-1 fro every input.
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            //
            // Initiaize start similarity to zero.
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            // Learning process will take 1000 iterations (cycles)
            int maxSPLearningCycles = 1000;

            //Intializing counter to break the loop after reaching stability
            int counter = 0;

            // Creating a Dictionary to store SDR values.
            Dictionary<int, List<List<int>>> SdrDictionary = new Dictionary<int, List<List<int>>>();

            // Define a list to store SDRs for the 0th input
            List<(int CycleNumber, List<int> SDR)> sdrsForInput0 = new List<(int CycleNumber, List<int> SDR)>();

            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle  * {cycle} * Stability: {isInStableState}");


                foreach (var input in inputs)
                {
                    double similarity;

                    // Learn the input pattern.
                    // Output lyrOut is the output of the last module in the layer.
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

                    if (Convert.ToInt32(input) == 0) // Check if the input is the 0th input
                    {
                        // Add the current cycle number and SDR for the 0th input to the list
                        sdrsForInput0.Add((cycle, actCols.ToList()));
                    }

                    Debug.WriteLine($"[cycle={cycle.ToString("D4")}, StableCycles ={counter}, i={input}, cols={actCols.Length} s={similarity}] SDR: {string.Join(", ", SdrDictionary[Convert.ToInt32(input)].Last())}");
                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;

                    

                }


                // Condition to check if the Spatial Pooler has Entered into Stable state.
                if (isInStableState)
                {

                    // Incrementing the Counter to get the desire value to print the SDR for 100 Stable Cycles 
                    counter++;
                    Debug.WriteLine($"Counter Value: {counter}");

                    if (counter == inputs.Length)
                    {
                        // Print the last 100 stable cycles.
                        int startCycle = Math.Max(0, cycle - 99); // Adjusted start cycle

                        // Checking the last 100th Cycle for the Iteration 
                        int endCycle = cycle; // End cycle is the current cycle

                        for (int i = startCycle; i <= endCycle; i++)
                        {
                            Debug.WriteLine($"Cycle ** {i} **:");

                            foreach (var kvp in SdrDictionary)
                            {
                                // Check if the key exists in the dictionary and has the required number of iterations.
                                if (SdrDictionary.ContainsKey(kvp.Key) && i - startCycle < SdrDictionary[kvp.Key].Count)
                                {
                                    // Get the SDRs for the current input key and cycle.
                                    List<int> sdrsForInput = SdrDictionary[kvp.Key][i - startCycle];

                                    // Print the SDRs in a formatted way.
                                    Debug.WriteLine($" Iteration: {kvp.Key} | SDR of {i - startCycle + 1} stable cycle: {Helpers.StringifyVector(sdrsForInput.ToArray())}");
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

                    // Clearing All SDR Stored in Dictionary during the Instable state.
                    SdrDictionary.Clear();
                    // Setting the counter to reset / 0 during the Instable state of Spatial pooler.
                    counter = 0;
                    Debug.WriteLine($"Counter is set to Zero Stability is not yet Reached");
                }

               

            }
            Debug.WriteLine("SDRs for the 0th input:");
            // Print input number, cycle number, and SDRs for the 0th input
            foreach (var (CycleNumber, SDR) in sdrsForInput0)
            {
                Debug.WriteLine($"Input No: 0, Cycle Number: {CycleNumber}, SDR: {string.Join(", ", SDR)}");
            }


            return sp;
        }

       
    }
}