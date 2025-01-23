using FlaxEngine;
namespace Game;

[ExecuteInEditMode]
/// <summary>
/// MapSettings Script.
/// </summary>
public class MapSettings : Script
{
    public struct MapInfo
    {
        public MapInfo() { }
        public enum ModType
        {
            hidden = 0,
            primary = 1,
            map = 3
        }


        public string name = "";
        public string shortname = "";
        public string description = "";
        public string author = "";
        public string version = "";
        public string mutator = "";
        public string mapfile = "";
        public ModType modtype = ModType.map;

        public string[] depend = [];
        public string[] replace = [];

        public float maphardness = 100.0f;
        public bool notDeformable = false;
        public float gravity = 130.0f;
        public float tidalStrength = 0.0f;
        public float maxMetal = 0.02f;
        public float extractorRadius = 500.0f;
        public bool voidWater = false;
        public bool voidGround = false;
        public float voidAlphaMin = 0.9f;

        public bool autoShowMetal = true;

        public Smf smf = new();
        public Resources resources = new();
        public Splats splats = new();
        public Atmosphere atmosphere = new();
        public Grass grass = new();
        public Lighting lighting = new();
        public Water water = new();
        public Sound sound = new();
        public Teams[] teams = [];
        public TerrainTypes[] terrainTypes = [new()];
        public Custom custom = new();

        public struct Smf
        {
            public Smf() { }

            public float minHeight = 0.0f;
            public float maxHeight = 0.0f;
            public string minimapTex = "";
            public string metalmapTex = "";
            public string typemapTex = "";
            public string grassmapTex = "";
            public string[] smtFileName = [];
        }
        public struct Sound
        {
            public Sound() { }

            public string preset = "default";

            public struct PassFilter()
            {
                public float gainlf = 1.0f;
                public float gainhf = 1.0f;
            }

            public PassFilter passfilter = new();

            public struct Reverb
            {
                //idk the type it is to enable it ??
                //like value is set so it is enabled then it is a bool else idk

                //density
                //diffusion
                //gain
                //gainhf
                //gainlf
                //decaytime
                //decayhflimit
                //decayhfratio
                //decaylfratio
                //reflectionsgain
                //reflectionsdelay
                //reflectionspan
                //latereverbgain
                //latereverbdelay
                //latereverbpan
                //echotime
                //echodepth
                //modtime
                //moddepth
                //airabsorptiongainhf
                //hfreference
                //lfreference
                //roomrollofffactor
            }

            public Reverb reverb = new();
        }
        public struct Resources
        {
            public Resources() { }

            public string grassBladeTex = "";
            public string grassShadingTex = "";
            public string detailTex = "";
            public string specularTex = "";
            public string splatDetailTex = "";
            public string splatDistrTex = "";
            public string skyReflectModTex = "";
            public string detailNormalTex = "";
            public string lightEmissionTex = "";
            public string parallaxHeightTex = "";
        }
        public struct Splats
        {
            public Splats() { }

            public float[] texScales = [0.2f, 0.2f, 0.2f, 0.2f];
            public float[] texMults = [1.0f, 1.0f, 1.0f, 1.0f];
        }
        public struct Atmosphere
        {
            public Atmosphere() { }

            public float minWind = 5.0f;
            public float maxWind = 25.0f;
            public float fogStart = 0.1f;
            public float fogEnd = 1.0f;

            public float[] fogColor = [0.7f, 0.7f, 0.8f];
            public float[] sunColor = [1.0f, 1.0f, 1.0f];
            public float[] skyColor = [0.1f, 0.15f, 0.7f];
            public float[] skyDir = [0.0f, 0.0f, -1.0f];
            public string skyBox = "";
            public float cloudDensity = 0.5f;
            public float[] cloudColor = [1.0f, 1.0f, 1.0f];
        }
        public struct Grass
        {
            public Grass() { }
            public float bladeWaveScale = 1.0f;
            public float bladeWidth = 0.32f;
            public float bladeHeight = 4.0f;
            public float bladeAngle = 1.57f;
            public int maxStrawsPerTurf = 150;
            public float[] bladeColor = [0.59f, 0.81f, 0.57f];

        }
        public struct Lighting
        {
            public Lighting() { }
            public float sunStartAngle = 0.0f;
            public float sunOrbitTime = 1440.0f;
            public float[] sunDir = [0.0f, 1.0f, 2.0f];
            public float[] groundAmbientColor = [0.5f, 0.5f, 0.5f];
            public float[] groundDiffuseColor = [0.5f, 0.5f, 0.5f];
            public float[] groundSpecularColor = [0.1f, 0.1f, 0.1f];
            public float groundShadowDensity = 0.8f;
            public float[] unitAmbientColor = [0.4f, 0.4f, 0.4f];
            public float[] unitDiffuseColor = [0.7f, 0.7f, 0.7f];
            public float[] unitSpecularColor = [0.7f, 0.7f, 0.7f];
            public float unitShadowDensity = 0.8f;
            public float specularExponent = 100.0f;
        }
        public struct Water
        {
            public Water() { }

            public float damage = 0.0f;
            public float repeatX = 0.0f;
            public float repeatY = 0.0f;
            public float[] absorb = [0.0f, 0.0f, 0.0f];
            public float[] baseColor = [0.0f, 0.0f, 0.0f];
            public float[] minColor = [0.0f, 0.0f, 0.0f];
            public float ambientFactor = 1.0f;
            public float diffuseFactor = 1.0f;
            public float specularFactor = 1.0f;
            public float specularPower = 20.0f;
            public float[] planeColor = [0.0f, 0.4f, 0.0f];
            public float[] surfaceColor = [0.75f, 0.8f, 0.85f];
            public float surfaceAlpha = 0.55f;
            public float[] diffuseColor = [1f, 1f, 1f];
            public float[] specularColor = [0.5f, 0.5f, 0.5f];
            public float fresnelMin = 0.2f;
            public float fresnelMax = 0.8f;
            public float fresnelPower = 4.0f;
            public float reflectionDistortion = 1.0f;
            public float blurBase = 2.0f;
            public float blurExponent = 1.5f;
            public float perlinStartFreq = 8.0f;
            public float perlinLacunarity = 3.0f;
            public float perlinAmplitude = 0.9f;
            public float windSpeed = 1.0f;
            public bool shoreWaves = true;
            public bool forceRendering = true;
        }
        public struct Teams
        {
            public Teams() { }
            public Float2 startPos = new();
        }
        public struct TerrainTypes
        {
            public TerrainTypes() { }

            public byte ID = 0;
            public string name = "Default";
            public float hardness = 1.0f;
            public bool receiveTracks = true;

            public struct MoveSpeeds
            {
                public MoveSpeeds() { }
                public float tank = 1.0f;
                public float kbot = 1.0f;
                public float hover = 1.0f;
                public float ship = 1.0f;
            }

            public MoveSpeeds moveSpeeds = new();
        }
        public struct Custom
        {
            public Custom() { }

            public struct Fog
            {
                public Fog() { }

                public float[] color = [0.26f, 0.30f, 0.41f];
                public string height = "80%";
                public float fogatten = 0.003f;

            };
            public struct Precipitation
            {
                public Precipitation() { }
                public int density = 30000;
                public float size = 1.5f;
                public int speed = 50;
                public float windscale = 1.2f;
                public string texture = "LuaGaia/effects/snowflake.png";
            }

            public Fog fog = new();
            public Precipitation precipitation = new();
        }
    }
    [MultilineText]
    public string output = string.Empty;

    [NoSerialize]public MapInfo mapInfo = new MapInfo();

    public bool run = false;

    /// <inheritdoc/>
    public override void OnStart()
    {
        // Here you can add code that needs to be called when script is created, just before the first game update
    }
    
    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        if (run)
            output = FlaxEngine.Json.JsonSerializer.Serialize(mapInfo);
        run = false;
        // Here you can add code that needs to be called every frame
    }
}
