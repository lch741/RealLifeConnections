using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
[ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _service;

        public ChatController(IChatService service)
        {
            _service = service;
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                    return BadRequest("Message content is required");

                int userId = 1; // TODO: 从JWT获取

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
                int userId = 1;

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
                int userId = 1;

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