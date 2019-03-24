﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VeeamTestAssignment {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WeamTestAssignment.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input file is not found at the specified path.
        /// </summary>
        internal static string Error_InputFileIsNotFound {
            get {
                return ResourceManager.GetString("Error_InputFileIsNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chunk size should be an integer value between 1 and 2^31-1 bytes.
        /// </summary>
        internal static string Error_InvalidChunkSizeValue {
            get {
                return ResourceManager.GetString("Error_InvalidChunkSizeValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Path to input file is unrecognized or invalid..
        /// </summary>
        internal static string Error_InvalidInputFilePath {
            get {
                return ResourceManager.GetString("Error_InvalidInputFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Path to output file is unrecognized or invalid..
        /// </summary>
        internal static string Error_InvalidOutputFilePath {
            get {
                return ResourceManager.GetString("Error_InvalidOutputFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid program arguments.
        /// </summary>
        internal static string Error_InvalidProgramArguments {
            get {
                return ResourceManager.GetString("Error_InvalidProgramArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workers count should be and integer value &gt;= 1.
        /// </summary>
        internal static string Error_InvalidWorkersCount {
            get {
                return ResourceManager.GetString("Error_InvalidWorkersCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t access to input file. Input file is exclusively open in another application..
        /// </summary>
        internal static string Error_NoAccessToInputFile {
            get {
                return ResourceManager.GetString("Error_NoAccessToInputFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cant&apos;t create or replace output file. .
        /// </summary>
        internal static string Error_NoAccessToOutputPath {
            get {
                return ResourceManager.GetString("Error_NoAccessToOutputPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This application has no permission to access input file..
        /// </summary>
        internal static string Error_NoPermissionToAccessInputFile {
            get {
                return ResourceManager.GetString("Error_NoPermissionToAccessInputFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string Error_NoPermissionToAccessOutputFile {
            get {
                return ResourceManager.GetString("Error_NoPermissionToAccessOutputFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unrecognized command. Only &quot;compress&quot; &amp; decompress values are acceptable.
        /// </summary>
        internal static string Error_UnrecognizedCommand {
            get {
                return ResourceManager.GetString("Error_UnrecognizedCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unrecognized parameter, currently only &apos;chunkSize&apos; and &apos;workers&apos; parameters are supported.
        /// </summary>
        internal static string Error_UnrecognizedParameter {
            get {
                return ResourceManager.GetString("Error_UnrecognizedParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to usage: GZipTest.exe compress|decompress &lt;inputPath&gt; &lt;outputPath&gt; [chunkSize &lt;size&gt;] [workers &lt;count&gt;]
        ///    compress - required. Archiver mode, &apos;compress&apos; for file archiving, &apos;decompress&apos; for file unarhiving.
        ///    inputPath - required. Source file to be compressed\decompressed
        ///    outputPath - required. Path where compression\decompression result should be saved
        ///    chunkSize &lt;size&gt; - optional. Chunk size in bytes. Default value is 1048576 bytes (1MB)
        ///    workers &lt;count&gt; - count of workers to use during c [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Info_HelpUsage {
            get {
                return ResourceManager.GetString("Info_HelpUsage", resourceCulture);
            }
        }
    }
}
