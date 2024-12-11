using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaganementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(INotificationService notificationService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

       
        public async Task<IActionResult> Index()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> SendGeneralNotification(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("", "Message content is required.");
                return View("Index");
            }

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await _notificationService.NotifyGeneralAnnouncementAsync(user.Id, message);
            }

            TempData["Message"] = "General notification sent successfully!";
            return RedirectToAction("Index");
        }

       
        [HttpPost]
        public async Task<IActionResult> SendSystemAlert(string alertMessage)
        {
            if (string.IsNullOrEmpty(alertMessage))
            {
                ModelState.AddModelError("", "Alert content is required.");
                return View("Index");
            }

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await _notificationService.NotifySystemAlertAsync(user.Id, alertMessage);
            }

            TempData["Message"] = "System alert sent successfully!";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ViewUsers()
        {
            
            var userList = await _userManager.Users.ToListAsync();

            
            var users = new List<UserViewModel>();
            foreach (var user in userList)
            {
               
                var roles = await _userManager.GetRolesAsync(user);

               
                users.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(users);
        }

       
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }


        
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("ViewUsers");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User updated successfully!";
                return RedirectToAction("ViewUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var viewModel = new AssignRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AvailableRoles = roles
            };

            return View(viewModel);
        }

       
        [HttpPost]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

           
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                var result = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                if (result.Succeeded)
                {
                    TempData["Message"] = "Role assigned successfully!";
                    return RedirectToAction("ViewUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
