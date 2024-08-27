using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MossadAgentsApi.Data;
using MossadAgentsApi.Models;

namespace MossadAgentsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private  readonly ApplicationDbContext _context;

        public MissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Missions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mission>>> GetMissions()
        {
            return await _context.Missions.ToListAsync();
        }

        // GET: api/Missions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Mission>> GetMission(int id)
        {
            var mission = await _context.Missions.FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            return mission;
        }

        // PUT: api/Missions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMissionStatus(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null || mission.status != Enum.MissionsStatus.option)
            {
                return BadRequest();
            }
            
            mission.status = Enum.MissionsStatus.Assignment;
            var agent = await _context.Agents.FindAsync(mission.agentId);
            var target = await _context.Targets.FindAsync(mission.targetId);
            agent.Status = Enum.AgentStatus.OnMission;
            target.Status = 2;

            try
            {
                await _context.SaveChangesAsync();

                var miss= await _context.Missions.ToListAsync();

                List<Mission> MissionsToRemove = Logic.MissionsToRemoveAfterAssigned(miss, mission);
                _context.Missions.RemoveRange( MissionsToRemove );
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MissionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Missions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Mission>> PostMission(Mission mission)
        {
            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMission", new { id = mission.Id }, mission);
        }


        //עדכון משימות
        // POST: api/Missions/update
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("update")]
        public async Task<ActionResult<Mission>>  UpdateMissions()
        {
            var mission =  _context.Missions.Where(mis => mis.status == Enum.MissionsStatus.Assignment).ToList();
            if (mission.Count != 0)
            {
                foreach (var miss in mission)
                {
                    var agent = await _context.Agents.FindAsync(miss.agentId);
                    var target = await _context.Targets.FindAsync(miss.targetId);
                    Logic.MoveAgentAfterTarget(ref agent, target);
                    if (agent.x == target.x && agent.y == target.y)
                    {
                        miss.status = Enum.MissionsStatus.Completed;
                        agent.Status = Enum.AgentStatus.WaitForMission;
                        target.Status = 3;
                        //nid to by kiil , fals , dan
                        
                    }
                    _context.Agents.Update(agent);
                    await _context.SaveChangesAsync();

                }

            }
            return NoContent();



        }


        //public async Task<ActionResult<Mission>>UpdateMissions(Mission missions)
        //{
        //    _context.Missions.Add(missions);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetMission", new { id = missions.Id }, missions);
        //}

        // DELETE: api/Missions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMission(int id)
        {
            var mission = await _context.Missions.FindAsync(id);
            if (mission == null)
            {
                return NotFound();
            }

            _context.Missions.Remove(mission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MissionExists(int id)
        {
            return _context.Missions.Any(e => e.Id == id);
        }
    }
}
