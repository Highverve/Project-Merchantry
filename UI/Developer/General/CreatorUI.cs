using AetaLibrary.Elements;
using AetaLibrary.Elements.Text;
using Merchantry.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.UI.Developer.General
{
    class CreatorUI
    {
        private void Test()
        {
            ConstructorInfo[] constructors = typeof(World.WorldObject).GetConstructors();

        }
    }
    class ObjectData
    {
        public CreatorUI UI { get; set; }
        public Func<object> Result { get; set; }
        public List<ConstructorData> Constructors { get; set; } = new List<ConstructorData>();

        public int ConstructorIndex { get; set; }

        public ObjectData(Type ObjectType)
        {

        }

        public object CreateObject()
        {
            return Constructors[ConstructorIndex].CreateObject();
        }
    }
    class ConstructorData
    {
        public ObjectData Data { get; set; }
        public ConstructorInfo Constructor { get; private set; }
        public List<Element> Elements { get; private set; } = new List<Element>();

        public ConstructorData(ConstructorInfo Constructor)
        {

        }

        public object CreateObject()
        {
            return null;//Constructor.Invoke();
        }
        private object[] Paramaters()
        {
            List<object> objs = new List<object>();
            foreach (Element e in Elements)
            {
                if (e is TextElement)
                    objs.Add(((TextElement)e).Text);
                if (e is NumberElement)
                    objs.Add(((NumberElement)e).Number);
            }
            return objs.ToArray();
        }
    }
}
