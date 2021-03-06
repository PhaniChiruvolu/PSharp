﻿//-----------------------------------------------------------------------
// <copyright file="TypeNameQualifier.cs">
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

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Microsoft.PSharp.LanguageServices.Syntax;

namespace Microsoft.PSharp.LanguageServices.Rewriting.PSharp
{
    /// <summary>
    /// Utility class to qualify type names, for typeof(Type) or Goto&lt;Type&gt;().
    /// </summary>
    class TypeNameQualifier
    {
        #region fields

        /// <summary>
        /// Set of all qualified state names in the current machine.
        /// </summary>
        internal HashSet<string> CurrentAllQualifiedStateNames = new HashSet<string>();

        /// <summary>
        /// Qualified state name corresponding to the procedure
        /// currently being rewritten.
        /// </summary>
        internal List<string> CurrentQualifiedStateName = new List<string>();

        /// <summary>
        /// Set of rewritten qualified methods.
        /// </summary>
        internal HashSet<QualifiedMethod> RewrittenQualifiedMethods = new HashSet<QualifiedMethod>();

        #endregion

        #region internal API

        /// <summary>
        /// Returns a fully-qualified name for the type inside the syntax node.
        /// </summary>
        /// <param name="typeUsed">Identifier of the type to rewrite</param>
        /// <param name="succeeded">Whether the fully qualified name was found</param>
        /// <returns>The fully qualified name</returns>
        internal string GetQualifiedName(TypeSyntax typeUsed, out bool succeeded)
        {
            var typeName = typeUsed.ToString();
            var fullyQualifiedName = this.GetFullyQualifiedStateName(typeName);
            succeeded = fullyQualifiedName != typeUsed.ToString();
            return fullyQualifiedName;
        }

        internal bool InitializeForNode(SyntaxNode node)
        {
            this.CurrentAllQualifiedStateNames.Clear();
            this.CurrentQualifiedStateName.Clear();

            // Gets containing method.
            var methoddecl = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methoddecl == null)
            {
                return false;
            }

            // Gets containing class.
            var classdecl = methoddecl.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classdecl == null)
            {
                return false;
            }

            // Gets containing namespace.
            var namespacedecl = classdecl.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespacedecl == null)
            {
                return false;
            }

            var rewrittenMethods = this.RewrittenQualifiedMethods.Where(
                val => val.Name.Equals(methoddecl.Identifier.ValueText) &&
                val.MachineName.Equals(classdecl.Identifier.ValueText) &&
                val.NamespaceName.Equals(namespacedecl.Name.ToString())).ToList();

            // Must be unique. 
            if (rewrittenMethods.Count == 0)
            {
                return false;
            }

            if (rewrittenMethods.Count > 1)
            {
                throw new RewritingException(
                    string.Format("Multiple definitions of the same method {0} in namespace {1}, machine {2}",
                      methoddecl.Identifier.ValueText,
                      namespacedecl.Name.ToString(),
                      classdecl.Identifier.ValueText)
                      );
            }

            var rewrittenMethod = rewrittenMethods.First();

            this.CurrentAllQualifiedStateNames.UnionWith(rewrittenMethod.MachineQualifiedStateNames);
            this.CurrentQualifiedStateName.AddRange(rewrittenMethod.QualifiedStateName);
            return true;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Given a partially-qualified state name, return the
        /// fully qualified state name.
        /// </summary>
        /// <param name="state">Partially qualified state name</param>
        /// <returns>Fully qualified state name</returns>
        private string GetFullyQualifiedStateName(string state)
        {
            if (this.CurrentQualifiedStateName.Count < 1 ||
                CurrentAllQualifiedStateNames.Count == 0)
            {
                return state;
            }

            for (int i = this.CurrentQualifiedStateName.Count - 2; i >= 0; i--)
            {
                var prefix = this.CurrentQualifiedStateName[0];
                for (int j = 1; j <= i; j++)
                {
                    prefix += "." + this.CurrentQualifiedStateName[j];
                }

                if (this.CurrentAllQualifiedStateNames.Contains(prefix + "." + state))
                {
                    return prefix + "." + state;
                }
            }

            return state;
        }

        /// <summary>
        /// Tokenizes a qualified name.
        /// </summary>
        /// <param name="state">Qualified name</param>
        /// <returns>Tokenized name</returns>
        private List<string> ToTokens(string state)
        {
            return state.Split('.').ToList();
        }

        /// <summary>
        /// Collapses a tokenized qualified name.
        /// </summary>
        /// <param name="state">Tokenized qualified name</param>
        /// <returns>Qualified name</returns>
        private string FromTokens(List<string> state)
        {
            return state.Aggregate("", (acc, name) => acc == "" ? name : acc + "." + name);
        }

        #endregion
    }
}
