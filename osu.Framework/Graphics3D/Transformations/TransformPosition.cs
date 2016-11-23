// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Transformations;
using osu.Framework.Timing;

namespace osu.Framework.Graphics3D.Transformations
{
    public class TransformPosition : TransformVector3D
    {
        public override void Apply(ITransformable3D t)
        {
            t.Position = CurrentValue;
        }
    }
}
