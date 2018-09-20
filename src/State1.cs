using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using Inferno.Runtime;
using Inferno.Runtime.Core;
using Inferno.Runtime.Graphics;
using Inferno.Runtime.Input;
using Inferno.Runtime.UI.Controls;

namespace Minesweeper
{
    /// <summary>
    /// This is the first screen in your game
    /// </summary>
    public class State1 : State
    {
        public int GridWidth = 9;
        public int GridHeight = 9;
        public int MineCount = 10;

        public int Total => GridWidth * GridHeight;

        private int[] _tileStates;
        private int[] _proximity;

        private const int hidden = 0;
        private const int revealed = 1;
        private const int flagged = 2;

        private const int mine = 9;

        private int remainingMines = 10;
        private bool running = true;

        private DateTime startTime;
        private bool firstClick = true;
        private DateTime endTime;

        /// <summary>
        /// Game stage, 0 = mid, 1 = win, 2 = lose
        /// </summary>
        private int stage = -1;

        public State1(Game parent, MinesweeperConfig config) : base(parent, config.Width * 32, config.Height * 32 + 64)
        {
            GridWidth = config.Width;
            GridHeight = config.Height;
            MineCount = config.MineCount;

            //Associate load and unload events
            OnStateLoad += State1_OnStateLoad;
            OnStateUnLoad += State1_OnStateUnLoad;
            OnStateDraw += State1_OnStateDraw;
            OnStateUpdate += State1_OnStateUpdate;

            //Add your state construction logic here

            _tileStates = new int[Total];
            _proximity = new int[Total];

            remainingMines = MineCount;

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
            for (int n = 0; n < Total; n++)
            {
                _tileStates[n] = hidden;
                _proximity[n] = 0;
            }

            remainingMines = MineCount;
            stage = -1;
            running = true;
            firstClick = true;
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
            DrawStage();
            DrawTime();
        }

        private void State1_OnStateUnLoad(object sender, System.EventArgs e)
        {
            //Add UnLoad logic
        }

        private void State1_OnStateLoad(object sender, System.EventArgs e)
        {
            var resetBtn = new Button(new Vector2(0, 0), this, "Reset", Game1.font, Color.Black);
            resetBtn.ControlClicked += ResetBtn_ControlClicked;
            AddInstance(resetBtn);
        }

        private void DrawGrid()
        {
            Drawing.Set_Color(Color.Black);
            for (int x = 0; x <= GridWidth * 32; x += 32)
            {
                Drawing.Draw_Line(new Vector2(x, 64), new Vector2(x, 64 + GridHeight * 32));
            }

            for (int y = 64; y <= 64 + GridHeight * 32; y += 32)
            {
                Drawing.Draw_Line(new Vector2(0, y), new Vector2(GridWidth * 32, y));
            }
        }

        private void DrawTiles()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if(_tileStates[i] == revealed)
                    {
                        if (_proximity[i] < mine)
                        {
                            Drawing.Set_Color(Color.Blue);
                            Drawing.Set_Font(Game1.font);
                            Drawing.Draw_Text(new Vector2(x * 32 + 8, y * 32 + 64 + 8), _proximity[i].ToString());
                        }
                        else
                        {
                            Drawing.Set_Color(Color.Red);
                            Drawing.Draw_Circle(new Vector2(x * 32 + 16, y * 32 + 16 + 64), 16, true, 1);
                        }
                    }
                    else
                    {
                        Drawing.Set_Color(new Color(128, 128, 128));
                        Drawing.Draw_Rectangle(new Vector2(x * 32, y * 32 + 64), 32, 32);
                    }

                    if (_tileStates[i] == flagged)
                    {
                        Drawing.Set_Color(Color.Red);
                        Drawing.Set_Font(Game1.font);
                        Drawing.Draw_Text(new Vector2(x * 32 + 8, y * 32 + 64 + 8), "!");
                    }
                    
                }
            }
        }

        private void DrawMines()
        {
            Drawing.Set_Color(Color.Black);
            Drawing.Set_Font(Game1.font);
            Drawing.Draw_Text(new Vector2(8, 20), "Mines: " + remainingMines);
        }

        private void DrawStage()
        {
            Drawing.Set_Color(Color.Black);
            Drawing.Set_Font(Game1.font);
            if (stage == 1)
                Drawing.Draw_Text(new Vector2(84, 20), "You lost");
            else if (stage == 2)
                Drawing.Draw_Text(new Vector2(84, 20), "You win");
        }

        private void DrawTime()
        {
            Drawing.Set_Color(Color.Black);
            Drawing.Set_Font(Game1.font);
            if (stage == 0)
            {
                Drawing.Draw_Text(new Vector2(150, 20), (DateTime.Now - startTime).ToString("t"));
            }
            else if (stage > 0)
            {
                Drawing.Draw_Text(new Vector2(150, 20), (endTime - startTime).ToString("t"));
            }
        }

        private void RevealTile(int i, bool noMine = false)
        {
            if (_tileStates[i] == flagged
                || _tileStates[i] == revealed)
                return;

            if (noMine && _proximity[i] == mine)
                return;

            _tileStates[i] = revealed;

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

            if (i < Total - GridWidth)
            {
                if (x > 0) RevealTile(i + GridWidth - 1, true);
                RevealTile(i + GridWidth, true);
                if (x + 1 < GridWidth) RevealTile(i + GridWidth + 1, true);
            }
        }

        private int _lastXClick = -1;
        private int _lastYClick = -1;
        private int _lastClickType = -1;

        private void FirstClick()
        {
            if (firstClick)
            {
                startTime = DateTime.Now;
                firstClick = false;
                stage = 0;
            }
        }

        private void HandleClicks()
        {
            if (stage > 0)
                return;

            var state = Mouse.GetMouseState(this);


            var x = state.X / 32;
            var y = state.Y / 32 - 2;
            var i = x + GridWidth * y;

            if (state.LeftButton == ButtonState.Pressed)
            {
                if (x == _lastXClick && y == _lastYClick && _lastClickType == 0)
                    return;

                if (i < Total && i >= 0)
                {
                    FirstClick();

                    if (_tileStates[i] != flagged)
                    {
                        RevealTile(i);

                        if (_proximity[i] == mine)
                            EndGame(0);
                    }
                }

                _lastClickType = 0;

                _lastXClick = x;
                _lastYClick = y;
            }
            else if (state.RightButton == ButtonState.Pressed)
            {
                if (x == _lastXClick && y == _lastYClick && _lastClickType == 1)
                    return;

                if (i < Total && i >= 0)
                {
                    FirstClick();
                    if (_tileStates[i] == hidden)
                    {
                        _tileStates[i] = flagged;
                        remainingMines--;
                    }
                    else if (_tileStates[i] == flagged)
                    {
                        _tileStates[i] = hidden;
                        remainingMines++;
                    }
                }

                _lastClickType = 1;

                _lastXClick = x;
                _lastYClick = y;
            }

            if (CheckWin())
                EndGame(1);
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
                _proximity[i] = mine;
                mineCount--;
            }

            //Calculate proximity
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if (_proximity[i] == mine)
                        continue;

                    if (i >= GridWidth)
                    {
                        if (x > 0)
                            if (_proximity[i - GridWidth - 1] == mine)
                                _proximity[i]++;
                        if (_proximity[i - GridWidth] == mine)
                            _proximity[i]++;
                        if (x + 1 < GridWidth)
                            if (_proximity[i - GridWidth + 1] == mine)
                            _proximity[i]++;
                    }

                    if (x > 0)
                        if (_proximity[i - 1] == mine)
                        _proximity[i]++;
                    if (x + 1 < GridWidth)
                        if (_proximity[i + 1] == mine)
                            _proximity[i]++;

                    if (i < Total - GridWidth)
                    {
                        if (x > 0)
                            if (_proximity[i + GridWidth - 1] == mine)
                                _proximity[i]++;
                        if (_proximity[i + GridWidth] == mine)
                            _proximity[i]++;
                        if (x + 1 < GridWidth)
                            if (_proximity[i + GridWidth + 1] == mine)
                                _proximity[i]++;
                    }
                }
            }
        }

        private bool CheckWin()
        {
            if (!running)
                return false;

            //Check all non-mines are cleared
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    var i = x + GridWidth * y;

                    if (_tileStates[i] == hidden
                        && _proximity[i] < mine)
                        return false;
                }
            }

            return true;
        }

        private void EndGame(int outcome)
        {
            //Reveal mines
            if (outcome < 1)
            {
                for (int i = 0; i < Total; i++)
                {
                    if (_proximity[i] == 9)
                        _tileStates[i] = revealed;
                }
            }

            endTime = DateTime.Now;
            stage = outcome + 1;
            running = false;
        }
    }
}
