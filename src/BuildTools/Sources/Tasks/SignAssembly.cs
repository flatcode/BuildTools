/***************************************************************************************************
 *
 *  Copyright © 2015 Flatcode.net Developer Community
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 *  and associated documentation files (the "Software"), to deal in the Software without
 *  restriction, including without limitation the rights to use, copy, modify, merge, publish,
 *  distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all copies or
 *  substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 *  BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 *  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 *  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 **************************************************************************************************/

#region Using Directives

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace Flatcode.BuildTools.Tasks
{
    /// <summary>
    /// Strong name signs a delay-signed assembly.
    /// </summary>
    public sealed class SignAssembly : Task
    {
        #region Fields

        String assemblyFile;
        String keyFile;
        Boolean recomputeHashes;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAssembly" /> class.
        /// </summary>
        public SignAssembly()
        {
            // Default instance initialization
            assemblyFile = null;
            keyFile = null;
            recomputeHashes = false;
        }

        #endregion

        #region Properties

        /// <summary>
        ///
        /// </summary>
        [Required]
        public String AssemblyFile {
            get { return assemblyFile; }
            set { assemblyFile = value; }
        }

        /// <summary>
        ///
        /// </summary>
        [Required]
        public String KeyFile {
            get { return keyFile; }
            set { keyFile = value; }
        }

        /// <summary>
        ///
        /// </summary>
        public Boolean RecomputeHashes {
            get { return recomputeHashes; }
            set { recomputeHashes = value; }
        }

        #endregion

        #region Methods: Overridden

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>True if the task executed successfully; otherwise, false.</returns>
        public override Boolean Execute()
        {
            try {
                return ExecuteCore();
            } catch (Exception e) {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        #endregion

        #region Implementation

        Boolean ExecuteCore()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
