using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI.Util
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIParticle : MaskableGraphic
    {
        const float HALF_PI = Mathf.PI * 0.5f;
        readonly UIVertex[] quad = new UIVertex[4];

        [SerializeField] bool withChildren = true;          public bool WithChildren => withChildren;
        ParticleSystem particle = null;                     public ParticleSystem Particle => particle;
        ParticleSystemRenderer particleRenderer = null;     public ParticleSystemRenderer ParticleRenderer => particleRenderer;

        ParticleSystem.MainModule mainModule = default;
        ParticleSystem.Particle[] infos = null;

        public override Texture mainTexture => material == null ? base.mainTexture : material.mainTexture;



        protected UIParticle()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void Awake()
        {
            base.Awake();
            FindComponent();
            Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            particle = null;
            particleRenderer = null;
            infos = null;
        }

        void FindComponent()
        {
            particle = GetComponent<ParticleSystem>();
            if (particle != null)
                particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
        }

        void Initialize()
        {
            if (particle != null)
            {
                mainModule = particle.main;
                if (infos == null
                 || infos.Length != mainModule.maxParticles)
                    infos = new ParticleSystem.Particle[mainModule.maxParticles];
            }

            if (particleRenderer != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif// UNITY_EDITOR
                    particleRenderer.enabled = false;
                SyncTexture();
            }
        }



        void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (infos == null)
                    Initialize();

                SyncTexture();
            }
#endif// UNITY_EDITOR

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif// UNITY_EDITOR
                particle?.Simulate(mainModule.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime, withChildren, false, true);
            UpdateGeometry();
        }

        void SyncTexture()
        {
            if (m_Material == null)
                material = new Material(defaultGraphicMaterial);

            var matPtc = particleRenderer == null ? null : particleRenderer.sharedMaterial;
            m_Material.mainTexture = matPtc == null ? null : matPtc.mainTexture;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!IsActive()) return;
            if (particle == null) return;
            if (infos == null) return;

            var modSheetAni = particle.textureSheetAnimation;
            var numTilesX = modSheetAni.numTilesX;
            var numTilesY = modSheetAni.numTilesY;
            int totalTiles = numTilesX * numTilesY;
            var ratioTileX = 1f / numTilesX;
            var ratioTileY = 1f / numTilesY;

            var count = particle.GetParticles(infos);
            for (int index = 0; index < count; ++index)
            {
                var info = infos[index];

                // Get position
                Vector2 position;
                switch (mainModule.simulationSpace)
                {
                case ParticleSystemSimulationSpace.Custom:
                    position = mainModule.customSimulationSpace == null || mainModule.customSimulationSpace == transform
                             ? info.position
                             : transform.InverseTransformPoint(mainModule.customSimulationSpace.TransformPoint(info.position));
                    break;

                case ParticleSystemSimulationSpace.World: position = transform.InverseTransformPoint(info.position); break;
                default: position = info.position; break;
                }

                if (mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                    position /= canvas.scaleFactor;

                // Get uv
                Vector4 uv;
                if (modSheetAni.enabled)
                {
                    var frameOverTime = modSheetAni.frameOverTime;
                    var startFrame = modSheetAni.startFrame;

                    float progress = 1f - (info.remainingLifetime / info.startLifetime);
                    progress =  Mathf.Repeat(frameOverTime.Evaluate(progress) + startFrame.Evaluate(progress), 1f);

                    int frame = 0;
                    switch (modSheetAni.animation)
                    {
                    case ParticleSystemAnimationType.WholeSheet: frame = Mathf.FloorToInt(progress * totalTiles); break;
                    case ParticleSystemAnimationType.SingleRow: frame = Mathf.FloorToInt(progress * numTilesX) + (modSheetAni.rowIndex * numTilesX); break;
                    }
                    frame %= totalTiles;

                    var divX = frame / numTilesX;
                    var remX = frame % numTilesX;
                    uv.x = remX * ratioTileX;
                    uv.z = uv.x + ratioTileX;
                    uv.w = 1f - (divX * ratioTileY);
                    uv.y = uv.w - ratioTileY;
                }
                else
                    uv = new Vector4(0, 0, 1, 1);


                // Set quad
                var vert = UIVertex.simpleVert;
                vert.color = info.GetCurrentColor(particle);

                vert.uv0.x = uv.x; vert.uv0.y = uv.y; quad[0] = vert;
                vert.uv0.x = uv.x; vert.uv0.y = uv.w; quad[1] = vert;
                vert.uv0.x = uv.z; vert.uv0.y = uv.w; quad[2] = vert;
                vert.uv0.x = uv.z; vert.uv0.y = uv.y; quad[3] = vert;


                // Calc position
                Vector2 halfSize = info.GetCurrentSize3D(particle) * 0.5f;
                float rotation = -info.rotation * Mathf.Deg2Rad;
                if (rotation == 0)
                {
                    var posLB = position - halfSize;
                    var posRT = position + halfSize;

                    Vector2 pos;
                    quad[0].position = posLB;

                    pos.x = posLB.x; pos.y = posRT.y;
                    quad[1].position = pos;
                    quad[2].position = posRT;

                    pos.x = posRT.x; pos.y = posLB.y;
                    quad[3].position = pos;
                }
                else
                {
                    float rotation90 = rotation + HALF_PI;
                    var right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * halfSize;
                    var up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * halfSize;
                    var plus = right + up;
                    var minus = right - up;

                    quad[0].position = position - plus;
                    quad[1].position = position - minus;
                    quad[2].position = position + plus;
                    quad[3].position = position + minus;
                }

                vh.AddUIVertexQuad(quad);
            }
        }

        public void Replay()
        {
            if (particle == null) return;
            particle.Simulate(0, withChildren, true);
            particle.Play(withChildren);
        }



#if UNITY_EDITOR
        [SerializeField] bool edit_ignoreShapeRotationError = false;

        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
            FindComponent();
            Initialize();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (particle == null)
                return;

            if (!edit_ignoreShapeRotationError)
            {
                if (particle.shape.rotation.x != 0f
                 || particle.shape.rotation.y != 0f)
                    Debug.LogError($"{nameof(UIParticle)}({name}) : x or y of {nameof(particle.shape)}.{nameof(particle.shape.rotation)} is not zero {particle.shape.rotation}");
            }
        }
#endif// UNITY_EDITOR
    }
}