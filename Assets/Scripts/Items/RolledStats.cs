using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [Serializable]
    public struct StatRange
    {
        public string key;
        public float min;
        public float max;
        public bool asInt;

        public StatRange(string key, float min, float max, bool asInt = false)
        {
            this.key = key;
            this.min = min;
            this.max = max;
            this.asInt = asInt;
        }

        public float Roll(System.Random rng)
        {
            float lo = Mathf.Min(min, max);
            float hi = Mathf.Max(min, max);

            if (asInt)
            {
                return rng.Next(Mathf.RoundToInt(lo), Mathf.RoundToInt(hi) + 1);
            }

            return lo + (float)rng.NextDouble() * (hi - lo);
        }
    }

    [Serializable]
    public struct RolledStat
    {
        public string key;
        public float value;

        public RolledStat(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class RolledStats
    {
        [SerializeField] private List<RolledStat> entries = new();

        public IReadOnlyList<RolledStat> Entries => entries;

        public void Roll(IEnumerable<StatRange> ranges, System.Random rng)
        {
            entries.Clear();

            if (ranges == null)
            {
                return;
            }

            foreach (var range in ranges)
            {
                if (string.IsNullOrEmpty(range.key))
                {
                    continue;
                }

                entries.Add(new RolledStat(range.key, range.Roll(rng)));
            }
        }

        public bool Has(string key)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    return true;
                }
            }

            return false;
        }

        public float Get(string key, float fallback = 0f)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].key == key)
                {
                    return entries[i].value;
                }
            }

            return fallback;
        }
    }
}
