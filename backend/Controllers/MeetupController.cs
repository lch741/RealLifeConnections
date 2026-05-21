using backend.DTO.Meetup;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/meetups")]
    public class MeetupController : ControllerBase
    {
        private readonly IMeetupService _meetupService;

        public MeetupController(IMeetupService meetupService)
        {
            _meetupService = meetupService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMeetupDto dto)
        {
            try
            {
                var result = await _meetupService.CreateMeetupAsync(User, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{meetupId:int}")]
        public async Task<IActionResult> GetById(int meetupId)
        {
            try
            {
                var result = await _meetupService.GetMeetupAsync(meetupId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("created")]
        public async Task<IActionResult> GetCreatedByMe()
        {
            try
            {
                var result = await _meetupService.GetUserCreatedMeetupsAsync(User);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("joined")]
        public async Task<IActionResult> GetJoinedByMe()
        {
            try
            {
                var result = await _meetupService.GetUserJoinedMeetupsAsync(User);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("matched")]
        public async Task<IActionResult> GetMatchedMeetups(
            [FromQuery] string activityType,
            [FromQuery] string suburb,
            [FromQuery] int limit = 20)
        {
            try
            {
                var result = await _meetupService.GetMatchedMeetupsAsync(User, activityType, suburb, limit);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{meetupId:int}")]
        public async Task<IActionResult> Update(int meetupId, [FromBody] UpdateMeetupDto dto)
        {
            try
            {
                var result = await _meetupService.UpdateMeetupAsync(User, meetupId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{meetupId:int}")]
        public async Task<IActionResult> Delete(int meetupId)
        {
            try
            {
                await _meetupService.DeleteMeetupAsync(User, meetupId);
                return Ok(new { message = "Meetup deleted." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{meetupId:int}/status")]
        public async Task<IActionResult> UpdateStatus(int meetupId, [FromQuery] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest("Status is required.");
                }

                var result = await _meetupService.UpdateMeetupStatusAsync(User, meetupId, status);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{meetupId:int}/apply")]
        public async Task<IActionResult> Apply(int meetupId)
        {
            try
            {
                var result = await _meetupService.JoinMeetupAsync(User, meetupId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{meetupId:int}/quit")]
        public async Task<IActionResult> Quit(int meetupId)
        {
            try
            {
                await _meetupService.LeaveMeetupAsync(User, meetupId);
                return Ok(new { message = "Meetup left." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{meetupId:int}/approve/{participantId:int}")]
        public async Task<IActionResult> ApproveParticipant(int meetupId, int participantId)
        {
            try
            {
                var result = await _meetupService.ConfirmParticipantAsync(User, meetupId, participantId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
