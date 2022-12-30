using UnityEngine;

namespace Scripts.Rotor
{
    public interface IRotor
    {
        void UpdateRotor(Rigidbody rb, float yTarget, bool isCharged);
    }

}
