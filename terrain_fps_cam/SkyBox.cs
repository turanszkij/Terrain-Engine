//You can set up a skybox or a skydome around your world
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace namespace_default
{
    public class SkyBox
    {
        Game1 Game;
        Model skyBoxModel;
        Texture2D[] skyBoxTextures;

        Vector3 center;
        float rotation = 0;

        public SkyBox(Game1 newGame, Vector3 newCenter, string newName)
        {
            Game = newGame;
            center = newCenter;

            skyBoxModel=LoadModel(newName, out skyBoxTextures);
        }


        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            Model newModel = Game.Content.Load<Model>(assetName);
            textures = new Texture2D[newModel.Meshes.Count];
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
                    meshPart.Effect = Game.skyEffect.Clone();
                }
            }

            return newModel;
        }
        public void Draw(Vector3 center, Matrix newView, short clip)
        {
            Game.device.DepthStencilState = DepthStencilState.None;
            Game.device.RasterizerState = RasterizerState.CullNone;
            Game.device.BlendState = BlendState.Opaque;

            Matrix[] skyboxTransforms = new Matrix[skyBoxModel.Bones.Count];
            skyBoxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            int i = 0;
            foreach (ModelMesh mesh in skyBoxModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = /*Matrix.CreateRotationY(rotation+=0.0001f) **/ Matrix.CreateScale(500f) * skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(center);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Sky"];
                    currentEffect.Parameters["xWorld"].SetValue(skyboxTransforms[mesh.ParentBone.Index]*worldMatrix);
                    currentEffect.Parameters["xViewProjection"].SetValue(newView * Game.cam.infinite_proj);
                    currentEffect.Parameters["xTexture"].SetValue(skyBoxTextures[i++]);
                    currentEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);
                    currentEffect.Parameters["xShEye"].SetValue(Game.cam.shEye);

                    currentEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
                    currentEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
                    currentEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);

                    currentEffect.Parameters["xClipping"].SetValue(clip);
                    currentEffect.Parameters["xClipHeight"].SetValue(Game.enviro.waterLevel);
                }
                mesh.Draw();
            }
            Game.device.BlendState = BlendState.Opaque;
            Game.device.RasterizerState = RasterizerState.CullClockwise;
            Game.device.DepthStencilState = DepthStencilState.Default;
        }
    }


    
        
}
