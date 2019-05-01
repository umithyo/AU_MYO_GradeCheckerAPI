using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using GradeCheckerAPI.Helpers;
using GradeCheckerAPI.Data;
using GradeCheckerAPI.Models;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GradeCheckerAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    [Authorize(Roles = "Admin")]
    public class ValuesController : ControllerBase
    {
        private readonly APIContext context;
        private readonly AppSettings appSettings;
        public ValuesController(APIContext _context, IOptions<AppSettings> _appSettings)
        {
            context = _context;
            appSettings = _appSettings.Value;
        }

        // GET api/GetUsers
        [HttpGet("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(context.Users.ToList());
        }

        // Post api/NewEntry
        [HttpPost("NewEntry")]
        public IActionResult NewEntry([FromBody]JObject data)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            var user = context.Users.FirstOrDefault(x=>x.Id == new Guid (data["userId"].ToString()));
            var className = data["className"].ToString();
            var seens = context.SeenClasses.Include(x => x.User).Where(x => x.User.Id == user.Id).ToList();
            if (seens.Any(x => x.ClassName == className))
                return Ok();
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", appSettings.OneSignalAPIKey);

            var obj = new
            {
                app_id = appSettings.OneSignalAppID,
                contents = new { en = className },
                headings = new { en = "Possible grade entry"},
                include_player_ids = new string[] { user.DeviceId.ToString() }
            };
            var param = JsonConvert.SerializeObject(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }

               
                if (!seens.Any(x=>x.ClassName == className))
                {
                    var seenClass = new Seen();
                    seenClass.ClassName = className;
                    seenClass.User = user;
                    context.Add(seenClass);
                    context.SaveChanges();
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
            return Ok();
        }



        // POST api/createuser
        [HttpPost("CreateUser")]
        [AllowAnonymous]
        public IActionResult CreateUser([FromBody] JObject data)
        {
            if (context.Users.Any(x => x.Username == data["username"].ToString()))
            {
                var user = context.Users.FirstOrDefault(x => x.Username == data["username"].ToString());
                
                user.Username = data["username"].ToString();
                //user.Password = Utils.Encrypt<AesManaged>(data["password"].ToString(), data["Username"].ToString(), DateTime.Now.Ticks.ToString());
            }
            else
            {
                var user = new User()
                {
                    Username = data["username"].ToString(),
                    //Password = Utils.Encrypt<AesManaged>(data["password"].ToString(), data["Username"].ToString(), DateTime.Now.Ticks.ToString())
            };
                context.Users.Add(user);
            }

            context.SaveChanges();

            return Ok(new { id = context.Users.FirstOrDefault(x => x.Username == data["username"].ToString()).Id });
        }

        [HttpPut("UpdateUser/{id}")]
        [AllowAnonymous]
        public IActionResult UpdateUser(Guid id, [FromBody] JObject data)
        {
            var user = context.Users.FirstOrDefault(x => x.Id == id);
            if (data["username"]!=null && data["password"]!=null)
            {
                user.Username = data["username"].ToString();
                var salt = Convert.ToBase64String(new byte[32]);
                user.Password = Utils.Encrypt<AesManaged>(data["password"].ToString(), user.Username, salt);
                user.Salt = salt;
            }
            user.DeviceId = new Guid(data["DeviceId"].ToString());
            context.SaveChanges();
            return Ok();
        }

        public string GenerateAdminToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.Now.AddYears(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
