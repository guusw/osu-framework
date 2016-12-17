// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Linq;
using System.Text;
using osu.Framework.Logging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GetPName = OpenTK.Graphics.OpenGL.GetPName;
using GL = OpenTK.Graphics.OpenGL.GL;
using StringName = OpenTK.Graphics.OpenGL.StringName;

namespace osu.Framework.Platform
{
    public abstract class BasicGameWindow : GameWindow
    {
#if DEBUG
        public const bool DebuggingEnabled = true;
        public const GraphicsContextFlags ContextFlags = GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug;
#else
        public const bool DebuggingEnabled = false;
        public const GraphicsContextFlags ContextFlags = GraphicsContextFlags.ForwardCompatible;
#endif

        internal Version GLVersion;
        internal Version GLSLVersion;

        public BasicGameWindow(int width, int height)
            : base(width, height, GraphicsMode.Default, 
                  "window", GameWindowFlags.Default, DisplayDevice.Default, 
                  3, 2, ContextFlags)
        {
            Closing += (sender, e) => e.Cancel = ExitRequested?.Invoke() ?? false;
            Closed += (sender, e) => Exited?.Invoke();
            Cursor = MouseCursor.Empty;

            MakeCurrent();

            string version = GL.GetString(StringName.Version);
            string versionNumberSubstring = GetVersionNumberSubstring(version);
            GLVersion = new Version(versionNumberSubstring);
            version = GL.GetString(StringName.ShadingLanguageVersion);
            if (!string.IsNullOrEmpty(version))
            {
                try
                {
                    GLSLVersion = new Version(versionNumberSubstring);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"couldn't set GLSL version using string '{version}'");
                }
            }

            if (GLSLVersion == null)
                GLSLVersion = new Version();

            //Set up OpenGL related characteristics
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            
            string extensions = "";
            int numExtensions = GL.GetInteger(GetPName.NumExtensions);
            for(int i = 0; i < numExtensions;)
            {
                extensions += GL.GetString(StringNameIndexed.Extensions, i);
                if(++i < numExtensions)
                    extensions += " ";
            }

            Logger.Log($@"GL Initialized
                        GL Version:                 {GL.GetString(StringName.Version)}
                        GL Renderer:                {GL.GetString(StringName.Renderer)}
                        GL Shader Language version: {GL.GetString(StringName.ShadingLanguageVersion)}
                        GL Vendor:                  {GL.GetString(StringName.Vendor)}
                        GL Extensions:              {extensions}", LoggingTarget.Runtime, LogLevel.Important);

            Context.MakeCurrent(null);
        }

        private string GetVersionNumberSubstring(string version)
        {
            string result = version.Split(' ').FirstOrDefault(s => char.IsDigit(s, 0));
            if (result != null) return result;
            throw new ArgumentException(nameof(version));
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Alt && e.Key == Key.Enter)
            {
                WindowState = WindowState == WindowState.Fullscreen ? WindowState = WindowState.Normal : WindowState.Fullscreen;
                return;
            }

            base.OnKeyDown(e);
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        /// <summary>
        /// Return value decides whether we should intercept and cancel this exit (if possible).
        /// </summary>
        public event Func<bool> ExitRequested;

        public event Action Exited;
        
        protected void OnExited()
        {
            Exited?.Invoke();
        }

        protected bool OnExitRequested()
        {
            return ExitRequested?.Invoke() ?? false;
        }

        public virtual void CentreToScreen()
        {
        }
    }
}
