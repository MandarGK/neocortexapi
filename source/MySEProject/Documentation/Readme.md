# Project Title: ML 23/24-03 Implement the new Spatial Learning experiment.

### This project implements a new Spatial Learning experiment using the NeoCortexApi. The experiment utilizes the Spatial Pooler (SP) algorithm to learn and predict spatial patterns of binary input vector using C#.
## Overview
* [Contributors](#Contributers)
* [Problem Statement](#Problem-Statement)
* [Introduction](#Introduction)
* [Encoder](#Encoder)
* [HTM Spatial Pooler](#Spatial-Pooler)
* [Homeostatic Plasticity Controller](#Homeostatic-Plasticity-Controller)
* [Experiment Execution](#Experiment-Execution)

  
## Contributors:

This project is created by the joint efforts of
* [Mandar Kale](https://github.com/MandarGK)
* [Anushruthpal Keshavathi Jayapal](https://github.com/Anushruth16)
* [Lavanya suresh](https://github.com/LavanyaSuresh23)


 ## Problem Statement: 

The new Spatial Learning experiment aims to enhance the Spatial Pooling (SP) algorithm by fine-tuning key parameters and optimizing the performance of the HTM Spatial Pooler while addressing specific challenges encountered during the learning phase. The main objectives include implementing persistent storage of Sparse Distributed Representations (SDRs) of inputs in a dictionary structure, detecting stability in SDR generation, tracking iterations to measure stability, and establishing criteria for experiment termination when SDRs remain unchanged for a specified duration.
The output format will provide detailed cycle information, including the number of stable iterations, input index, active columns, overlap score, and current SDR representation.
This experiment is based on the neocortexapi repository's SpatialPatternLearning.cs file and seeks to improve the SP algorithm's learning capabilities and efficiency.


 ## Introduction:



 
 ### Encoder:

In this experiment, the scalar encoder was utilized to convert scalar values into encoded representations before presenting them to the Spatial Pooler during the learning phase. As an input, sequence of 0 and 100 values were used. Prior to being fed into the Spatial Pooler, each input value underwent encoding with 200 bits, with each value represented by 15 non-zero bits.

_**Encoder Parameters**_
```
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
```
 ### HTM Spatial Pooler:
The Spatial Pooler (SP) is a crucial algorithm within the Hierarchical Temporal Memory (HTM) framework designed to learn spatial patterns inspired by the neo-cortex. It is designed to learn the pattern in a few iteration steps and to generate
the Sparse Distributed Representation (SDR) of the input. This SDR serves as a compact and efficient representation of the input data, capturing its essential spatial features. Through the process of spatial pooling, the SP identifies and activates a subset of neurons, known as mini-columns, based on the input's spatial patterns, thus facilitating further processing within the HTM network.



_**Spatial Pooler Parameters**_

```
HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
{
    CellsPerColumn = 10,
    MaxBoost = maxBoost,
    DutyCyclePeriod = 1000,
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
```


### Homeostatic Plasticity Controller:

The Homeostatic Plasticity Controller (HPC) extends the default Spatial Pooler (SP) algorithm within the Hierarchical Temporal Memory (HTM) framework. Its primary function is to initiate the SP in a newborn stage at the onset of the learning process. During this phase, the SP initiates boosting mechanisms to stimulate mini-columns, allowing for initial instability in the learning process. After a predefined number of iterations, the HPC deactivates boosting and awaits the SP to achieve stability. This approach enables the SP to converge to a stable state, enhancing the quality of learning and facilitating reliable solutions. Additionally, applications can be notified about the SP's state changes, improving adaptability and performance in various contexts.

 **[Go to top &uarr;](#Overview)**
 
 ## Experiment Execution:


**Input Generation**: Random input values are generated for the experiment.

**SP Initialization**: The Spatial Pooler is initialized with the specified configuration and encoder.

**Learning Process**: The experiment iterates over a defined number of cycles, presenting input values to the SP and observing its behavior. The SP adjusts its connections and activation patterns based on the input.

**Stability Detection**: A Homeostatic Plasticity Controller monitors the SP's stability. Once the SP enters a stable state, it notifies the experiment, indicating that learning is complete.

**Output Analysis**: During stable periods, the experiment records the SDRs generated by the SP for each input value. It also tracks the similarity between consecutive SDRs to measure stability.

**Result Visualization**: The experiment outputs the SDRs generated by the SP for the 99th input value over consecutive cycles. This provides insight into the SP's behavior and learning progression.

 **[Go to top &uarr;](#Overview)**

 ## Running the Experiment:

 To run the experiment: