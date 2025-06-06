using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BulletGame.Core
{
    public interface ICollider
    {
        IEnumerable<Vector2> GetAxes();
        Projection Project(Vector2 axis);
    }
}