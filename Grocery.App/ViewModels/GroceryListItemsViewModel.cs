using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {

            AvailableProducts.Clear();
            //GetAll haalt alle producten op
            var allproducts = _productService.GetAll();

            foreach (var product in allproducts)
            { 
                //als er geen stock is slaat hij het over en gaat verder
                if(product.Stock <= 0) continue;

                //bool check of het product al op het boodschappenlijstje staat
                bool alreadyInlist = MyGroceryListItems.Any(item => item.ProductId == product.Id);

                if (alreadyInlist)
                {
                    AvailableProducts.Add(product);
                }
            }         
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null || product.Id <= 0)
            {
                return;
            }

            //nieuw GroceryListItem aanmaken
            var newItem = new GroceryListItem(0, GroceryList.Id, product.Id, 1);

            //opgeslagen via service
            _groceryListItemsService.Add(newItem);

            //vooraad updaten
            product.Stock--;
            _productService.Update(product);

            //lijst bijwerken.
            OnGroceryListChanged(GroceryList);
        }
    }
}
