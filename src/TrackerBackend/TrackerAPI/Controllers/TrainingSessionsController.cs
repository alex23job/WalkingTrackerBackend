using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackerAPI.Data;
using TrackerAPI.Data.Entities;
using TrackerAPI.DTOs;

namespace TrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingSessionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainingSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainingSessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingSessionDto>>> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _context.TrainingSessions
                .Where(ts => ts.UserId.ToString() == userId)
                .OrderByDescending(ts => ts.StartTime)
                .Select(ts => new TrainingSessionDto
                {
                    Id = ts.Id,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    DistanceMeters = ts.DistanceMeters,
                    RouteGeometryWkt = ts.RouteGeometryWkt
                })
                .ToListAsync();

            return Ok(sessions);
        }

        // POST: api/TrainingSessions
        [HttpPost]
        public async Task<ActionResult<TrainingSessionDto>> Create([FromBody] TrainingSessionDto sessionDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var session = new TrainingSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartTime = sessionDto.StartTime,
                EndTime = sessionDto.EndTime,
                DistanceMeters = sessionDto.DistanceMeters,
                RouteGeometryWkt = sessionDto.RouteGeometryWkt
            };

            _context.TrainingSessions.Add(session);
            await _context.SaveChangesAsync();

            // Возвращаем созданную запись со сгенерированным Id
            sessionDto.Id = session.Id;
            return CreatedAtAction(nameof(GetAll), new { id = session.Id }, sessionDto);
        }
    }
}
