using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig) // Injectam serviciul CloudinarySettings
        {
            this._context = context;
            this._userManager = userManager;
            this._cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(_cloudinaryConfig.Value.CloudName, _cloudinaryConfig.Value.ApiKey, _cloudinaryConfig.Value.ApiSecret); // Ceem un cont nou in interiorul constructorului pentru a avea acces la metodele Cloudinary deoarece atunci cand un moderator/admin da reject la o fotografie va trebui s-o si stergem din Cloudinary

            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await _context.Users
                .OrderBy(x => x.UserName)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles
                             join role in _context.Roles
                             on userRole.RoleId
                             equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();

            return Ok(userList);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDto.RoleNames;

            // selectedRoles =  selectedRoles != null ? selectedRoles : new string[] {}; ( null coalescing operator )
            selectedRoles = selectedRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove the roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration() // Adaugam o noua metoda in controller-ul pentru admini ce va returna toate fotografiile ce trebuie moderate
        {
            var photos = await _context.Photos
                            .Include(u => u.User) // Va include informatiile despre user deoarece dorim da afisam username-ul clientului
                            .IgnoreQueryFilters() // Trebuie sa ingoram query filter-ul deoare dorim sa obtinem fotografiile care nu sunt approved
                            .Where(p => p.IsApproved == false) // Luam doar fotofrafiile ce au IsApproved == false
                            .Select(u => new // Apoi folosim Select pentru a proiecat datele in obiecte noi pe care le putem returna mai apoi
                            {
                                Id = u.Id,
                                UserName = u.User.UserName,
                                Url = u.Url,
                                IsApproved = u.IsApproved
                            }).ToListAsync();

            return Ok(photos);
        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _context.Photos // Obtinem fotografia pe care dorim sa o aprobam
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(p => p.Id == photoId);

            photo.IsApproved = true; // Ii setam IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _context.Photos // Obtinem fotografia pe care dorim sa o aprobam
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo.IsMain)
                return BadRequest("You cannot reject the main photo");

            if (photo.PublicId != null) // Verificam daca fotografia are un PublicId pentru a sti daca este provenita din Cloudinary
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                    _context.Photos.Remove(photo);
            }

            if (photo.PublicId == null)
                _context.Photos.Remove(photo);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}