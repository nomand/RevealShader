using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runningtap
{
    [RequireComponent(typeof(BoxCollider))]
    public class Painter : MonoBehaviour
    {
        public enum Mode
        {
            auto,
            manual
        }

        public enum Resolution
        {
            _32 = 32, _64 = 64, _128 = 128, _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048
        }

        [Tooltip("Auto mode will iterate through all the objectes to find the bounds. Manual will use Box Collider attached as world bounds. Use manual for complex scenes with a lot of objects.")]
        public Mode LookupMode = Mode.auto;
        public Resolution SplatResolution = Resolution._256;
        [Tooltip("Splat Resolution ratio according to world width/length ratio. Longest side will use resolution selected. Helps minimize projeciton stretching.")]
        public bool UseRelative;
        public bool FadeOverTime;

        public Transform Brush;
        [Range(0, 1)]
        public float BrushSize = 1;
        [Range(0, 1)]
        public float BrushStrength = 1;
        [Range(0, 1)]
        public float FadeSpeed = 1;

        public GameObject World;
        public BoxCollider ManualBounds;

        public Shader RenderTextureShader;

        private Bounds WorldBounds = new Bounds(Vector3.zero, Vector3.zero);
        private Vector4 shaderInfo; //XY = Bounds worldspace offset, ZW = Bounds scale.
        private RenderTexture splatmap;
        private int width;
        private int height;
        private Material drawMaterial;
        private Renderer[] renderers;

        //Shader Strings
        private const string rtxCoord = "_Coordinate";
        private const string rtxStrength = "_Strength";
        private const string rtxSize = "_Size";
        private const string rtxFade = "_Fade";

        private const string revColor = "_Color";
        private const string revRTX = "_Splat";
        private const string revRTXLocaiton = "_SplatLocation";
        private const string revRTXRemap = "_SplatRemap";

        public RenderTexture DebugRenderTexture
        {
            get { return splatmap; }
        }

        void Start()
        {
            ManualBounds = GetComponent<BoxCollider>();
            renderers = World.GetComponentsInChildren<Renderer>();

            drawMaterial = new Material(RenderTextureShader);
            drawMaterial.SetVector(revColor, Color.red);

            CalculateBounds();
            CalculateResolution();

            splatmap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);

            SetShaders();
            ManualBounds.enabled = false; // Box collider is only needed in editor to compute bounds. Turning off to remove it from physics.
        }

        // Calculate bounds of paintable objects
        void CalculateBounds()
        {
            if(LookupMode == Mode.auto)
            {
                foreach (var entity in renderers)
                    WorldBounds.Encapsulate(entity.bounds);

                if (ManualBounds != null) // Reshape box collider to visualize bounds in auto mode.
                {
                    ManualBounds.center = WorldBounds.center;
                    ManualBounds.size = WorldBounds.extents * 2;
                }
            }
            else
                WorldBounds = ManualBounds.bounds;

            shaderInfo = new Vector4
            (
                WorldBounds.center.x - WorldBounds.size.x / 2,
                WorldBounds.center.z - WorldBounds.size.z / 2,
                WorldBounds.size.x,
                WorldBounds.size.z
            );
        }

        // Calculate Splatmap resolution ratio if set to Use Relative.
        void CalculateResolution()
        {
            if (UseRelative)
            {
                if (WorldBounds.size.x > WorldBounds.size.z)
                {
                    var ratio = WorldBounds.size.z / WorldBounds.size.x;
                    width = (int)SplatResolution;
                    height = Mathf.RoundToInt(width * ratio);
                }
                else
                {
                    var ratio = WorldBounds.size.x / WorldBounds.size.z;
                    height = (int)SplatResolution;
                    width = Mathf.RoundToInt(height * ratio);
                }
            }
            else
            {
                width = (int)SplatResolution;
                height = (int)SplatResolution;
            }
        }

        // Update drawable shaders.
        void SetShaders()
        {
            foreach (var entity in renderers)
            {
                if (entity.material.HasProperty(revRTX))
                {
                    entity.material.SetTexture(revRTX, splatmap);
                    entity.material.SetVector(revRTXLocaiton, shaderInfo);
                    entity.material.SetVector(revRTXRemap, shaderInfo);
                }
                else
                    Debug.Log("GameObject <color=yellow>" + entity.gameObject.name + "</color> does not have the reveal shader applied, skipped.", entity);
            }
        }

        void Update()
        {
            /* Wrap this in any other functionality. (eg. when Brush object is touching the ground etc.).
             * Avoid re-drawing the Render Texture if you don't have to update the splat map. */
            DoPaint();
        }

        // Update Splat Map based on Brush object position.
        public void DoPaint()
        {
            Vector4 worldToRtx = new Vector4(
                map(Brush.transform.position.x, WorldBounds.center.x - WorldBounds.size.x / 2, WorldBounds.center.x + WorldBounds.size.x / 2, 0, 1),
                map(Brush.transform.position.z, WorldBounds.center.z - WorldBounds.size.z / 2, WorldBounds.center.z + WorldBounds.size.z / 2, 0, 1),
                0,
                0
            );

            drawMaterial.SetVector(rtxCoord, worldToRtx);
            drawMaterial.SetFloat(rtxStrength, BrushStrength);
            drawMaterial.SetFloat(rtxSize, map(BrushSize, 0, 1, 1, 0));
            drawMaterial.SetFloat(rtxFade, FadeSpeed);
            RenderTexture temp = RenderTexture.GetTemporary(splatmap.width, splatmap.height, 0, RenderTextureFormat.ARGBFloat);
            Graphics.Blit(splatmap, temp);
            Graphics.Blit(temp, splatmap, drawMaterial);
            RenderTexture.ReleaseTemporary(temp);
        }

        // Remap a number in a range to another range
        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        // Work out world size ratio for a non-square Splatmap
        float ratio(float resolution)
        {
            float r;

            if (WorldBounds.size.x > WorldBounds.size.z)
            {
                r = WorldBounds.size.z / WorldBounds.size.x;
            }
            else
                r = WorldBounds.size.x / WorldBounds.size.z;

            return r;
        }
    }
}