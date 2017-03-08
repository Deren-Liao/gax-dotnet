/*
 * Copyright 2017 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using ProtoWellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Google.Api.Gax
{
    public class SourceContext
    {
        private const string SourceContextFileName = "source-context.json";
        private ProtoWellKnownTypes.Struct _sourceContext => FindSourceContext();
        private ProtoWellKnownTypes.Struct _git => _sourceContext?.Fields["git"]?.StructValue;
        private readonly static Lazy<SourceContext> s_instance = new Lazy<SourceContext>(() => new SourceContext());

        /// <summary>
        /// Best way to get application folder path.
        /// http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path
        /// </summary>
        /// <exception cref="AppDomainUnloadedException">Possibly threw by getting base directory.</exception>
        private static string SourceContextFilePath => 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SourceContextFileName);

        private SourceContext() { }

        public string GitSha => _git?.Fields["revisionId"].StringValue;

        public string GitRepoUrl => _sourceContext?.Fields["url"].StringValue;

        public static SourceContext Current => s_instance.Value;

        private static ProtoWellKnownTypes.Struct FindSourceContext()
        {
            string sourceContext = ReadSourceContextFile();
            if (sourceContext == null)
            {
                return null;
            }
            try
            {
                return JsonParser.Default.Parse<ProtoWellKnownTypes.Struct>(sourceContext);
            }
            catch (Exception ex) when (IsProtobufParserException(ex))
            {
                return null;
            }
        }


        /// <summary>
        /// Find source context file and open the content.
        /// </summary>
        private static string ReadSourceContextFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(SourceContextFilePath)))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex) when (IsIOException(ex))
            {
                return null;
            }
        }

        private static bool IsIOException(Exception ex)
        {
            return ex is FileNotFoundException
                || ex is DirectoryNotFoundException
                || ex is IOException;
        }

        private static bool IsProtobufParserException(Exception ex)
        {
            return ex is InvalidProtocolBufferException
                || ex is InvalidJsonException;
        }
    }
}
