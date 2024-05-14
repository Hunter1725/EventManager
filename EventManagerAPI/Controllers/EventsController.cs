using Contract.Services;
using Domain.DTOs.EventDTO;
using Domain.DTOs.UserDTO;
using Domain.Entities;
using Domain.Interfaces.Services;
using Domain.ResponseMessage;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ILogger<EventsController> _logger;
        private readonly UserForRead _currentUser;

        public EventsController(
            IEventService eventService,
            IUserService userService,
            ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _userService = userService;
            _currentUser = _userService.GetCurrentUser();
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateEvent([FromBody] EventForCreate eventForCreate)
        {
            try
            {
                //Check if user is authenticated
                if (_currentUser == null)
                {
                    //Log error
                    _logger.LogError("User is not authenticated");
                    return Unauthorized(ErrorMessage.Unauthorized);
                }          
                await _eventService.CreateEvent(eventForCreate, _currentUser.Id);

                return Ok(SuccessMessage.CreateEvent);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        //Get events of current user or all events if user is admin
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public IActionResult GetEvents()
        {
            if (_currentUser == null)
            {
                //Log error
                _logger.LogError("User is not authenticated");
                return Unauthorized(ErrorMessage.Unauthorized);
            }
            var events = _currentUser.Role == "Admin" ? _eventService.GetEvents() :
                _eventService.GetEventsByUserId(_currentUser.Id);
            return Ok(events);
        }

        //Get a event by id. Just admin role and current user can access
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetEventById(int id)
        {
            try
            {
                if (_currentUser == null)
                {
                    //Log error
                    _logger.LogError("User is not authenticated");
                    return Unauthorized(ErrorMessage.Unauthorized);
                }
                var eventRead = await _eventService.GetEventById(id);

                //Check event exists
                if (eventRead == null)
                {
                    return NotFound(ErrorMessage.EventNotFound);
                }

                if (eventRead.UserId == _currentUser.Id || _currentUser.Role == "Admin")
                {
                    return Ok(eventRead);
                }
                _logger.LogError("User is not authenticated");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        //Just owner of event can update
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventForUpdate eventUpdate)
        {
            try
            {
                if (_currentUser == null)
                {
                    //Log error
                    _logger.LogError("User is not authenticated");
                    return Unauthorized(ErrorMessage.Unauthorized);
                }
                var eventInDb = await _eventService.GetEventById(id);

                //Check event exists
                if (eventInDb == null)
                {
                    return NotFound(ErrorMessage.EventNotFound);
                }

                if (eventInDb.UserId != _currentUser.Id)
                {
                    //Log error
                    _logger.LogError("User {0} is not owner of the event {1}", _currentUser.Username, id);
                    return Unauthorized(ErrorMessage.NotOwner);
                }
                await _eventService.UpdateEvent(id, eventUpdate);
                return Ok(SuccessMessage.UpdateEvent);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                if (_currentUser == null)
                {
                    //Log error
                    _logger.LogError("User is not authenticated");
                    return Unauthorized(ErrorMessage.Unauthorized);
                }
                var eventDelete = await _eventService.GetEventById(id);

                //Check event exists
                if (eventDelete == null)
                {
                    return NotFound(ErrorMessage.EventNotFound);
                }

                if (eventDelete.UserId != _currentUser.Id && _currentUser.Role == "Admin")
                {
                    //Log error
                    _logger.LogError("User {0} is not owner of the event {1}", _currentUser.Username, id);
                    return Unauthorized(ErrorMessage.Unauthorized);
                }
                await _eventService.DeleteEvent(id);
                return Ok(SuccessMessage.DeleteEvent);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
