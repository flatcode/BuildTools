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
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

#endregion

namespace Flatcode.BuildTools
{
    /// <summary>
    /// Provides a managed wrapper for the native Git command-line client.
    /// </summary>
    public sealed class GitClient : ScmClient
    {
        #region Fields

        static readonly Lazy<String> executableName = new Lazy<String>(GetExecutableName);
        static readonly Lazy<String> executablePath = new Lazy<String>(GetExecutablePath);

        #endregion

        #region Constructors

        internal GitClient(String workingDirectory) : base(workingDirectory)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the Git client executable.
        /// </summary>
        public override String ExecutableName {
            get { return executableName.Value; }
        }

        /// <summary>
        /// Gets the path of the Git client executable.
        /// </summary>
        public override String ExecutablePath {
            get { return executablePath.Value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a specified command on the Git client.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="ScmClientResult"/> that contains the execution result.</returns>
        protected override ScmClientResult ExecuteCore(String command)
        {
            // Create process start information
            ProcessStartInfo psi = new ProcessStartInfo() {
                Arguments = command,
                CreateNoWindow = true,
                FileName = Path.Combine(ExecutablePath, ExecutableName),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory
            };

            ScmClientResult result = ScmClientResult.Empty;

            // Create Git client process and run it
            using (Process p = Process.Start(psi)) {
                p.WaitForExit();
                result = new ScmClientResult(p.StandardOutput.ReadToEnd(),
                                             p.StandardError.ReadToEnd(),
                                             p.ExitCode);
            }

            return result;
        }

        #endregion

        #region Methods: Implementation

        static String GetExecutableName()
        {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    return "git.exe";
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return "git";
                default:
                    // Return the empty string on non-supported platforms
                    return String.Empty;
            }
        }

        static String GetExecutablePath()
        {
            // Check the PATH first
            String pathVar = Environment.GetEnvironmentVariable("PATH");
            String exeName = executableName.Value;

            if (pathVar != null) {
                foreach (String path in pathVar.Split(';')) {
                    if (!Directory.Exists(Path.Combine(path, exeName)) &&
                        File.Exists(Path.Combine(path, exeName))) {
                        return path;
                    }
                }
            }

            // On Windows, check for official Git client installation
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                String keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Git_is1";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName)) {
                    String installLocation = key.GetValue("InstallLocation") as String;
                    if (installLocation != null) {
                        if (!Directory.Exists(Path.Combine(installLocation, exeName)) &&
                            File.Exists(Path.Combine(installLocation, exeName))) {
                            return installLocation;
                        }
                    }
                }
            }

            // Return the empty string if no path could be found
            return String.Empty;
        }

        #endregion
    }
}
