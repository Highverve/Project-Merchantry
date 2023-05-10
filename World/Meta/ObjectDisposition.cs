using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectDisposition
    {
        private int integrity;
        public int Integrity
        {
            get { return integrity; }
            set { integrity = MathHelper.Clamp(value, -100, 100); }
        }

        public Dictionary<string, int> Dispositions { get; private set; }
        public void SetDisposition(string id, int value)
        {
            if (Dispositions.ContainsKey(id))
                Dispositions[id] = value;
            else
                Dispositions.Add(id, value);
        }
        public void AdjustDisposition(string id, int adjustment)
        {
            if (Dispositions.ContainsKey(id))
                Dispositions[id] += adjustment;
            else
                Dispositions.Add(id, adjustment);
        }

        public ObjectDisposition()
        {
            Dispositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
