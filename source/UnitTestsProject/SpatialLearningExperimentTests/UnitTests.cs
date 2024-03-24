using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using NeoCortexApi;

namespace UnitTestsProject.SimilarityUnitTests
{
    /// <summary>
    /// Test for similarity calculation.
    /// </summary>
    [TestClass]
    public class UnitTests
    {
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 1, 2, 3, 4, 5, 6 }, 100.0)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 1, 2, 3, 4 }, 66.66666666666666)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 4, 5, 6 }, 50.0)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { }, -1.0)]
        [DataRow(new int[] { }, new int[] { 1, 2, 3, 4, 5, 6 }, -1.0)]
        [TestMethod]
        [TestCategory("Prod")]
        public void TestJaccardSimilarity(int[] arr1, int[] arr2, double expectedSimilarity)
        {
            double calculatedSimilarity = MathHelpers.JaccardSimilarity(arr1, arr2);

            Assert.AreEqual(expectedSimilarity, calculatedSimilarity);

            Console.WriteLine($"{calculatedSimilarity}");
            Console.WriteLine($"{Helpers.StringifyVector(arr1)}");
            Console.WriteLine($"{Helpers.StringifyVector(arr2)}");
            Console.WriteLine();

        }

        [DataRow(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, true)]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { 1, 2, 4 }, false)]
        [DataRow(new int[] { 1, 2, 3 }, null, false)]
        [DataRow(null, new int[] { 1, 2, 3 }, false)]
        [DataRow(null, null, false)]
        [TestMethod]
        [TestCategory("Prod")]
        public void TestAreArraysEqual(int[] arr1, int[] arr2, bool expectedResult)
        {
            bool result = MathHelpers.AreArraysEqual(arr1, arr2);

            Assert.AreEqual(expectedResult, result);
        }


    }
}
