using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components;
using NewGame.Shared.Components.Generation;
using Nez;
using Nez.DeferredLighting;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;

namespace NewGame.Shared.Entities
{
    public static class EntityFactory
    {
        public static EntityBuilder<Player> Player()
        {
            return new EntityBuilder<Player>();
        }

        private static EntityBuilder<Map> Map()
        {
            return new EntityBuilder<Map>();
        }

        private static EntityBuilder<MiniMap> Minimap()
        {
            return new EntityBuilder<MiniMap>();
        }

        private static EntityBuilder<Torch> Torch()
        {
            return new EntityBuilder<Torch>();
        }

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
                var texture = Core.content.Load<Texture2D>(Content.Textures.Test);

                var sprite = new Sprite(texture)
                {
                    renderLayer = Constants.RenderLayerPlayer,
                    material = new DeferredSpriteMaterial(TextureUtils.createNormalMap(texture, TextureUtils.EdgeDetectionFilter.FiveTap))
                };

                var light = new PointLight(Color.Orange)
                    .setRadius(250f)
                    .setIntensity(1.8f)
                    .setZPosition(2);

                light.renderLayer = Constants.RenderLayerLight;

                return EntityFactory.Player()
                    .With<Velocity>()
                    .With<Mover>()
                    .With(new CircleCollider(4f))
                    .With(light)
                    .With(sprite);
            }

            public static EntityBuilder<Map> Map()
            {
                return EntityFactory.Map()
                    .With<ExplorableTerrainComponent>();
            }

            public static EntityBuilder<Map> DungeonMap(DungeonMapSettings settings)
            {
                var dungeonMap = new DungeonMapComponent()
                {
                    renderLayer = Constants.RenderLayerMap,
                    material = new DeferredSpriteMaterial(Core.content.Load<Texture2D>(Content.Textures.Tileset_subtiles_test_normal))
                };

                dungeonMap.Generate(settings);

                return EntityFactory.Map()
                    .With(dungeonMap);
            }

            public static EntityBuilder<MiniMap> Minimap(Tile[,] tiles)
            {
                var minimap = new MiniMapComponent()
                {
                    renderLayer = Constants.RenderLayerScreenSpace
                };

                minimap.Build(tiles);

                return EntityFactory.Minimap()
                    .With(minimap);
            }

            public static EntityBuilder<Torch> Torch()
            {
                // Add sprite with animation
                var sprite = new Sprite<TorchAnimation>
                {
                    renderLayer = Constants.RenderLayerProps
                };
                var propsTileset = Core.content.Load<Texture2D>(Content.Textures.Tileset_props);
                var subTextures = Subtexture.subtexturesFromAtlas(propsTileset, 8, 8);
                sprite.addAnimation(TorchAnimation.Flickering, new SpriteAnimation(new List<Subtexture>
                {
                    subTextures[0], subTextures[1]
                }));
                sprite.play(TorchAnimation.Flickering);

                // Add light effect
                var light = new PointLight(Entities.Torch.Color)
                    .setRadius(Entities.Torch.BaseRadius)
                    .setZPosition(2);
                light.renderLayer = Constants.RenderLayerLight;

                return EntityFactory.Torch()
                    .With(sprite)
                    .With(light);
            }
        }


    }

    public class EntityBuilder<T> where T : Entity, new()
    {
        private readonly List<Component> _components;
        private Vector2 _initialPosition;

        public EntityBuilder()
        {
            _components = new List<Component>();
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

        public EntityBuilder<T> AddInput<C>() where C : InputController, new()
        {
            return With<C>();
        }

        public T Create()
        {
            var entity = new T();
            entity.setPosition(_initialPosition);
            foreach (var component in _components)
            {
                entity.addComponent(component);
            }
            return entity;
        }


    }
}
