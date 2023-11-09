using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RahmanoForOCS.Data;
using RahmanoForOCS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using RahmanoForOCS.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RahmanoForOCS.Controllers
{
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client = new HttpClient();
        public ProductController(IWebHostEnvironment webHostEnvironment, IConfiguration iConfig)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = iConfig;
            _client.BaseAddress = new Uri(_configuration.GetValue<string>("MySetting:baseAddress") + "product/");// baseAddress;
        }
        public IActionResult Index()
        {
            List<Product> obj = new List<Product>();
            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    obj = JsonConvert.DeserializeObject<List<Product>>(data);
                }
            }
            catch (Exception err)
            { ViewBag.comment = err.Message.ToString(); }

            return View(obj);
        }

        public IActionResult Upsert(int? id = 0)
        {
            ProductVM productVM = searchVM(id);
            if(productVM.Product.ImageUrl != null)
            {
                productVM.Product.ImageUrl = "https://localhost:44369/" + productVM.Product.ImageUrl;
            }
            if (productVM == null)
            {
                return NotFound();
            }else { 
                return View(productVM); 
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            string dirName = _configuration.GetValue<string>("MySetting:imageAddress");// imageAddress;
          
            string webRootPath = _webHostEnvironment.WebRootPath;
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string upload = webRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extension = "";
                string fullName = "";
                if (files.Count > 0)
                {
                    //Creating
                    extension = Path.GetExtension(files[0].FileName);
                    fullName = dirName + fileName + extension;
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = fileName + extension;
                }

                int panjang = productVM.Product.ImageUrl.IndexOf("/");
                while(panjang >= 0)
                {
                    productVM.Product.ImageUrl = productVM.Product.ImageUrl.Substring(panjang + 1, productVM.Product.ImageUrl.Length - panjang - 1);
                    panjang = productVM.Product.ImageUrl.IndexOf("/");
                }


                if (productVM.Product.ShortDesc == null) { productVM.Product.ShortDesc = ""; }
                if (productVM.Product.Description == null) { productVM.Product.Description = ""; }
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Put, "https://localhost:44369/api/product");
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(productVM.Product.Id.ToString()), "id");
                content.Add(new StringContent(productVM.Product.Name), "Name");
                content.Add(new StringContent(productVM.Product.ShortDesc), "ShortDesc");
                content.Add(new StringContent(productVM.Product.Description), "Description");
                content.Add(new StringContent(productVM.Product.Price.ToString()), "Price");
                if (fullName != "") { content.Add(new StreamContent(System.IO.File.OpenRead(fullName)), "Image", fileName + extension); }
                content.Add(new StringContent(productVM.Product.CategoryId.ToString()), "CategoryId");
                content.Add(new StringContent(productVM.Product.ApplicationTypeId.ToString()), "ApplicationTypeId");
                content.Add(new StringContent(productVM.Product.ImageUrl), "ImageUrl");
                request.Content = content;

                var response = client.SendAsync(request);
                response.Wait();

                var result = response.Result;
                if (result.IsSuccessStatusCode)
                {
                    ViewBag.comment = "Ok";
                    
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View(productVM);
            }
        }
        public IActionResult Delete(int? id)
        {
            Product obj = searchId(id);
            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            using (_client)
            {
                var putTask = _client.DeleteAsync(_client.BaseAddress + id.ToString());
                putTask.Wait();

                var result = putTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
        private Product searchId(int? id)
        {
            Product obj = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var putTask = _client.GetAsync(_client.BaseAddress + id.ToString());
                    putTask.Wait();

                    var result = putTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<Product>();
                        readTask.Wait();
                        obj = readTask.Result;
                        if (obj == null) { ViewBag.comment = result.ReasonPhrase; }
                        return obj;
                    }else { ViewBag.comment = result.ReasonPhrase; }
                }
                catch (Exception err) { ViewBag.comment = err.Message; }
            }
            return obj;
        }

        private ProductVM searchVM(int? id)
        {
            ProductVM obj = null;
            using (var client = new HttpClient())
            {
                try
                {
                    var putTask = _client.GetAsync(_client.BaseAddress + "getVM/" + id.ToString());
                    putTask.Wait();
                    var result = putTask.Result;

                    if (result.StatusCode.ToString() != "OK")
                    {
                        return null;
                    }
                    else
                    {
                        var readTask = result.Content.ReadAsAsync<ProductVM>();
                        readTask.Wait();
                        obj = readTask.Result;
                        ViewBag.comment = result.ReasonPhrase;
                        return obj;
                    }
                }
                catch (Exception err) {
                    ViewBag.comment = err.Message;
                    Response.StatusCode = 404;
                }
            }
            return obj;
        }
    }
}
