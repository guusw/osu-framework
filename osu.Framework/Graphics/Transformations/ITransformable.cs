// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics.Colour;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Framework.Graphics.Transformations
{
    public interface ITransformable
    {
        float Alpha { get; set; }
        SRGBColour Colour { get; set; }
    }

    public interface ITransformable2D : ITransformable
    {
        float Rotation { get; set; }
        Vector2 Scale { get; set; }
        Vector2 Size { get; set; }
        Vector2 Position { get; set; }
    }
    
    public interface ITransformable3D : ITransformable
    {
        Vector3 Scale { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
    }
}