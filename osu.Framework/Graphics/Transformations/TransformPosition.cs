// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Timing;

namespace osu.Framework.Graphics.Transformations
{
    public class TransformPosition : TransformVector2D
    {
        public override void Apply(ITransformable t)
        {
            var t1 = t as ITransformable2D;
            if(t1 != null)
            {
                t1.Position = CurrentValue;
            }
        }
    }
}
