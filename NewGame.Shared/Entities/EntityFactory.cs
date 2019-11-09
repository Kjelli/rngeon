using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace NewGame.Shared.Entities
{
    public static class EntityFactory
    {
        public static EntityBuilder<Player> Player()
        {
            return new EntityBuilder<Player>();
        }

        public static class Presets
        {
            public static EntityBuilder<Player> Player()
            {
                var texture = Core.content.Load<Texture2D>(Content.Textures.Test);

                return EntityFactory.Player()
                    .With<Velocity>()
                    .With<Mover>()
                    .With<CameraTracker>()
                    .With(new CircleCollider(8f))
                    .With(new Sprite(texture));
            }

            public static EntityBuilder<Map> Map()
            {
                return EntityFactory.Map()
                    .With<ExplorableTerrainComponent>();
            }

            public static EntityBuilder<Map> DungeonMap(int width, int height, int? seed = null)
            {
                return EntityFactory.Map()
                    .With(new DungeonMapComponent(width, height, seed));
            }
        }

        private static EntityBuilder<Map> Map()
        {
            return new EntityBuilder<Map>();
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
            With<C>(new C());
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
