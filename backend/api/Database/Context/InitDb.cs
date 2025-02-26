﻿using Api.Database.Models;

namespace Api.Database.Context;

public static class InitDb
{
    public static readonly List<Robot> Robots = GetRobots();
    public static readonly List<Mission> Missions = GetMissions();

    private static VideoStream VideoStream =>
        new()
        {
            Name = "Front camera",
            Url = "http://localhost:5000/stream?topic=/camera/rgb/image_raw"
        };

    private static PlannedInspection PlannedInspection =>
        new() { InspectionType = IsarStep.InspectionTypeEnum.Image, TimeInSeconds = 0.5f };

    private static PlannedTask PlannedTask =>
        new()
        {
            Inspections = new List<PlannedInspection>(),
            TagId = "Tagid here",
            URL = new Uri("https://www.I-am-echo-stid-tag-url.com")
        };

    private static List<Robot> GetRobots()
    {
        var robot1 = new Robot
        {
            Name = "R2-D2",
            Model = "R2",
            SerialNumber = "D2",
            Status = RobotStatus.Available,
            Enabled = true,
            Host = "localhost",
            Logs = "",
            Port = 3000,
            VideoStreams = new List<VideoStream>(),
            Pose = new Pose()
        };

        var robot2 = new Robot
        {
            Name = "Shockwave",
            Model = "Decepticon",
            SerialNumber = "SS79",
            Status = RobotStatus.Busy,
            Enabled = true,
            Host = "localhost",
            Logs = "logs",
            Port = 3000,
            VideoStreams = new List<VideoStream>(),
            Pose = new Pose()
        };

        var robot3 = new Robot
        {
            Name = "Ultron",
            Model = "AISATW",
            SerialNumber = "Earth616",
            Status = RobotStatus.Available,
            Enabled = false,
            Host = "localhost",
            Logs = "logs",
            Port = 3000,
            VideoStreams = new List<VideoStream>(),
            Pose = new Pose()
        };

        return new List<Robot>(new Robot[] { robot1, robot2, robot3 });
    }

    private static List<Mission> GetMissions()
    {
        var mission1 = new Mission
        {
            Robot = Robots[0],
            AssetCode = "test",
            EchoMissionId = 1,
            IsarMissionId = "1",
            MissionStatus = MissionStatus.Pending,
            StartTime = DateTimeOffset.UtcNow,
            PlannedTasks = new List<PlannedTask>(),
        };

        var mission2 = new Mission
        {
            Robot = Robots[1],
            AssetCode = "test",
            EchoMissionId = 1,
            IsarMissionId = "1",
            MissionStatus = MissionStatus.Pending,
            StartTime = DateTimeOffset.UtcNow.AddHours(7),
            EndTime = DateTimeOffset.UtcNow.AddHours(9),
            PlannedTasks = new List<PlannedTask>(),
        };

        var mission3 = new Mission
        {
            Robot = Robots[2],
            AssetCode = "test",
            EchoMissionId = 1,
            IsarMissionId = "1",
            MissionStatus = MissionStatus.Pending,
            StartTime = DateTimeOffset.UtcNow.AddHours(8),
            EndTime = DateTimeOffset.UtcNow.AddHours(9),
            PlannedTasks = new List<PlannedTask>(),
        };

        return new List<Mission>(new Mission[] { mission1, mission2, mission3 });
    }

    public static void PopulateDb(FlotillaDbContext context)
    {
        foreach (var robot in Robots)
        {
            robot.VideoStreams.Add(VideoStream);
        }

        foreach (var mission in Missions)
        {
            var plannedTask = PlannedTask;
            plannedTask.Inspections.Add(PlannedInspection);
            mission.PlannedTasks.Add(plannedTask);
        }
        context.AddRange(Robots);
        context.AddRange(Missions);
        context.SaveChanges();
    }
}
