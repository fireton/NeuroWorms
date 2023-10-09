using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuroWorms.Core;

namespace NeuroWorms
{
    public class Game1 : Game
    {
        
        private const double SimulationSpeed = 0.05;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Color bgColor = new(0xBAE8ED);
        private SimulationEngine simulationEngine = new();
        private FieldRenderer fieldRenderer;
        private double timeSinceLastUpdate = 0.0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 960;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            fieldRenderer = new FieldRenderer(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastUpdate >= SimulationSpeed)
            {
                simulationEngine.NextMove();
                timeSinceLastUpdate = 0.0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(bgColor);
            _spriteBatch.Begin();
            fieldRenderer.Render(20, 30, simulationEngine.Field, _spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}