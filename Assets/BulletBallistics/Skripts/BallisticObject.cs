using UnityEngine;
using System.Collections;

namespace Ballistics
{
    public abstract class BallisticObject : MonoBehaviour
    {
        [HideInInspector]
        public int MatID;

        /// <summary>
        /// Bullet Impact with this object
        /// </summary>
        /// <param name="weapon">Weapon the bullet has been fired with</param>
        /// <param name="bulletDir">Direction if the bullet</param>
        /// <param name="bulletSpeedBefore">Bulletspeed before impact</param>
        /// <param name="bulletSpeedAfter">Bulletspeed after impact</param>
        /// <param name="penetrationDepth">Impact depth</param>
        /// <param name="rayHit">RayCastHit of the impact</param>
        /// <param name="myBallisticObjData">Material data of this object</param>
        public abstract void BulletImpact(Weapon weapon, Vector3 bulletDir, float bulletSpeedBefore, float bulletSpeedAfter, float penetrationDepth, RaycastHit rayHit, BallisticObjectData myBallisticObjData);
    }
}
