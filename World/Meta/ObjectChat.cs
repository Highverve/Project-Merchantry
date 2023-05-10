using AetaLibrary.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectChat
    {
        private Random random;
        private List<ChatPacket> Messages = new List<ChatPacket>();

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                LastMessage = value;
            }
        }
        public string LastMessage { get; set; }
        public bool HasMessage() { return !string.IsNullOrEmpty(Message); }

        public int Lifetime { get; set; }
        public int Timer { get; set; }
        public int CalculateTime(string message)
        {
            if (!string.IsNullOrEmpty(message))
                return Lifetime = 2000 + message.Length * 30;
            return 0;
        }
        public void SetCalculatedTime()
        {
            Timer = 0;
            Lifetime = CalculateTime(Message);
        }

        public void Add(string key, string message) { Messages.Add(new ChatPacket(key, message)); }
        public void Add(string key, Func<string> format) { Messages.Add(new ChatPacket(key, format)); }

        private List<ChatPacket> results = new List<ChatPacket>();
        public void SetPacket(string key, int lifetime)
        {
            results.Clear();

            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Key.ToUpper() == key.ToUpper())
                    results.Add(Messages[i]);
            }

            if (results.Count > 0)
            {
                ChatPacket chat = results[random.Next(0, results.Count)];
                if (chat.Format == null)
                    Message = chat.Message;
                else
                    Message = chat.Format.Invoke();

                Timer = 0;
                Lifetime = lifetime;
            }
        }
        public void SetPacket(string key)
        {
            int time = Timer;
            SetPacket(key, Lifetime);
            Timer = time;
        }
        public void SetMessage(string message, int lifetime)
        {
            Message = message;
            Timer = 0;
            Lifetime = lifetime;
        }
        public void SetMessage(string message) { Message = message; }

        public ObjectChat()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Update(GameTime gt)
        {
            if (HasMessage())
            {
                Timer += gt.ElapsedGameTime.Milliseconds;
                if (Timer >= Lifetime)
                    Message = string.Empty;
            }
        }
    }
    class ChatPacket
    {
        public string Key { get; set; }
        public string Message { get; set; }
        public Func<string> Format { get; set; }

        public ChatPacket(string Key, string Message)
        {
            this.Key = Key;
            this.Message = Message;
        }
        public ChatPacket(string Key, Func<string> Format)
        {
            this.Key = Key;
            this.Format = Format;
        }
    }
}
