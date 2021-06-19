using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using server.Interfaces;
using server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace server.db
{
    public class DbService
    {
        IMongoDatabase database { get;set; }
        Dictionary<string, IMongoCollection<ICollectionModel>> collections { get; set; }
        public object TryParseHexString { get; private set; }

        public DbService()
        {
          
            string connectionString = "mongodb+srv://Egor-source:Krotov_1998@cluster0.fok2o.mongodb.net/Diplom?retryWrites=true&w=majority";
            MongoClient client = new MongoClient(connectionString);
            database = client.GetDatabase("Diplom");

            collections = new Dictionary<string, IMongoCollection<ICollectionModel>>();
        }

        public void CreateColletion(string collectionName)
        {
            collections.Add(collectionName, database.GetCollection<ICollectionModel>(collectionName));
        }

        public async Task Create(string collectionName, ICollectionModel model)
        {
            await collections[collectionName].InsertOneAsync(model);
        }

        public async Task Update(string collectionName, ICollectionModel model)
        {
            var filter = Builders<ICollectionModel>.Filter.Eq("confirenceId", model.confirenceId);
            await collections[collectionName].ReplaceOneAsync(filter, model);
        }

        public async Task Remove(string collectionName, string confirenceId)
        {
            var filter = Builders<ICollectionModel>.Filter.Eq("confirenceId", confirenceId);
            await collections[collectionName].DeleteOneAsync(filter);
        }

        public async Task<ICollectionModel> Find(string collectionName, string confirenceId)
        {
            var filter = Builders<ICollectionModel>.Filter.Eq("confirenceId", confirenceId);
            return await collections[collectionName].Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<ICollectionModel>> Find(string collectionName, FilterDefinition<ICollectionModel> filter)
        {
            return await collections[collectionName].Find(filter).ToListAsync();
        }

        public  async  Task<IEnumerable<ICollectionModel>> GetAll(string collectionName)
        {
            var a = await collections[collectionName].Find(model => true).ToListAsync();

            return a;
        }
    }
}
