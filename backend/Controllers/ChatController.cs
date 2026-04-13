using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _service;

        public ChatController(IChatService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var rawUserId = 
                User.FindFirstValue(ClaimTypes.NameIdentifier)??
                User.FindFirstValue("sub");
            if (!int.TryParse(rawUserId, out int userId))
                throw new InvalidOperationException("Invalid user ID");

            return userId;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                    return BadRequest("Message content is required");

                int userId = GetCurrentUserId();

                await _service.SendMessageAsync(userId, dto);

                return Ok(new { message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var messages = await _service.GetMessagesAsync(userId, otherUserId);

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                int userId = GetCurrentUserId();

                var convos = await _service.GetConversationsAsync(userId);

                return Ok(convos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}