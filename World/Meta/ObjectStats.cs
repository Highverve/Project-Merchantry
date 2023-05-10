using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectStats
    {
        private int experience, level;
        public int Experience
        {
            get { return experience; }
            set
            {
                if (level < MaxLevel)
                {
                    experience = MathHelper.Max(value, 0);
                    onExperienceGain?.Invoke();

                    if (experience > ExperienceLevel[level])
                    {
                        Level++;
                        onLevelUp?.Invoke();

                        experience = 0;
                    }
                }
            }
        }
        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                if (level >= MaxLevel)
                    onMaxLevel?.Invoke();
            }
        }
        public int MaxLevel { get; set; }

        public int[] ExperienceLevel = new int[]
        {
            100,
            1000,
            10000,
            100000,
            1000000,
            -1
        };

        #region Events

        private event Action onLevelUp, onExperienceGain, onMaxLevel, onProfessionChange;
        public event Action OnLevelUp { add { onLevelUp += value; } remove { onLevelUp -= value; } }
        public event Action OnExperienceGain { add { onExperienceGain += value; } remove { onExperienceGain -= value; } }
        public event Action OnMaxLevel { add { onMaxLevel += value; } remove { onMaxLevel -= value; } }
        public event Action OnProfessionChange { add { onProfessionChange += value; } remove { onProfessionChange -= value; } }

        #endregion

        public enum Professions { Blacksmith, Apothecary, Sage, Baker, Clothier, Wordsmith }
        private Professions profession;
        public Professions Profession
        {
            get { return profession; }
            set
            {
                if (profession != value)
                {
                    profession = value;
                    onProfessionChange?.Invoke();
                }
            }
        }

        //The weight of the object, which influences how easily it is pushed.
        public float Mass { get; set; } = 10;
        //
        private float speed;
        public float Speed
        {
            get { return speed; }
            set { speed = MathHelper.Clamp(value, 0, MaxSpeed); }
        }
        public float MaxSpeed { get; set; } = 500;

        public bool IsImmovable { get; set; } = false;
        public bool IsCollidable { get; set; } = true;
    }
}
