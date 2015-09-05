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

#endregion

namespace Flatcode.BuildTools
{
    /// <summary>
    /// The exception that is thrown when an attempt to execute a source code management client
    /// that is not present or installed fails.
    /// </summary>
    public class ScmClientNotFoundException : ApplicationException
    {
        #region Fields

        readonly String path;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScmClientNotFoundException"/> class.
        /// </summary>
        /// <param name="path">The path to the source code management client that is the cause of
        /// the current exception.</param>
        /// <param name="message">An optional exception message.</param>
        /// <param name="innerException">An optional inner exception.</param>
        public ScmClientNotFoundException(String path = null, String message = null, Exception innerException = null) :
            base(message, innerException)
        {
            // Instance initialization
            this.path = path;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the path to the source code management client that is the cause of the current
        /// exception.
        /// </summary>
        public String Path {
            get { return path; }
        }

        #endregion
    }
}
