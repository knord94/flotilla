﻿using Api.Controllers.Models;
using Api.Database.Context;
using Api.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public interface IRobotService
    {
        public abstract Task<Robot> Create(CreateRobotQuery newRobot);
        public abstract Task<IEnumerable<Robot>> ReadAll();
        public abstract Task<Robot?> ReadById(string id);
        public abstract Task<Robot?> ReadByName(string name);
        public abstract Task<Robot> Update(Robot robot);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Globalization",
        "CA1309:Use ordinal StringComparison",
        Justification = "EF Core refrains from translating string comparison overloads to SQL"
    )]
    public class RobotService : IRobotService
    {
        private readonly FlotillaDbContext _context;

        public RobotService(FlotillaDbContext context)
        {
            _context = context;
        }

        private IQueryable<Robot> GetRobotsWithSubModels()
        {
            return _context.Robots.Include(r => r.VideoStreams);
        }

        public async Task<Robot> Create(CreateRobotQuery newRobot)
        {
            var videoStreams = new List<VideoStream>();
            foreach (var videoStreamQuery in newRobot.VideoStreams)
            {
                var videoStream = new VideoStream
                {
                    Name = videoStreamQuery.Name,
                    Url = videoStreamQuery.Url
                };
                videoStreams.Add(videoStream);
            }

            var robot = new Robot
            {
                Name = newRobot.Name,
                Model = newRobot.Model,
                SerialNumber = newRobot.SerialNumber,
                VideoStreams = videoStreams,
                Host = newRobot.Host,
                Port = newRobot.Port,
                Enabled = newRobot.Enabled,
                Status = newRobot.Status
            };

            await _context.Robots.AddAsync(robot);
            await _context.SaveChangesAsync();
            return robot;
        }

        public async Task<IEnumerable<Robot>> ReadAll()
        {
            return await GetRobotsWithSubModels().ToListAsync();
        }

        public async Task<Robot?> ReadById(string id)
        {
            return await GetRobotsWithSubModels().FirstOrDefaultAsync(robot => robot.Id.Equals(id));
        }

        public async Task<Robot?> ReadByName(string name)
        {
            return await GetRobotsWithSubModels()
                .FirstOrDefaultAsync(robot => robot.Name.Equals(name));
        }

        public async Task<Robot> Update(Robot robot)
        {
            var entry = _context.Update(robot);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
