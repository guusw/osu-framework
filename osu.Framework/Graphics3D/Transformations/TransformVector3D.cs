// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Framework.Graphics3D.Transformations
{
    public abstract class TransformVector3D : Transform<Vector3>
    {
        public override void Apply(ITransformable t)
        {
            base.Apply(t);
            var t1 = t as ITransformable3D;
            if(t1 != null)
            {
                Apply(t1);
            }
        }

        public abstract void Apply(ITransformable3D t);

        protected override Vector3 CurrentValue
        {
            get
            {
                double time = Time?.Current ?? 0;
                if(time < StartTime) return StartValue;
                if(time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }
        }
    }
}