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
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace Flatcode.BuildTools.Tasks
{
    /// <summary>
    /// Generates versioning information for a managed assembly.
    /// </summary>
    public sealed class GenerateVersioningInfo : Task
    {
        #region Constants

        /// <summary>
        /// Defines the default value for <see cref="OutputName"/>.
        /// </summary>
        public const String DefaultOutputName = "VersioningInfo";

        #endregion

        #region Fields

        SourceLanguage language;
        String outputName;
        String outputPath;
        Boolean overrideExisiting;
        String versioningFile;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateVersioningInfo"/> class.
        /// </summary>
        public GenerateVersioningInfo()
        {
            // Default instance initialization
            language = SourceLanguage.CSharp;
            outputName = DefaultOutputName;
            outputPath = null;
            overrideExisiting = true;
            versioningFile = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the language for the versioning information file.
        /// </summary>
        public String Language {
            get { return language.ToString(); }
            set {
                SourceLanguage languageValue;
                if (Enum.TryParse<SourceLanguage>(value, out languageValue)) {
                    language = languageValue;
                } else {
                    throw new ArgumentException("The specified language is not supported by this task.",
                                                nameof(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the output name for the versioning information file.
        /// </summary>
        public String OutputName {
            get { return outputName; }
            set { outputName = value; }
        }

        /// <summary>
        /// Gets or sets the output path for the versioning information file.
        /// </summary>
        [Required]
        public String OutputPath {
            get { return outputPath; }
            set { outputPath = value; }
        }

        /// <summary>
        /// Gets or sets whether an exisiting versioning information file should be overridden.
        /// </summary>
        public Boolean OverrideExisiting {
            get { return overrideExisiting; }
            set { overrideExisiting = value; }
        }

        /// <summary>
        /// Gets or sets the source versioning file name.
        /// </summary>
        [Required]
        public String VersioningFile {
            get { return versioningFile; }
            set { versioningFile = value; }
        }

        #endregion

        #region Methods: Implementation

        void EmitCSharpAttributes(StreamWriter w, String baseVersion, String fileVersion, String infoVersion)
        {
            w.WriteLine("// Auto-generated. Do not edit manually.");
            w.WriteLine($"[assembly:System.Reflection.AssemblyVersionAttribute(\"{baseVersion}\")]");
            w.WriteLine($"[assembly:System.Reflection.AssemblyFileVersionAttribute(\"{fileVersion}\")]");
            w.WriteLine($"[assembly:System.Reflection.AssemblyInformationalVersionAttribute(\"{infoVersion}\")]");
        }

        void EmitVisualBasicAttributes(StreamWriter w, String baseVersion, String fileVersion, String infoVersion)
        {
            w.WriteLine("' Auto-generated. Do not edit manually.");
            w.WriteLine($"<Assembly:System.Reflection.AssemblyVersionAttribute(\"{baseVersion}\")>");
            w.WriteLine($"<Assembly:System.Reflection.AssemblyFileVersionAttribute(\"{fileVersion}\")>");
            w.WriteLine($"<Assembly:System.Reflection.AssemblyInformationalVersionAttribute(\"{infoVersion}\")>");
        }

        Boolean ExecuteCore()
        {
            // Validate properties
            if (Directory.Exists(VersioningFile) || !File.Exists(VersioningFile)) {
                throw new FileNotFoundException("Couldn't find versioning file.", VersioningFile);
            }

            // Initialize language-specific output file name
            String outputFile;

            switch (language) {
                case SourceLanguage.VisualBasic:
                    outputFile = $"{Path.Combine(OutputPath, OutputName)}.vb";
                    break;
                case SourceLanguage.CSharp:
                default:
                    outputFile = $"{Path.Combine(OutputPath, OutputName)}.cs";
                    break;
            }

            // Check if the output file already exisits
            if (File.Exists(outputFile)) {
                // Determine if an exisiting file should be overridden
                if (OverrideExisiting) {
                    File.Delete(outputFile);
                } else {
                    Log.LogMessage("Versioning information file already exists.");
                    return true;
                }
            }

            // Create and load XML document to load the versioning file
            XmlDocument versionXml = new XmlDocument();
            versionXml.Load(VersioningFile);

            Int32 major = 0;
            Int32 minor = 0;
            Int32 patch = 0;

            // Get static versioning info from versioning file
            XmlElement versionElement = versionXml["Version"];
            Int32.TryParse(versionElement["Major"].InnerText, out major);
            Int32.TryParse(versionElement["Minor"].InnerText, out minor);
            Int32.TryParse(versionElement["Patch"].InnerText, out patch);
            String tag = versionElement["Tag"].InnerText;

            Int32 commitCount = 0;
            String commitHash = String.Empty;
            String branchName = String.Empty;

            // Get dynamic versioning info from source code management
            var scmClient = ScmClient.Create(Path.GetDirectoryName(VersioningFile));

            if (scmClient != null) {
                ScmClientResult scmClientResult;
                if (scmClient is GitClient) {
                    // Get commit count
                    scmClientResult = scmClient.Execute("rev-list HEAD --count");
                    Int32.TryParse(scmClientResult.Output, out commitCount);
                    // Get commit hash
                    scmClientResult = scmClient.Execute("rev-parse HEAD");
                    commitHash = scmClientResult.Output.TrimEnd(' ', '\n');
                    if (commitHash.Length > 10) {
                        // Trim hash to ten characters, if necessary
                        commitHash = commitHash.Substring(0, 10);
                    }
                    // Get branch name
                    scmClientResult = scmClient.Execute("rev-parse --abbrev-ref HEAD");
                    branchName = scmClientResult.Output.TrimEnd(' ', '\n');
                }
            }

            // Create and format versioning strings
            String baseVersion = $"{major}.{minor}.{patch}.0";
            String fileVersion = $"{major}.{minor}.{patch}.{commitCount}";

            StringBuilder infoVersionBuilder = new StringBuilder($"{major}.{minor}.{patch}");

            if (!String.IsNullOrWhiteSpace(tag)) {
                infoVersionBuilder.Append($"-{tag}");
            }
            if (!String.IsNullOrWhiteSpace(branchName)) {
                infoVersionBuilder.Append($"+{branchName}");
                if (!String.IsNullOrWhiteSpace(commitHash)) {
                    infoVersionBuilder.Append($".{commitHash}");
                }
            }

            String infoVersion = infoVersionBuilder.ToString();

            // Create new output file if it does not exists or should be overridden
            using (FileStream outStream = File.OpenWrite(outputFile)) {
                using (StreamWriter writer = new StreamWriter(outStream)) {
                    switch (language) {
                        case SourceLanguage.VisualBasic:
                            EmitVisualBasicAttributes(writer, baseVersion, fileVersion, infoVersion);
                            break;
                        case SourceLanguage.CSharp:
                        default:
                            EmitCSharpAttributes(writer, baseVersion, fileVersion, infoVersion);
                            break;
                    }

                    writer.Flush();
                }
            }

            Log.LogMessage("Versioning information file generated successfully.");
            return true;
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
                Log.LogMessage("Generating versioning information file...");
                return ExecuteCore();
            } catch (Exception e) {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        #endregion
    }
}
