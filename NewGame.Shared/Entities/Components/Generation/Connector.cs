﻿using Microsoft.Xna.Framework;

namespace NewGame.Shared.Entities.Components.Generation
{
    public class Connector
    {
        public Rectangle[] Rectangles { get; }
        public Connector(params Rectangle[] rectangles)
        {
            Rectangles = rectangles;
        }

    }
}