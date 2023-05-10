using AetaLibrary;
using AetaLibrary.Extensions;
using ExtensionsLibrary._2D;
using ExtensionsLibrary.Extensions;
using ExtensionsLibrary.Input;
using ExtensionsLibrary.Support;
using Merchantry.Extensions;
using Merchantry.UI.Developer.General;
using Merchantry.UI.Items;
using Merchantry.World.Meta;
using Merchantry.World.Meta.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Merchantry.World
{
    /// <summary>
    /// An object in the game world from which all should derive from. Distinct from UI elements and stuff like that.
    /// </summary>
    public class WorldObject : IProperties
    {
        #region Meta

        public string ID { get; set; }
        public string DisplayName { get; set; } = "object";

        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        #endregion

        #region Editor

        private AetaLibrary.Elements.Text.TextElement visibleLabel, enabledLabel;
        private UI.Elements.NumberElement numberX, numberY;
        public virtual void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader(GetType().Name);
            ui.PROPERTY_AddText("ID: " + ID);
            numberX = ui.PROPERTY_AddNumber("Position.X: ", Position.X, null, () => Position += new Vector2(10, 0), () => Position -= new Vector2(10, 0));
            numberY = ui.PROPERTY_AddNumber("Position.Y: ", Position.Y, null, () => Position += new Vector2(0, 10), () => Position -= new Vector2(0, 10));
            numberX.Fade(Color.Gray, 20);
            numberY.Fade(Color.Gray, 20);

            ui.PROPERTY_AddSpacer(20);

            enabledLabel = ui.PROPERTY_AddButton("Enabled", () => IsEnabled = !IsEnabled);
            visibleLabel = ui.PROPERTY_AddButton("Visible", () => IsVisible = !IsVisible);
            ui.PROPERTY_AddButton("Focus", SetFocus);
            ui.PROPERTY_AddButton("Destroy", Destroy);

            ui.PROPERTY_AddDivider(10);
            ui.PROPERTY_AddSpacer(10);

            ui.PROPERTY_AddHeader("Classes");

            if (physics != null)
                ui.PROPERTY_AddLabelButton("Physics", () => ui.SetSelected(Physics));
            if (storage != null)
                ui.PROPERTY_AddLabelButton("Storage", () => ui.SetSelected(Storage));
            if (shape != null)
                ui.PROPERTY_AddLabelButton("Shapes", () => ui.SetSelected(Shape));
            if (ai != null)
                ui.PROPERTY_AddLabelButton("AI", () => ui.SetSelected(AI));

            /*List<WorldObject> t = World.Temp();
            for (int i = 0; i < 10; i++)
            {
                WorldObject obj = t[i];
                ui.PROPERTY_AddLabelButton(obj.ID, () => { ui.SetSelected(obj); });
            }*/
        }
        public void RefreshProperties()
        {
            numberX.Number = Position.X;
            numberY.Number = Position.Y;
            visibleLabel.Text = (IsVisible ? "Visible" : "Hidden");
            enabledLabel.Text = (IsEnabled ? "Enabled" : "Disabled");
        }
        public virtual void NullifyProperties()
        {
            numberX = null;
            numberY = null;
            enabledLabel = null;
            visibleLabel = null;
        }

        #endregion

        public Vector2 Position { get; set; }
        public Vector2 LastPosition { get; private set; }

        private Vector2 origin, lastScale;
        private float lastRotation;
        public Rectangle SourceRect { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Rotation
        {
            get { return SmoothRotation.Result; }
            set
            {
                SmoothRotation.Result = value;
                CalculateMatrix();
            }
        }
        public Vector2 Origin
        {
            get { return origin; }
            set
            {
                origin = value;
                CalculateMatrix();
            }
        }
        public float Depth { get; set; }

        #region Transitions

        private SmoothFloat smoothRotation;
        public SmoothFloat SmoothRotation
        {
            get
            {
                if (smoothRotation == null)
                    smoothRotation = new SmoothFloat();
                return smoothRotation;
            }
            set { smoothRotation = value; }
        }

        //Rotation
        public float ROTATE_Speed { get { return SmoothRotation.Speed; } set { SmoothRotation.Speed = value; } }
        public void ROTATE_SetNull(float result) { SmoothRotation.SetNull(result); }
        public void ROTATE_SetNull() { ROTATE_SetNull(SmoothRotation.Result); }
        public void ROTATE_Lerp(float target) { SmoothRotation.SetLerp(target); }
        public void ROTATE_SmoothStep(float target) { SmoothRotation.SetSmoothStep(target); }
        public void ROTATE_Loose(float target, float minDistance = .001f) { SmoothRotation.SetLoose(target, minDistance); }
        //Rotation effects
        public void ROTATE_Wobble(float power, float powerResistance, float speedMultiplier)
        {
            int count = 0, timer = 0;
            while (power > 0)
            {
                float powerTemp = power;
                if (count % 2 == 0)
                    Queue.Add(timer += (int)(speedMultiplier * power), () => { ROTATE_Loose(powerTemp, 0); });
                else
                    Queue.Add(timer += (int)(speedMultiplier * power), () => { ROTATE_Loose(-powerTemp, 0); });

                power -= powerResistance;
                power = MathHelper.Max(power, 0);
                count++;
            }
            if (count > 0)
                Queue.Add(timer, () => { ROTATE_Loose(0); });
        }
        public void ROTATE_Shake(float power, int count, int delay)
        {
            for (int i = 0; i < count; i++)
                Queue.Add(delay * i, () => ROTATE_Loose(power *= -1));
            Queue.Add(delay * count, () => ROTATE_Loose(0));
        }

        //Scale
        private SmoothVector smoothScale;
        public SmoothVector SmoothScale
        {
            get
            {
                if (smoothScale == null)
                {
                    smoothScale = new SmoothVector();
                    smoothScale.Result = Vector2.One;
                }
                return smoothScale;
            }
            set { smoothScale = value; }
        }
        public Vector2 Scale
        {
            get { return SmoothScale.Result; }
            set
            {
                SmoothScale.Result = value;
                CalculateMatrix();
            }
        }

        public float SCALE_Speed { set { SmoothScale.Speed = value; } }
        public void SCALE_SetNull(Vector2 target) { SmoothScale.SetNull(target); }
        public void SCALE_SetNull() { SCALE_SetNull(SmoothScale.Result); }
        public void SCALE_Lerp(Vector2 target) { SmoothScale.SetLerp(target); }
        public void SCALE_SmoothStep(Vector2 target) { SmoothScale.SetSmoothStep(target); }
        public void SCALE_Loose(Vector2 target, float minDistance = .001f) { SmoothScale.SetLoose(target, minDistance); }
        public void SCALE_Loose(float target, float minDistance = .001f) { SCALE_Loose(new Vector2(target), minDistance); }

        //Scale effects
        public void SCALE_Bounce(float x, float y, float offsetX, float offsetY, int delay)
        {
            SCALE_Loose(offsetX, offsetY);
            Queue.Add(delay, () => { SCALE_Loose(x, y); });
        }
        public void SCALE_Pulse(float x, float y, int count, int delay, int countDelay, float homeX = -1, float homeY = -1)
        {
            if (homeX == -1)
                homeX = Scale.X;
            if (homeY == -1)
                homeY = Scale.Y;

            int timer = 0;
            for (int i = 0; i < count; i++)
            {
                Queue.Add(timer, () => { SCALE_Loose(x, y); });
                timer += delay;
                Queue.Add(timer, () => { SCALE_Loose(homeX, homeY); });
                timer += countDelay;
            }
        }

        public virtual void Intro()
        {
            SCALE_Speed = 10f;
            SCALE_Loose(1);
        }
        public virtual void Outro()
        {
            SCALE_Speed = 7.5f;
            SCALE_Loose(0);
        }

        #endregion

        private bool isStopped = false;
        private event Action<GameTime> onPositionMove;
        private event Action onPositionStop;
        public event Action<GameTime> OnPositionMove { add { onPositionMove += value; } remove { onPositionMove -= value; } }
        public event Action OnPositionStop { add { onPositionStop += value; } remove { onPositionStop -= value; } }

        public float FocusMultiplier { get; set; } = .5f;
        public Vector2 FocusOffset { get; set; }
        public float FocusScale { get; set; } = 2f;
        public float FocusSpeed { get; set; } = 3;
        public virtual Vector2 FocusPosition()
        {
            return Vector2.Lerp(Physics.AltitudePosition() + ToMatrix(FocusOffset), Camera.ToWorld(Controls.MouseVector()), FocusMultiplier + BonusFocus());
        }
        private float BonusFocus()
        {
            float w = References.Settings.Resolution.X;
            if (w >= 1920) return 0;
            if (w >= 1600) return FocusMultiplier * .5f;
            if (w >= 1024) return FocusMultiplier * 1f;
            return 0;
        }
        public virtual void SetFocus()
        {
            World.Focused = this;
            Camera.TargetScale = new Vector2(FocusScale);
            Camera.ScaleSpeed = FocusSpeed;
        }

        #region Classes

        protected Random trueRandom, seedRandom;
        protected References References { get; private set; }

        public WorldManager World { get; set; }
        public Camera Camera { get; set; }
        public Controls Controls { get; set; }
        public TimeQueue Queue { get; set; }

        protected ObjectPhysics physics;
        protected ObjectStorage storage;
        protected ObjectStats stats;
        protected ObjectMemory memory;
        protected ObjectChat chat;
        protected ObjectShape shape;
        protected ObjectGoals ai;
        protected ObjectAnimation animation;
        protected ObjectPath path;

        public ObjectPhysics Physics
        {
            get
            {
                if (physics == null)
                {
                    physics = new ObjectPhysics();
                    physics.Object = this;
                }
                return physics;
            }
            set { physics = value; }
        }
        public ObjectStorage Storage
        {
            get
            {
                if (storage == null)
                {
                    storage = new ObjectStorage();
                    storage.Object = this;
                }
                return storage;
            }
            set { storage = value; }
        }
        public ObjectStats Stats
        {
            get
            {
                if (stats == null)
                    stats = new ObjectStats();
                return stats;
            }
            set { stats = value; }
        }
        public ObjectMemory Memory
        {
            get
            {
                if (memory == null)
                    memory = new ObjectMemory();
                return memory;
            }
            set { memory = value; }
        }
        public ObjectChat Chat
        {
            get
            {
                if (chat == null)
                    chat = new ObjectChat();
                return chat;
            }
            set { chat = value; }
        }
        public ObjectShape Shape
        {
            get
            {
                if (shape == null)
                {
                    shape = new ObjectShape();
                    shape.Object = this;
                }
                return shape;
            }
            set { shape = value; }
        }
        public ObjectGoals AI
        {
            get
            {
                if (ai == null)
                {
                    ai = new ObjectGoals();

                    ai.Object = this;
                    ai.Name = "AI";
                    ai.BasePriority = 1;
                }
                return ai;
            }
            set { ai = value; }
        }
        public ObjectAnimation Animation
        {
            get
            {
                if (animation == null)
                    animation = new ObjectAnimation();
                return animation;
            }
            set { animation = value; }
        }
        public ObjectPath Path
        {
            get
            {
                if (path == null)
                {
                    path = new ObjectPath();
                    path.Object = this;
                }
                return path;
            }
            set { path = value; }
        }

        public Random TrueRandom { get { return trueRandom; } }
        public Random SeedRandom { get { return seedRandom; } }

        //Collision
        public bool Intersects(WorldObject obj) { return Intersects(obj, null); }
        public bool Intersects(WorldObject obj, string group)
        {
            if (shape != null && obj.shape != null)
            {
                foreach (Shape s in obj.Shape.Shapes.Values)
                {
                    if (s.IsGroup(group) && shape.Intersects(s, group))
                        return true;
                }
            }

            return false;
        }

        public void UpdateCollision()
        {
            if (IsSectoring == true)
            {
                Sector.Iterate(this, (obj) =>
                {
                    if (obj.Stats.IsCollidable && Intersects(obj, SHAPE_Collision))
                        PushOut(obj);
                });
            }
        }

        public void PushOut(WorldObject obj)
        {
            float dist = Vector2.Distance(obj.Position, Position);
            Vector2 dir = Position - obj.Position;
            if (dir != Vector2.Zero)
                dir.Normalize();

            float sum = Stats.Mass + obj.Stats.Mass;
            if (obj.Stats.IsImmovable == false)
                obj.Physics.AddVelocity((-dir * dist) * (Stats.Mass / sum));
            else
                Physics.AddVelocity((dir * dist));
        }

        private Sector sector;
        public Sector Sector
        {
            get
            {
                if (sector == null && IsSectoring == true)
                {
                    sector = World.Sectors.Sector(World.Sectors.ToSector(Position));
                    sector.AddObject(this);
                }

                return sector;
            }
            set
            {
                if (sector != null)
                    sector.RemoveObject(this);

                sector = value;

                if (sector != null)
                    sector.AddObject(this);
            }
        }
        protected void VerifySector()
        {
            if (sector != null && IsSectoring == true)
            {
                //Not the same sector? Find sector
                if (World.Sectors.InSector(Position, Sector) == false)
                    Sector = World.Sectors.Sector(World.Sectors.ToSector(Position));
            }
        }
        public bool IsSectoring { get; set; } = true;

        #endregion

        #region Shapes

        public const string SHAPE_Select = "SELECT";
        public const string SHAPE_Collision = "COLLIDE";
        public bool SelectContains(Vector2 position)
        {
            return Shape.Contains(position, SHAPE_Select);
        }
        public bool SelectIntersects(Shape shape)
        {
            return Shape.Intersects(shape, SHAPE_Select);
        }

        #endregion

        #region AI

        public void AI_FollowPath(int x, int y, float stopDistance)
        {
            AI.AddGoal("Pathfind", new Meta.Goals.FollowPath(new Point(x, y), stopDistance, stopDistance - 4, 1));
        }

        #endregion

        private Matrix relativeMatrix;
        public Matrix RelativeMatrix() { return relativeMatrix; }
        private void CalculateMatrix()
        {
            relativeMatrix = Matrix.CreateRotationZ(Rotation) *
                             Matrix.CreateScale(Scale.X, Scale.Y, 1);
        }
        public Vector2 ToMatrix(Vector2 position)
        {
            return Vector2.Transform(position, RelativeMatrix());
        }

        #region Attaching

        public WorldObject Attached { get; private set; }
        /// <summary>
        /// Attaches this object to another. This is the "sub-object".
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="offset"></param>
        public void Attach(WorldObject obj, Vector2 offset)
        {
            if (obj != null)
            {
                Attached = obj;
                AttachOffset = offset;
                Stats.IsCollidable = false;

                onAttach?.Invoke(obj);
            }
            else
                Detach();
        }
        public void Detach()
        {
            if (IsAttached())
            {
                if (AttachOffset.Y <= 0)
                {
                    float height = -AttachOffset.Y + Attached.Physics.Altitude;
                    Physics.Altitude = height;
                    Position += new Vector2(0, height);
                }
                AttachOffset = Vector2.Zero;
                Queue.Add(500, () => Stats.IsCollidable = true);

                Attached = null;
                onDetach?.Invoke();
            }
        }
        public bool IsAttached() { return Attached != null; }

        private event Action<WorldObject> onAttach;
        private event Action onDetach;
        public event Action<WorldObject> OnAttach { add { onAttach += value; } remove { onAttach -= value; } }
        public event Action OnDetach { add { onDetach += value; } remove { onDetach -= value; } }
        public Vector2 AttachOffset { get; set; }
        public void UpdateAttach(GameTime gt)
        {
            Vector2 target = Attached.Physics.AltitudePosition() + Vector2.Transform(AttachOffset, Attached.RelativeMatrix());
            float distance = Vector2.Distance(target, Position);
            if (distance > 5)
            {
                Vector2 direction = target - Position;
                if (direction != Vector2.Zero)
                    direction.Normalize();
                Position += direction * (distance * 15) * (float)gt.ElapsedGameTime.TotalSeconds;
            }
            else
                Position = target;
            //Physics.Altitude = Attached.Physics.Altitude + altitude;
            //Physics.AltitudeVelocity = 0;
            //Rotation = Attached.Rotation;
            //Scale = Attached.Scale;
            Depth = Attached.Depth + World.PixelDepth();
        }

        #endregion

        public WorldObject(string ID, Vector2 Position)
        {
            this.ID = ID;
            this.Position = new Vector2((int)Position.X, (int)Position.Y);

            seedRandom = new Random(ID.AddChars());
            trueRandom = new Random(Guid.NewGuid().GetHashCode());
        }

        public virtual void Initialize()
        {
            References = GameManager.References();

            SCALE_Speed = 7.5f;
            ROTATE_Speed = 5f;

            OnPositionMove += (gt) =>
            {
                if (IsAttached() == false)
                    Depth = World.CalculateDepth(Position.Y);
            };

            if (IsSectoring == true)
                Sector = World.Sectors.Sector(World.Sectors.ToSector(Position));

            CalculateMatrix();

            IsContains = (p) => SelectContains(p);
        }
        public virtual void Load(ContentManager cm) { }

        public virtual void Update(GameTime gt)
        {
            lastRotation = Rotation;
            lastScale = Scale;

            UpdatePositionEvents(gt);

            if (storage != null) storage.Update(gt);
            if (ai != null) ai.Update(gt);
            if (physics != null) physics.Update(gt);
            if (chat != null) chat.Update(gt);
            if (memory != null) memory.Update(gt);
            if (animation != null)
            {
                animation.Update(gt);
                SourceRect = Animation.Source();
            }

            if (smoothRotation != null)
                smoothRotation.Update(gt);
            if (smoothScale != null)
                smoothScale.Update(gt);

            if (Attached != null) UpdateAttach(gt);
            if (lastScale != Scale || lastRotation != Rotation)
                CalculateMatrix();
        }
        private void UpdatePositionEvents(GameTime gt)
        {
            if (LastPosition != Position)
            {
                onPositionMove?.Invoke(gt);
                LastPosition = Position;

                //Various classes
                VerifySector();
                UpdateCollision();

                isStopped = false;
            }
            else if (isStopped == false)
            {
                onPositionStop?.Invoke();
                isStopped = true;
            }
        }

        public virtual void Draw(SpriteBatch sb) { }

        #region Controls

        public Func<Vector2, bool> IsContains { get; set; }
        public bool IsSelectable { get; set; } = true;
        public virtual void HoverEnter(WorldObject user) { }
        public virtual void HoverExit(WorldObject user) { }

        public virtual void LeftClick(WorldObject user) { onLeftClick?.Invoke(user); }
        public virtual void RightClick(WorldObject user) { onRightClick?.Invoke(user); }
        public bool IsAcceptingItems { get; set; } = true;
        public virtual void ItemClick(ItemObject item, WorldObject user)
        {
            onItemClick?.Invoke(user);
        }

        private event Action<WorldObject> onLeftClick, onRightClick, onItemClick;
        public event Action<WorldObject> OnLeftClick { add { onLeftClick += value; } remove { onLeftClick -= value; } }
        public event Action<WorldObject> OnRightClick { add { onRightClick += value; } remove { onRightClick -= value; } }
        public event Action<WorldObject> OnItemClick { add { onItemClick += value; } remove { onItemClick -= value; } }

        public virtual void UpdateControls(GameTime gt) { }

        #endregion

        public bool IsDestroyed { get; set; }
        public void Destroy()
        {
            IsDestroyed = true;
            if (sector != null)
                sector.RemoveObject(this);
            Queue.Add(0, DestroyObjects);
        }
        /// <summary>
        /// Called by Destroy(). Remove all world objects (and others) here.
        /// </summary>
        protected virtual void DestroyObjects()
        {
            World.RemoveObject(this);

            if (this == References.Screens.Properties.Selected)
                References.Screens.Properties.SetSelected(null, false);
        }

        public virtual WorldObject Copy()
        {
            WorldObject copy = (WorldObject)MemberwiseClone();
            return copy;
        }
    }
}
