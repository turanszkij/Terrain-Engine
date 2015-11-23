//Predefined controls for either a first person or a third person camera(which updates even the position of its focus object)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace namespace_default
{
    public class Camera
    {
        Game1 Game;

        public Matrix viewMatrix, reflectionViewMatrix, shadowViewMatrix;
        public Matrix projectionMatrix, shadowProjectionMatrix;
        public Matrix infinite_proj;

        public Vector3 cameraPosition,refl_cameraPosition,shEye;
        float leftrightRot = 0;
        float updownRot = 0;
        const float rotationSpeed = 0.1f;
         float moveSpeed = 50.0f;
        MouseState originalMouseState;


        Vector3 cameraUpDirection;
        public Quaternion rotation;

        public bool fpsState;


        public Camera(Game1 newGame, Vector3 position)
        {
            Game = newGame;
            fpsState = true;
            Mouse.SetPosition(Game.device.Viewport.Width / 2, Game.device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            cameraPosition = position;
            SetUpCamera();
        }

        public void SetUpCamera()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 1.1f, Game.enviro.viewDistance);
            infinite_proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 0.1f, 100);

            shEye = new Vector3(-1400, 1200, 3000);
            shadowViewMatrix = Matrix.CreateLookAt(shEye, new Vector3(256, 10, 256), Vector3.Up);
            shadowProjectionMatrix = Matrix.CreateOrthographic(700, 700, 3000, 4000);
            //shadowProjectionMatrix = projectionMatrix;

            FPS_UpdateViewMatrix();
        }

        public void FPS_UpdateViewMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 0.1f, Game.enviro.viewDistance);
            infinite_proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 0.1f, 1000000000);
            
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);

            refl_cameraPosition = cameraPosition;
            if (Game.enviro.underWater)
                refl_cameraPosition.Y = Game.enviro.waterLevel + (Game.enviro.waterLevel - cameraPosition.Y);
            else
                refl_cameraPosition.Y = Game.enviro.waterLevel - (cameraPosition.Y - Game.enviro.waterLevel);
            Vector3 reflTargetPos = cameraFinalTarget;
            reflTargetPos.Y = -cameraFinalTarget.Y + Game.enviro.waterLevel * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPos - refl_cameraPosition);
            reflectionViewMatrix = Matrix.CreateLookAt(refl_cameraPosition, reflTargetPos, invUpVector);
        }
        public void TPS_UpdateViewMatrix(Vector3 target, Quaternion targetrot)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 0.1f, Game.enviro.viewDistance);
            infinite_proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Game.enviro.fieldOfView), Game.device.Viewport.AspectRatio, 0.1f, 1000000000);

            Vector3 camTarget = target + new Vector3(0, 1.5f, 0);

            Vector3 campos = new Vector3(0, 0.1f, 1.1f);

            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(rotation));
            campos += camTarget;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(rotation));

            viewMatrix = Matrix.CreateLookAt(campos, camTarget, camup);

            cameraPosition = campos;
            cameraUpDirection = camup;
            if(Game.enviro.smoothCamera)
                rotation = Quaternion.Lerp(rotation, targetrot, 0.3f);
            else
                rotation = targetrot;


            refl_cameraPosition = cameraPosition;
            if (Game.enviro.underWater)
                refl_cameraPosition.Y = Game.enviro.waterLevel + (Game.enviro.waterLevel - cameraPosition.Y);
            else
                refl_cameraPosition.Y = Game.enviro.waterLevel - (cameraPosition.Y - Game.enviro.waterLevel);
            Vector3 reflTargetPos = camTarget;
            reflTargetPos.Y = -camTarget.Y + Game.enviro.waterLevel * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), rotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPos - refl_cameraPosition);
            reflectionViewMatrix = Matrix.CreateLookAt(refl_cameraPosition, reflTargetPos, invUpVector);
        }

        public void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();

            if (currentMouseState.RightButton == ButtonState.Pressed && currentMouseState != originalMouseState)
            {
                Game.IsMouseVisible = false;
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                Mouse.SetPosition(originalMouseState.X, originalMouseState.Y);
                FPS_UpdateViewMatrix();
            }
            else
            {
                Game.IsMouseVisible = true;
                originalMouseState = Mouse.GetState();
            }


            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (!keyState.Equals(new KeyboardState()))
            {
                if (keyState.IsKeyDown(Keys.W))
                    moveVector += new Vector3(0, 0, -1);
                if (keyState.IsKeyDown(Keys.S))
                    moveVector += new Vector3(0, 0, 1);
                if (keyState.IsKeyDown(Keys.D))
                    moveVector += new Vector3(1, 0, 0);
                if (keyState.IsKeyDown(Keys.A))
                    moveVector += new Vector3(-1, 0, 0);
                if (keyState.IsKeyDown(Keys.Space))
                    moveVector += new Vector3(0, 1, 0);
                if (keyState.IsKeyDown(Keys.LeftControl))
                    moveVector += new Vector3(0, -1, 0);

                if (keyState.IsKeyDown(Keys.LeftShift))
                    moveSpeed = 20;
                else if (keyState.IsKeyUp(Keys.LeftShift))
                    moveSpeed = 2;
            }
            AddToCameraPosition(moveVector * amount);
            FPS_UpdateViewMatrix();

            /*
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                shEye = cameraPosition;
                shadowViewMatrix = viewMatrix;
            }*/
        }
        public void ProcessInput(float amount, ref Vector3 target, ref Quaternion targetrot)
        {
            MouseState currentMouseState = Mouse.GetState();
            Quaternion cameraRotation=Quaternion.Identity;
            if (currentMouseState.RightButton == ButtonState.Pressed && currentMouseState != originalMouseState)
            {
                Game.IsMouseVisible = false;
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount/7f;
                updownRot -= rotationSpeed * yDifference * amount/7f;
                Mouse.SetPosition(originalMouseState.X, originalMouseState.Y);
            }
            else
            {
                Game.IsMouseVisible = true;
                originalMouseState = Mouse.GetState();
            }

            cameraRotation =
                Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot)
                * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRot);

            KeyboardState keyState = Keyboard.GetState();
            if (!keyState.Equals(new KeyboardState()))
            {
                if (keyState.IsKeyDown(Keys.W))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -0.1f), targetrot);
                    target += addVector * amount;
                    targetrot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
                }
                if (keyState.IsKeyDown(Keys.S))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(0, 0, 0.1f), targetrot);
                    target += addVector * amount;
                    targetrot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
                }
                if (keyState.IsKeyDown(Keys.A))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(-0.1f, 0, 0), targetrot);
                    target += addVector * amount;
                    targetrot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
                }
                if (keyState.IsKeyDown(Keys.D))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(0.1f, 0, 0), targetrot);
                    target += addVector * amount;
                    targetrot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
                }
                if (keyState.IsKeyDown(Keys.Space))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(0, 0.1f, 0), targetrot);
                    target += addVector * amount;
                }
                if (keyState.IsKeyDown(Keys.LeftControl))
                {
                    Vector3 addVector = Vector3.Transform(new Vector3(0, -0.1f, 0), targetrot);
                    target += addVector * amount;
                }
            }

            TPS_UpdateViewMatrix(target, cameraRotation);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            FPS_UpdateViewMatrix();
        }
    }
}
