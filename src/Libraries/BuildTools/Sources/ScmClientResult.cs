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
using System.Runtime.InteropServices;

#endregion

namespace Flatcode.BuildTools
{
    /// <summary>
    /// Represents the result of a source code management client execution.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ScmClientResult
    {
        /// <summary>
        /// Represents the empty result of a source code management client execution.
        /// </summary>
        public static readonly ScmClientResult Empty = new ScmClientResult(String.Empty);

        #region Fields

        readonly String output;
        readonly String error;
        readonly Int32 exitCode;

        #endregion

        #region Constructors

        /// <summary>
        ///
        /// </summary>
        /// <param name="output"></param>
        public ScmClientResult(String output) :
            this(output, String.Empty, 0)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="output"></param>
        /// <param name="exitCode"></param>
        public ScmClientResult(String output, Int32 exitCode) :
            this(output, String.Empty, exitCode)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <param name="exitCode"></param>
        public ScmClientResult(String output, String error, Int32 exitCode)
        {
            // Argument validation
            if (output == null) {
                throw new ArgumentNullException(nameof(output));
            }

            if (error == null) {
                throw new ArgumentNullException(nameof(error));
            }

            // Instance initialization
            this.output = output;
            this.error = error;
            this.exitCode = exitCode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the standard error result.
        /// </summary>
        public String Error {
            get { return error; }
        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        public Int32 ExitCode {
            get { return exitCode; }
        }

        /// <summary>
        /// Gets the standard output result.
        /// </summary>
        public String Output {
            get { return output; }
        }

        #endregion
    }
}
