using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RahmanoForOCS.Data;
using RahmanoForOCS.Models;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RahmanoForOCS.Controllers
{
    public class ApplicationTypeController : Controller
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly IConfiguration _configuration;

        public ApplicationTypeController(ApplicationDbContext db, IConfiguration iConfig)
        {
            _configuration = iConfig;
            _client.BaseAddress = new Uri(_configuration.GetValue<string>("MySetting:baseAddress") + "applicationtype/");// baseAddress;
        }
        public IActionResult Index()
        {
            List<ApplicationType> obj = new List<ApplicationType>();
            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    obj = JsonConvert.DeserializeObject<List<ApplicationType>>(data);
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


        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                using (_client)
                {
                    var posTask = _client.PostAsJsonAsync<ApplicationType>(_client.BaseAddress, obj);
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

        //GET Edit
        public IActionResult Edit(int? id)
        {
            ApplicationType obj = searchId(id);
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType obj)
        {
            if (ModelState.IsValid)
            {
                using (_client)
                {
                    var putTask = _client.PutAsJsonAsync<ApplicationType>(_client.BaseAddress, obj);
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
            ApplicationType obj = searchId(id);
            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? TypeID)
        {
            using(_client)
            {
                var putTask = _client.DeleteAsync(_client.BaseAddress + TypeID.ToString());
                putTask.Wait();

                var result = putTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        private ApplicationType searchId(int? id)
        {
            ApplicationType obj = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var putTask = _client.GetAsync(_client.BaseAddress + id.ToString());
                    putTask.Wait();

                    var result = putTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<ApplicationType>();
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
