// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using OpenTK;

namespace osu.Framework.Graphics3D.Transformations
{
    public class TransformRotation : Transform<Quaternion>
    {
        protected override Quaternion CurrentValue {
            get
            {
                double time = Time?.Current ?? 0;
                if(time < StartTime) return StartValue;
                if(time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }
        }

        public override void Apply(ITransformable t)
        {
            base.Apply(t);
            var t1 = t as ITransformable3D;
            if(t1 != null)
                t1.Rotation = CurrentValue;
        }
    }
}
