using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Interfaces;

namespace server.Models
{
  
    public class SessionModel:ICollectionModel
    {
        public ObjectId _id = ObjectId.GenerateNewId();
        public string confirenceId { get; set; }
        public string adminName { get; set; }

        public string adminPassword { get; set; }

        public string usersPassword { get; set; }

        public DateTime date { get; set; }

        public bool conferenceStarted { get; set; }
    }
}
