using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MossadAgentsApi.Data;
using MossadAgentsApi.Models;
using System.Text.Json;

namespace MossadAgentsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AgentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Agents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
        {
            return await _context.Agents.ToListAsync();
        }

        // GET: api/Agents/5
        [HttpGet("{id}")]
        public async Task<   ActionResult<Agent>> GetAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);

            if (agent == null)
            {
                return NotFound();
            }

            return agent;
        }

        // PUT: api/Agents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAgent(int id, Agent agent)
        {
            if (id != agent.Id)
            {
                return BadRequest();
            }

            _context.Entry(agent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AgentExists(id))
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

        [HttpPut("{id}/pin")]
        public async Task<IActionResult> PutAgentPin(int id, int x, int y)
        {
            var agent = await _context.Agents.FindAsync(id);

            if (agent == null)
            {
                return BadRequest();
            }
            else
            {
                agent.x = x;
                agent.y = y;
                try
                {
                    await _context.SaveChangesAsync();
                    var mission = _context.Missions.Where(mis => mis.status == Enum.MissionsStatus.option).ToList();
                    if (mission.Count != 0)
                    {
                        _context.Missions.RemoveRange(mission);
                        await _context.SaveChangesAsync();

                    }


                    var agents = await _context.Agents.ToListAsync();
                    var targets = await _context.Targets.ToListAsync();
                    List<Mission> missions = Logic.AgentToTarget(agents, targets);
                    _context.Missions.AddRange(missions);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return NoContent();
        }

        // POST: api/Agents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostAgent(Agent agent)
        {
            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            return JsonSerializer.Serialize(new { agent.Id });
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> PutTargetMove(int id, string direction)
        {
            var agent = await _context.Agents.FindAsync(id);
            string error = string.Empty;
            if (agent == null)
            {
                return BadRequest();
            }
            else
            {
                int x = agent.x;
                int y = agent.y;
                if (x != 0) // if agent dont have begin location
                {
                    
                    if (agent.Status == Enum.AgentStatus.WaitForMission)
                    {
                        Logic.MoveAgentOrTarget(ref x, ref y, direction);

                        if (x <= Logic.Xlenght && x > 0 && y <= Logic.Ylenght && y > 0)
                        {
                            agent.x = x;
                            agent.y = y;
                        }
                        else
                        {
                            error = "Cannot move outside the matrix.";
                        }
                    }
                    else // agent is in a mission
                    {
                        error = "agent is in a mission";
                    }
                }
                else
                {
                   error = "agent dont have begin location";
                }

                if (error != string.Empty)
                { 
                    return StatusCode( 400, new { error,} ); 
                }




                try
                {
                    await _context.SaveChangesAsync();

                    var mission =  _context.Missions.Where(mis => mis.status == Enum.MissionsStatus.option).ToList() ;
                    if (mission.Count != 0)
                    {
                        _context.Missions.RemoveRange(mission);
                        await _context.SaveChangesAsync();

                    }

                    
                    var agents = await _context.Agents.ToListAsync();
                    var targets = await _context.Targets.ToListAsync();
                    List<Mission> missions = Logic.AgentToTarget(agents, targets);
                    _context.Missions.AddRange(missions);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! AgentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return NoContent();
        }

        // DELETE: api/Agents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            _context.Agents.Remove(agent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AgentExists(int id)
        {
            return _context.Agents.Any(e => e.Id == id);
        }

        
    }
}
