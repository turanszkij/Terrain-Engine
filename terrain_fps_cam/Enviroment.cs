//This class stores all the data once it is loaded
using Microsoft.Xna.Framework;

namespace namespace_default
{
    public class Enviroment
    {
        public string map = "map1";
        public string texture = "grass";
        public string grassTexture = "grassbb";
            public bool billboardedGrass = true;

        public float reflectionQuality = 0.5f;

        public bool enableDefaultLighting = true;
        public Vector3 lightDirection = new Vector3(-0.5f, -0.5f, 0.4f);

        public bool enablePointLight = false;
        public Vector3 pointLightPos = new Vector3(50, 15.5f, 50);
        public float lightscatter = 10.0f;

        public float ambient = 0.2f;
        public float lightpower = 1.0f;

        public bool grayScale = false, invertColors = false;
        public bool smoothCamera = false;
        public bool FlyMode = true;
        public bool detailWater = true;

        public int grassIntensity = 0;
        public float grassFade = 100.0f;
        public float grassWidth = 1.0f;
        public float grassHeight = 1.0f;

        public bool fogEnabled = true;
        public float fogStart = 10, fogEnd = 100;
        public Vector4 fogColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        public float viewDistance = 200.0f;
        public float fieldOfView = 90.0f;

        public Vector3 WindDirection = new Vector3(0, 0, 1);
        public float WindWaveSize = 0.8f;
        public float WindRandomness = 1.5f;
        public float WindAmount = 0.2f;
        public float WindTime;

        public bool enableSnow = false, enableRain = false;
        public int weatherParticles = 10000;

        public float waterLevel = 9.0f;
        public float waveHeight = 0.1f;
        public bool underWater = false;


        public float depthbias = 0.0022f;

        public Enviroment()
        {
            lightDirection.Normalize();
        }

        public void Update(GameTime gameTime)
        {
            WindTime = (float)gameTime.TotalGameTime.TotalSeconds * 0.333f;
        }
    }
}
