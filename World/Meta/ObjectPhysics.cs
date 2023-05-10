using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectPhysics : IProperties
    {
        public WorldObject Object { get; set; }

        private Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
            set
            {
                LastVelocity = velocity;
                velocity = value;
            }
        }
        public Vector2 LastVelocity { get; set; }
        public float GroundFriction { get; set; } = 10;
        public float AirFriction { get; set; } = 5;

        public void AddVelocity(float x, float y) { Velocity += new Vector2(x, y); }
        public void AddVelocity(Vector2 velocity) { Velocity += velocity; }
        public void MoveTo(GameTime gt, Vector2 position, float speed, float stopDistance)
        {
            float distance = Vector2.Distance(position, Object.Position);
            if (distance >= stopDistance)
            {
                Vector2 direction = position - Object.Position;
                if (direction != Vector2.Zero)
                    direction.Normalize();

                AddVelocity(direction * speed * (float)gt.ElapsedGameTime.TotalSeconds);
            }
        }
        public void FlyTo(GameTime gt, float altitude, float speed, float minDistance)
        {
            float dirDist = altitude - Altitude;
            if (Math.Abs(dirDist) > Altitude + minDistance || Math.Abs(dirDist) < Altitude - minDistance)
            {
                float direction = MathHelper.Clamp(dirDist, -1, 1);
                AltitudeVelocity += direction * speed * (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }
        public void UpdateVelocity(GameTime gt)
        {
            if (Object != null)
                Object.Position += TickMovement = Velocity * (float)gt.ElapsedGameTime.TotalSeconds;

            if (IsAir() == false)
                Velocity -= (Velocity * GroundFriction) * (float)gt.ElapsedGameTime.TotalSeconds;
            else
                Velocity -= (Velocity * AirFriction) * (float)gt.ElapsedGameTime.TotalSeconds;
        }

        public Vector2 TickMovement { get; private set; }
        public float PixelsPerSecond(float pixels)
        {
            return (TickMovement * new Vector2(pixels)).Length() / 60;
        }
        public float PixelsPerMinute(float pixels)
        {
            return PixelsPerSecond(pixels) * 60;
        }
        public Vector2 TrajectoryPosition(float steps)
        {
            return TickMovement * steps;
        }
        public int TicksToTarget(Vector2 position, Vector2 target, float minDistance)
        {
            float currentDistance = 1000000, lastDistance = 10000001;
            int tick = 1;

            while (currentDistance < lastDistance)
            {
                position += TrajectoryPosition(tick);

                lastDistance = currentDistance;
                currentDistance = Vector2.Distance(position, target);

                tick++;
            }
            if (currentDistance > lastDistance)
                return -1;

            return tick;
        }

        #region Air-oriented

        private float altitude;
        public float Altitude
        {
            get { return altitude; }
            set
            {
                LastAltitude = altitude;
                altitude = Math.Max(value, Floor);
            }
        }
        public float LastAltitude { get; set; }
        public Vector2 AltitudePosition(float multiplier = 1f)
        {
            if (Object != null)
                return Object.Position - new Vector2(0, Altitude * multiplier);
            return Vector2.Zero;
        }

        public float AltitudeVelocity { get; set; }
        public float LastHeightVelocity { get; set; }
        public float Gravity { get; set; } = 1500;
        public bool IsGravityEnabled { get; set; } = true;

        public float Floor { get; set; } = 0;
        private float bounceMultiplier = .35f;
        public float BounceMultiplier
        {
            get { return bounceMultiplier; }
            set { bounceMultiplier = MathHelper.Clamp(value, 0, 1); }
        }
        public bool IsAir() { return Altitude > Floor; }
        
        public void Jump(float speed)
        {
            if (IsAir() == false)
            {
                AltitudeVelocity += speed;
                onJump?.Invoke();
            }
        }
        public void SetAltitude(float altitude) { Altitude = altitude; }
        private void Bounce()
        {
            if (BounceMultiplier > 0)
            {
                AltitudeVelocity = -(AltitudeVelocity * BounceMultiplier);
                Velocity -= Velocity * .25f;

                float multi = 30 * (1 + BounceMultiplier);
                if (AltitudeVelocity >= -multi &&
                    AltitudeVelocity <= multi)
                {
                    AltitudeVelocity = 0;

                    Altitude = 0;
                    LastAltitude = 0;
                }
            }
        }

        public void ApplyGravity(GameTime gt)
        {
            //Add gravity to altitude velocity, but only if in air.
            if (IsAir() && IsGravityEnabled == true)
                AltitudeVelocity -= Gravity * (float)gt.ElapsedGameTime.TotalSeconds;

            //Add altitude velocity to altitude.
            Altitude += AltitudeVelocity * (float)gt.ElapsedGameTime.TotalSeconds;

            //Call impact event, indicating the object has hit the floor.
            if (LastAltitude > Floor && Altitude <= Floor)
                onImpact?.Invoke();
        }

        #endregion

        //Events
        private event Action onJump, onImpact, onVelocityChange;
        public event Action OnJump { add { onJump += value; } remove { onJump -= value; } }
        public event Action OnImpact { add { onImpact += value; } remove { onImpact -= value; } }
        public event Action OnVelocityChange { add { onVelocityChange += value; } remove { onVelocityChange -= value; } }

        public ObjectPhysics()
        {
            onImpact += () => Bounce();
        }
        public void Update(GameTime gt)
        {
            ApplyGravity(gt);
            UpdateVelocity(gt);
        }

        #region Editor

        private UI.Elements.NumberElement velocityXNumber, velocityYNumber, altitudeNumber, gravityNumber, bounceNumber, airFrictionNumber, groundFrictionNumber;
        private AetaLibrary.Elements.Text.TextElement gravityText;
        public void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader("Physics");
            ui.PROPERTY_AddText("Owner: " + Object.ID);
            velocityXNumber = ui.PROPERTY_AddNumber("Velocity.X: ", Velocity.X, null, () => velocity.X += 10, () => velocity.X -= 10);
            velocityXNumber.Fade(Color.Beige, 100);
            velocityYNumber = ui.PROPERTY_AddNumber("Velocity.Y: ", Velocity.Y, null, () => velocity.Y += 10, () => velocity.Y -= 10);
            velocityYNumber.Fade(Color.Beige, 100);
            groundFrictionNumber = ui.PROPERTY_AddNumber("Ground Friction: ", GroundFriction, null, () => GroundFriction += .5f, () => GroundFriction -= .5f);
            groundFrictionNumber.Fade(Color.Beige, 100);

            ui.PROPERTY_AddSpacer(15);

            altitudeNumber = ui.PROPERTY_AddNumber("Altitude: ", Altitude, null, () => Altitude += 10, () => Altitude -= 10);
            altitudeNumber.Fade(Color.LightBlue, 100);
            gravityNumber = ui.PROPERTY_AddNumber("Gravity: ", Gravity, null, () => Gravity += 50, () => Gravity -= 50);
            gravityNumber.Fade(Color.LightBlue, 100);
            bounceNumber = ui.PROPERTY_AddNumber("Bounce: ", BounceMultiplier, null, () => BounceMultiplier += .05f, () => BounceMultiplier -= .05f);
            bounceNumber.Fade(Color.LightBlue, 100);
            airFrictionNumber = ui.PROPERTY_AddNumber("Air Friction: ", AirFriction, null, () => AirFriction += .5f, () => AirFriction -= .5f);
            airFrictionNumber.Fade(Color.LightBlue, 100);

            ui.PROPERTY_AddDivider(10);
            ui.PROPERTY_AddSpacer(10);

            gravityText = ui.PROPERTY_AddButton("Gravity: " + (IsGravityEnabled ? "On" : "Off"), () => IsGravityEnabled = !IsGravityEnabled);
        }
        public void RefreshProperties()
        {
            velocityXNumber.Number = Velocity.X;
            velocityYNumber.Number = Velocity.Y;
            groundFrictionNumber.Number = GroundFriction;
            altitudeNumber.Number = Altitude;
            gravityNumber.Number = Gravity;
            bounceNumber.Number = BounceMultiplier;
            airFrictionNumber.Number = AirFriction;
            gravityText.Text = "Gravity: " + (IsGravityEnabled ? "On" : "Off");
        }
        public void NullifyProperties()
        {
            velocityXNumber = null;
            velocityYNumber = null;
            groundFrictionNumber = null;
            altitudeNumber = null;
            gravityNumber = null;
            bounceNumber = null;
            airFrictionNumber = null;
            gravityText = null;
        }

        #endregion
    }
}
