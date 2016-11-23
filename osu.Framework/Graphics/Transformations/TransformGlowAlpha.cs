// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Framework.Graphics.Transformations
{
    public class TransformGlowAlpha : TransformFloat
    {
        public override void Apply(ITransformable t)
        {
            base.Apply(t);
            Container c = t as Container;

            EdgeEffect e = c.EdgeEffect;
            e.Colour.Linear.A = CurrentValue;
            c.EdgeEffect = e;
        }
    }
}