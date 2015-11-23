//Load textured or vertex colored models
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace namespace_default
{
    public class ModelLoader
    {
        Game1 Game;

        Model model;
        Texture2D[] textures;
        public Vector3 position;
        public Quaternion rotation;
        float size;

        bool textured;
        short texturecount;

        public ModelLoader(Game1 newGame, string newModelName, Vector3 newPosition, float newSize)//call for vertex-colored load
        {
            Game = newGame;
            position = newPosition;
            rotation = Quaternion.Identity;
            size = newSize;

            textured = false;

            model=LoadModel(newModelName);
        }
        public ModelLoader(Game1 newGame, string newModelName, Vector3 newPosition, float newSize, short newTextureCount)//or call this for textured models
        {
            Game = newGame;
            position = newPosition;
            rotation = Quaternion.Identity;
            size = newSize;

            textured = true;
            texturecount = newTextureCount;
            model = LoadTexturedModel(newModelName, out textures);
        }

        private Model LoadModel(string assetName)
        {
            Model newModel = Game.Content.Load<Model>(assetName);

            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Game.texturedEffect.Clone();
                }
            }

            return newModel;
        }
        private Model LoadTexturedModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = Game.Content.Load<Model>(assetName);
            textures = new Texture2D[texturecount];
            int i = 0;

            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    textures[i++] = currentEffect.Texture;
                }
            }
            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Game.texturedEffect.Clone();
                }
            }

            return newModel;
        }
        public void Draw(Matrix view, short clip)
        {
            Game.device.DepthStencilState = DepthStencilState.Default;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.device.BlendState = BlendState.Opaque;

            Matrix worldMatrix =
                  Matrix.CreateScale(size)
                * Matrix.CreateRotationY(MathHelper.Pi)
                * Matrix.CreateFromQuaternion(rotation)
                * Matrix.CreateTranslation(position);

            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            int i = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    if (textured)
                    {
                        currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                        currentEffect.Parameters["xTexture0"].SetValue(textures[i++]);
                        currentEffect.Parameters["xSpecular"].SetValue(true);
                    }
                    else
                        currentEffect.CurrentTechnique = currentEffect.Techniques["ColorLight"];


                    currentEffect.Parameters["xTextureSh"].SetValue(Game.shadowMap);
                    currentEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);
                    currentEffect.Parameters["xWorld"].SetValue(modelTransforms[mesh.ParentBone.Index]
                        * worldMatrix);
                    currentEffect.Parameters["xViewProjection"].SetValue(view * Game.cam.projectionMatrix);
                    currentEffect.Parameters["xEnableLight"].SetValue(Game.enviro.enableDefaultLighting);
                    currentEffect.Parameters["xShViewProjection"].SetValue(Game.cam.shadowViewMatrix * Game.cam.shadowProjectionMatrix);
                    currentEffect.Parameters["xShEye"].SetValue(Game.cam.shEye);

                    currentEffect.Parameters["xLightDirection"].SetValue(Game.enviro.lightDirection);
                    currentEffect.Parameters["xAmbient"].SetValue(Game.enviro.ambient);
                    currentEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
                    currentEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
                    currentEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);
                    currentEffect.Parameters["xEnableFog"].SetValue(Game.enviro.fogEnabled);
                    currentEffect.Parameters["xFogStart"].SetValue(Game.enviro.fogStart);
                    currentEffect.Parameters["xFogEnd"].SetValue(Game.enviro.fogEnd);
                    currentEffect.Parameters["xFogColor"].SetValue(Game.enviro.fogColor);

                    currentEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
                    currentEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
                    currentEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);
                    currentEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);

                    currentEffect.Parameters["xClipping"].SetValue(clip);
                    currentEffect.Parameters["xClipHeight"].SetValue(Game.enviro.waterLevel);
                    Game.texturedEffect.Parameters["xDepthBias"].SetValue(Game.enviro.depthbias);
                }
                mesh.Draw();
            }
        }
    }
}
