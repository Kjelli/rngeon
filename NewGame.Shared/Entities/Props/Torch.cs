using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.DeferredLighting;
using Nez.Particles;

namespace NewGame.Shared.Entities
{
    public class Torch : Entity
    {
        public const float BaseIntensity = 3.00f;
        public const float BaseRadius = 155f;
        public static readonly Color BaseColor = Color.Orange;

        private float _flickerOffset;
        private float _flickerSpeed;
        private float _flickerIntensity = 0.4f;

        private ParticleEmitter _emitter;

        public Color Color { get; set; } = BaseColor;

        public Torch()
        {
            _flickerOffset = Random.NextFloat() * 10f;
            _flickerSpeed = 500f + Random.NextFloat() * 300f;
        }

        public override void OnAddedToScene()
        {
            _emitter = new ParticleEmitter(new ParticleEmitterConfig
            {
                BlendFuncSource = Blend.BlendFactor,
                Angle = 270,
                AngleVariance = 15,
                Speed = 10f,
                SpeedVariance = 3f,
                RotatePerSecond = 3f,
                StartColor = Color,
                StartColorVariance = Color,
                FinishColor = Color.Black,
                FinishParticleSize = 0f,
                StartParticleSize = 2f,
                EmissionRate = 1.5f + Random.NextFloat() * 0.5f,
                Duration = -1,
                Gravity = new Vector2(0, 8f),
                SourcePosition = Position,
                ParticleLifespan = 3f,
                ParticleLifespanVariance = 1f,
                EmitterType = ParticleEmitterType.Gravity,
                MaxParticles = 10000,
            });

            _emitter.SetRenderLayer(Constants.RenderLayerProps);
            AddComponent(_emitter);
        }


        public override void Update()
        {
            base.Update();
            var flickerFactor = Mathf.Sin(Time.TimeSinceSceneLoad * Time.DeltaTime * _flickerSpeed + _flickerOffset);
            var newIntensity = BaseIntensity - _flickerIntensity * flickerFactor + 1f;
            GetComponent<PointLight>().SetIntensity(newIntensity);
        }
    }

    public enum TorchAnimation
    {
        Flickering
    }
}
