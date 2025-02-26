﻿using System.Text.Json.Serialization;
using Api.Utilities;

namespace Api.Controllers.Models
{
    /// <summary>
    /// The input ISAR expects as a mission description in the /schedule/start-mission endpoint
    /// </summary>
    public class IsarMissionDefinition
    {
        [JsonPropertyName("tasks")]
        public List<IsarTaskDefinition> Tasks { get; set; }

        public IsarMissionDefinition(List<IsarTaskDefinition> tasks)
        {
            Tasks = tasks;
        }

        public IsarMissionDefinition(EchoMission echoMission)
        {
            Tasks = echoMission.Tags.Select(tag => new IsarTaskDefinition(tag)).ToList();
        }
    }

    public class IsarTaskDefinition
    {
        public class IsarOrientation
        {
            [JsonPropertyName("x")]
            public float X { get; set; }

            [JsonPropertyName("y")]
            public float Y { get; set; }

            [JsonPropertyName("z")]
            public float Z { get; set; }

            [JsonPropertyName("w")]
            public float W { get; set; }

            [JsonPropertyName("frame_name")]
            public string FrameName { get; set; }

            public IsarOrientation(float x, float y, float z, float w, string frameName)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
                FrameName = frameName;
            }
        }

        public class IsarPosition
        {
            [JsonPropertyName("x")]
            public float X { get; set; }

            [JsonPropertyName("y")]
            public float Y { get; set; }

            [JsonPropertyName("z")]
            public float Z { get; set; }

            [JsonPropertyName("frame_name")]
            public string FrameName { get; set; }

            public IsarPosition(float x, float y, float z, string frameName)
            {
                X = x;
                Y = y;
                Z = z;
                FrameName = frameName;
            }
        }

        public class IsarPose
        {
            [JsonPropertyName("position")]
            public IsarPosition Position { get; set; }

            [JsonPropertyName("orientation")]
            public IsarOrientation Orientation { get; set; }

            [JsonPropertyName("frame_name")]
            public string FrameName { get; set; }

            public IsarPose(IsarPosition position, IsarOrientation orientation, string frameName)
            {
                Position = position;
                Orientation = orientation;
                FrameName = frameName;
            }
        }

        [JsonPropertyName("pose")]
        public IsarPose Pose { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("inspection_target")]
        public IsarPosition InspectionTarget { get; set; }

        [JsonPropertyName("inspection_types")]
        public List<string> InspectionTypes { get; set; }

        [JsonPropertyName("video_duration")]
        public float? VideoDuration { get; set; }

        public IsarTaskDefinition(
            IsarPose pose,
            string tag,
            IsarPosition inspectionTarget,
            List<string> sensorTypes,
            float? videoDuration
        )
        {
            Pose = pose;
            Tag = tag;
            InspectionTarget = inspectionTarget;
            InspectionTypes = sensorTypes;
            VideoDuration = videoDuration;
        }

        public IsarTaskDefinition(EchoTag echoTag)
        {
            Tag = echoTag.TagId;
            InspectionTypes = echoTag.Inspections.Select(t => t.InspectionType.ToString()).ToList();
            Pose = TagPositioner.GetPoseFromTag(echoTag);
            InspectionTarget = TagPositioner.GetTagPositionFromTag(echoTag);
            VideoDuration = echoTag.Inspections
                .Where(t => t.TimeInSeconds.HasValue)
                .FirstOrDefault()
                ?.TimeInSeconds;
        }
    }
}
