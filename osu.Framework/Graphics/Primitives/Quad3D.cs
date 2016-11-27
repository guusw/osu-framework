// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Framework.Graphics.Primitives
{
    /// <summary>
    /// Same as <see cref="Quad"/> but used Vector3's for position
    /// </summary>
    public struct Quad3D
    {
        public Vector3 TopLeft;
        public Vector3 TopRight;
        public Vector3 BottomLeft;
        public Vector3 BottomRight;

        public Quad3D(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public Quad3D(Quad quad)
        {
            TopLeft = new Vector3(quad.TopLeft.X, quad.TopLeft.Y, 0);
            TopRight = new Vector3(quad.TopRight.X, quad.TopRight.Y, 0);
            BottomLeft = new Vector3(quad.BottomLeft.X, quad.BottomLeft.Y, 0);
            BottomRight = new Vector3(quad.BottomRight.X, quad.BottomRight.Y, 0);
        }

        public Quad3D(float x, float y, float width, float height)
            : this()
        {
            TopLeft = new Vector3(x, y, 0);
            TopRight = new Vector3(x + width, y, 0);
            BottomLeft = new Vector3(x, y + height, 0);
            BottomRight = new Vector3(x + width, y + height, 0);
        }

        public double ConservativeArea
        {
            get
            {
                if(Precision.AlmostEquals(TopLeft.Y, TopRight.Y))
                    return Math.Abs((TopLeft.Y - BottomLeft.Y) * (TopLeft.X - TopRight.X));

                // Uncomment this to speed this computation up at the cost of losing accuracy when considering shearing.
                //return Math.Sqrt(Vector2.DistanceSquared(TopLeft, TopRight) * Vector2.DistanceSquared(TopLeft, BottomLeft));

                Vector3 d1 = TopLeft - TopRight;
                float l1sq = d1.LengthSquared;

                Vector3 d2 = TopLeft - BottomLeft;
                float l2sq = Vector3.DistanceSquared(d2, d1 * Vector3.Dot(d2, d1 * (MathHelper.InverseSqrtFast(l1sq))));

                return (float)Math.Sqrt(l1sq * l2sq);
            }
        }
    }
}