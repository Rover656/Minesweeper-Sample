﻿using System;
using System.IO;
using Newtonsoft.Json;
using Inferno;
using Inferno.Audio;
using Inferno.Content;
using Inferno.Graphics;
using Inferno.Graphics.Text;

namespace Minesweeper
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static Font font;
        public static Sound Win;
        public static Sound Explode;

        //Replace with the dimensions for your game resolution
        public Game1() : base(9*32, 9*32 + 64)
        {
            //Allow your game to be resized
            Window.AllowResize = true;

            BackColor = Color.White;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            var cnfg = new MinesweeperConfig();;

            if (!File.Exists("config.json"))
            {
                cnfg.Width = 9;
                cnfg.Height = 9;
                cnfg.MineCount = 10;

                File.WriteAllText("config.json", JsonConvert.SerializeObject(cnfg));
            }
            else
            {
                string json = File.ReadAllText("config.json");
                cnfg = JsonConvert.DeserializeObject<MinesweeperConfig>(json);
            }

            if (cnfg.MineCount > cnfg.Width * cnfg.Height)
            {
                //Exit();
                Environment.Exit(0);
            }

            Resize(cnfg.Width * 32, cnfg.Height * 32 + 64);

            SetState(new State1(this, cnfg));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            FocusPause = false;

            font = Font.CreateFont("Arial", 14);
            Win = ContentLoader.LoadWaveFromFile("win.wav");
            Explode = ContentLoader.LoadWaveFromFile("explode.wav");
            Win.Volume = 0.5f;
            Explode.Volume = 0.5f;

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            font.Dispose();
            Win.Dispose();
            Explode.Dispose();
            base.UnloadContent();
        }
    }
}
