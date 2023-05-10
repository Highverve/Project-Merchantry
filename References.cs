using ExtensionsLibrary.Input;
using Merchantry.Assets;
using Merchantry.Culture;
using Merchantry.Extensions;
using Merchantry.Particles;
using Merchantry.UI;
using Merchantry.World;

namespace Merchantry
{
    public class References
    {
        #region Classes

        public GameManager Game { get; set; }
        public WorldManager World { get; set; }
        public ScreenManager Screens { get; set; }
        public ParticleManager Particles { get; set; }
        public AssetManager Assets { get; set; }
        public Calendar Calendar { get; set; }

        public Settings Settings { get; set; }
        public Debugging Debugging { get; set; }
        public Controls Controls { get; set; }

        #endregion

        public References() { }
    }
}
