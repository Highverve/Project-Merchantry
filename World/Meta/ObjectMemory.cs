using Merchantry.Culture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectMemory
    {
        public const string HasItem = "HasItem";
        public const string Crafted = "Crafted";

        private Dictionary<MemoryTags, MemoryPacket> memories = new Dictionary<MemoryTags, MemoryPacket>();

        private float counter = 0;
        public void Update(GameTime gt)
        {
            counter += (float)gt.ElapsedGameTime.TotalMilliseconds;

            if (counter >= 1000)
            {
                foreach (MemoryPacket memory in memories.Values)
                {
                    if (memory.IsLongterm == false)
                    {
                        memory.LifeTime -= 1;

                        if (memory.LifeTime < 0)
                            GameManager.References().World.Queue.Add(0, () => memories.Remove(memory.Key));
                    }
                }

                counter -= 1000;
            }
        }

        public void AddMemory(object memory, params string[] tags)
        {
            AddMemory(new MemoryTags(tags), memory);
        }
        public void AddMemory(MemoryTags key, object memory)
        {
            AddMemory(new MemoryPacket(key, memory, 0, true));
        }
        public void AddMemory(MemoryTags key, object memory, int lifetime)
        {
            AddMemory(new MemoryPacket(key, memory, lifetime, false));
        }
        public void AddMemory(MemoryPacket memory)
        {
            if (memory != null)
            {
                memory.DateCreated = GameManager.References().Calendar.Now();

                if (!memories.ContainsKey(memory.Key))
                    memories.Add(memory.Key, memory);
                else
                {
                    memories[memory.Key] = memory;
                    memories[memory.Key].RenewTime();
                }
            }
        }
        public void RemoveMemory(MemoryTags key)
        {
            if (memories.ContainsKey(key))
                memories.Remove(key);
        }

        public bool Contains(MemoryTags key)
        {
            return memories.ContainsKey(key);
        }
        public bool Contains(params string[] tags)
        {
            return memories.ContainsKey(new MemoryTags(tags));
        }
        public bool ContainsAny(params string[] key)
        {
            List<KeyValuePair<MemoryTags, MemoryPacket>> temp = memories.Where(m => m.Key.ContainsAny(key)).ToList();
            return temp.Count > 0;
        }

        public MemoryPacket GetMemory(MemoryTags key)
        {
            if (memories.ContainsKey(key))
                return memories[key];
            return null;
        }
        public MemoryPacket Tag(string key)
        {
            return memories.Where(m => m.Key.ContainsTag(key)).First().Value;
        }
        public MemoryPacket Any(Random random, params string[] key)
        {
            List<KeyValuePair<MemoryTags, MemoryPacket>> temp = memories.Where(m => m.Key.ContainsAny(key)).ToList();
            return temp[random.Next(0, temp.Count)].Value;
        }
        public MemoryPacket All(params string[] key)
        {
            return memories.Where(m => m.Key.ContainsAny(key)).First().Value;
        }

        public void ClearMemories() { memories.Clear(); }
    }
    public class MemoryPacket
    {
        public MemoryTags Key { get; private set; }
        public object Memory { get; set; }

        public DateStamp DateCreated { get; set; }
        public int LifeTime { get; set; }
        public bool IsLongterm { get; set; }
        private int baseTime;
        public void RenewTime() { LifeTime = baseTime; }

        public MemoryPacket(MemoryTags Key, object Memory, int LifeTime, bool IsLongterm)
        {
            this.Key = Key;
            this.Memory = Memory;
            this.LifeTime = LifeTime;
            this.IsLongterm = IsLongterm;

            baseTime = LifeTime;
        }

        public string SaveData() { return string.Empty; }
        public override string ToString()
        {
            return Memory.ToString() + "[" + DateCreated.ToString() + "]";
        }
    }
    public class MemoryTags
    {
        public string[] Tags { get; private set; }

        public MemoryTags(params string[] Tags)
        {
            this.Tags = Tags;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < Tags.Length; i++)
            {
                unchecked
                {
                    hash = hash * 23 + Tags[i].GetHashCode();
                }
            }
            return hash;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is string[])
                return ContainsAll(obj as string[]);
            if (obj is MemoryTags)
                return ContainsAll((obj as MemoryTags).Tags);

            return base.Equals(obj);
        }

        public bool ContainsTag(string tag) { return Tags.Contains(tag); }
        public bool ContainsAny(params string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (Tags.Contains(tags[i]) == true)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// True if all values of parameter 'array' are contained in field 'Tags'.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private bool ContainsAll(params string[] array)
        {
            if (array.Length != Tags.Length)
                return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (Tags.Contains(array[i]) == false)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join(", ", Tags);
        }
    }
}
