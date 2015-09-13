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

#endregion

namespace Flatcode.BuildTools
{
    /// <summary>
    /// Represents the base class for managed wrappers of source code management clients.
    /// </summary>
    public abstract class ScmClient
    {
        #region Fields

        readonly String workingDirectory;

        #endregion

        #region Constructors

        /// <summary>
        /// Provides default initialization logic for classes that derive from <see cref="ScmClient"/>.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        protected ScmClient(String workingDirectory)
        {
            // Argument validation
            if (workingDirectory == null) {
                throw new ArgumentNullException(nameof(workingDirectory));
            }

            if (!Directory.Exists(workingDirectory)) {
                throw new DirectoryNotFoundException("Couldn't find the specified path.");
            }

            // Default instance initialization
            this.workingDirectory = workingDirectory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// In derived classes, gets the name of the client executable.
        /// </summary>
        public abstract String ExecutableName { get; }

        /// <summary>
        /// In derived classes, gets the path of the client executable.
        /// </summary>
        public abstract String ExecutablePath { get; }

        /// <summary>
        /// Gets the working directory.
        /// </summary>
        public String WorkingDirectory {
            get { return workingDirectory; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a specified command on the client.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="ScmClientResult"/> that contains the execution result.</returns>
        public ScmClientResult Execute(String command)
        {
            // Validate argument
            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }

            // Validate client
            String clientPath = Path.Combine(ExecutablePath, ExecutableName);

            if (Directory.Exists(clientPath) && !File.Exists(clientPath)) {
                throw new ScmClientNotFoundException(clientPath, "The client exectuable could not be found.");
            }

            // Continue with implementation-specific execution logic
            return ExecuteCore(command);
        }

        /// <summary>
        /// In derived classes, implements the client execution logic.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected abstract ScmClientResult ExecuteCore(String command);

        /// <summary>
        /// Tries to execute a specified command on the client.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="result">The <see cref="ScmClientResult"/> variable that receives the result
        /// of the exection.</param>
        /// <returns>True if the execution was successful; otherwise, false.</returns>
        public Boolean TryExecute(String command, out ScmClientResult result)
        {
            try {
                result = Execute(command);
                return true;
            } catch {
                result = ScmClientResult.Empty;
                return false;
            }
        }

        #endregion

        #region Methods: Static

        /// <summary>
        /// Creates and initializes a <see cref="ScmClient"/> based on the kind of repository.
        /// </summary>
        /// <param name="path">The repository path.</param>
        /// <returns>An initialized <see cref="ScmClient"/> instance specific to the kind of the
        /// repository, or a null reference if <paramref name="path"/> is not a repository or the
        /// kind of repository is not supported.</returns>
        public static ScmClient Create(String path)
        {
            // Argument validation
            if (path == null) {
                throw new ArgumentNullException(nameof(path));
            }

            if (!Directory.Exists(path)) {
                throw new DirectoryNotFoundException("Couldn't find the specified path.");
            }

            // Create source code management client based on repository type
            ScmClient scmClient;
            ScmClientResult scmClientResult;

            // Create Git client and check if path is a Git repository
            scmClient = new GitClient(path);
            if (scmClient.TryExecute("rev-parse", out scmClientResult)) {
                if (scmClientResult.ExitCode == 0) {
                    return scmClient;
                }
            }

            return null;
        }

        #endregion
    }
}
