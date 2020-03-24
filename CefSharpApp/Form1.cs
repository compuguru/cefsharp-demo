﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CefSharpApp
{
    public partial class Form1 : Form
    {
        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        public ChromiumWebBrowser chromeBrowser;

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void InitializeChromium()
        {
            Debug.Assert(SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_System_DPI_Aware) == false);

            CefSettings settings = new CefSettings();

            // Specifying a CachePath is required for persistence of cookies, saving of passwords, etc.
            settings.CachePath = "cache";

            // For Windows 7 and above, best to include relevant app.manifest entries as well
            //Cef.EnableHighDPISupport();

            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.RemoteDebuggingPort = 8088;

            // Initialize cef with the provided settings
            Cef.Initialize(settings);

            #region MultiThreadedMessageLoop
            // CEF message loop integrated into the application message loo (Part 1)
            //settings.MultiThreadedMessageLoop = false;

            //// CEF message loop integrated into the application message loo (Part 2)
            //var timer = new Timer();
            //timer.Interval = 16;
            //timer.Tick += Timer_Tick;
            //timer.Start();
            #endregion

            // Create a browser component
            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string indexFile = Path.Combine(currentDirectory, "index.html");
            chromeBrowser = new ChromiumWebBrowser(indexFile);
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.KeyboardHandler = new DummyKeyboardHandler();
            chromeBrowser.RequestHandler = new DummyRequestHandler();
            chromeBrowser.IsBrowserInitializedChanged += ChromeBrowser_IsBrowserInitializedChanged;

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);

            chromeBrowser.JavascriptObjectRepository.Register("coolGuidGenerator", new GuidGenerator());
            chromeBrowser.JavascriptObjectRepository.Register("coolGuidGenerator2", new GuidGenerator(), isAsync: true);
            chromeBrowser.JavascriptObjectRepository.Register("boundAsync", new BoundObject(), true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Cef.DoMessageLoopWork();
        }

        private void ChromeBrowser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            var chromeBrowser = sender as ChromiumWebBrowser;
            if (!chromeBrowser.IsBrowserInitialized)
            {
                throw new InvalidOperationException();
            }

            // DevTools can be called from here too
            // chromeBrowser.ShowDevTools();
        }

        // Before CefSharp 75
        //private void ChromeBrowser_IsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        //{
        //    var chromeBrowser = sender as ChromiumWebBrowser;
        //    if (!chromeBrowser.IsBrowserInitialized)
        //    {
        //        throw new InvalidOperationException();
        //    }

        //    // DevTools can be called from here too
        //    // chromeBrowser.ShowDevTools();
        //}
    }
}
