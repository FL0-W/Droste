using UnityEngine;

namespace Scripts.Action
{
    public class Action
    {
        private DroneController drone;
        private ActionType type;
        private float xLocation;
        private float yLocation;
        private float zLocation;

        private bool finished;


        public Action(DroneController drone, ActionType type, float xLocation, float yLocation, float zLocation)
        {
            this.drone = drone;
            this.xLocation = xLocation;
            this.yLocation = yLocation;
            this.zLocation = zLocation;
            this.finished = false;
            this.type = type;
        }

        public void Execute(){
            if(type == ActionType.MOVINGTOLOCATION){
                UpdateDistances();
                drone.GoToAndStabilize(xLocation, zLocation);
            }
            else if(type == ActionType.GOINGUPDOWN){
                drone.GoToTargetedHeight(yLocation);
            }
            UpdateStatus();
        }

        public ActionType GetType()
        {
            return type;
        }

        public bool IsFinished()
        {
            return finished;
        }

        public void UpdateDistances(){
            //Assign distance on xAxis
            if(drone.GetDrone().position.x > xLocation){
                drone.SetDistanceX(drone.GetDrone().position.x - xLocation);
            }else{
                drone.SetDistanceX(xLocation - drone.GetDrone().position.x);
            }
            //Assign distance on zAxis
            if(drone.GetDrone().position.z > zLocation){
                drone.SetDistanceZ(drone.GetDrone().position.z - zLocation);
            }else{
                drone.SetDistanceZ(zLocation - drone.GetDrone().position.z);
            }
        }

        public void UpdateStatus()
        {
            if(type == ActionType.MOVINGTOLOCATION){
                finished = (Mathf.Round(drone.GetDrone().position.x) == Mathf.Round(xLocation)
                && Mathf.Round(drone.GetDrone().position.z) == Mathf.Round(zLocation)
                && drone.GetDrone().velocity.x < 0.001f && drone.GetDrone().velocity.x > -0.001f
                && drone.GetDrone().velocity.z < 0.001f && drone.GetDrone().velocity.z > -0.001f);
            }
            else if(type == ActionType.GOINGUPDOWN){
                float margin = 3f;
                finished = (drone.GetDrone().position.y < yLocation+margin && drone.GetDrone().position.y > yLocation
                && drone.GetDrone().velocity.y < 0.1f && drone.GetDrone().velocity.y > -0.1f);
            }

            if(finished){
                // print("OK");
                // Debug.Log("ACTION : "+type+" ON x="+xLocation+" ; y="+yLocation+" ; z="+zLocation+" FINISHED");
            }
        }


    }

}