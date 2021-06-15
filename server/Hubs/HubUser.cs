using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Hubs
{
    public class HubUser
    {
        public HubUser(string login, string userRole,string connectionId)
        {
            this.login = login;
            this.userRole = userRole;
            this.connectionId = connectionId;
        }

        public string login { get;private set; }
        public string userRole { get;private set; }
        public string connectionId { get;private set; }
    }
}
