﻿//-----------------------------------------------------------------------
// <copyright file="ExhaustiveDelayBoundingStrategy.cs">
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

namespace Microsoft.PSharp.TestingServices.Scheduling
{
    /// <summary>
    /// Class representing an exhaustive delay-bounding scheduling strategy.
    /// </summary>
    public sealed class ExhaustiveDelayBoundingStrategy : DelayBoundingStrategy, ISchedulingStrategy
    {
        #region fields

        /// <summary>
        /// Cache of delays across iterations.
        /// </summary>
        private List<int> DelaysCache;

        #endregion

        #region public API

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="delays">Max number of delays</param>
        public ExhaustiveDelayBoundingStrategy(Configuration configuration, int delays)
            : base(configuration, delays)
        {
            this.DelaysCache = Enumerable.Repeat(0, base.MaxDelays).ToList();
        }

        /// <summary>
        /// Prepares for the next scheduling choice. This is invoked
        /// directly after a scheduling choice has been chosen, and
        /// can be used to invoke specialised post-choice actions.
        /// </summary>
        public override void PrepareForNextChoice() { }

        /// <summary>
        /// Prepares for the next scheduling iteration. This is invoked
        /// at the end of a scheduling iteration. It must return false
        /// if the scheduling strategy should stop exploring.
        /// </summary>
        /// <returns>True to start the next iteration</returns>
        public override bool PrepareForNextIteration()
        {
            base.MaxExploredSteps = Math.Max(base.MaxExploredSteps, base.ExploredSteps);
            base.ExploredSteps = 0;

            var bound = Math.Min(
                (base.IsFair() ? base.Configuration.MaxFairSchedulingSteps :
                base.Configuration.MaxUnfairSchedulingSteps),
                base.MaxExploredSteps);
            for (var idx = 0; idx < base.MaxDelays; idx++)
            {
                if (this.DelaysCache[idx] < bound)
                {
                    this.DelaysCache[idx] = this.DelaysCache[idx] + 1;
                    break;
                }

                this.DelaysCache[idx] = 0;
            }

            base.RemainingDelays.Clear();
            base.RemainingDelays.AddRange(this.DelaysCache);
            base.RemainingDelays.Sort();

            return true;
        }

        /// <summary>
        /// Resets the scheduling strategy. This is typically invoked by
        /// parent strategies to reset child strategies.
        /// </summary>
        public override void Reset()
        {
            this.DelaysCache = Enumerable.Repeat(0, base.MaxDelays).ToList();
            base.Reset();
        }

        /// <summary>
        /// Returns a textual description of the scheduling strategy.
        /// </summary>
        /// <returns>String</returns>
        public override string GetDescription()
        {
            var text = base.MaxDelays + "' delays, delays '[";
            for (int idx = 0; idx < this.DelaysCache.Count; idx++)
            {
                text += this.DelaysCache[idx];
                if (idx < this.DelaysCache.Count - 1)
                {
                    text += ", ";
                }
            }

            text += "]'.";
            return text;
        }

        #endregion
    }
}
