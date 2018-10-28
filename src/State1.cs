using System;
using System.Collections.Generic;
using Inferno;
using Inferno.Graphics;
using Inferno.Input;
using Inferno.UI;
using Inferno.UI.Controls;

namespace Minesweeper
{
    /// <summary>
    /// This is the first screen in your game
    /// </summary>
    public class State1 : GameState
    {
        public int GridWidth;
        public int GridHeight;
        public int MineCount;

        public int Total => GridWidth * GridHeight;

        private readonly int[] _tileStates;
        private readonly int[] _proximity;

        private const int Hidden = 0;
        private const int Revealed = 1;
        private const int Flagged = 2;

        private const int Mine = 9;

        private int _remainingMines;
        private bool _running = true;

        private DateTime _startTime;
        private bool _firstClick = true;
        private DateTime _endTime;

        /// <summary>
        /// Game stage, 0 = mid, 1 = win, 2 = lose
        /// </summary>
        private byte _stage = 0;

        private const byte WaitState = 0;
        private const byte PlayState = 1;
        private const byte LoseState = 2;
        private const byte WinState = 3;

        public State1(Game parent, MinesweeperConfig config) : base(parent, config.Width * 32, config.Height * 32 + 64, Color.White)
        {
            //Setup grid
            GridWidth = config.Width;
            GridHeight = config.Height;
            MineCount = config.MineCount;

            //Associate load and unload events
            OnLoad += State1_OnStateLoad;
            OnDraw += State1_OnStateDraw;
            OnUpdate += State1_OnStateUpdate;

            //Setup variables
            _tileStates = new int[Total];
            _proximity = new int[Total];

            _remainingMines = MineCount;

            SetMines();
        }

        private void ResetBtn_ControlClicked()
        {
            Console.WriteLine("Reset clicked");
            ClearGrid();
            SetMines();
        }

        private void ClearGrid()
        {
            for (var n = 0; n < Total; n++)
            {
                _tileStates[n] = Hidden;
                _proximity[n] = 0;
            }

            _remainingMines = MineCount;
            _stage = WaitState;
            _running = true;
            _firstClick = true;
        }

        private void State1_OnStateUpdate(object sender, EventArgs e)
        {
            HandleClicks();
        }

        private void State1_OnStateDraw(object sender, System.EventArgs e)
        {
            DrawTiles();
            DrawGrid();
            DrawMines();
            DrawTime();
        }

        private void State1_OnStateLoad(object sender, System.EventArgs e)
        {
            //Add reset button
            var resetBtn = new Button(new Vector2(0, 0), "Reset", Game1.font, Color.Black, Color.Transparent, Color.Black);
            resetBtn.ControlClicked += ResetBtn_ControlClicked;
            UserInterface.AddControl(resetBtn);
        }

        private void DrawGrid()
        {
            for (var x = 0; x <= GridWidth * 32; x += 32)
            {
                Game.Renderer.DrawLine(new Vector2(x, 64), new Vector2(x, 64 + GridHeight * 32), Color.Black, 1, 2f);
            }

            for (var y = 64; y <= 64 + GridHeight * 32; y += 32)
            {
                Game.Renderer.DrawLine(new Vector2(0, y), new Vector2(GridWidth * 32, y), Color.Black, 1, 2f);
            }
        }

        private void DrawTiles()
        {
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if(_tileStates[i] == Revealed)
                    {
                        if (_proximity[i] < Mine)
                        {
                            if (_proximity[i] <= 0)
                                continue;
                            Game.Renderer.DrawText(_proximity[i].ToString(), new Vector2(x * 32 + 8, y * 32 + 64 + 8), Game1.font, Color.Blue, -2f);
                        }
                        else
                        {
                            Game.Renderer.DrawCircle(new Vector2(x * 32 + 16, y * 32 + 16 + 64), 16, Color.Red);
                        }
                    }
                    else
                    {
                        Game.Renderer.DrawRectangle(new Rectangle(x * 32, y * 32 + 64, 32, 32), new Color(128, 127, 128));

                    }

                    if (_tileStates[i] != Flagged)
                        continue;

                    Game.Renderer.DrawText("!", new Vector2(x * 32 + 8, y * 32 + 64 + 8), Game1.font, Color.Red, 1f);

                }
            }
        }

        private void DrawMines()
        {
            Game.Renderer.DrawText("Mines: " + _remainingMines, new Vector2(8, 20), Game1.font, Color.Black);
        }

        private void DrawTime()
        {
            if (_stage == PlayState)
            {
                Game.Renderer.DrawText((DateTime.Now - _startTime).ToString("t"), new Vector2(120, 20), Game1.font, Color.Black);
            }
            else if (_stage > PlayState)
            {
                Game.Renderer.DrawText((_endTime - _startTime).ToString("t"), new Vector2(120, 20), Game1.font, Color.Black);
            }
        }

        private void RevealTile(int i, bool noMine = false)
        {
            if (_tileStates[i] == Flagged
                || _tileStates[i] == Revealed)
                return;

            if (noMine && _proximity[i] == Mine)
                return;

            _tileStates[i] = Revealed;

            if (_proximity[i] == 0)
                ShowNeighbours(i);
        }

        private void ShowNeighbours(int i)
        {
            var x = i % GridWidth;
            if (i >= GridWidth)
            {
                if (x > 0) RevealTile(i - GridWidth - 1, true);
                RevealTile(i - GridWidth, true);
                if (x + 1 < 9) RevealTile(i - GridWidth + 1, true);
            }

            if (x > 0) RevealTile(i - 1, true);
            if (x + 1 < 9) RevealTile(i + 1, true);

            if (i >= Total - GridWidth)
                return;

            if (x > 0) RevealTile(i + GridWidth - 1, true);
            RevealTile(i + GridWidth, true);
            if (x + 1 < GridWidth) RevealTile(i + GridWidth + 1, true);
        }

        private void FirstClick()
        {
            if (!_firstClick)
                return;

            _startTime = DateTime.Now;
            _firstClick = false;
            _stage = PlayState;
        }

        private void HandleClicks()
        {
            if (_stage > PlayState)
                return;

            var state = Mouse.GetState(this);

            var x = state.X / 32;
            var y = state.Y / 32 - 2;
            var i = x + GridWidth * y;

            if (state.LeftButton == ButtonState.Pressed)
            {
                if (i < Total && i >= 0)
                {
                    FirstClick();

                    if (_tileStates[i] != Flagged)
                    {
                        RevealTile(i);

                        if (_proximity[i] == Mine)
                            EndGame(false);
                    }
                }

                Mouse.ClearLeftButton();
            }
            else if (state.RightButton == ButtonState.Pressed)
            {
                if (i < Total && i >= 0)
                {
                    FirstClick();
                    if (_tileStates[i] == Hidden)
                    {
                        _tileStates[i] = Flagged;
                        _remainingMines--;
                    }
                    else if (_tileStates[i] == Flagged)
                    {
                        _tileStates[i] = Hidden;
                        _remainingMines++;
                    }
                }
                Mouse.ClearRightButton();
            }

            if (CheckWin())
                EndGame(true);
        }

        private void SetMines()
        {
            var mines = new List<int>();

            var mineCount = MineCount;

            var rand = new Random(DateTime.Now.Millisecond);

            while (mineCount > 0)
            {
                var i = rand.Next(Total);

                if (mines.Contains(i))
                    continue;

                mines.Add(i);
                _proximity[i] = Mine;
                mineCount--;
            }

            //Calculate proximity
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if (_proximity[i] == Mine)
                        continue;

                    if (i >= GridWidth)
                    {
                        if (x > 0)
                            if (_proximity[i - GridWidth - 1] == Mine)
                                _proximity[i]++;
                        if (_proximity[i - GridWidth] == Mine)
                            _proximity[i]++;
                        if (x + 1 < GridWidth)
                            if (_proximity[i - GridWidth + 1] == Mine)
                            _proximity[i]++;
                    }

                    if (x > 0)
                        if (_proximity[i - 1] == Mine)
                        _proximity[i]++;
                    if (x + 1 < GridWidth)
                        if (_proximity[i + 1] == Mine)
                            _proximity[i]++;

                    if (i >= Total - GridWidth)
                        continue;

                    if (x > 0)
                        if (_proximity[i + GridWidth - 1] == Mine)
                            _proximity[i]++;
                    if (_proximity[i + GridWidth] == Mine)
                        _proximity[i]++;
                    if (x + 1 < GridWidth)
                        if (_proximity[i + GridWidth + 1] == Mine)
                            _proximity[i]++;
                }
            }
        }

        private bool CheckWin()
        {
            if (!_running)
                return false;

            //Check all non-mines are cleared
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if (_tileStates[i] != Revealed
                        &&_proximity[i] < Mine)
                        return false;
                }
            }

            return true;
        }

        private void EndGame(bool outcome)
        {
            //Reveal mines
            if (outcome == false)
            {
                for (var i = 0; i < Total; i++)
                    if (_proximity[i] == 9)
                        _tileStates[i] = Revealed;
            }

            _stage = outcome ? WinState : LoseState;

            if (_stage == WinState)
            {
                Game1.Win.Play();
            }
            else
            {
                Game1.Explode.Play();
            }

            _endTime = DateTime.Now;
            _running = false;

            MessageBox.Show("Minesweeper", _stage == LoseState ? "You lost." : "You won.", MessageBoxType.Information, true);
        }
    }
}
