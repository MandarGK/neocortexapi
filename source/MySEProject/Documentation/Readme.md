Project Title :- To Implement The new Spatial Learning Experiment.

Project Description:- 
- In this Experiment the Output of Spatial Pooler which is SDR (Sparse Distribution Representation) is getting compared with the previous Output.
- An SDR consists of a large array of bits in which most of them are zeros and a few are ones. Each bit carries some semantic meaning so if two SDRs have more than a few overlapping one-bits, then those two SDRs have a similarity.
- During the Learning Time, the Spatial Pooler will create SDRs for the same input. After a while the SDR will maintain the Stable state. Same input should always produce the same SDR output.
- The Stable state is derived on the basis of a value isInStableState is set to true, Since there is No change Detected in further SDRs.
- Since the Experiment runs to its Pre-Defined 1000 Cycles But the Stable state is achieved before reaching the 1000th Cycle. Here, Stability is set to TRUE after attaining 50 stable cycles.
- We need to set some rules of Comparing the SDR and Build a New Logic to Compare the SDRs in appropriate manner so that it should take a set of Min Cycles to Achieve the Stable State and as soon as it get into Stable state we will exit the Loop.

