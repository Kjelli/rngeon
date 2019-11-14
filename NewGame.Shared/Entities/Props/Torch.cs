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
        public static readonly Color Color = Color.Orange;

        private float _flickerOffset;
        private float _flickerSpeed;
        private float _flickerIntensity = 0.4f;

        private ParticleEmitter _emitter;

        public Torch()
        {
            _flickerOffset = Random.nextFloat() * 10f;
            _flickerSpeed = 500f + Random.nextFloat() * 300f;
            _emitter = new ParticleEmitter(new ParticleEmitterConfig
            {
                blendFuncSource = Blend.BlendFactor,
                angle = 270,
                angleVariance = 15,
                speed = 10f,
                speedVariance = 3f,
                rotatePerSecond = 3f,
                startColor = Color.Orange,
                finishColor = Color.Black,
                finishParticleSize = 0f,
                startParticleSize = 2f,
                emissionRate = 0.5f + Random.nextFloat() * 0.5f,
                duration = -1,
                gravity = new Vector2(0, 8f),
                sourcePosition = position,
                particleLifespan = 3f,
                particleLifespanVariance = 1f,
                emitterType = ParticleEmitterType.Gravity,
                maxParticles = 10000,
            });

            _emitter.renderLayer = Constants.RenderLayerProps;
            addComponent(_emitter);
        }


        public override void update()
        {
            base.update();
            getComponent<PointLight>().setIntensity(BaseIntensity - _flickerIntensity * (Mathf.sin(Time.time * Time.deltaTime * _flickerSpeed + _flickerOffset) + 1f));
        }
    }

    public enum TorchAnimation
    {
        Flickering
    }
}
