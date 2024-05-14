using Contract.Services;
using Domain.DTOs.UserDTO;
using Domain.ResponseMessage;
using EventManagerAPI.Services;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTService _hashService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IJWTService hashService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _hashService = hashService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login(UserForLogin user)
        {
            try
            {
                bool isUserValid = _userService.Login(user);
                if (isUserValid)
                {
                    UserForRead userForReturn = _userService.GetUser(user.Username);

                    string jwtToken = _hashService.GenerateToken(userForReturn);
                    return Ok(new { token = jwtToken });
                }
                //Log error
                _logger.LogError(ErrorMessage.NotAuthenticated);
                return Unauthorized(ErrorMessage.SomethingWrong);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForCreate user)
        {
            try
            {
                await _userService.Register(user);
                return Ok(SuccessMessage.Register);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(UserForForgotPassword user)
        {
            try
            {
                await _userService.ForgotPassword(user);
                return Ok(SuccessMessage.ForgotPassword);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(UserForResetPassword user)
        {
            try
            {
                await _userService.ResetPassword(user);
                return Ok(SuccessMessage.ResetPassword);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult Profile()
        {
            try
            {
                UserForRead userReturn = _userService.GetCurrentUser();
                return Ok(userReturn);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Get User Information. Just admin role and current user can access
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                UserForRead userReturn = _userService.GetCurrentUser();
                if (userReturn.Role == "Admin" || userReturn.Id == id)
                {
                    UserForRead userInfo = await _userService.GetUserById(id);
                    return Ok(userInfo);
                }
                return Unauthorized(ErrorMessage.Unauthorized);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Delete User. Just admin role can access
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUser(id);
                return Ok(SuccessMessage.DeleteUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateUser(UserForUpdate user)
        {
            try
            {
                await _userService.UpdateCurrentUser(user);
                return Ok(SuccessMessage.UpdateUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
