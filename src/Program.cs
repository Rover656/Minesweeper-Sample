﻿using System;
using Inferno.UI;

namespace Minesweeper
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                using (var game = new Game1())
                    game.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("Fatal error", e.ToString(), MessageBoxType.Error);
                Environment.Exit(0);
            }
        }
    }
}
