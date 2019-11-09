using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using RNG = Nez.Random;

namespace NewGame.Shared.Components.Generation
{
    public class BinarySpacePartition
    {
        public BinarySpacePartition[] SubPartitions;
        public Rectangle Bounds { get; }

        public BinarySpacePartition(int x, int y, int width, int height)
        {
            Bounds = new Rectangle(x, y, width, height);
        }

        public void Split(int minimumArea, int maxIterations, int iteration = 1)
        {
            TryPartition(minimumArea);

            if (iteration > 3 && RNG.chance(2))
            {
                Console.WriteLine("Due to 2% chance, stopped splitting");
                return;
            }

            if (SubPartitions != null && iteration < maxIterations)
            {
                SubPartitions[0].Split(minimumArea, maxIterations, iteration + 1);
                SubPartitions[1].Split(minimumArea, maxIterations, iteration + 1);
            }
        }

        private void TryPartition(int minimumArea)
        {
            var horizontalBias = Math.Min(Bounds.Width / Bounds.Height, 1);
            var verticalBias = Math.Min(Bounds.Height / Bounds.Width, 1);
            var horizontal = RNG.chance(50 + horizontalBias * 40 - verticalBias * 40);

            if (horizontal)
            {
                int splitAt;
                int tries = 0, maxRetries = 10;
                do
                {
                    tries++;

                    var splitPercent = 0.5f + 0.1f * (RNG.nextFloat() - 0.5f);
                    splitAt = (int)Mathf.lerp(0, Bounds.Width, splitPercent);
                }
                while (splitAt * Bounds.Height < minimumArea && tries < maxRetries);

                if (tries >= maxRetries)
                {
                    Console.WriteLine("Stopping split early due to minimum area constraint");
                    return;
                }

                SubPartitions = new BinarySpacePartition[2];
                SubPartitions[0] = new BinarySpacePartition(Bounds.X, Bounds.Y,
                    splitAt, Bounds.Height);
                SubPartitions[1] = new BinarySpacePartition(Bounds.X + splitAt,
                    Bounds.Y, Bounds.Width - splitAt, Bounds.Height);

                Console.WriteLine($"Split width {Bounds.Width} into {SubPartitions[0].Bounds.Width} + {SubPartitions[1].Bounds.Width}");
            }
            else
            {
                int splitAt;
                int tries = 0, maxRetries = 10;
                do
                {
                    tries++;

                    var splitPercent = 0.5f + 0.1f * (RNG.nextFloat() - 0.5f);
                    splitAt = (int)Mathf.lerp(0, Bounds.Height, splitPercent);
                }
                while (splitAt * Bounds.Width < minimumArea && tries < maxRetries);

                if (tries >= maxRetries)
                {
                    Console.WriteLine("Stopping split early due to minimum area constraint");
                    return;
                }

                SubPartitions = new BinarySpacePartition[2];
                SubPartitions[0] = new BinarySpacePartition(Bounds.X, Bounds.Y,
                        Bounds.Width, splitAt);
                SubPartitions[1] = new BinarySpacePartition(Bounds.X, Bounds.Y + splitAt,
                        Bounds.Width, Bounds.Height - splitAt);

                Console.WriteLine($"Split height {Bounds.Height} into {SubPartitions[0].Bounds.Height} + {SubPartitions[1].Bounds.Height}");
            }
        }

        public void GetLeafNodes(List<BinarySpacePartition> nodes)
        {
            if (SubPartitions == null)
            {
                nodes.Add(this);
            }
            else
            {
                SubPartitions[0].GetLeafNodes(nodes);
                SubPartitions[1].GetLeafNodes(nodes);
            }
        }
    }
}
