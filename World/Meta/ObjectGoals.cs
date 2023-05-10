using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectGoals : ObjectGoal
    {        
        public Dictionary<string, ObjectGoal> Goals { get; set; } = new Dictionary<string, ObjectGoal>(StringComparer.OrdinalIgnoreCase);
        public ObjectGoal CurrentGoal { get; set; }
        public ObjectGoal LastGoal { get; set; }

        public override void Initialize() { base.Initialize(); }
        public override void Update(GameTime gt)
        {
            SetGoalByPriority();

            if (CurrentGoal != null)
            {
                CurrentGoal.Update(gt);

                if (CurrentGoal.IsDestruct)
                    CurrentGoal = null;
            }

            base.Update(gt);
        }
        public void SetGoalByPriority()
        {
            ObjectGoal final = null;
            foreach (ObjectGoal goal in Goals.Values)
            {
                if (goal.IsDestruct == true)
                    Object.Queue.Add(0, () => Goals.Remove(goal.Name));

                if (goal.IsDestruct == false)
                {
                    if (final != null)
                        final = (goal.Priority() > final.Priority()) ? goal : final;
                    else
                        final = goal;
                }
            }

            if (final != null)
            {
                if (final != CurrentGoal)
                {
                    CurrentGoal = final;
                    CurrentGoal.Initialize();
                    CurrentGoal.IsDestruct = false;
                }
            }
        }

        public void AddGoal(string name, ObjectGoal goal)
        {
            if (!Goals.ContainsKey(name))
            {
                goal.Name = name;
                goal.Object = Object;

                Goals.Add(name, goal);
            }
        }
        public void RemoveGoal(string name)
        {
            if (Goals.ContainsKey(name))
                Goals.Remove(name);
        }
        public void SetGoal(string name)
        {
            if (Goals.ContainsKey(name) && CurrentGoal != Goals[name])
            {
                LastGoal = CurrentGoal;
                CurrentGoal = Goals[name];
                CurrentGoal.Initialize();
            }
        }
        public void SetGoal(ObjectGoal goal)
        {
            if (Goals.ContainsValue(goal) && CurrentGoal != goal)
            {
                LastGoal = CurrentGoal;
                CurrentGoal = goal;
                CurrentGoal.Initialize();
            }
        }
        public bool IsGoal(string name)
        {
            return (CurrentGoal != null && CurrentGoal.Name.ToUpper() == name.ToUpper());
        }

        private AetaLibrary.Elements.Text.TextElement currentText;
        public override void SetProperties(PropertiesUI ui)
        {
            base.SetProperties(ui);

            currentText = ui.PROPERTY_AddText("Current: " + (CurrentGoal != null ? CurrentGoal.Name : "None"));
            currentText.Fade(Color.Lerp(Color.Gold, Color.White, .25f), 100);

            foreach (ObjectGoal goal in Goals.Values)
                ui.PROPERTY_AddLabelButton("  " + goal.Name, () => ui.SetSelected(goal)).Fade(Color.Gray, 100);
        }
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            currentText.Text = "Current: " + (CurrentGoal != null ? CurrentGoal.Name : "None");
        }
        public override void NullifyProperties()
        {
            base.NullifyProperties();
            currentText = null;
        }
    }
    [System.Diagnostics.DebuggerDisplay("{Name} - {Priority()}")]
    public class ObjectGoal : IProperties
    {
        #region Meta

        public string Name { get; set; } = "Unnamed";
        public WorldObject Object { get; set; }

        public Func<float> PriorityFormula { get; set; } = () => { return 0f; };
        public float BasePriority { get; set; }
        public float Priority() { return (float)PriorityFormula?.Invoke() + BasePriority; }
        public bool IsDestruct { get; set; } = false;

        #endregion

        #region External Actions

        public Action OnInitialize { get; set; }
        public Action<GameTime> OnUpdate { get; set; }
        public Action OnSuccess { get; set; }
        public Action OnFail { get; set; }

        #endregion

        public ObjectGoal() { }
        public ObjectGoal(Action OnInitialize, Action<GameTime> OnUpdate,
                          Action OnSuccess, Action OnFail)
        {
            this.OnInitialize = OnInitialize;
            this.OnUpdate = OnUpdate;
            this.OnSuccess = OnSuccess;
            this.OnFail = OnFail;
        }

        public virtual void Initialize() { OnInitialize?.Invoke(); }
        public virtual void Update(GameTime gt) { OnUpdate?.Invoke(gt); }
        public virtual void Success() { OnSuccess?.Invoke(); }
        public virtual void Fail() { OnFail?.Invoke(); }

        private AetaLibrary.Elements.Text.TextElement ownerLabel, priorityLabel;
        public virtual void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader(Name);
            ownerLabel = ui.PROPERTY_AddLabelButton("Owner: " + Object.ID, () => ui.SetSelected(Object));
            priorityLabel = ui.PROPERTY_AddText("Priority: " + Priority());
            ownerLabel.Fade(Color.BurlyWood, 100);
            priorityLabel.Fade(Color.BurlyWood, 100);

            ui.PROPERTY_AddDivider(10);
            ui.PROPERTY_AddSpacer(10);
        }
        public virtual void RefreshProperties()
        {
            ownerLabel.Text = "Owner: " + Object.ID;
            priorityLabel.Text = "Priority: " + Priority();
        }
        public virtual void NullifyProperties()
        {
            ownerLabel = null;
            priorityLabel = null;
        }
    }
}
