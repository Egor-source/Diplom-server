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
using server.db;
using MongoDB.Driver;

namespace server.Hubs
{
    public class ConfirencesHub:Hub
    {
      static Dictionary<string, List<HubUser>> groups { get; set; }
      private readonly DbService db;

        public ConfirencesHub(DbService context)
        {
            db = context;
            db.CreateColletion("groups");
            db.CreateColletion("sessions");
        }

       public async Task UserConnection(string json)
       {
            HubModel session = JsonConvert.DeserializeObject<HubModel>(json);

            GropModel groupModel = await db.Find("groups", session.confirenceId) as GropModel;

            if (groupModel != null)
            {
                List<HubUser> group  = groupModel.users;

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

                groupModel.users.Add(new HubUser(session.login, session.userRole, Context.ConnectionId));

                await Clients.Group(session.confirenceId).SendAsync("newUserConnected", session.login);

                await db.Update("groups", groupModel);

            }
            else
            {

                List<HubUser> list = new List<HubUser>();

                list.Add(new HubUser(session.login,session.userRole, Context.ConnectionId));

                GropModel newGroup = new GropModel();

                newGroup.confirenceId = session.confirenceId;
                newGroup.users = list;

                await Clients.Caller.SendAsync("getUsersList", JsonConvert.SerializeObject(new List<HubUser>()), session.login);

                await db.Create("groups", newGroup);

            }

            await Groups.AddToGroupAsync(Context.ConnectionId, session.confirenceId);

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
            var collection = await db.GetAll("groups");

            List<GropModel> groups = new List<GropModel>();

            foreach(var i in collection)
            {
                groups.Add(i as GropModel);
            }

            GropModel group = groups.Where(group => group.users.Where(user => user.connectionId == Context.ConnectionId).FirstOrDefault() != null).FirstOrDefault();

            HubUser disconectedUser = group.users.Where(x => x.connectionId == Context.ConnectionId).FirstOrDefault();

            if (disconectedUser.userRole == "admin")
            {
                await Clients.Group(group.confirenceId).SendAsync("endSession");
                await db.Remove("groups", group.confirenceId);
                await db.Remove("sessions", group.confirenceId);
            }
            else
            {
                group.users.Remove(disconectedUser);
                await Clients.Group(group.confirenceId).SendAsync("userDisconnected", disconectedUser.login);
                await db.Update("groups", group);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

}
