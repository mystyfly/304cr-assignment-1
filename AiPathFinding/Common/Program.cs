﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AiPathFinding.View;

namespace AiPathFinding.Common
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
