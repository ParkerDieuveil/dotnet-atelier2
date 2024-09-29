using atelier2.Models;
using atelier2.Models.Repositories;
using atelier2.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace atelier2.Controllers
{
    public class ProductController : Controller
    {
        //Injection de dependance
        readonly IRepository<Product> productRepo;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IRepository<Product> ProdRepository, IWebHostEnvironment hostingEnvironment)
        {
            productRepo = ProdRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ProductController
        public ActionResult Index()
        {
            var products = productRepo.GetAll();
            return View(products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            var product = productRepo.Get(id);
            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                // If the Photo property on the incoming model object is not null, then the user has selected an image to upload.
                if (model.ImagePath != null)
                {
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    model.ImagePath.CopyTo(new FileStream(filePath, FileMode.Create));
                }


                Product newProduct = new Product

                {
                    Désignation = model.Désignation,
                    Prix = model.Prix,
                    Quantite = model.Quantite,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    Image = uniqueFileName
                };
                productRepo.Add(newProduct);
                return RedirectToAction("details", new { id = newProduct.Id });
            }
            return View();
        }



        // GET: ProductController/Edit/5

        public ActionResult Edit(int id)
        {
            Product product = productRepo.Get(id);
            EditViewModel productEditViewModel = new EditViewModel
            {
                Id = product.Id,
                Désignation = product.Désignation,
                Prix = product.Prix,
                Quantite = product.Quantite,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }



        // POST: ProductController/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            // Check if the provided data is valid, if not rerender the edit view
            // so the user can correct and resubmit the edit form
            if (ModelState.IsValid)
            {
                // Retrieve the product being edited from the database
                Product product = productRepo.Get(model.Id);
                // Update the product object with the data in the model object
                product.Désignation = model.Désignation;
                product.Prix = model.Prix;
                product.Quantite = model.Quantite;
                // If the user wants to change the photo, a new photo will be
                // uploaded and the Photo property on the model object receives
                // the uploaded photo. If the Photo property is null, user did
                // not upload a new photo and keeps his existing photo
                if (model.ImagePath != null)
                {
                    // If a new photo is uploaded, the existing photo must be
                    // deleted. So check if there is an existing photo and delete
                    if (model.ExistingImagePath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        System.IO.File.Delete(filePath);
                    }
                    // Save the new photo in wwwroot/images folder and update
                    // PhotoPath property of the product object which will be
                    // eventually saved in the database
                    product.Image = ProcessUploadedFile(model);
                }
                // Call update method on the repository service passing it the

                // product object to update the data in the database table
                Product updatedProduct = productRepo.Update(product);
                if (updatedProduct != null)
                    return RedirectToAction("Index");
                else
                    return NotFound();

            }
            return View(model);
        }
        [NonAction]
        private string ProcessUploadedFile(EditViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            var product = productRepo.Get(id);
            // Si le produit n'existe pas, retourne une erreur 404
            if (product == null)
            {
                return NotFound();
            }

            // // Passe le produit à la vue pour confirmation de la suppression
            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Product product)
        {
            try
            {
                // Supprime le produit
                productRepo.Delete(product.Id);

                // Redirige vers l'index après la suppression
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // En cas d'exception, retourne à la vue avec le produit
                return View();
            }
        }

        public ActionResult Search(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return View("Index", productRepo.GetAll()); // Si la recherche est vide, retourne tous les produits
            }

            var result = productRepo.GetAll()
                                    .Where(p => p.Désignation.Contains(term, StringComparison.OrdinalIgnoreCase));
            return View("Index", result);
        }
    }

}
