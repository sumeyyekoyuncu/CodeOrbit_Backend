using CodeOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeOrbit.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(int userId, [FromQuery] bool unreadOnly = false)
        {
            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            return Ok(result);
        }

        [HttpGet("{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);
            if (!result)
                return NotFound();
            return Ok("Bildirim okundu olarak işaretlendi.");
        }

        [HttpPut("{userId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok("Tüm bildirimler okundu olarak işaretlendi.");
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var result = await _notificationService.DeleteNotificationAsync(notificationId);
            if (!result)
                return NotFound();
            return Ok("Bildirim silindi.");
        }

        [HttpPost("check-streak-warnings")]
        public async Task<IActionResult> CheckStreakWarnings()
        {
            await _notificationService.CheckAndSendStreakWarningsAsync();
            return Ok("Streak uyarıları kontrol edildi.");
        }
    }
}