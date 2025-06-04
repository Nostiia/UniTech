using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTech.Models;
using Microsoft.EntityFrameworkCore;
using UniTech.ViewsModels;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UniTech.Controllers
{
    public class PostsController : Controller
    {
        private PostContext postContext;
        private UserContext userContext;
        public PostsController(PostContext postContext, UserContext userContext)
        {
            this.postContext = postContext;
            this.userContext = userContext;
        }

        public async Task<IActionResult> Index(string filter)
        {
            var filtered = await postContext.Posts
                .Include(p => p.Teacher)
                .ToListAsync();

            if (!string.IsNullOrEmpty(filter))
            {
                filtered = filtered
                    .Where(p => p.Topic.Contains(filter, StringComparison.OrdinalIgnoreCase)
                             || p.Search_hucks.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(filtered); // make sure your View expects List<Post>
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            if (ModelState.IsValid)
            {
                Post post = await postContext.Posts.FirstOrDefaultAsync(u => u.Topic == model.Topic);
                if (post == null)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                        return RedirectToAction("Login", "Account");

                    int userId = int.Parse(userIdClaim.Value);
                    post = new Post
                    {
                        Topic = model.Topic,
                        Search_hucks = model.Search_hucks,
                        Post_text = model.Post_text,
                        TeacherId = userId
                    };

                    postContext.Posts.Add(post);
                    await postContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await postContext.Posts.FirstOrDefaultAsync(u => u.Post_Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post)
        {
            if (id != post.Post_Id)
            {
                return NotFound();
            }

            try
            {
                var existingPost = await postContext.Posts.FindAsync(id);
                if (existingPost == null)
                    return NotFound();

                // Update only fields you allow to change
                existingPost.Topic = post.Topic;
                existingPost.Search_hucks = post.Search_hucks;
                existingPost.Post_text = post.Post_text;

                await postContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(post.Post_Id))
                    return NotFound();
                throw;
            }

        }

        private bool PostExists(int id)
        {
            return postContext.Posts.Any(e => e.Post_Id == id);
        }
    }
}
