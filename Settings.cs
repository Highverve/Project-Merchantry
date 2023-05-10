using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry
{
    public class Settings
    {
        #region Resolution

        private Vector2 resolution;
        public Vector2 Resolution
        {
            get { return resolution; }
            set
            {
                resolution = value;
                onResolutionChange?.Invoke();
            }
        }

        private event Action onResolutionChange;
        public event Action OnResolutionChange { add { onResolutionChange += value; } remove { onResolutionChange -= value; } }

        #endregion

        #region Timescale

        public float MasterTimescale { get; set; } = 1f;
        public float PlayerTimescale { get; set; } = 1f;
        public float WorldTimescale { get; set; } = 1f;

        #endregion

        #region Pausing

        private bool isPaused = false;
        public bool IsPaused
        {
            get { return isPaused; }
            set
            {
                isPaused = value;

                if (isPaused == false)
                    onResume?.Invoke();
                else
                    onPause?.Invoke();
            }
        }
        private event Action onPause, onResume;
        public event Action OnPause { add { onPause += value; } remove { onPause -= value; } }
        public event Action OnResume { add { onResume += value; } remove { onResume -= value; } }

        #endregion
    }
}
