using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RahmanoForOCS.Models;
using RahmanoForOCS.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace RahmanoForOCS.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client = new HttpClient();
        public CategoryController(IConfiguration iConfig)
        {
            _configuration = iConfig;
            _client.BaseAddress = new Uri(_configuration.GetValue<string>("MySetting:baseAddress") + "category/");// baseAddress;
        }
        public IActionResult Index()
        {
            List<Category> obj = new List<Category>();
            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    obj = JsonConvert.DeserializeObject<List<Category>>(data);
                }
            }
            catch (Exception err)
            { ViewBag.comment = err.Message.ToString(); }

            return View(obj);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                using (_client)
                {
                    var posTask = _client.PostAsJsonAsync<Category>(_client.BaseAddress, obj);
                    posTask.Wait();

                    var result = posTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(obj);
        }
        public IActionResult Edit(int? id)
        {
            Category obj = searchId(id);
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                using (_client)
                {
                    var putTask = _client.PutAsJsonAsync<Category>(_client.BaseAddress, obj);
                    putTask.Wait();

                    var result = putTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            Category obj = searchId(id);
            return View(obj);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int? CategoryId)
        {
            using (_client)
            {
                var putTask = _client.DeleteAsync(_client.BaseAddress + CategoryId.ToString());
                putTask.Wait();

                var result = putTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        private Category searchId(int? id)
        {
            Category obj = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var putTask = _client.GetAsync(_client.BaseAddress + id.ToString());
                    putTask.Wait();

                    var result = putTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<Category>();
                        readTask.Wait();
                        obj = readTask.Result;
                        if (obj == null) { ViewBag.comment = result.ReasonPhrase; }
                        return obj;
                    }
                }
                catch (Exception err) { ViewBag.comment = err.Message; }
            }
            return obj;
        }
    }
}
