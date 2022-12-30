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
        private float packageHeight;
        private bool landing;

        private bool finished;


        public Action(DroneController drone, ActionType type, float xLocation, float yLocation, float zLocation, float packageHeight = 0.5f)
        {
            this.drone = drone;
            this.type = type;
            if(type == ActionType.GOBACKANDSHUTDOWN){
                this.xLocation = drone.GetInitialX();
                this.yLocation = drone.GetInitialY();
                this.zLocation = drone.GetInitialZ();
            }else{
                this.xLocation = xLocation;
                this.yLocation = yLocation;
                this.zLocation = zLocation;
            }
            this.packageHeight = packageHeight;
            this.landing = false;
            this.finished = false;

        }

        public void Execute(){
            switch (type)
            {
                case ActionType.MOVINGTOLOCATION:
                    UpdateDistances();
                    drone.GoToAndStabilize(xLocation, zLocation);
                    break;
                case ActionType.GOINGUPDOWN:
                    drone.GoToTargetedHeight(yLocation);
                    break;
                case ActionType.GETTINGAPACKAGE:
                    drone.GoDownAndGetPackage(yLocation, packageHeight);
                    break;
                case ActionType.GOBACKANDSHUTDOWN:
                    drone.GoBackAndShutDown(landing);
                    break;
                default:
                    break;
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
            float margin = 3f;
            switch (type)
            {
                case ActionType.MOVINGTOLOCATION:
                    finished = (Mathf.Round(drone.GetDrone().position.x) == Mathf.Round(xLocation)
                        && Mathf.Round(drone.GetDrone().position.z) == Mathf.Round(zLocation)
                        && drone.GetDrone().velocity.x < 0.001f && drone.GetDrone().velocity.x > -0.001f
                        && drone.GetDrone().velocity.z < 0.001f && drone.GetDrone().velocity.z > -0.001f);
                    break;
                case ActionType.GOINGUPDOWN:
                    finished = (drone.GetDrone().position.y < yLocation+margin && drone.GetDrone().position.y > yLocation
                        && drone.GetDrone().velocity.y < 0.1f && drone.GetDrone().velocity.y > -0.1f);
                    break;
                case ActionType.GETTINGAPACKAGE:
                    finished = ( drone.IsCharged() &&
                        drone.GetDrone().position.y < yLocation+margin && drone.GetDrone().position.y > yLocation
                        && drone.GetDrone().velocity.y < 0.1f && drone.GetDrone().velocity.y > -0.1f);
                    break;
                case ActionType.GOBACKANDSHUTDOWN:
                    landing = (Mathf.Round(drone.GetDrone().position.x) == Mathf.Round(xLocation)
                        && Mathf.Round(drone.GetDrone().position.z) == Mathf.Round(zLocation)
                        && drone.GetDrone().velocity.x < 0.001f && drone.GetDrone().velocity.x > -0.001f
                        && drone.GetDrone().velocity.z < 0.001f && drone.GetDrone().velocity.z > -0.001f);

                    finished = !drone.IsActive();
                    break;
                default:
                    break;
            }

            if(finished){
                // print("OK");
                // Debug.Log("ACTION : "+type+" ON x="+xLocation+" ; y="+yLocation+" ; z="+zLocation+" FINISHED");
            }
        }


    }

}