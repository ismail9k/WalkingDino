using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;

namespace WalkingDino
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // Dino modal
        Model Dino;
        Model Ground;
        ClipPlayer DinoClipPlayer;
        SkinningData DinoSkinningData;
        AnimationClip DinoClip;

        // Dino translation
        Vector3 DinoPosition;
        Vector3 DinoVelocity;
        float DinoRotation;

        // Keyboard controller
        Controller Control;
        List<Action> ActionList;

        // Keys screen image
        IconImage Up;
        IconImage Down;
        IconImage Left;
        IconImage Right;
        IconImage Jump;
        IconImage Camera;
        IconImage Attack;

        // Camera
        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = -50;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Screen reslaution
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 560;
            graphics.ApplyChanges();

            

            // List of Keyboard Actions
            ActionList = new List<Action>();
            ActionList.Add(new Action("Up", Keys.Up));
            ActionList.Add(new Action("Down", Keys.Down));
            ActionList.Add(new Action("Right", Keys.Right));
            ActionList.Add(new Action("Left", Keys.Left));
            ActionList.Add(new Action("Jump", Keys.Space));
            ActionList.Add(new Action("Attack", Keys.A));
            ActionList.Add(new Action("Camera", Keys.C));
            ActionList.Add(new Action("Reset", Keys.R));
            ActionList.Add(new Action("ZoomIn", Keys.Z));
            ActionList.Add(new Action("ZoomOut", Keys.X));
            Control = new Controller(PlayerIndex.One, ActionList);

            // Keyboard Screen Imgaes
            Up = new IconImage(new Vector2(750f, 380f));
            Down = new IconImage(new Vector2(750f, 420f));
            Left = new IconImage(new Vector2(710f, 420f));
            Right = new IconImage(new Vector2(790f, 420f));
            Jump = new IconImage(new Vector2(710f, 460f));
            Camera = new IconImage(new Vector2(150f, 420f));
            Attack = new IconImage(new Vector2(110f, 380f));

       
            DinoRotation = 0;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load models to the game.
            Dino = Content.Load<Model>("Dino");
            Ground = Content.Load<Model>("ground");

            // Look up our custom skinning information.
            DinoSkinningData = Dino.Tag as SkinningData;


            // Create an animation player, and start decoding an animation clip.
            DinoClipPlayer = new ClipPlayer(DinoSkinningData, 24);
            DinoClip = DinoSkinningData.AnimationClips["Take 001"];
            DinoClipPlayer.Play(DinoClip, 1, 10, true);
            DinoClipPlayer.AnimationEnded += DinoClipPlayer_AnimationEnded;


            // Load screen keys images
            Up.KeyOff = Content.Load<Texture2D>("Icons\\Up");
            Up.KeyOn = Content.Load<Texture2D>("Icons\\UpActive");
            Down.KeyOff = Content.Load<Texture2D>("Icons\\Down");
            Down.KeyOn = Content.Load<Texture2D>("Icons\\DownActive");
            Right.KeyOff = Content.Load<Texture2D>("Icons\\Right");
            Right.KeyOn = Content.Load<Texture2D>("Icons\\RightActive");
            Left.KeyOff = Content.Load<Texture2D>("Icons\\Left");
            Left.KeyOn = Content.Load<Texture2D>("Icons\\LeftActive");
            Jump.KeyOff = Content.Load<Texture2D>("Icons\\Jump");
            Jump.KeyOn = Content.Load<Texture2D>("Icons\\JumpActive");
            Camera.KeyOff = Content.Load<Texture2D>("Icons\\Camera");
            Camera.KeyOn = Content.Load<Texture2D>("Icons\\CameraActive");
            Attack.KeyOff = Content.Load<Texture2D>("Icons\\Attack");
            Attack.KeyOn = Content.Load<Texture2D>("Icons\\AttackActive");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            // Screen Icons
            Up.Current = Up.KeyOff;
            Down.Current = Down.KeyOff;
            Left.Current = Left.KeyOff;
            Right.Current = Right.KeyOff;
            Jump.Current = Jump.KeyOff;
            Camera.Current = Camera.KeyOff;
            Attack.Current = Attack.KeyOff;


            // Game controller
            Control.Update();

            DinoVelocity = new Vector3(0, 0, 0);

            if (!(Control.IsActionPressed("Camera")))
            {
                // Change Dino translation
                if (Control.IsActionPressed("Left"))
                {
                    DinoRotation += 1;
                    Left.Current = Left.KeyOn;
                }
                else if ((Control.IsActionPressed("Right")))
                {
                    DinoRotation -= 1;
                    Right.Current = Right.KeyOn;
                }
                if (Control.IsActionPressed("Up"))
                {
                    DinoVelocity = new Vector3(0f, 0f, .3f);
                    Up.Current = Up.KeyOn;
                }
                else if (Control.IsActionPressed("Down"))
                {
                    DinoVelocity = new Vector3(0f, 0f, -.2f);
                    Down.Current = Down.KeyOn;
                }
            
                // Change animation clip
                if (Control.IsActionPressed("Up") || Control.IsActionPressed("Down"))
                {
                    if (!DinoClipPlayer.InRange(65, 83))
                        DinoClipPlayer.Switch(13, 32);
                }
                else
                {
                    if(DinoClipPlayer.InRange(13, 32))
                        DinoClipPlayer.Switch(1, 10);
                }

                if (Control.IsActionPressed("Attack"))
                {
                    DinoClipPlayer.Switch(35, 63);
                    Attack.Current = Attack.KeyOn;
                }
                else if (Control.IsActionPressed("Jump"))
                {
                    Jump.Current = Jump.KeyOn;
                    DinoClipPlayer.Switch(65, 83);
                }
                if (Control.IsActionPressed("Reset"))
                {
                    DinoPosition = new Vector3(0, 0, 0);
                }
            }
            else
            {
                Camera.Current = Camera.KeyOn;
                // Check for input to rotate the camera up and down around the model.
                if (Control.IsActionPressed("Up"))
                {
                    cameraArc += 1f;
                    Up.Current = Up.KeyOn;
                }
                else if (Control.IsActionPressed("Down"))
                {
                    cameraArc += -1f;
                    Down.Current = Down.KeyOn;
                }

                // Limit the arc movement.
                if (cameraArc > 90.0f)
                    cameraArc = 90.0f;
                else if (cameraArc < -90.0f)
                    cameraArc = -90.0f;


                if (Control.IsActionPressed("Left"))
                {
                    cameraRotation -= 1f;
                    Left.Current = Left.KeyOn;
                }
                else if (Control.IsActionPressed("Right"))
                {
                    cameraRotation += 1f;
                    Right.Current = Right.KeyOn;
                }

                if (Control.IsActionPressed("ZoomIn"))
                    cameraDistance += 0.25f;

                else if (Control.IsActionPressed("ZoomOut"))
                    cameraDistance += -0.25f;

                // Limit the camera distance.
                if (cameraDistance > 500.0f)
                    cameraDistance = 500.0f;

                if (Control.IsActionPressed("Reset"))
                {
                    cameraArc = 0;
                    cameraRotation = 0;
                    cameraDistance = -50;
                }
            }


            DinoPosition += Vector3.Transform(DinoVelocity, Matrix.CreateRotationY(MathHelper.ToRadians(DinoRotation)));

            Matrix Translation = Matrix.CreateRotationY(MathHelper.ToRadians(DinoRotation)) * 
                                 Matrix.CreateTranslation(DinoPosition);
            DinoClipPlayer.Update(gameTime.ElapsedGameTime, true, Translation);
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            


            Matrix[] bones = DinoClipPlayer.GetSkinTransforms();

            Matrix world = Matrix.CreateWorld(new Vector3(0, 0, 0) , Vector3.Forward, Vector3.Up);

            Matrix view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *        // change camera rotation
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *             // change camera orbit
                          Matrix.CreateLookAt(new Vector3(20, 20, -cameraDistance),             // change camera looking place
                                              new Vector3(0, 0, 0), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            // Reset graphics
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            // Display Dino
            foreach (ModelMesh mesh in Dino.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.15f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }

            // Display ground
            foreach (ModelMesh mesh in Ground.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;

                }

                mesh.Draw();
            }


            // Draw icons on screen
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(Up.Current, Up.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Down.Current, Down.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Right.Current, Right.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Left.Current, Left.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Jump.Current, Jump.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Camera.Current, Camera.KeyPosition, Color.Wheat);
            spriteBatch.Draw(Attack.Current, Attack.KeyPosition, Color.Wheat);
            spriteBatch.End();
        }

        // This function use for rest dino animation to stand by
        void DinoClipPlayer_AnimationEnded()
        {
            DinoClipPlayer.Switch(1, 10);
        }
    }
}
