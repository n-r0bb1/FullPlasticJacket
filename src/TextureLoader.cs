using Silk.NET.SDL;
using StbImageSharp;

namespace TheAdventure;

/*Start of AI generated code*/
public static unsafe class TextureLoader
{
    public static Texture* Load(Sdl sdl, Renderer* renderer, string path)
    {
        var result = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* pixels = result.Data)
        {
            var surface = sdl.CreateRGBSurfaceWithFormatFrom(
                pixels,
                result.Width, result.Height,
                32, result.Width * 4,
                (uint)PixelFormatEnum.Rgba32);

            var texture = sdl.CreateTextureFromSurface(renderer, surface);
            sdl.FreeSurface(surface);
            return texture;
        }
    }
}
/*End of AI generated code*/
