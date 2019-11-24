using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities.Components;
using NewGame.Shared.Entities.Components.Generation;
using NewGame.Shared.Entities.Props;
using Nez;
using Nez.DeferredLighting;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;

namespace NewGame.Shared.Entities
{
    public static class EntityFactory
    {
        public static class Presets
        {
            public static EntityBuilder<Player> Player()
            {
                return UntrackedPlayer()
                    .With<MiniMapTracker>()
                    .With<CameraTracker>();
            }

            public static EntityBuilder<Player> UntrackedPlayer()
            {
                var texture = Core.Content.Load<Texture2D>(Content.Textures.Test);

                var sprite = new SpriteRenderer(texture)
                    .SetRenderLayer(Constants.RenderLayerPlayer)
                    .SetMaterial(new DeferredSpriteMaterial(TextureUtils.CreateNormalMap(texture, TextureUtils.EdgeDetectionFilter.FiveTap)));

                var light = new PointLight(Color.White)
                    .SetRadius(250f)
                    .SetIntensity(1.8f)
                    .SetZPosition(2);
                light.RenderLayer = Constants.RenderLayerLight;

                return new EntityBuilder<Player>()
                    .With<Velocity>()
                    .With<Mover>()
                    .With(new CircleCollider(4f))
                    .With<PlayerCollisionTriggerListener>()
                    .With(light)
                    .With(sprite);
            }

            public static EntityBuilder<DungeonMap> DungeonMap()
            {
                return new EntityBuilder<DungeonMap>();
            }

            public static EntityBuilder<MiniMap> Minimap(List<Tile[,]> layers)
            {
                var minimap = new MiniMapComponent()
                    .SetRenderLayer(Constants.RenderLayerScreenSpace) as MiniMapComponent;

                minimap.Build(layers);

                return new EntityBuilder<MiniMap>()
                    .With(minimap);
            }

            public static EntityBuilder<Spawn> Spawn()
            {
                var propAtlas = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props);
                var propAtlasNormal = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props_normal);
                var sprite = Sprite.SpritesFromAtlas(propAtlas, 16, 16)[5];

                var spriteRenderer = new SpriteRenderer(sprite)
                    .SetRenderLayer(Constants.RenderLayerProps)
                    .SetMaterial(new DeferredSpriteMaterial(propAtlasNormal));

                // :: TODO - Ambient color from previous dungeon?
                var light = new PointLight(Color.Black)
                    .SetIntensity(1.0f)
                    .SetRadius(255f)
                    .SetZPosition(4);
                light.LocalOffset = new Vector2(0, -Tile.Height / 2);
                light.RenderLayer = Constants.RenderLayerLight;

                return new EntityBuilder<Spawn>()
                    .With(spriteRenderer)
                    .With(light);
            }

            public static EntityBuilder<Exit> Exit()
            {
                var propAtlas = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props);
                var propAtlasNormal = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props_normal);
                var sprite = Sprite.SpritesFromAtlas(propAtlas, 16, 16)[4];

                var spriteRenderer = new SpriteRenderer(sprite)
                    .SetRenderLayer(Constants.RenderLayerProps)
                    .SetMaterial(new DeferredSpriteMaterial(propAtlasNormal));

                var collider = new BoxCollider(Tile.Width / 2, Tile.Height / 2)
                {
                    IsTrigger = true
                };

                return new EntityBuilder<Exit>()
                    .With(spriteRenderer)
                    .With(collider);
            }

            public static EntityBuilder<Torch> Torch(Color? torchColor = null)
            {
                // Add sprite with animation

                var propAtlas = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props);
                var propAtlasNormal = Core.Content.Load<Texture2D>(Content.Textures.Tileset_props_normal);

                var animation = new SpriteAnimator()
                    .SetRenderLayer(Constants.RenderLayerProps)
                    .SetMaterial(new DeferredSpriteMaterial(propAtlasNormal)) as SpriteAnimator;

                var subTextures = Sprite.SpritesFromAtlas(propAtlas, 8, 8);
                animation.AddAnimation(nameof(TorchAnimation.Flickering), new SpriteAnimation(new Sprite[]
                {
                    subTextures[0], subTextures[1]
                }, 4));
                animation.Play(nameof(TorchAnimation.Flickering));

                // Add light effect
                var light = new PointLight(torchColor ?? Entities.Torch.BaseColor)
                    .SetRadius(Entities.Torch.BaseRadius)
                    .SetZPosition(3);
                light.RenderLayer = Constants.RenderLayerLight;

                var torch = new EntityBuilder<Torch>()
                    .With(animation)
                    .With(light);

                if (torchColor.HasValue)
                {
                    torch = torch.With(t => t.Color = torchColor.Value);

                }

                return torch;
            }
        }
    }

    public class EntityBuilder<T> where T : Entity, new()
    {
        private readonly List<Component> _components;
        private readonly List<Action<T>> _actions;

        private Vector2 _initialPosition;

        public EntityBuilder()
        {
            _components = new List<Component>();
            _actions = new List<Action<T>>();
        }

        public EntityBuilder<T> With<C>(C component) where C : Component
        {
            _components.Add(component);
            return this;
        }


        public EntityBuilder<T> With<C>() where C : Component, new()
        {
            With(new C());
            return this;
        }

        public EntityBuilder<T> AtPosition(Vector2 position)
        {
            _initialPosition = position;
            return this;
        }

        public EntityBuilder<T> AtPosition(float x, float y)
        {
            return AtPosition(new Vector2(x, y));
        }

        public EntityBuilder<T> AtTilePosition(int x, int y)
        {
            return AtPosition(new Vector2(Tile.Width * (x + 0.5f), Tile.Height * (y + 0.5f)));
        }

        public EntityBuilder<T> AtTilePosition(Point position)
        {
            position.Deconstruct(out int x, out int y);
            return AtPosition(new Vector2(Tile.Width * (x + 0.5f), Tile.Height * (y + 0.5f)));
        }

        public EntityBuilder<T> AddInput<C>() where C : InputController, new()
        {
            return With<C>();
        }

        public EntityBuilder<T> With(Action<T> func)
        {
            _actions.Add(func);
            return this;
        }

        public T Create()
        {
            var entity = new T();
            foreach (var action in _actions)
            {
                action.Invoke(entity);
            }
            entity.SetPosition(_initialPosition);
            foreach (var component in _components)
            {
                entity.AddComponent(component);
            }
            return entity;
        }


    }
}
