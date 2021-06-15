using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class SessionModel
    {
        public string confirenceId { get; set; }

        public string adminName { get; set; }

        public string adminPassword { get; set; }

        public string usersPassword { get; set; }

        public DateTime date { get; set; }

        public bool conferenceStarted { get; set; }
    }
}
