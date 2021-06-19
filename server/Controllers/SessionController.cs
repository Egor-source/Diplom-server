using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using server.Models;
using Newtonsoft.Json;
using System.IO;
using server.Exeptions;
using server.db;
using MongoDB.Bson;

namespace server.Controllers
{
    [ApiController]
    public class SessionController : ControllerBase
    {

        //List<SessionModel> sessions = new List<SessionModel>();
        private readonly DbService db;

        public SessionController(DbService context)
        {
            //using(StreamReader sr=new StreamReader("sessions.json"))
            //{
            //    sessions = JsonConvert.DeserializeObject<List<SessionModel>>(sr.ReadToEnd());

            //    sr.Close();
            //}

            //if (sessions == null)
            //{
            //    sessions = new List<SessionModel>();
            //}

            db = context;
            db.CreateColletion("sessions");
        }

        /// <summary>
        /// Метод создания конференции
        /// </summary>
        /// <param name="newSession">Информация о конверенции</param>
        /// <returns>Статус запроса</returns>
        [Route("api/[controller]/Create")]
        [HttpPost]
        public  async Task<ActionResult> Create(SessionModel newSession)
        {

            try
            {
                //    sessions.Add(newSession);

                //    using (StreamWriter sw = new StreamWriter("sessions.json"))
                //    {
                //        sw.WriteLine(JsonConvert.SerializeObject(sessions));
                //        sw.Close();
                //    }
               await db.Create("sessions", newSession);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Метод для входа в конференцию
        /// </summary>
        /// <param name="user">Информация о пользователи</param>
        /// <returns>Статус запроса</returns>
        [Route("api/[controller]/Login")]
        [HttpPost]
        public  async Task<ActionResult> Login(UserModel user)
        {
            SessionModel session =await db.Find("sessions", user.confirenceId) as SessionModel;

            string userRole;

            try
            {
                if (session == null)
                {
                  throw new LoginExceptions("Конференция с данным адресом не зарегестрирована");
                }

                if (session.adminName == user.login)
                {

                    if (session.adminPassword == user.password)
                    {
                        //sessions.Remove(session);
                        session.conferenceStarted = true;
                        //sessions.Add(session);

                        await db.Update("sessions", session);

                        userRole = "admin";

                        //using (StreamWriter sw = new StreamWriter("sessions.json"))
                        //{
                        //    sw.WriteLine(JsonConvert.SerializeObject(sessions));
                        //    sw.Close();
                        //}
                    }
                    else
                    {
                        throw new LoginExceptions("Неверный пароль");
                    }
                }
                else
                {
                    if(session.usersPassword!=user.password)
                    {
                        throw new LoginExceptions("Неверный пароль");
                    }

                    if (!session.conferenceStarted)
                    {
                        throw new LoginExceptions("Конференция еще не началась");
                    }

                    userRole = "user";
                }
            }
            catch(LoginExceptions e)
            {
                return BadRequest(e.Message);
            }
           

            return Ok(userRole);
        }

        [Route("api/[controller]/CheckLocalStorage")]
        [HttpPost]
        public  async  Task<string> CheckLocalStorage(string[] confirencesId)
        {
            List<string> existingСonferences = new List<string>();

            foreach (var i in confirencesId)
            {
                SessionModel session = await db.Find("sessions", i) as SessionModel;
                if (session != null)
                {
                    existingСonferences.Add(session.confirenceId);
                }

            }

            return JsonConvert.SerializeObject(existingСonferences);
        }
    }
}
