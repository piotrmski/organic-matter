using Godot;

namespace Organicmatter.Scripts.Internal.RenderingStrategy
{
    internal interface IRenderer
    {
        Image RenderedImage { get; }

        void UpdateImage();
    }
}
