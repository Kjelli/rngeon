using Microsoft.Xna.Framework;
using NewGame.Shared.Data;
using NewGame.Shared.Utilities;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using RNG = Nez.Random;

namespace NewGame.Shared.Components.Generation
{
    public class DungeonMapGenerator
    {
        private readonly int _width;
        private readonly int _height;

        private BinarySpacePartition _tree;


        private IList<Room> _rooms;
        private IList<Connector> _connectors;
        private IList<RectangleF> _colliderBounds;
        private TileType[,] _tileMap;

        private TileSheet _sheet;

        public GenerationResult Result { get; private set; }

        public DungeonMapGenerator(int width, int height)
        {
            _width = width;
            _height = height;
            _sheet = YamlSerializer.Deserialize<TileSheet>(Content.Data.Tileset_subtiles_test_atlas);
            _sheet.Load();
        }

        public void Generate(int? seed)
        {
            if (seed != null)
            {
                RNG.setSeed(seed.Value);
            }

            Stage1();
            Stage2();
            Stage3();
            Stage4();
            Stage5();
            Stage6();
            Complete();
        }

        /// <summary>
        /// Generate space partitions
        /// </summary>
        private void Stage1()
        {
            _tree = new BinarySpacePartition(0, 0, _width, _height);
            _tree.Split(300, 6);

            var nodes = new List<BinarySpacePartition>();
            _tree.GetLeafNodes(nodes);

            var totalSpace = nodes.Select(b => b.Bounds)
                .Sum(b => b.Width * b.Height);

            Console.WriteLine($"Actual space: {totalSpace}, Expected space: {_width * _height}");
        }

        /// <summary>
        /// Assign space for rooms in partitions
        /// </summary>
        private void Stage2()
        {
            var nodes = new List<BinarySpacePartition>();
            _tree.GetLeafNodes(nodes);

            _rooms = nodes.Select(FitRectangleIntoPartition).ToList();
        }

        /// <summary>
        /// Connect rooms with connectors
        /// </summary>
        private void Stage3()
        {
            _connectors = new List<Connector>();
            var unconnected = new List<Room>(_rooms);
            var connected = new List<Room>();

            connected.Add(_rooms.randomItem());

            while (unconnected.Count > 0)
            {
                var rect = unconnected.randomItem();
                var closest = GetClosestRoom(connected, rect);

                ConnectRooms(rect.Bounds, closest.Bounds);

                connected.Add(rect);
                unconnected.Remove(rect);
            }
        }

        /// <summary>
        /// Map tiles for rooms
        /// </summary>
        private void Stage4()
        {
            MapTilesForRoom();
        }

        /// <summary>
        /// Smooth rooms out
        /// </summary>
        private void Stage5()
        {
            SmoothRooms();
        }
        /// <summary>
        /// Generate marker rectangles for collision zones
        /// </summary>
        private void Stage6()
        {
            GenerateCollisionRectangles();
        }

        private void GenerateCollisionRectangles()
        {
            var tempBounds = new Dictionary<int, List<RectangleF>>();

            for (var y = 0; y < _height; y++)
            {
                var startX = -1;
                tempBounds[y] = new List<RectangleF>();
                for (var x = 0; x < _width; x++)
                {
                    if (startX < 0 && _tileMap[x, y] != TileType.Floor)
                    {
                        startX = x;
                    }

                    if (startX >= 0 && (x == _width - 1 || _tileMap[x, y] == TileType.Floor))
                    {
                        var horizontalBounds = new RectangleF(startX * Tile.TileWidth, y * Tile.TileHeight, (x - startX) * Tile.TileWidth, Tile.TileHeight);
                        tempBounds[y].Add(horizontalBounds);
                        startX = -1;
                    }
                }
            }

            for (var y = 0; y < _height - 1; y++)
            {
                for (var i = tempBounds[y].Count - 1; i >= 0; i--)
                {
                    var yRect = tempBounds[y][i];
                    var rectMatchingWidth = tempBounds[y + 1]
                        .FirstOrDefault(nextYRect =>
                                nextYRect.left == yRect.left
                             && nextYRect.right == yRect.right);

                    if (rectMatchingWidth == default) continue;

                    var merged = new RectangleF(yRect.x, yRect.y, yRect.width, yRect.height + rectMatchingWidth.height);

                    tempBounds[y].Remove(yRect);
                    tempBounds[y + 1].Remove(rectMatchingWidth);
                    tempBounds[y + 1].Add(merged);
                }
            }

            _colliderBounds = tempBounds.Values.SelectMany(v => v).ToList();
        }

        /// <summary>
        /// Does various things to remove bad placed tiles
        /// </summary>
        private void SmoothRooms()
        {
            Pass1();
            Pass2();
        }

        private void Pass1()
        {
            for (var x = 2; x < _width - 2; x += 1)
            {
                for (var y = 2; y < _height - 2; y += 1)
                {
                    // Walls with floors on either side are replaced with floors
                    if (_tileMap[x, y] == TileType.Wall
                        && _tileMap[x - 1, y] == TileType.Floor
                        && _tileMap[x + 1, y] == TileType.Floor)
                    {
                        _tileMap[x, y] = TileType.Floor;
                    }

                    if (_tileMap[x, y] == TileType.Wall
                        && _tileMap[x, y - 1] == TileType.Floor
                        && _tileMap[x, y + 1] == TileType.Floor)
                    {
                        _tileMap[x, y] = TileType.Floor;
                    }

                    // Double walls with floors on either side are replaced with floors
                    if (_tileMap[x, y] == TileType.Wall
                        && _tileMap[x + 1, y] == TileType.Wall
                        && _tileMap[x - 1, y] == TileType.Floor
                        && _tileMap[x + 2, y] == TileType.Floor)
                    {
                        _tileMap[x, y] = TileType.Floor;
                        _tileMap[x + 1, y] = TileType.Floor;
                    }

                    if (_tileMap[x, y] == TileType.Wall
                        && _tileMap[x, y + 1] == TileType.Wall
                        && _tileMap[x, y - 1] == TileType.Floor
                        && _tileMap[x, y + 2] == TileType.Floor)
                    {
                        _tileMap[x, y] = TileType.Floor;
                        _tileMap[x, y + 1] = TileType.Floor;
                    }
                }
            }
        }
        private void Pass2()
        {
            for (var x = 1; x < _width - 1; x += 1)
            {
                for (var y = 1; y < _height - 1; y += 1)
                {
                    if (_tileMap[x, y] == TileType.Floor
                        && _tileMap[x, y + 1] == TileType.Floor
                        && _tileMap[x, y - 1] == TileType.Floor
                        && _tileMap[x + 1, y] == TileType.Floor
                        && _tileMap[x - 1, y] == TileType.Floor
                        && _tileMap[x - 1, y + 1] == TileType.Floor
                        && _tileMap[x - 1, y - 1] == TileType.Floor
                        && _tileMap[x + 1, y + 1] == TileType.Floor
                        && _tileMap[x + 1, y - 1] == TileType.Floor
                        && RNG.chance(3))
                    {
                        _tileMap[x, y] = TileType.Wall;
                    }
                }
            }
        }

        /// <summary>
        /// Fill in tiles in rooms
        /// </summary>
        private void Complete()
        {
            var tiles = MakeTilesForRoom().ToList();
            Result = new GenerationResult
            {
                Tiles = tiles,
                ColliderBounds = _colliderBounds,
            };
        }

        /// <summary>
        /// Map tiles based on the generated rectangles
        /// </summary>
        private void MapTilesForRoom()
        {
            _tileMap = new TileType[_width, _height];

            var connectorBounds = _connectors.SelectMany(c => c.GetRectangles()).ToList();
            var roomBounds = _rooms.Select(r => r.Bounds);
            foreach (var r in roomBounds.Concat(connectorBounds))
            {
                foreach (var x in Enumerable.Range(r.Left, r.Width))
                {
                    foreach (var y in Enumerable.Range(r.Top, r.Height))
                    {
                        var type = TileType.Floor; // Floor

                        if (x == 0 || x == r.Left || x == r.Right - 1
                            || y == 0 || y == r.Top || y == r.Bottom - 1)
                        {
                            type = TileType.Wall; // Wall
                        }
                        _tileMap[x, y] = (TileType)Math.Max((int)_tileMap[x, y], (int)type);
                    }
                }
            }
        }

        /// <summary>
        /// Connects specified rooms by creating three-part connectors to form a corridor
        /// </summary>
        private void ConnectRooms(Rectangle src, Rectangle dest)
        {
            var leftMost = src.Left > dest.Left ? dest : src;
            var rightMost = leftMost == src ? dest : src;
            var topMost = src.Top > dest.Top ? dest : src;
            var bottomMost = topMost == src ? dest : src;

            var connectorWidth = 4 + RNG.nextInt(2) * 2;

            // Connect horizontally
            var isHorizontallyCloser = rightMost.Left - leftMost.Right > bottomMost.Top - topMost.Bottom;

            if (isHorizontallyCloser)
            {
                var horizontalConWidth = Math.Max((rightMost.Left - leftMost.Right) / 2, connectorWidth) + 2;

                var leftConX = leftMost.Right - 2;
                var leftConY = leftMost.Y + RNG.nextInt(leftMost.Height - connectorWidth);
                var leftConnector = new Rectangle(leftConX, leftConY, horizontalConWidth, connectorWidth);

                var rightConX = leftConnector.Right;
                var rightConY = rightMost.Y + RNG.nextInt(rightMost.Height - connectorWidth);
                var rightConnector = new Rectangle(rightConX, rightConY, horizontalConWidth, connectorWidth);

                var bottomCon = rightConnector.Y > leftConnector.Y ? rightConnector : leftConnector;
                var topCon = bottomCon == leftConnector ? rightConnector : leftConnector;

                var midConX = leftConnector.Right - connectorWidth / 2;
                var midConY = topCon.Y;
                var midConHeight = bottomCon.Bottom - topCon.Top;
                var midConnector = new Rectangle(midConX, midConY, connectorWidth, midConHeight);

                var connector = new Connector
                {
                    A = leftConnector,
                    Mid = midConnector,
                    B = rightConnector
                };

                _connectors.Add(connector);
            }
            // Connect vertically
            else
            {
                var topConX = topMost.X + RNG.nextInt(topMost.Width - connectorWidth);
                var topConY = topMost.Bottom - 2;
                var topConHeight = Math.Max(bottomMost.Top - topMost.Bottom, connectorWidth) / 2 + 2;
                var topConnector = new Rectangle(topConX, topConY, connectorWidth, topConHeight);

                var bottomConX = bottomMost.X + RNG.nextInt(bottomMost.Width - connectorWidth);
                var bottomConY = topConnector.Bottom;
                var bottomConHeight = Math.Max(bottomMost.Top - topMost.Bottom, connectorWidth) / 2 + 3;
                var bottomConnector = new Rectangle(bottomConX, bottomConY, connectorWidth, bottomConHeight);

                var rightCon = bottomConnector.X > topConnector.X ? bottomConnector : topConnector;
                var leftCon = rightCon == topConnector ? bottomConnector : topConnector;

                var midConX = leftCon.X;
                var midConY = topConnector.Bottom - connectorWidth / 2;
                var midConWidth = rightCon.Right - leftCon.Left;
                var midConnector = new Rectangle(midConX, midConY, midConWidth, connectorWidth);

                var connector = new Connector
                {
                    A = topConnector,
                    Mid = midConnector,
                    B = bottomConnector
                };

                _connectors.Add(connector);
            }
        }

        /// <summary>
        ///  Gets closest rectangle to the provided rectangle, measured by hypotenuse from center
        /// </summary>
        private Room GetClosestRoom(List<Room> others, Room room)
        {
            var candidates = others.Where(r => r != room);

            double closestDistance = double.MaxValue;
            var closest = candidates.First();
            foreach (var candidate in candidates)
            {
                var distance = DistanceToRectangle(room.Bounds, candidate.Bounds);
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        /// <summary>
        /// Calculates hypotenuse distance between rectangle a and b
        /// </summary>
        public double DistanceToRectangle(Rectangle a, Rectangle b)
        {
            return Math.Sqrt(Math.Pow(a.Center.X - b.Center.X, 2) + Math.Pow(a.Center.Y - b.Center.Y, 2));
        }

        private IEnumerable<Tile> MakeTilesForRoom()
        {
            var tiles = Enumerable.Range(0, _width)
                .SelectMany(x => Enumerable.Range(0, _height)
                    .Select(y =>
                    {
                        var type = _tileMap[x, y];
                        return new Tile(x, y, type, _sheet.ForType(type, x, y, _tileMap));
                    }));
            return tiles;
        }

        private Room FitRectangleIntoPartition(BinarySpacePartition partition)
        {
            var x = RNG.nextInt(partition.Bounds.Width / 8);
            var y = RNG.nextInt(partition.Bounds.Height / 8);

            var width = partition.Bounds.Width / 2 + RNG.nextInt(partition.Bounds.Width / 2 - x);
            var height = partition.Bounds.Height / 2 + RNG.nextInt(partition.Bounds.Height / 2 - y);

            var bounds = new Rectangle(partition.Bounds.X + x, partition.Bounds.Y + y, width, height);
            var room = new Room()
            {
                Bounds = bounds
            };
            return room;
        }
    }

    public class GenerationResult
    {
        public IList<Tile> Tiles { get; set; }
        public IList<RectangleF> ColliderBounds { get; set; }
    }
}
