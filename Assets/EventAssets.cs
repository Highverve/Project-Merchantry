using Merchantry.UI;
using Merchantry.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    //Honestly, think of a replacement for this class.
    public class EventAssets
    {
        public Dictionary<string, MessageData> Events { get; private set; } = new Dictionary<string, MessageData>(StringComparer.OrdinalIgnoreCase);
        private References references;

        public void Load(ContentManager cm)
        {
            references = GameManager.References();

            Texture2D note = cm.Load<Texture2D>("UI/Icons/Note");

            Events.Add("Arrival", new MessageData("Arrival", "Event", "Passing through the meadows, your destination\nappears. It's the small hamlet of Lake Oro.\n\nWeary from the journey, you find a nook on\nthe edge of town to set up market. As you\nrummage through your backpack, you recall\nfinding something useful along the way.\n\nWhat was it again?", note,
                    new ButtonOption("Tokens.", "", () =>
                    {
                        references.Screens.Message.LAYOUT_SetMessage(Events["FoundTokens"]);
                    }, null),
                    new ButtonOption("Crafting supplies.", "", () =>
                    {
                        references.Screens.Message.LAYOUT_SetMessage(Events["CraftingSupplies"]);
                    }, null),
                    new ButtonOption("A better view on life.", "", () =>
                    {
                        references.Screens.Message.LAYOUT_SetMessage(Events["BetterView"]);
                    }, null)) { SubtitleColor = Color.SkyBlue });

            Events.Add("CraftingSupplies", new MessageData("Arrival", "Event", "By the path, you stumbled upon an abandoned cart.\nYou luckily found a bundle of valuable resources.", note,
                new ButtonOption("A decent start.", "", () =>
                {
                    references.World.Controlled.Storage.AddItem("Stick", 4);
                    references.World.Controlled.Storage.AddItem("Stone", 6);
                    references.World.Controlled.Storage.AddItem("String", 3);
                    references.World.Controlled.Storage.AddItem("IronIngot", 1);

                    references.Screens.Message.Minimize();
                    references.World.IsAllowSelecting = true;
                    references.Settings.IsPaused = false;
                }, null)) { SubtitleColor = Color.SkyBlue });
            Events.Add("FoundTokens", new MessageData("Arrival", "Event", "That's right. You discovered ten tokens on\na dirt path a few days back, just south of\nthe Lesser Mire.", note,
                new ButtonOption("This luck will persist.", "", () =>
                {
                    references.World.Controlled.Storage.AddItem("Currency", 10);

                    references.Screens.Message.Minimize();
                    references.World.IsAllowSelecting = true;
                    references.Settings.IsPaused = false;
                }, null)) { SubtitleColor = Color.SkyBlue });
            Events.Add("BetterView", new MessageData("Arrival", "Event", "You pondered the misfortunes that come with a life as a travelling\nmerchant, and came to the conclusion you could be worse off.\n\nYou're certain this new outlook will help you in some way.", note,
                new ButtonOption("Okay.", "", () =>
                {
                    references.Screens.Message.Minimize();
                    references.World.IsAllowSelecting = true;
                    references.Settings.IsPaused = false;
                }, null),
                new ButtonOption("No wait, I found tokens.", "", () =>
                {
                    references.Screens.Message.LAYOUT_SetMessage(Events["FoundTokensAlt"]);
                }, null)) { SubtitleColor = Color.SkyBlue });
            Events.Add("FoundTokensAlt", new MessageData("Arrival", "Event", "A glitter catches your eye. By odd coincidence,\nyou spot two tokens resting on the\nfence post.", note,
                new ButtonOption("Better than nothing.", "", () =>
                {
                    references.World.Controlled.Storage.AddItem("Currency", 2);

                    references.Screens.Message.Minimize();
                    references.Settings.IsPaused = false;
                    references.World.IsAllowSelecting = true;
                }, null)) { SubtitleColor = Color.SkyBlue });

            /*references.Calendar.OnTime(776, 1, 1, 11, 5, () =>
            {
                GameManager.References().Settings.IsPaused = true;
                GameManager.References().World.IsAllowSelecting = false;
                references.Screens.Message.LAYOUT_SetMessage(Events["Arrival"]);
            });*/
        }
    }
}
