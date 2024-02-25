Project Title :- To Implement The new Spatial Learning Experiment.

Project Description:- 
- In this Experiment, the Output of Spatial Pooler which is SDR (Sparse Distribution Representation) is getting compared with the previous cycle Output.
- An SDR consists of a large array of bits in which most of them are zeros and a few are ones. Each bit carries some semantic meaning so if two SDRs have more than a few overlapping one-bits, then those two SDRs have a similarity.
- The similarity between two SDRs can be determined using an overlap score which is simply the number of ON bits in common, or in the same location between the vectors.
- Any type of concept can be encoded in an SDR, including different types of sensory data, words,images,audio,locations, and behaviors.
- During the Learning Time, the Spatial Pooler will create SDRs for the same input. After a while the SDR will maintain the Stable state. Same input should always produce the same SDR output.
- The Stable state is derived on the basis of a value isInStableState is set to true, Since there is No change Detected in further SDRs.
- Since the Experiment runs to its Pre-Defined 1000 Cycles But the Stable state is achieved before reaching the 1000th Cycle. Here, Stability is set to TRUE after attaining 50 stable cycles.
- We have to build a New Logic to Compare the SDRs of two consecutive cycles in appropriate manner so that it should take a set of Min Cycles to achieve the Stable State.
- Once the stable state is achieved, we have to set a condition to exit.
Spatial Pooler:
- The HTM system consists of an encoder, the HTM spatial pooler, the HTM temporal memory, and an SDR classifier.
- The HTM spatial pooler represents a learning algorithm for creating sparse representations from noisy data streams in an online fashion. It models how neurons learn feedforward connections and form efficient representations of the input. 
- It converts arbitrary binary input patterns into sparse distributed representations (SDR) using homeostatic excitability control.
SDR:
- SDRs are not moved around in memory, like data in computers.
