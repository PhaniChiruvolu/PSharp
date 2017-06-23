﻿//-----------------------------------------------------------------------
// <copyright file="DelayBoundingStrategy.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.PSharp.IO;
using Microsoft.PSharp.Utilities;

namespace Microsoft.PSharp.TestingServices.Scheduling
{
    /// <summary>
    /// Class representing an abstract delay-bounding scheduling strategy.
    /// </summary>
    public abstract class DelayBoundingStrategy : ISchedulingStrategy
    {
        #region fields

        /// <summary>
        /// The configuration.
        /// </summary>
        protected Configuration Configuration;

        /// <summary>
        /// Nondeterminitic seed.
        /// </summary>
        protected int Seed;

        /// <summary>
        /// Randomizer.
        /// </summary>
        protected IRandomNumberGenerator Random;

        /// <summary>
        /// The maximum number of explored steps.
        /// </summary>
        protected int MaxExploredSteps;

        /// <summary>
        /// The number of explored steps.
        /// </summary>
        protected int ExploredSteps;

        /// <summary>
        /// The maximum number of delays.
        /// </summary>
        protected int MaxDelays;

        /// <summary>
        /// The remaining delays.
        /// </summary>
        protected List<int> RemainingDelays;

        #endregion

        #region public API

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="delays">Max number of delays</param>
        public DelayBoundingStrategy(Configuration configuration, int delays)
        {
            this.Configuration = configuration;
            this.Seed = this.Configuration.RandomSchedulingSeed ?? DateTime.Now.Millisecond;
            this.Random = new DefaultRandomNumberGenerator(this.Seed);
            this.MaxExploredSteps = 0;
            this.ExploredSteps = 0;
            this.MaxDelays = delays;
            this.RemainingDelays = new List<int>();
        }

        /// <summary>
        /// Returns the next choice to schedule.
        /// </summary>
        /// <param name="next">Next</param>
        /// <param name="choices">Choices</param>
        /// <param name="current">Curent</param>
        /// <returns>Boolean</returns>
        public virtual bool TryGetNext(out ISchedulable next, List<ISchedulable> choices, ISchedulable current)
        {
            var currentMachineIdx = choices.IndexOf(current);
            var orderedMachines = choices.GetRange(currentMachineIdx, choices.Count - currentMachineIdx);
            if (currentMachineIdx != 0)
            {
                orderedMachines.AddRange(choices.GetRange(0, currentMachineIdx));
            }

            var enabledChoices = orderedMachines.Where(choice => choice.IsEnabled).ToList();
            if (enabledChoices.Count == 0)
            {
                next = null;
                return false;
            }

            int idx = 0;
            while (this.RemainingDelays.Count > 0 && this.ExploredSteps == this.RemainingDelays[0])
            {
                idx = (idx + 1) % enabledChoices.Count;
                this.RemainingDelays.RemoveAt(0);
                Debug.WriteLine("<DelayLog> Inserted delay, '{0}' remaining.", this.RemainingDelays.Count);
            }

            next = enabledChoices[idx];

            this.ExploredSteps++;

            return true;
        }

        /// <summary>
        /// Returns the next boolean choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <param name="next">Next</param>
        /// <returns>Boolean</returns>
        public virtual bool GetNextBooleanChoice(int maxValue, out bool next)
        {
            next = false;
            if (this.RemainingDelays.Count > 0 && this.ExploredSteps == this.RemainingDelays[0])
            {
                next = true;
                this.RemainingDelays.RemoveAt(0);
                Debug.WriteLine("<DelayLog> Inserted delay, '{0}' remaining.", this.RemainingDelays.Count);
            }

            this.ExploredSteps++;

            return true;
        }

        /// <summary>
        /// Returns the next integer choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <param name="next">Next</param>
        /// <returns>Boolean</returns>
        public virtual bool GetNextIntegerChoice(int maxValue, out int next)
        {
            next = this.Random.Next(maxValue);
            this.ExploredSteps++;
            return true;
        }

        /// <summary>
        /// Returns the explored steps.
        /// </summary>
        /// <returns>Explored steps</returns>
        public int GetExploredSteps()
        {
            return this.ExploredSteps;
        }

        /// <summary>
        /// True if the scheduling strategy has reached the max
        /// scheduling steps for the given scheduling iteration.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool HasReachedMaxSchedulingSteps()
        {
            var bound = (this.IsFair() ? this.Configuration.MaxFairSchedulingSteps :
                this.Configuration.MaxUnfairSchedulingSteps);

            if (bound == 0)
            {
                return false;
            }
            
            return this.ExploredSteps >= bound;
        }

        /// <summary>
        /// Checks if this a fair scheduling strategy.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsFair()
        {
            return false;
        }

        /// <summary>
        /// Prepares the next scheduling iteration.
        /// </summary>
        /// <returns>False if all schedules have been explored</returns>
        public abstract bool PrepareForNextIteration();

        /// <summary>
        /// Resets the scheduling strategy.
        /// </summary>
        public virtual void Reset()
        {
            this.Random = new DefaultRandomNumberGenerator(this.Seed);
            this.MaxExploredSteps = 0;
            this.ExploredSteps = 0;
            this.RemainingDelays.Clear();
        }

        /// <summary>
        /// Returns a textual description of the scheduling strategy.
        /// </summary>
        /// <returns>String</returns>
        public abstract string GetDescription();

        #endregion
    }
}
