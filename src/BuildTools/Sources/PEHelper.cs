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
    /// Provides static helper methods for working with PE images.
    /// </summary>
    public static class PEHelper
    {
        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="peStream"></param>
        /// <param name="revoke"></param>
        public static void FakeStrongNameSignature(Stream peStream, Boolean revoke)
        {
            // Argument validation
            if (peStream == null) {
                throw new ArgumentNullException(nameof(peStream));
            }

            if (!peStream.CanRead || !peStream.CanWrite || !peStream.CanSeek) {
                throw new ArgumentException("The stream must be read-/write- and seekable.", nameof(peStream));
            }

            if (peStream.Length < 512) {
                throw new ArgumentException("The stream has an invalid size.", nameof(peStream));
            }

            // Ensure the stream is positioned at the beginning
            peStream.Position = 0;

            // Create binary reader
            BinaryReader peReader = new BinaryReader(peStream);

            // Verify the DOS signature
            if (peReader.ReadUInt16() != 0x5a4d /* "MZ" */) {
                // Missing or invalid DOS signature
                throw new BadImageFormatException("The image is missing or has an invalid DOS signature.");
            }

            // Get the PE file header offset (stored at 0x3c) and jump to it
            peStream.Position = 0x3c;
            peStream.Position = peReader.ReadUInt32();

            // Verify the PE signature
            if (peReader.ReadUInt32() != 0x00004550 /* "PE\0\0" */) {
                // Missing or invalid PE signature
                throw new BadImageFormatException("The image is missing or has an invalid PE signature.");
            }

            // Verify the Machine field
            if (peReader.ReadUInt16() != 0x014c /* I386 */) {
                // Machine is always 0x014c (I386) on managed assemblies
                throw new BadImageFormatException("The image has an invalid target CPU type.");
            }

            // Read the number of sections
            UInt16 sectionCount = peReader.ReadUInt16();
            peStream.Position += 12; // Skip fields

            // Read and verify the optional header size
            UInt16 optionalHeaderSize = peReader.ReadUInt16();
            if (optionalHeaderSize != 0xe0) {
                // Header size must be 224 (0xe0)
                throw new BadImageFormatException("The image has an invalid optional header size.");
            }

            peStream.Position += 2; // Skip the rest of the file header

            // Get and verify the CLI header RVA and size
            peStream.Position += (optionalHeaderSize - 16);
            UInt32 cliHeaderRva = peReader.ReadUInt32();
            UInt32 cliHeaderSize = peReader.ReadUInt32();

            if (cliHeaderRva == 0 || cliHeaderSize == 0) {
                // No CLI header RVA or size indicates the image is not a managed assembly
                throw new BadImageFormatException("The image is not a managed assembly.");
            }

            peStream.Position += 8; // Skip the rest of the optional header

            // Get the text section RVA and data pointer
            UInt32 textRva = 0;
            UInt32 textDataPtr = 0;

            for (UInt32 i = 0; i < sectionCount; i++) {
                UInt64 sectionName = peReader.ReadUInt64();
                if (sectionName != 0x000000747865742e /* ".text\0\0\0" */) {
                    peStream.Position += 32;
                    continue;
                } else {
                    peStream.Position += 4; // Skip virtual size
                    textRva = peReader.ReadUInt32();
                    peStream.Position += 4; // Skip data size
                    textDataPtr = peReader.ReadUInt32();
                    break;
                }
            }

            // Verify text section RVA and data pointer
            if (textRva == 0 || textDataPtr == 0) {
                throw new BadImageFormatException("The image is missing or has an invalid code section.");
            }
            if (textRva > cliHeaderRva) {
                throw new BadImageFormatException("The image has an invalid code section address.");
            }

            // Calculate the CLI header address
            UInt32 cliHeaderOffset = cliHeaderRva - textRva;
            UInt32 cliHeaderPtr = textDataPtr + cliHeaderOffset;

            // Jump to the CLI header
            peStream.Position = cliHeaderPtr;

            // Verify CLI header size and runtime version
            if (peReader.ReadUInt32() != cliHeaderSize) {
                // CLI header size must be 0x48
                throw new BadImageFormatException("The image has an invalid CLI header.");
            }
            if (peReader.ReadUInt16() != 2) {
                // Major CLR runtime version must be 2
                throw new BadImageFormatException("The image CLR runtime version is not supported.");
            }

            // Define offsets
            UInt32 flagsPtr = cliHeaderPtr + 16; // CLI header flags
            UInt32 snRvaPtr = cliHeaderPtr + 32; // CLI header strong name RVA

            // Jump to and read the CLI header flags
            peStream.Position = flagsPtr;
            UInt32 flags = peReader.ReadUInt32();

            // Check flags
            UInt32 currentFlag = flags & 0x08;

            if (revoke) {
                // Cannot revoke strong-name signature from unsigned assembly
                if (currentFlag != 0x08) {
                    throw new InvalidOperationException("The image has no strong-name signature.");
                }
            } else {
                // Cannot fake sign an already strong-name signed assembly
                if (currentFlag != 0x00) {
                    throw new InvalidOperationException("The image already has a strong-name signature.");
                }
            }

            // Check if a signature is present
            peStream.Position = snRvaPtr;
            if (peReader.ReadUInt64() == 0) {
                throw new InvalidOperationException("The image is not a delay-signed assembly.");
            }

            // Flip signature flag
            BinaryWriter peWriter = new BinaryWriter(peStream);

            // Re-position the stream to flags
            peStream.Position = flagsPtr;

            // Modify old flags
            if (revoke) {
                flags &= unchecked((UInt32)~0x00000008);
            } else {
                flags |= 0x00000008;
            }

            // Write new flags
            peWriter.Write(flags);
        }

        #endregion
    }
}
