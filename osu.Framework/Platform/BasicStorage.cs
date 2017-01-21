// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.IO.File;
using SQLite.Net;
using SQLite.Net.Interop;

namespace osu.Framework.Platform
{
    public abstract class BasicStorage
    {
        public string BaseName { get; set; }

        protected BasicStorage(string baseName)
        {
            BaseName = FileSafety.FilenameStrip(baseName);
        }

        public abstract bool Exists(string path);

        public abstract void Delete(string path);

        public abstract Stream GetStream(string path, FileAccess mode = FileAccess.Read);

        public abstract SQLiteConnection GetDatabase(string name, SQLiteOpenFlags openFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create,
            bool storeDateTimeAsTicks = true, IBlobSerializer serializer = null,
            IDictionary<string, TableMapping> tableMappings = null,
            IDictionary<Type, string> extraTypeMappings = null, IContractResolver resolver = null);

        public abstract void OpenInNativeExplorer();
    }
}