﻿using System;
using Microsoft.PSharp;
using Microsoft.PSharp.TestingServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Raft.PSharpLibrary
{
    /// <summary>
    /// A single-process implementation of the Raft consensus protocol written using P#
    /// as a C# library.
    /// 
    /// The description of Raft can be found here: https://raft.github.io/raft.pdf
    ///  
    /// Note: this is an abstract implementation aimed primarily to showcase the testing
    /// capabilities of P#.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Optional: increases verbosity level to see the P# runtime log.
            var configuration = Configuration.Create().WithVerbosityEnabled(2);

            // Creates a new P# runtime instance, and passes an optional configuration.
            var runtime = PSharpRuntime.Create(configuration);

            // Executes the P# program.
            Program.Execute(runtime);

            // The P# runtime executes asynchronously, so we wait
            // to not terminate the process.
            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();
        }

        [Microsoft.PSharp.Test]
        public static void Execute(PSharpRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(SafetyMonitor));
            runtime.CreateMachine(typeof(ClusterManager));
        }
    }

    [TestClass]
    public class VsTest
    {
        [TestMethod]
        public void UnitTest()
        {
            var report =
                TestingEngineFactory.RunTester("/i:10 /max-steps:100", Program.Execute);

            Assert.IsTrue(true); 
        }
    }
}
