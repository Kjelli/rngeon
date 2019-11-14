using Microsoft.Xna.Framework;
using NewGame.Shared.Data;
using NewGame.Shared.Entities;
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
        private readonly TileSheet _sheet;
        private readonly int? _seed;
        private readonly int _width;
        private readonly int _height;
        private readonly Point _minRoomSize;
        private readonly Point _maxRoomSize;

        private BinarySpacePartition _tree;


        private IList<Room> _rooms;
        private IList<Connector> _connectors;
        private IList<RectangleF> _colliderBounds;
        private TileType[,] _tileMap;
        private IList<Entity> _entities;


        public GenerationResult Result { get; private set; }

        public DungeonMapGenerator(DungeonMapSettings settings)
        {
            _seed = settings.Seed;
            _width = settings.Width;
            _height = settings.Height;
            _minRoomSize = settings.MinRoomSize;
            _maxRoomSize = settings.MaxRoomSize;
            _sheet = YamlSerializer.Deserialize<TileSheet>(settings.TileSheet);
            _sheet.Load();
        }

        /// <summary>
        /// Initiates random seed, and generates a map to be populated in the Result property
        /// </summary>
        public void Generate()
        {
            if (_seed != null)
            {
                RNG.setSeed(_seed.Value);
            }

            Stage1();
            Stage2();
            Stage3();
            Stage4();
            Stage5();
            Stage6();
            Stage7();
            Complete();
        }

        /// <summary>
        /// Generate space partitions
        /// </summary>
        private void Stage1()
        {
            _tree = new BinarySpacePartition(0, 0, _width, _height);
            _tree.Split(_minRoomSize, _maxRoomSize);
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
            CreateRoomConnections();
            EnsureAllRoomsConnected();
        }

        /// <summary>
        /// For all rooms, create a new connection to another
        /// </summary>

        private void CreateRoomConnections()
        {
            var shuffledRooms = new List<Room>(_rooms);
            shuffledRooms.shuffle();
            foreach (var room in shuffledRooms)
            {
                var closest = GetClosestNewConnectionForRoom(shuffledRooms, room);
                ConnectRooms(closest, room);
            }
        }

        /// <summary>
        /// If not all rooms are accessible, creates new connections until they are
        /// </summary>
        private void EnsureAllRoomsConnected()
        {
            var roomsFound = new HashSet<Room>();
            var roomsToSearch = new List<Room>();

            do
            {
                roomsFound.Clear();
                roomsToSearch.Clear();
                // Search from first room
                roomsToSearch.Add(_rooms.First());

                // Add every connection to rooms found
                for (var i = 0; i < roomsToSearch.Count; i++)
                {
                    var current = roomsToSearch[i];
                    // Add connected rooms we haven't seen before
                    var newConnections = current.Connections
                        .Where(connection => !roomsFound.Contains(connection));

                    if (newConnections.Count() > 0)
                    {
                        roomsToSearch.AddRange(newConnections);
                        i = 0;
                    }
                    roomsFound.Add(current);
                    roomsToSearch.Remove(current);
                }

                // If first room didn't have any connections
                if (roomsFound.Count == 1)
                {
                    // Connect it to a random room
                    var closestFound = GetClosestNewConnectionForRoom(_rooms, roomsFound.First());
                    ConnectRooms(roomsFound.First(), closestFound);
                    continue;
                }

                // If we did not find all rooms
                if (roomsFound.Count != _rooms.Count)
                {
                    // Connect a room we didn't find, to closest room of the ones we found
                    var roomNotFound = _rooms.Except(roomsFound).First();
                    var closestFound = GetClosestNewConnectionForRoom(roomsFound, roomNotFound);
                    ConnectRooms(roomNotFound, closestFound);
                }
            } while (roomsFound.Count != _rooms.Count); // Repeat until all rooms are found
        }


        /// <summary>
        /// Connects specified rooms by creating connectors to form a corridor
        /// </summary>
        private void ConnectRooms(Room a, Room b)
        {
            var leftMost = a.Bounds.Center.X < b.Bounds.Center.X ? a.Bounds : b.Bounds;
            var rightMost = a.Bounds == leftMost ? b.Bounds : a.Bounds;
            var topMost = a.Bounds.Center.Y < b.Bounds.Center.Y ? a.Bounds : b.Bounds;
            var bottomMost = a.Bounds == topMost ? b.Bounds : a.Bounds;

            var size = RNG.choose(5, 7);
            var midPoint = new Point(rightMost.Center.X + size / 2, bottomMost.Center.Y - size / 2);

            var horizontalCorridor = new Rectangle(leftMost.Center.X - size / 2, midPoint.Y - size / 2, midPoint.X - leftMost.Center.X + size / 2, size);
            var verticalCorridor = new Rectangle(topMost.Center.X - size / 2, topMost.Center.Y - size / 2, size, midPoint.Y - topMost.Center.Y + size);

            a.Connect(b);

            _connectors.Add(new Connector(CropRectangle(horizontalCorridor), CropRectangle(verticalCorridor)));
        }

        /// <summary>
        /// Guard for attempting to place rooms/connections outside the map bounds
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private Rectangle CropRectangle(Rectangle rect)
        {
            var copy = rect;
            while (copy.X < 0)
            {
                copy = new Rectangle(copy.X + 1, copy.Y, copy.Width - 1, copy.Height);
            }

            while (copy.Y < 0)
            {
                copy = new Rectangle(copy.X, copy.Y + 1, copy.Width, copy.Height + 1);
            }
            return copy;
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
                        var horizontalBounds = new RectangleF(startX * Tile.Width, y * Tile.Height, (x - startX) * Tile.Width, Tile.Height);
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
            //Pass2();
        }

        /// <summary>
        /// Removes 1x1, 2x1, and 1x2 walls when floors are on either side
        /// </summary>
        private void Pass1()
        {
            for (var i = 0; i < 2; i++)
            {
                for (var x = 2; x < _width - 2; x += 1)
                {
                    for (var y = 2; y < _height - 2; y += 1)
                    {
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

                        // Walls with floors on either side are replaced with floors
                        if (_tileMap[x, y] == TileType.Wall
                            && _tileMap[x - 1, y] == TileType.Floor
                            && _tileMap[x + 1, y] == TileType.Floor)
                        {
                            _tileMap[x, y] = TileType.Floor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fill in tiles. Populate result with tiles, entities and colliders
        /// </summary>
        private void Complete()
        {
            var tiles = MakeTilesForRoom().ToList();
            Result = new GenerationResult
            {
                Tiles = tiles,
                ColliderBounds = _colliderBounds,
                Entities = _entities
            };
        }

        /// <summary>
        /// Map tiles based on the generated rectangles
        /// </summary>
        private void MapTilesForRoom()
        {
            _tileMap = new TileType[_width, _height];

            var connectorBounds = _connectors.SelectMany(c => c.Rectangles).ToList();
            var roomBounds = _rooms.Select(r => r.Bounds);
            foreach (var r in connectorBounds)
            {
                foreach (var x in Enumerable.Range(r.Left, r.Width))
                {
                    foreach (var y in Enumerable.Range(r.Top, r.Height))
                    {
                        var type = TileType.Floor; // Floor

                        if (x == 0 || x == _width - 1 || x == r.Left || x == r.Right - 1
                            || y == 0 || y == _height - 1 || y == r.Top || y == r.Bottom - 1)
                        {
                            type = TileType.Wall; // Wall
                        }
                        _tileMap[x, y] = (TileType)Math.Max((int)_tileMap[x, y], (int)type);
                    }
                }
            }

            foreach (var r in roomBounds)
            {
                foreach (var x in Enumerable.Range(r.Left, r.Width))
                {
                    foreach (var y in Enumerable.Range(r.Top, r.Height))
                    {
                        var type = TileType.Floor; // Floor

                        if (x == 0 || x == _width - 1 || x == r.Left || x == r.Right - 1
                            || y == 0 || y == _height - 1 || y == r.Top || y == r.Bottom - 1)
                        {
                            type = TileType.Wall; // Wall
                        }
                        _tileMap[x, y] = (TileType)Math.Max((int)_tileMap[x, y], (int)type);
                    }
                }
            }
        }

        /// <summary>
        ///  Gets closest room to the provided room (and its connected rooms), measured by hypotenuse from center
        /// </summary>
        private Room GetClosestNewConnectionForRoom(ICollection<Room> others, Room room)
        {
            var candidates = others
                .Where(other => other != room)
                .Where(other => !room.Connections.Contains(other));

            double closestDistance = double.MaxValue;
            var closest = candidates.First(other => other != room);

            foreach (var candidate in candidates)
            {
                var subCandidates = candidate.Connections
                    .Where(c => c != room)
                    .Where(c => !c.Connections.Contains(room))
                    .Where(c => others.Contains(c));
                foreach (var subCandidate in candidates.Concat(subCandidates))
                {
                    var distance = DistanceToRectangle(room.Bounds, subCandidate.Bounds);
                    if (distance < closestDistance)
                    {
                        closest = subCandidate;
                        closestDistance = distance;
                    }
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

        /// <summary>
        /// Populates the tilemap with tile objects, resolved from a spritesheet
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tile> MakeTilesForRoom()
        {
            var tiles = Enumerable.Range(0, _width)
                .SelectMany(x => Enumerable.Range(0, _height)
                    .Select(y =>
                    {
                        return new Tile(x, y, _tileMap[x, y], _sheet.ForType(_tileMap[x, y], x, y, _tileMap));
                    }));
            return tiles;
        }

        /// <summary>
        /// For each partition, fit a rectangle into it, leaving a certain space around it for variation
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
        private Room FitRectangleIntoPartition(BinarySpacePartition partition)
        {
            var wiggleWidth = (partition.Bounds.Width - _minRoomSize.X) / 2;
            var wiggleHeight = (partition.Bounds.Height - _minRoomSize.Y) / 2;

            var width = _minRoomSize.X + RNG.nextInt(wiggleWidth);
            var height = _minRoomSize.Y + RNG.nextInt(wiggleHeight);

            var x = RNG.nextInt(wiggleWidth);
            var y = RNG.nextInt(wiggleHeight);


            var bounds = new Rectangle(partition.Bounds.X + x, partition.Bounds.Y + y, width, height);
            var room = new Room()
            {
                Bounds = bounds
            };
            return room;
        }

        /// <summary>
        /// Place torches
        /// </summary>
        private void Stage7()
        {
            _entities = new List<Entity>();
            foreach (var room in _rooms)
            {
                var chance = 0.05f;
                foreach (var x in Enumerable.Range(room.Bounds.X, room.Bounds.Width))
                {
                    foreach (var y in Enumerable.Range(room.Bounds.Y, room.Bounds.Height))
                    {
                        if (x != room.Bounds.Left && x != room.Bounds.Right - 1 && y != room.Bounds.Top && y != room.Bounds.Bottom - 1) continue;
                        if (x == room.Bounds.Left && (y == room.Bounds.Bottom - 1 || y == room.Bounds.Top)) continue;
                        if (x == room.Bounds.Right - 1 && (y == room.Bounds.Bottom - 1 || y == room.Bounds.Top)) continue;
                        if (_tileMap[x, y] != TileType.Wall) continue;

                        if (RNG.chance(chance))
                        {
                            _entities.Add(EntityFactory.Presets
                                .Torch()
                                .AtPosition(x * Tile.Width + 8, y * Tile.Height + 8)
                                .Create());

                            chance = 0f;
                        }
                        else
                        {
                            chance += 0.015f;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// The result of the generation. Contains tiles, colliderbounds and other entities of interest.
    /// </summary>
    public class GenerationResult
    {
        public IList<Tile> Tiles { get; set; }
        public IList<RectangleF> ColliderBounds { get; set; }
        public IList<Entity> Entities { get; set; }
    }
}
