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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace Flatcode.BuildTools.Tasks
{
    /// <summary>
    /// Fake signs a delay-signed assembly by flipping the strong name signed bit in its CLI header.
    /// </summary>
    public sealed class FakeSign : Task
    {
        #region Fields

        String assemblyFile;
        Boolean revoke;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeSign" /> class.
        /// </summary>
        public FakeSign()
        {
            // Default instance initialization
            assemblyFile = null;
            revoke = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the assembly file to be fake signed.
        /// </summary>
        [Required]
        public String AssemblyFile {
            get { return assemblyFile; }
            set { assemblyFile = value; }
        }

        /// <summary>
        /// Gets or sets a value determining whether an applied fake signature should be revoked.
        /// </summary>
        public Boolean Revoke {
            get { return revoke; }
            set { revoke = value; }
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
            // Log startup message
            if (!Revoke) {
                Log.LogMessage("Attempting to fake sign assembly '{0}'...",
                               AssemblyFile);
            } else {
                Log.LogMessage("Attempting to revoke fake signature from assembly '{0}'...",
                               AssemblyFile);
            }

            // Property validation
            if (!File.Exists(AssemblyFile)) {
                throw new FileNotFoundException("Couldn't find assembly file.", AssemblyFile);
            }

            // Open file stream to patch the assembly
            using (FileStream peStream = File.Open(AssemblyFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)) {
                PEHelper.FakeStrongNameSignature(peStream, Revoke);
                peStream.Flush(true);
            }

            // Log success message
            Log.LogMessage("Success.");
            return true;
        }

        #endregion
    }
}
