using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuroWorms.Core;
using System.Threading.Tasks;

namespace NeuroWorms
{
    public class Game1 : Game
    {

        private const double SimulationSpeed = 0.00;
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Color bgColor = new(0xBAE8ED);
        private readonly SimulationEngine simulationEngine = new();
        private FieldRenderer fieldRenderer;
        private double timeSinceLastUpdate = 0.0;
        private SpriteFont displayFont;
        private bool skipGenerations = false;
        private Task currentTask = null;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 960;
            _graphics.PreferMultiSampling = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            fieldRenderer = new FieldRenderer(GraphicsDevice);
            displayFont = Content.Load<SpriteFont>("DisplayFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.V))
            {
                skipGenerations = !skipGenerations;
            }



            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (currentTask == null || currentTask.IsCompleted)
            {
                if (!skipGenerations)
                {
                    currentTask = Task.Run(() => simulationEngine.NextMove());
                }
                else
                {
                    currentTask = Task.Run(() => simulationEngine.RunTillNextGeneration());
                }
            }

            timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastUpdate >= SimulationSpeed)
            {
                timeSinceLastUpdate = 0.0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(bgColor);
            _spriteBatch.Begin();
            fieldRenderer.Render(30, 30, simulationEngine.Field, _spriteBatch);
            _spriteBatch.DrawString(displayFont, $"Generation: {simulationEngine.CurrentGeneration}", new Vector2(1000, 30), Color.MidnightBlue);
            _spriteBatch.DrawString(displayFont, $"Moves count: {simulationEngine.CurrentTick}", new Vector2(1000, 70), Color.DarkSlateGray);
            _spriteBatch.DrawString(displayFont, $"Worms count: {simulationEngine.Worms.Count}", new Vector2(1000, 100), Color.DarkSlateGray);
            _spriteBatch.DrawString(displayFont, $"Longest worm: {simulationEngine.LongestWorm}", new Vector2(1000, 130), Color.DarkSlateGray);
            if (skipGenerations)
            {
                   _spriteBatch.DrawString(displayFont, $"Skipping generations", new Vector2(1000, 200), Color.DarkSlateGray);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}