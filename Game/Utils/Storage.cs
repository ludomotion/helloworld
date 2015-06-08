using Phantom;
using Phantom.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HelloWorld.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Saved : Attribute
    {
        /// <summary>
        /// Alternate name, otherwise field is used.
        /// </summary>
        public string name;


        /// <summary>
        /// Default value, if not set null or default() is used.
        /// </summary>
        public object defaultValue;


        /// <summary>
        /// Version of this type, increasing it will nuke old version.
        /// </summary>
        public int version;

        internal FieldInfo field;
        internal Type type;

        public Saved()
        {
            version = 0;
        }
    }

    public class Storage : Component
    {
        [Saved]
        public static DateTime LastSaved;

        public string StateFile
        {
            get
            {
                return this.statefile;
            }
        }

        private List<Saved> saves;
        private Dictionary<Tuple<string, string>, Saved> savesMap;
        private string path;
        private string statefile;
        private bool didSaveForExit;
        private bool initialized;

        public Storage()
        {
            this.saves = new List<Saved>();
            this.savesMap = new Dictionary<Tuple<string, string>, Saved>();
        }

        public override void OnAdd(Component parent)
        {
            if (!(parent is PhantomGame))
                throw new InvalidOperationException("You must add Storage to a PhantomGame.");
            base.OnAdd(parent);

            this.path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.path = Path.Combine(this.path, PhantomGame.Game.Name);
            this.statefile = Path.Combine(this.path, "state.dat");

            this.FindSaveFields();

            this.initialized = true;
            this.LoadState();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnProcessExit;
        }

        public override void HandleMessage(Message message)
        {
            if (message == Messages.GameExit)
                this.OnProcessExit();
            base.HandleMessage(message);
        }

        /// <summary>
        /// Open a file for this game.
        /// </summary>
        /// <param name="savename">The name of the savefile (without and extension)</param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        public FileStream OpenSaveFile(string savename, FileMode mode, FileAccess access=FileAccess.ReadWrite)
        {
            return File.Open(Path.Combine(this.path, savename + ".sav"), mode, access);
        }

        /// <summary>
        /// Remove the state file for this game.
        /// </summary>
        public void DeleteState()
        {
            File.Delete(this.statefile);
        }

        /// <summary>
        /// Save the state of this game to disk.
        /// </summary>
        public void SaveState()
        {
            if (!this.initialized)
                throw new InvalidOperationException("Storage not yet Added to the game.");
            Directory.CreateDirectory(this.path);
            Storage.LastSaved = DateTime.Now;

            EntryList entries = new EntryList();
            foreach (Saved save in this.saves)
            {
                entries.Add(save);
            }

            if (File.Exists(this.statefile))
            {
                EntryList prev = RawRead();
                if (prev != null)
                {
                    entries = EntryList.Merge(prev, entries);
                }
            }
            Stream f = new GZipStream(new FileStream(this.statefile, FileMode.Create), CompressionLevel.Fastest);
            IFormatter formatter = this.CreateFormatter();
            formatter.Serialize(f, entries.Export());
            f.Close();
        }

        private void LoadState()
        {
            if (!this.initialized)
                throw new InvalidOperationException("Storage not yet Added to the game.");
            if (!File.Exists(this.statefile))
                return;
            EntryList entries = RawRead();
            if (entries != null)
            {
                int len = entries.Count;
                var map = new Dictionary<Tuple<string, string>, int>();

                // First iterate through all to find the latest version (for each key):
                for (int i = 0; i < len; i++)
                {
                    var key = entries[i].Key;
                    if (map.ContainsKey(key))
                    {
                        if (entries[map[key]].Version < entries[i].Version)
                            map[key] = i;
                    }
                    else
                    {
                        map.Add(key, i);
                    }
                }

                // Then set values based on the latest version:
                foreach (var pair in this.savesMap)
                {
                    if (map.ContainsKey(pair.Key))
                    {
                        Entry e = entries[map[pair.Key]];
                        if (e.Version == pair.Value.version)
                        {
                            pair.Value.field.SetValue(null, e.Value);
                        }
                        // TODO: else, try and call a convert method
                    }
                }
            }
        }

        private EntryList RawRead()
        {
            Stream f = new GZipStream(new FileStream(this.statefile, FileMode.Open), CompressionMode.Decompress);
            try
            {
                IFormatter formatter = this.CreateFormatter();
                object[,] raw = formatter.Deserialize(f) as object[,];
                if (raw == null)
                    return null;
                return EntryList.Import(raw);
            }
            catch (Exception e)
            {
                DateTime n = DateTime.Now;
                File.Move(this.statefile, this.statefile + ".failed-" + n.ToString("yyyyMMddHHmmss"));
                return null;
            }
            finally
            {
                f.Close();
            }
        }
       
        private void FindSaveFields()
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            Type[] types = Assembly.GetAssembly(typeof(Storage)).GetTypes();
            foreach (var type in types)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                foreach (var field in fields)
                {
                    Saved save = field.GetCustomAttribute<Saved>();
                    if (save != null)
                    {
                        save.type = type;
                        save.field = field;

                        this.RegisterField(save);

                    }
                }
            }
#if DEBUG
            sw.Stop();
            if (sw.ElapsedMilliseconds > 10)
            {
                // Dear future-self,
                // If this breakpoint hit, you might want to reconsider the above code.
                Debugger.Break();
            }
#endif
        }

        private void RegisterField(Saved save)
        {
            save.name = save.name ?? save.field.Name;

            this.saves.Add(save);

            var typeFieldPair = Tuple.Create<string, string>(save.type.FullName, save.name);
            this.savesMap.Add(typeFieldPair, save);

            if (save.defaultValue != null)
                save.field.SetValue(null, save.defaultValue);
        }

        private void OnProcessExit(object sender = null, EventArgs e = null)
        {
            if (this.didSaveForExit)
                return;
            if (e is UnhandledExceptionEventArgs)
            {
                if (!((UnhandledExceptionEventArgs)e).IsTerminating)
                    return;
                DateTime n = DateTime.Now;
                string backupfile = this.statefile + ".crashed-" + n.ToString("yyyyMMddHHmmss");
                File.Copy(this.statefile, backupfile);
            }
            this.didSaveForExit = true;
            this.SaveState();
        }

        private IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        private class Entry : IComparable<Entry>
        {
            public const int TypeIndex = 0;
            public const int FieldIndex = 1;
            public const int VersionIndex = 2;
            public const int ValueIndex = 3;

            public string Type;
            public string Field;
            public int Version;
            public object Value;

            public Tuple<string, string> Key
            {
                get
                {
                    return Tuple.Create<string, string>(Type, Field);
                }
            }

            public Tuple<string, string, int> VersionedKey
            {
                get
                {
                    return Tuple.Create<string, string, int>(Type, Field, Version);
                }
            }

            public Entry(string type, string field, int version, object value)
            {
                this.Type = type;
                this.Field = field;
                this.Version = version;
                this.Value = value;
            }

            public void Insert(int i, object[,] container)
            {
                container[i, (int)TypeIndex] = Type;
                container[i, (int)FieldIndex] = Field;
                container[i, (int)VersionIndex] = Version;
                container[i, (int)ValueIndex] = Value;
            }

            int IComparable<Entry>.CompareTo(Entry other)
            {
                return this.Version.CompareTo(other.Version);
            }
        }

        private class EntryList : List<Entry>
        {
            public void Add(Saved save)
            {
                this.Add(new Entry(
                    save.type.FullName,
                    save.name,
                    save.version,
                    save.field.GetValue(null)
                ));
            }

            public object[,] Export()
            {
                int i = 0;
                object[,] result = new object[this.Count, 4];
                foreach (Entry e in this)
                {
                    e.Insert(i, result);
                    i++;
                }
                return result;
            }

            public static EntryList Import(object[,] raw)
            {
                EntryList result = new EntryList();

                int len = raw.GetLength(0);
                for (int i = 0; i < len; i++)
                {
                    result.Add(new Entry(
                        (string)raw[i, Entry.TypeIndex],
                        (string)raw[i, Entry.FieldIndex],
                        (int)raw[i, Entry.VersionIndex],
                        raw[i, Entry.ValueIndex]
                    ));
                }

                return result;
            }

            public static EntryList Merge(EntryList a, EntryList b)
            {
                var uniq = new Dictionary<Tuple<string, string, int>, Entry>();

                foreach (var e in a)
                {
                    uniq[e.VersionedKey] = e;
                }
                foreach (var e in b)
                {
                    uniq[e.VersionedKey] = e;
                }

                EntryList result = new EntryList();
                result.AddRange(uniq.Values);
                return result;
            }
        }
    }
}
