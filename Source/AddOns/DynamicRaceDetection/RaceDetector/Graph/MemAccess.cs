﻿//-----------------------------------------------------------------------
// <copyright file="MemAccess.cs">
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

namespace Microsoft.PSharp.DynamicRaceDetection
{
    internal class MemAccess : Node
    {
        public bool IsWrite;
        public UIntPtr Location;
        public UIntPtr ObjHandle;
        public UIntPtr Offset;
        public string SrcLocation;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MemAccess(bool isWrite, UIntPtr location, UIntPtr objHandle,
            UIntPtr offset, string srcLocation, int machineId)
        {
            this.IsWrite = isWrite;
            this.Location = location;
            this.ObjHandle = objHandle;
            this.Offset = offset;
            this.SrcLocation = srcLocation;
            this.MachineId = machineId;
        }
    }
}
