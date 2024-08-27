using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MossadAgentsApi.Data;
using MossadAgentsApi.Models;
using static MossadAgentsApi.Logic;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MossadAgentsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {

        
        private readonly ApplicationDbContext _context;

        public TargetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Targets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Target>>> GetTargets()
        {
            return await _context.Targets.ToListAsync();
        }

        // GET: api/Targets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Target>> GetTarget(int id)
        {
            var target = await _context.Targets.FindAsync(id);

            if (target == null)
            {
                return NotFound();
            }

            return target;
        }

        // PUT: api/Targets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTarget(int id, Target target)
        {
            if (id != target.Id)
            {
                return BadRequest();
            }

            _context.Entry(target).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TargetExists(id))
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
        public async Task<IActionResult> PutTargetPin(int id, int x, int y)
        {
            var target = await _context.Targets.FindAsync(id);
           
            if (target == null)
            {
                return BadRequest();
            }
            else
            {
                target.x = x;
                target.y = y;
                
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
                    if (!TargetExists(id))
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

        [HttpPut("{id}/move")]
        public async Task<IActionResult> PutTargetMove(int id, string direction)
        {
            var target = await _context.Targets.FindAsync(id);
            string error = string.Empty;

            if (target == null)
            {
                return BadRequest();
            }
            else
            {
                int x = target.x;
                int y = target.y;
                if (x != 0 ) // if target dont have begin location
                {

                    if (target.Status == 1)
                    {
                        Logic.MoveAgentOrTarget(ref x, ref y, direction);

                        if (x <= Logic.Xlenght && x > 0 && y <= Logic.Ylenght && y > 0)
                        {
                            target.x = x;
                            target.y = y;
                        }
                        else
                        {
                            error = "Cannot move outside the matrix.";
                        }
                    }
                    else // target is running or dead
                    {
                        if (target.Status == 2)
                        {
                            error = "target is in a running";
                        }
                        else
                        {
                            error = "target is dead";
                        }
                       
                    }
                }
                else
                {
                    error = "target dont have begin location";
                }

                if (error != string.Empty)
                {
                    return StatusCode(400, new { error, });
                }



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
                    if (!TargetExists(id))
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

        // POST: api/Targets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostTarget(Target target)
        {
            _context.Targets.Add(target);
            await _context.SaveChangesAsync();

            return JsonSerializer.Serialize(new { target.Id});
        }

        // DELETE: api/Targets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarget(int id)
        {
            var target = await _context.Targets.FindAsync(id);
            if (target == null)
            {
                return NotFound();
            }

            _context.Targets.Remove(target);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TargetExists(int id)
        {
            return _context.Targets.Any(e => e.Id == id);
        }
    }
}
