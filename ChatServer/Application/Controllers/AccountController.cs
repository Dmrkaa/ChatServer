using AutoMapper;
using ChatServer.SharedModels;
using ChatServer.Application.Services;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Repository_Interfaces;
using ChatServer.Infrstructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatServer.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<ChatUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IMapper _mapper;
        private readonly IRoomRepository _roomRepository;
        private readonly IAuthManager _authManager;
        public AccountController(IRoomRepository roomRepository,
                                ILogger<AccountController> logger,
                                IMapper mapper,
                                UserManager<ChatUser> usrManager,
                                IAuthManager authManager)
        {
            _roomRepository = roomRepository;
            _logger = logger;
            _mapper = mapper;
            _userManager = usrManager;
            _authManager = authManager;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] LoginModel login)
        {
            _logger.LogInformation($"Try to register for {login.UserName} to room: {login.RoomName}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var room = await _roomRepository.GetRoom(login.RoomName);
                if (room == null)
                {
                    room = await _roomRepository.CreateRoom(login.RoomName);
                }
                var user = new ChatUser() { UserName = login.UserName, RoomId = room.Id };
                user.Room = room;
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError(err.Code, err.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            return Accepted();
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            _logger.LogInformation($"Try to login for {login.UserName} to room: {login.RoomName}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (!await _authManager.ValidateUser(login))
                {
                    return Unauthorized();
                }
                var userRoom = await _roomRepository.GetRoomByUserName(login.UserName);
                if (userRoom.Name != login.RoomName)
                {
                    return Unauthorized();
                }
                return Accepted(new { Token = _authManager.CreateToken() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something is wrong in the {nameof(Login)}");
                return Problem($"Something is wrong in the {nameof(Login)}", statusCode: 500);
            }

        }

    }
}
