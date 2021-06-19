using MongoDB.Bson;
using server.Hubs;
using server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models
{
    public class GropModel:ICollectionModel
    {
        public ObjectId _id = ObjectId.GenerateNewId();
        public string confirenceId { get; set; }

        public List<HubUser> users { get; set; }
    }
}
