using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using server.Models;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace server.Hubs
{
    public class ConfirencesHub:Hub
    {
      static Dictionary<string, List<HubUser>> groups { get; set; } 

       public ConfirencesHub()
        {

            using (StreamReader sr = new StreamReader("groups.json"))
            {
                groups = JsonConvert.DeserializeObject<Dictionary<string, List<HubUser>>>(sr.ReadToEnd());
                sr.Close();
            }

            if (groups == null)
            {
                groups = new Dictionary<string, List<HubUser>>();
            }

        }

       public async Task UserConnection(string json)
       {
            HubModel session = JsonConvert.DeserializeObject<HubModel>(json);

            List<HubUser> group=new List<HubUser>();

            if (groups.ContainsKey(session.confirenceId))
            {

                group = groups[session.confirenceId];
                List<string> logins = new List<string>();

                Regex regex = new Regex(@"\((\d){1,3}\)");

                foreach (var i in group)
                {

                    if(i.login == session.login)
                    {
                        MatchCollection matches = regex.Matches(session.login);

                        if (matches.Count > 0)
                        {
                            logins.Add(i.login);
                            session.login = regex.Replace(session.login, $"({logins.Count+1})");
                        }
                        else
                        {
                            session.login += " (1)";
                        }

                        session.userRole = "user";
                        
                    }

                }


                await Clients.Caller.SendAsync("getUsersList", JsonConvert.SerializeObject(group), session.login);

                groups[session.confirenceId].Add(new HubUser(session.login,session.userRole,Context.ConnectionId));

                await Clients.Group(session.confirenceId).SendAsync("newUserConnected", session.login);
            }
            else
            {

                List<HubUser> list = new List<HubUser>();

                list.Add(new HubUser(session.login,session.userRole, Context.ConnectionId));

                groups.Add(session.confirenceId, list);

                await Clients.Caller.SendAsync("getUsersList", JsonConvert.SerializeObject(group), session.login);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, session.confirenceId);


            using (StreamWriter sw = new StreamWriter("groups.json"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(groups));
                sw.Close();
            }

        }


        public async Task BetrayPublicKey(string confirenceId, string publicKey)
        {
            await Clients.Group(confirenceId).SendAsync("getMainKey", publicKey,Context.ConnectionId);
        }

        public async Task BetrayMainKey(string callerId, string mainKey)
        {
            await Clients.Client(callerId).SendAsync("setMainKey", mainKey);
        }

        public async Task SendMessage(string confirenceId, string message)
        {
            await Clients.GroupExcept(confirenceId, Context.ConnectionId).SendAsync("getMessage", message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {

            string endSessionId = null;

            foreach (var i in groups)
            {
                HubUser disconectedUser = i.Value.Where(x => x.connectionId == Context.ConnectionId).FirstOrDefault();
                if(disconectedUser != null)
                {

                    if(disconectedUser.userRole== "admin")
                    {
                        endSessionId = i.Key;
                        groups.Remove(i.Key);
                        await Clients.Group(i.Key).SendAsync("endSession");
                    }
                    else
                    {
                        i.Value.Remove(disconectedUser);
                        await Clients.Group(i.Key).SendAsync("userDisconnected", disconectedUser.login);
                    }

                    using (StreamWriter sw = new StreamWriter("groups.json"))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(groups));
                        sw.Close();
                    }

                    break;
                }
            }

            if (endSessionId != null)
            {

               List<SessionModel> sessions ;

                using (StreamReader sr = new StreamReader("sessions.json"))
                {
                    sessions = JsonConvert.DeserializeObject<List<SessionModel>>(sr.ReadToEnd());
                    sr.Close();
                }

                sessions.Remove(sessions.Where(x => x.confirenceId == endSessionId).First());

                using (StreamWriter sw = new StreamWriter("sessions.json"))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(sessions));
                    sw.Close();
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}
