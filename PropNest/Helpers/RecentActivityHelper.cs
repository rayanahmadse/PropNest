using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PropNest.Helpers
{
    public class ActivityLogEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    public static class RecentActivityHelper
    {
        private static string GetCookieName(HttpContext context)
        {
            var username = context.Session.GetString("Username") ?? "anonymous";
            // Sanitize username for safe cookie name characters
            var safeUsername = string.Concat(username.Where(c => char.IsLetterOrDigit(c)));
            return $"propnest_recent_activities_{safeUsername}";
        }

        public static List<ActivityLogEntry> GetActivities(HttpContext context)
        {
            var cookieName = GetCookieName(context);
            var cookie = context.Request.Cookies[cookieName];
            if (string.IsNullOrEmpty(cookie))
            {
                return new List<ActivityLogEntry>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<ActivityLogEntry>>(cookie) ?? new List<ActivityLogEntry>();
            }
            catch
            {
                return new List<ActivityLogEntry>();
            }
        }

        public static void LogActivity(HttpContext context, string title, string description, string actionUrl)
        {
            var activities = GetActivities(context);

            // Remove duplicates to keep it clean
            activities.RemoveAll(a => a.ActionUrl.Equals(actionUrl, StringComparison.OrdinalIgnoreCase));

            // Add new activity at the top
            activities.Insert(0, new ActivityLogEntry
            {
                Title = title,
                Description = description,
                ActionUrl = actionUrl,
                Timestamp = DateTime.Now.ToString("MMM dd, h:mm tt")
            });

            // Keep only the last 5
            activities = activities.Take(5).ToList();

            var json = JsonSerializer.Serialize(activities);
            var cookieName = GetCookieName(context);
            context.Response.Cookies.Append(cookieName, json, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false, // Set false so JavaScript/Views can read it if needed
                SameSite = SameSiteMode.Lax
            });
        }
    }
}
