using MossadAgentsApi.Data;
using MossadAgentsApi.Models;
using System;

namespace MossadAgentsApi
{
    public class Logic
    {
        private readonly ApplicationDbContext _context;
        public static int Xlenght = 1000;
         public static int Ylenght = 1000;

        public enum TargetStatus
        {
            IsAlive = 1,
            OnCatch = 2,
            IsDead = 3    
        }



        public static List<Mission> AgentToTarget(List<Agent> agents, List<Target> targets)
        {
            List<Mission> CurrentMissions = new List<Mission>();       
            
            targets= targets.Where(target => target.Status == 1 ).Where(target => target.x != 0).ToList();
          
            foreach (Agent agent in agents)
            {
                if(agent.Status != Enum.AgentStatus.WaitForMission)
                {
                    continue;
                }
                foreach (Target target in targets)
                {
                    //if ( agent.Status == false)
                    //{
                    double Distance = Math.Sqrt(Math.Pow(Convert.ToDouble(agent.x) - Convert.ToDouble(target.x), 2) + Math.Pow(Convert.ToDouble(agent.y) - Convert.ToDouble(target.y), 2));
                    if (Distance <= 200)
                    {
                        Mission mission = new Mission();
                        mission.agentId = agent.Id;
                        mission.targetId = target.Id;
                        mission.TimeToTarget = Convert.ToInt32(Distance / 40);
                        CurrentMissions.Add(mission);
                    }
                    //}

                }
            }

            return CurrentMissions;
        }

        public static void MoveAgentOrTarget(ref int x, ref int y, string direction)
        {
            switch (direction)
            {
                case "nw":
                    {
                        x--; y++;
                    }
                    break;
                case "n":
                    {
                        y++;
                    }
                    break;
                case "ne":
                    {
                        x++; y++;
                    }
                    break;
                case "e":
                    {
                        x++;
                    }
                    break;
                case "se":
                    {
                        x++; y--;
                    }
                    break;
                case "s":
                    {
                        y--;
                    }
                    break;
                case "sw":
                    {
                        x--; y--;
                    }
                    break;
                case "w":
                    {
                        x--; 
                    }
                    break;
                default:

                    break;
            }
        }

        public static void MoveAgentAfterTarget(ref Agent agent, Target target)
        {
          
            if (agent.x > target.x)
            {
                agent.x--;
            }
            if (agent.x < target.x)
            {
                agent.x++;
            }
            if (agent.y > target.y)
            {
                agent.y--;
            }
            if (agent.y < target.y)
            {
                agent.y++;
            }

        }

        public static List<Mission> MissionsToRemoveAfterAssigned(List<Mission> missions , Mission AssigneMission)
        { 
            List<Mission> missionsToRemove = new List<Mission>();
            foreach (Mission mission in missions)
            {
                if (mission.status ==  Enum.MissionsStatus.option)
                {
                    if (mission.agentId == AssigneMission.agentId || mission.targetId == AssigneMission.targetId)
                    {
                        missionsToRemove.Add(mission);
                    }
                }
                
            }
            return missionsToRemove;
        }
    }
}
