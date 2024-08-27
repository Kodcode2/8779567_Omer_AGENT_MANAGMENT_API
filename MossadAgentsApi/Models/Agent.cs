using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using MossadAgentsApi.Enum;

namespace MossadAgentsApi.Models
{
    public class Agent
    {
        [Key]
        public int Id { get; set; }
        public string? photo_url { get; set; }
        public string? nickname { get; set; }
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public AgentStatus Status { get; set; } = AgentStatus.NotOnBoard;
    }
}
