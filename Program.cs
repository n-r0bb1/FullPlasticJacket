using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer);
        if (sdlInitResult < 0)
            throw new InvalidOperationException("Failed to initialise SDL.");

        IntPtr window;
        unsafe
        {
            window = (IntPtr)sdl.CreateWindow(
                "Full Plastic Jacket",
                Sdl.WindowposUndefined, Sdl.WindowposUndefined,
                600, 600,
                (uint)WindowFlags.AllowHighdpi);

            if (window == IntPtr.Zero)
                throw sdl.GetErrorAsException() ?? new Exception("Failed to create window.");
        }

        IntPtr renderer;
        unsafe
        {
            renderer = (IntPtr)sdl.CreateRenderer(
                (Window*)window, -1, (uint)RendererFlags.Accelerated);
            sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        if (renderer == IntPtr.Zero)
            throw sdl.GetErrorAsException() ?? new Exception("Failed to create renderer.");

        using var game = new Game(sdl, window, renderer);

        var timer = new Stopwatch();
        timer.Start();
        var ev = new Event();

        bool running = true;
        while (running)
        {
            while (sdl.PollEvent(ref ev) != 0)
            {
                if (!game.HandleEvent(ref ev))
                {
                    running = false;
                    break;
                }
            }

            if (!running) break;

            var elapsed = timer.Elapsed;
            timer.Restart();

            game.Update(elapsed.TotalSeconds);
            game.Render();
        }

        // Renderer and window are cleaned up by Game.Dispose() via using.
        sdl.Quit();
    }
}
