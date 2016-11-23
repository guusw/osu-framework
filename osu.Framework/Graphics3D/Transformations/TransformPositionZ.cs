// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Framework.Graphics3D.Transformations
{
    public class TransformPositionZ : TransformFloat
    {
        public override void Apply(ITransformable t)
        {
            base.Apply(t);

            var t2 = t as ITransformable3D;
            if(t2 != null)
            {
                t2.Position = new Vector3(t2.Position.X, t2.Position.Y, CurrentValue);
                return;
            }
        }
    }
}