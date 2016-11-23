// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;

namespace osu.Framework.Graphics3D
{
    public class Drawable3DComparer : IComparer<Drawable3D>
    {
        public int Compare(Drawable3D x, Drawable3D y)
        {
            return x.LifetimeStart.CompareTo(y.LifetimeStart);
        }
    }
}