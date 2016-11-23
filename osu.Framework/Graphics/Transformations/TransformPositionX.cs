// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Timing;
using OpenTK;

namespace osu.Framework.Graphics.Transformations
{
    public class TransformPositionX : TransformFloat
    {
        public override void Apply(ITransformable t)
        {
            base.Apply(t);
            var t1 = t as ITransformable2D;
            if(t1 != null)
            {
                t1.Position = new Vector2(CurrentValue, t1.Position.Y);
                return;
            }

            var t2 = t as ITransformable3D;
            if(t2 != null)
            {
                t2.Position = new Vector3(CurrentValue, t2.Position.Y, t2.Position.Z);
                return;
            }
        }
    }
}
