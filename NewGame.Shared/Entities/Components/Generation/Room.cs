using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NewGame.Shared.Entities.Components.Generation
{
    internal class Room
    {
        public HashSet<Room> Connections { get; set; }
        public Rectangle Bounds { get; internal set; }

        public Room()
        {
            Connections = new HashSet<Room>();
        }
        public void Connect(Room other)
        {
            Connections.Add(other);
            other.Connections.Add(this);
        }
    }
}