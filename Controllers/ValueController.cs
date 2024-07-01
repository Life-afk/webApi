using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication6.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace WebApplication6.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValueController : ControllerBase
    {
        private DB _db;
        public ValueController(DB db)
        {
            _db = db;
        }
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Ошибка. Файл не был загружен.");
            }
            var stream = new MemoryStream();

            await file.CopyToAsync(stream);
            try
            {
                var user = new logins();
                stream.Position = 0;
                XSSFWorkbook workbook = new XSSFWorkbook(stream);
                ISheet sheet = workbook.GetSheetAt(0);
                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    IRow excelRow = sheet.GetRow(row);
                    if (excelRow != null)
                    {
                        int id = int.Parse(excelRow.GetCell(0)?.ToString());
                        string login = excelRow.GetCell(1)?.ToString();
                        string password = excelRow.GetCell(2)?.ToString();
                        DateTimeOffset d_c = DateTimeOffset.Parse(excelRow.GetCell(3).StringCellValue).UtcDateTime;
                        string post = excelRow.GetCell(4)?.ToString();

                        var existingUser = await _db.logins.FindAsync(id);
                        if (existingUser == null)
                        {
                            user = new logins
                            {
                                login = login,
                                Password = password,
                                d_c = d_c,
                                post = post
                            };

                            _db.logins.Add(user);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                return Ok("Данные успешно загружены");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }




        //[Authorize(Roles = "admin")]
        [HttpGet("logins")]
        public async Task<ActionResult<IEnumerable<logins>>> Get()
        {
            var result = await _db.logins.ToListAsync();
            return Ok(result);
        }
        [Authorize(Roles = "admin")]
        // GET api/values/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<logins>> Get(int id, logins logins)
        {
            logins user = await _db.logins.FirstOrDefaultAsync(x => x.id == id&& x.login == logins.login);
            if (user == null)
                return NotFound();
            return new ObjectResult(user);
        }
        // POST api/users
        [HttpPost("reg")]
        public async Task<ActionResult<logins>> Post(logins user)
        {

            var users = await _db.logins.ToListAsync();
            if (user == null)
            {
                return BadRequest("Введите логин и пароль пользователя");
            }
            else
            {
                if (users.Any(a => a.login == user.login))
                {
                    return BadRequest("Пользователь с таким логином уже существует");
                }
            }

            user.post = "employee";
            _db.logins.Add(user);
            await _db.SaveChangesAsync();
            return Ok(user);
        }
        // DELETE api/values/{id}
        [Authorize(Roles ="admin")]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> Delete(int id)
        {
            var user = await _db.logins.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _db.logins.Remove(user);
            await _db.SaveChangesAsync();
            return Ok("Пользователь удалён");
        }

        // DELETE api/values/delete
        [Authorize(Roles = "admin")]
        [HttpDelete("delete")]
        public async Task<ActionResult<logins>> Delete(logins logins)
        {

            var user = await _db.logins.FirstOrDefaultAsync(u => u.login == logins.login);
            if (user == null)
            {
                return NotFound();
            }
            _db.logins.Remove(user);
            await _db.SaveChangesAsync();
            return Ok("Пользователь удалён");
        }
        //UPGRAGE pw api/values/put/{id},{pw}
        [Authorize(Roles = "admin, employee")]
        [HttpPut("put/{id},{pw}")]
        public async Task<ActionResult<logins>> Put(int id, string pw, [FromBody] logins logins)
        {

            if (logins == null || id != logins.id)
            {
                return BadRequest("Неверные данные пользователя");
            }
            else
            {
               
                pw = logins.Password;
                var user = await _db.logins.FirstOrDefaultAsync(u => u.id == id && u.Password.Trim() == pw.Trim());
                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }
                else
                {
                    user.Password = logins.Password;
                    _db.logins.Update(user);
                    await _db.SaveChangesAsync();
                }
            }
            return Ok("Замена была успешной");
        }
        //UPGRAGE login api/values/put/{login}
        [Authorize(Roles = "admin, employee")]
        [HttpPut("put/{login}/{pw}")]
        public async Task<ActionResult<logins>> put_login(string login, string pw, [FromBody] logins logins)
        {

            if (logins == null || pw != logins.Password)
            {
                return BadRequest("Неверные данные пользователя");
            }

            var user = await _db.logins.FirstOrDefaultAsync(u => u.login == login && u.Password == pw);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            user.login = logins.login;
            _db.logins.Update(user);
            await _db.SaveChangesAsync();
            return Ok(user);
        }
        //UPGRAGE login api/values/put/{login}
        [Authorize(Roles = "admin")]
        [HttpPut("putpost/{id}")]
        public async Task<ActionResult<logins>> putpost(int id,[FromBody] logins logins)
        {

            if (logins == null || id != logins.id)
            {
                return BadRequest("Неверные данные пользователя");
            }

            var user = await _db.logins.FirstOrDefaultAsync(u => u.id == logins.id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            user.post = logins.post;
            _db.logins.Update(user);
            await _db.SaveChangesAsync();
            return Ok(user);
        }
        [HttpPost("Auto")]
        public async Task<IActionResult> Login([FromBody] logins logins)
        {
            if (logins == null)
            {
                return BadRequest("Введите логин и пароль");
            }

            // Поиск пользователя с совпадающими учетными данными
            var user = await _db.logins.SingleOrDefaultAsync(u => u.login == logins.login && u.Password == logins.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, user.post)
                };

                var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(60)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Ok(new { token });
            }

            return Unauthorized();
        }


        [HttpGet("secure")]
        [Authorize]
        public IActionResult Secure()
        {

            var user = HttpContext.User.Claims.ToArray()[0].Value;
            return Ok(user);
        }

        [NonAction]
        private async Task<logins> GetAuthorizeUser(string login)
        {
            return await _db.logins.SingleOrDefaultAsync(w => w.login == login);
        }
    }
}

