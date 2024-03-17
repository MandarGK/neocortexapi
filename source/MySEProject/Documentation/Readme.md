iProject Title :- To Implement The new Spatial Learning Experiment.

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
- An SDR consists of thousands of bits where at any point in time a small percentage of the bits are 1’s and the rest are 0’s. The bits in an SDR correspond to neurons in the brain, a 1 being a relatively active neuron and a 0 being a relatively inactive neuron. The most important property of SDRs is that each bit has meaning. Therefore, the set of active bits in any particular representation
encodes the set of semantic attributes of what is being represented. The bits are not labeled (that is to say, no one assigns meanings to the bits), but rather, the semantic meanings of bits are learned. If two SDRs have active bits in the same locations, they share the semantic attributes represented by those bits.
- It converts arbitrary binary input patterns into sparse distributed representations (SDR) using homeostatic excitability control.
- The output of the Spatial Pooler represents the mini columns i.e., the pyramidal neuron in the cortices.
- We need learning in Spatial Pooler because if inputs are random, an untrained random Spatial pooler will do just as good as any trained Spatial Pooler.
- However, real inputs are structured. Input SDRs occur with non-equal probabilities.
SDR:
- SDRs are not moved around in memory, like data in computers. Instead the set of active neurons, within a fixed population of neurons, changes over time.
- At one moment a set of neurons represents one thing; the next moment it represents something else.
- Within one set of neurons, an SDR at one point in time can associatively link to the next occurring SDR. In this way, sequences of SDRs are learned. 
- Associative linking also occurs between different populations of cells (layer to layer or region to region).
- Because the binary representation is more biologically reasonable and highly computationally efficient, HTM considers the binary SDR converted from a specific encoder. Even though the number of possible inputs exceeds the number of possible representations, the binary SDR does not result in a functional loss of information due to the critical features of the SDR.

Solution:

1. The SDRs of all inputs are implemented in the form of dictionary as follows,
    for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
    
    {

        Dictionary<int, List<int>> SdrDictionary = new Dictionary<int, List<int>>();
         SdrDictionary.Add(input, { Helpers.StringifyVector(actCols)} );
         Console.WriteLine("Printing the dictionary:");
          foreach(var kvp in SdrDictionary)
                 {
                     Console.Write($"Key: {kvp.Key}, Values: [");
         
                     // Printing the elements of the List<int>
                     for (int i = 0; i < kvp.Value.Count; i++)
                     {
                         Console.Write($"{kvp.Value[i]}");
                         if (i < kvp.Value.Count - 1)
                         {
                             Console.Write(", ");
                         }
                     }
         
                    Console.WriteLine("]");
         
                 }
    }
2. During learning time, the Spatial Pooler creates different SDRs for the same input and after sometime it keeps the SDR stable.
   To ensure there is no SDR change is detected (instable state) after the stable state is set to true, we have implemented the following,
   (.) By modifying the "No.of stable cycles to wait on change"
        a. No.of stable cycles to wait on change = 50
        It is observed that SP enters STABLE state in 409th cycle after 50 consecutive cycles no SDR change, But in 453rd cycle, SP becomes unstable because of a small SDR change. Later in 503rd cycle SP again enters STABLE state, and later it is stable till 1000 cycles.
        
        b. No.of stable cycles to wait on change = 75
        It is observed that SP enters STABLE state in 434th cycle after 50 consecutive cycles no SDR change, But in 453rd cycle, SP becomes unstable because of a small SDR change. Later in 528th cycle SP again enters STABLE state, and later it is stable till 1000 cycles.

        c. No.of stable cycles to wait on change = 100
        It is observed that SP enters STABLE state in 509th cycle after that no SDR change, the SP remains in stable state.

   (.) By modifying the Similarity threshold.
        a. when similarity threshold is 0.97

        b. when similarity threshold is 0.95

3. When the stability is achieved, we check if it remains stable throughout.


