using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using RNG = Nez.Random;

namespace NewGame.Shared.Entities.Components.Generation
{
    public class BinarySpacePartition
    {
        public BinarySpacePartition[] SubPartitions;
        public Rectangle Bounds { get; }

        public BinarySpacePartition(int x, int y, int width, int height)
        {
            Bounds = new Rectangle(x, y, width, height);
        }

        public void Split(Point minPartitionSize, Point maxPartitionSize)
        {
            bool shouldSplit = true;

            if (Bounds.Width > minPartitionSize.X && Bounds.Width < maxPartitionSize.X
                && Bounds.Height > minPartitionSize.Y && Bounds.Height < maxPartitionSize.Y)
            {
                var chance = (float)Math.Pow((Bounds.Width * Bounds.Height) / (maxPartitionSize.X * maxPartitionSize.Y), 2);
                shouldSplit = RNG.Chance(chance);
            }

            if (!shouldSplit)
            {
                Console.WriteLine($"Satisfied room size with {(Bounds.Width * Bounds.Height)} ({minPartitionSize.X * minPartitionSize.Y}-{maxPartitionSize.X * maxPartitionSize.Y})");
                return;
            }

            TryPartition(minPartitionSize);

            if (SubPartitions != null)
            {
                SubPartitions[0].Split(minPartitionSize, maxPartitionSize);
                SubPartitions[1].Split(minPartitionSize, maxPartitionSize);
            }
        }

        private void TryPartition(Point minPartitionSize)
        {
            var horizontalBias = Math.Min(Bounds.Width / Bounds.Height, 1);
            var verticalBias = Math.Min(Bounds.Height / Bounds.Width, 1);
            int splitAt;
            int tries = 0, maxRetries = 5;

            bool isValidSplit;
            bool isHorizontalSplit;
            do
            {
                isHorizontalSplit = RNG.Chance(50 + horizontalBias * 20 - verticalBias * 20);
                var splitPercent = 0.5f + 0.5f * (RNG.NextFloat() - 0.5f);

                if (isHorizontalSplit)
                {
                    tries++;

                    splitAt = (int)Mathf.Lerp(0, Bounds.Width, splitPercent);
                    var remainder = Bounds.Width - splitAt;

                    isValidSplit = splitAt >= minPartitionSize.X && remainder >= minPartitionSize.X;
                }
                else
                {

                    splitAt = (int)Mathf.Lerp(0, Bounds.Height, splitPercent);
                    var remainder = Bounds.Height - splitAt;

                    isValidSplit = splitAt >= minPartitionSize.Y && remainder >= minPartitionSize.Y;
                }
                tries++;
            } while (!isValidSplit && tries < maxRetries);

            if (!isValidSplit)
            {
                Console.WriteLine($"Reached minimum space constraints {Bounds.Width} x {Bounds.Height}");
                return;
            }

            if (isHorizontalSplit)
            {
                SubPartitions = new BinarySpacePartition[2];
                SubPartitions[0] = new BinarySpacePartition(Bounds.X, Bounds.Y,
                    splitAt, Bounds.Height);
                SubPartitions[1] = new BinarySpacePartition(Bounds.X + splitAt,
                    Bounds.Y, Bounds.Width - splitAt, Bounds.Height);
                Console.WriteLine($"Split width {Bounds.Width} into {SubPartitions[0].Bounds.Width} + {SubPartitions[1].Bounds.Width}");
            }
            else
            {
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
