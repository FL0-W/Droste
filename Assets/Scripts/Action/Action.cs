using UnityEngine;

namespace Scripts.Action
{
    public class Action
    {
        private DroneController drone;
        private float xLocation;
        private float zLocation;


        public Action(DroneController drone, float xLocation, float zLocation)
        {
            this.drone = drone;
            this.xLocation = xLocation;
            this.zLocation = zLocation;
        }

        public void Execute(){
            drone.GoToAndStabilize(xLocation, zLocation);
        }
    }

}