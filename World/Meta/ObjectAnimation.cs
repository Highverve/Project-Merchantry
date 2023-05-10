using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectAnimation
    {
        private Point currentFrame, lastFrame, frameSize;
        public Point CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value; }
        }
        public int X { get { return currentFrame.X; } set { currentFrame.X = value; } }
        public int Y { get { return currentFrame.Y; } set { currentFrame.Y = value; } }
        public Point LastFrame
        {
            get { return lastFrame; }
            set { lastFrame = value; }
        }
        public Point FrameSize
        {
            get { return frameSize; }
            set { frameSize = value; }
        }
        public int FrameTime { get; set; }
        public int FrameSpeed { get; set; }

        public string FrameState { get; set; }
        public string LastFrameState { get; set; }
        protected Dictionary<string, AnimationPacket> frameStates;
        public Dictionary<string, AnimationPacket> FrameStates
        {
            get
            {
                if (frameStates == null)
                    frameStates = new Dictionary<string, AnimationPacket>(StringComparer.OrdinalIgnoreCase);
                return frameStates;
            }
            set { frameStates = value; }
        }

        public bool IsPaused { get; set; }
        public int PauseTime { get; set; }
        public void Pause(int time)
        {
            IsPaused = true;
            PauseTime = time;
        }

        public Rectangle Source()
        {
            return new Rectangle(FrameSize.X * CurrentFrame.X, FrameSize.Y * CurrentFrame.Y, FrameSize.X, FrameSize.Y);
        }

        public ObjectAnimation()
        {
            //Simple example. Walk animation, from frame 0,0 to 3,0.
            /*AddState("Walk", () =>
            {
                FrameSpeed = 100;
                CurrentFrame = Point.Zero;
            }, () =>
            {
                currentFrame.X++;
                if (currentFrame.X > 3)
                    currentFrame.X = 0;
            });*/
        }

        public void Update(GameTime gt)
        {
            if (LastFrameState != FrameState)
            {
                FrameStates[FrameState].Initialize?.Invoke();
                LastFrameState = FrameState;
            }

            if (IsPaused == false)
            {
                FrameTime += gt.ElapsedGameTime.Milliseconds;
                if (FrameTime > FrameSpeed)
                {
                    if (frameStates != null && !string.IsNullOrEmpty(FrameState))
                    {
                        if (FrameStates.ContainsKey(FrameState))
                            FrameStates[FrameState].Update?.Invoke();
                    }

                    FrameTime = 0;
                }
            }
            else
            {
                PauseTime -= gt.ElapsedGameTime.Milliseconds;
                if (PauseTime <= 0)
                {
                    IsPaused = false;
                    PauseTime = 0;
                }
            }
        }
        public void AddState(string Name, Action Initialize, Action Update)
        {
            FrameStates.Add(Name, new AnimationPacket(Name, Initialize, Update));
        }
    }
    public class AnimationPacket
    {
        public string Name { get; set; }
        public Action Initialize { get; set; }
        public Action Update { get; set; }

        public AnimationPacket(string Name, Action Initialize, Action Update)
        {
            this.Name = Name;
            this.Initialize = Initialize;
            this.Update = Update;
        }
    }
}
