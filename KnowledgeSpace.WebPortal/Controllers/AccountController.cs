using KnowledgeSpace.ViewModels.Contents;
using KnowledgeSpace.WebPortal.Extensions;
using KnowledgeSpace.WebPortal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeSpace.WebPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IKnowledgeBaseApiClient _knowledgeBaseApiClient;
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly ILabelApiClient _labelApiClient;
        public AccountController(IUserApiClient userApiClient,
            IKnowledgeBaseApiClient knowledgeBaseApiClient,
             ILabelApiClient labelApiClient,
            ICategoryApiClient categoryApiClient)
        {
            _userApiClient = userApiClient;
            _categoryApiClient = categoryApiClient;
            _labelApiClient = labelApiClient;
            _knowledgeBaseApiClient = knowledgeBaseApiClient;
        }
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "oidc");
        }

        public IActionResult SignOut()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, "Cookies", "oidc");
        }

        [Authorize]
        public async Task<ActionResult> MyProfile()
        {
            var user = await _userApiClient.GetById(User.GetUserId());
            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyKnowledgeBases(int page = 1, int pageSize = 10)
        {
            var kbs = await _userApiClient.GetKnowledgeBasesByUserId(User.GetUserId(), page, pageSize);
            return View(kbs);
        }

        [HttpGet]
        public async Task<IActionResult> CreateNewKnowledgeBase()
        {
            await SetCategoriesViewBag();
            await SetLabelsViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewKnowledgeBase([FromForm] KnowledgeBaseCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _knowledgeBaseApiClient.PostKnowlegdeBase(request);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> EditKnowledgeBase(int id)
        {
            var knowledgeBase = await _knowledgeBaseApiClient.GetKnowledgeBaseDetail(id);
            await SetCategoriesViewBag();
            await SetLabelsViewBag();
            return View(new KnowledgeBaseCreateRequest()
            {
                CategoryId = knowledgeBase.CategoryId,
                Description = knowledgeBase.Description,
                Environment = knowledgeBase.Environment,
                ErrorMessage = knowledgeBase.ErrorMessage,
                Labels = knowledgeBase.Labels,
                Note = knowledgeBase.Note,
                Problem = knowledgeBase.Problem,
                StepToReproduce = knowledgeBase.StepToReproduce,
                Title = knowledgeBase.Title,
                Workaround = knowledgeBase.Workaround,
                Id = knowledgeBase.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditKnowledgeBase([FromForm] KnowledgeBaseCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _knowledgeBaseApiClient.PutKnowlegdeBase(request.Id.Value, request);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        private async Task SetCategoriesViewBag(int? selectedValue = null)
        {
            var categories = await _categoryApiClient.GetCategories();

            var items = categories.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString(),
            }).ToList();

            items.Insert(0, new SelectListItem()
            {
                Value = null,
                Text = "--Chọn danh mục--"
            });
            ViewBag.Categories = new SelectList(items, "Value", "Text", selectedValue);
        }

        private async Task SetLabelsViewBag(int? selectedValue = null)
        {
            var labels = await _labelApiClient.GetLabels();

            var items = labels.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString(),
            }).ToList();

            ViewBag.labels = new SelectList(items, "Value", "Text", selectedValue);
        }
    }
}
