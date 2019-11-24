using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities.Components;
using NewGame.Shared.Entities.Components.Generation;
using NewGame.Shared.Entities.Props;
using Nez;
using Nez.DeferredLighting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.Entities
{
    public class DungeonMap : Entity
    {
        private DungeonMapGenerator _generator;

        private int _mapWidth;
        private int _mapHeight;

        public List<Tile[,]> TileLayers;

        public Spawn Spawn { get; set; }
        public Exit Exit { get; set; }

        public void Generate(DungeonMapGeneratorSettings settings)
        {
            _mapWidth = settings.Width;
            _mapHeight = settings.Height;

            _generator = new DungeonMapGenerator(settings);
            _generator.Generate();

            var result = _generator.Result;

            TileLayers = new List<Tile[,]>();

            var normalMap = Core.Content.Load<Texture2D>(Content.Textures.Tileset_subtiles_test_normal);
            var material = new DeferredSpriteMaterial(normalMap);

            foreach (var tileGroup in result.Tiles)
            {
                var layer = new Tile[_mapWidth, _mapHeight];
                layer.Initialize();

                foreach (var tile in tileGroup)
                {
                    layer[tile.TilePosition.X, tile.TilePosition.Y] = tile;
                }
                TileLayers.Add(layer);

                var layerRenderer = new DungeonMapLayerRenderer();
                var renderLayer = DetermineRenderLayerForGroup(tileGroup.Key);
                layerRenderer.SetRenderLayer(renderLayer);
                layerRenderer.SetMaterial(material);
                layerRenderer.SetTiles(layer);

                AddComponent(layerRenderer);
            }

            foreach (var Entity in result.Entities)
            {
                Core.Scene.AddEntity(Entity);
            }

            foreach (var colliderBounds in result.ColliderBounds)
            {
                AddComponent(new BoxCollider(colliderBounds));
            }

            Spawn = result.Entities.First(e => e is Spawn) as Spawn;
            Exit = result.Entities.First(e => e is Exit) as Exit;
        }

        private int DetermineRenderLayerForGroup(TileType type)
        {
            if (type == TileType.Floor)
            {
                return Constants.RenderLayerMapFloor;
            }
            else
            {
                return Constants.RenderLayerMapWall;
            }
        }

        public Tile GetTileAtPosition(float x, float y)
        {
            var xi = (int)Math.Floor(x / Tile.Width);
            var yi = (int)Math.Floor(y / Tile.Height);
            return TileLayers.FirstOrDefault(l => l[xi, yi] != null)?[xi, yi];
        }
    }
}
