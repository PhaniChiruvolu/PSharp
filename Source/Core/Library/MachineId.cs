﻿//-----------------------------------------------------------------------
// <copyright file="MachineId.cs">
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
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.PSharp
{
    /// <summary>
    /// Unique machine id.
    /// </summary>
    [DataContract]
    public sealed class MachineId
    {
        #region fields

        /// <summary>
        /// The P# runtime that executes the machine with this id.
        /// </summary>
        public readonly PSharpRuntime Runtime;

        /// <summary>
        /// Name of the machine.
        /// </summary>
        [DataMember]
        public readonly string Name;

        /// <summary>
        /// Optional friendly name of the machine.
        /// </summary>
        [DataMember]
        private readonly string FriendlyName;

        /// <summary>
        /// Type of the machine with this id.
        /// </summary>
        [DataMember]
        public readonly string Type;

        /// <summary>
        /// Unique id value.
        /// </summary>
        [DataMember]
        internal readonly ulong Value;

        /// <summary>
        /// Endpoint.
        /// </summary>
        [DataMember]
        public readonly string Endpoint;

        #endregion

        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Machine type</param>
        /// <param name="friendlyName">Friendly machine name</param>
        /// <param name="runtime">PSharpRuntime</param>
        internal MachineId(Type type, string friendlyName, PSharpRuntime runtime)
        {
            this.FriendlyName = friendlyName;
            this.Runtime = runtime;

            this.Type = type.FullName;
            this.Endpoint = this.Runtime.NetworkProvider.GetLocalEndpoint();
            
            // Atomically increments and safely wraps into an unsigned long.
            this.Value = (uint)Interlocked.Increment(ref runtime.MachineIdCounter);

            // Checks for overflow.
            Runtime.Assert(this.Value != ulong.MaxValue, "Detected MachineId overflow.");

            if (this.FriendlyName != null && this.FriendlyName.Length > 0)
            {
                this.Name = string.Format("{0}({1})", this.FriendlyName, this.Value);
            }
            else
            {
                this.Name = string.Format("{0}({1})", this.Type, this.Value);
            }
        }

        /// <summary>
        /// Constructor. This is only used to partially reconstruct
        /// a machine id from a given machine type and id value.
        /// </summary>
        /// <param name="type">Machine type</param>
        /// <param name="value">Id value</param>
        internal MachineId(string type, ulong value)
        {
            this.Type = type;
            this.Value = value;
        }

        #endregion

        #region generic public and override methods
        
        /// <summary>
        /// Determines whether the specified System.Object is equal
        /// to the current System.Object.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            MachineId mid = obj as MachineId;
            if (mid == null)
            {
                return false;
            }

            return this.Value == mid.Value;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current machine id.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}
