using System;
using System.Linq;
using System.Net;
using Checkout;
using Checkout.ApiServices.Shopping.RequestModels;
using FluentAssertions;
using NUnit.Framework;
using Environment = Checkout.Helpers.Environment;

namespace Tests.ShoppingListService
{
    [TestFixture(Category = "ShoppingListApi")]
    public class ShoppingListServiceTests
    {
        protected APIClient LocalServiceClient;

        [SetUp]
        public void Init()
        {
            LocalServiceClient = new APIClient(Environment.Local);
        }

        [TestCase("Coke", 100)]
        [TestCase("Pepsi", 100)]
        [TestCase("Lemonade", 100)]
        [TestCase("Juice", 100)]
        [TestCase("Beer", 100)]
        [Test]
        public void AddDrink(string name, int quantity)
        {
            var newDrink = CreateDrink(name, quantity);
            var response = LocalServiceClient.ShoppingListService.AddDrink(newDrink);
            response.Should().NotBeNull();
            response.HttpStatusCode.Should().Be(HttpStatusCode.Created);
            response.Model.Created.Should().BeAfter(DateTime.MinValue);
            response.Model.Name.ShouldBeEquivalentTo(name);
            response.Model.Quantity.ShouldBeEquivalentTo(quantity);

            //Data Cleanup
            LocalServiceClient.ShoppingListService.DeleteDrink(name);
        }

        [Test]
        public void GetDrinkList()
        {

            //Init 
            var drink1 = LocalServiceClient.ShoppingListService.AddDrink(CreateDrink("drink1", 1)).Model;
            var drink2 = LocalServiceClient.ShoppingListService.AddDrink(CreateDrink("drink2", 1)).Model;

            var response = LocalServiceClient.ShoppingListService.GetDrinkList(new DrinkGetList());
            response.Should().NotBeNull();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.Model.Count.Should().BeGreaterOrEqualTo(2);
            response.Model.Items.FirstOrDefault(m => m.Name == drink1.Name).Should().NotBeNull();
            response.Model.Items.FirstOrDefault(m => m.Name == drink2.Name).Should().NotBeNull();

            //Data Cleanup
            LocalServiceClient.ShoppingListService.DeleteDrink(drink1.Name);
            LocalServiceClient.ShoppingListService.DeleteDrink(drink2.Name);

        }

        [Test]
        public void GetDrink()
        {
            //Init 
            var drink1 = LocalServiceClient.ShoppingListService.AddDrink(CreateDrink("drink1", 1)).Model;

            var response = LocalServiceClient.ShoppingListService.GetDrink(drink1.Name);
            response.Should().NotBeNull();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.Model.Name.Should().Be(drink1.Name);

            //Data Cleanup
            LocalServiceClient.ShoppingListService.DeleteDrink(drink1.Name);

        }

        [Test]
        public void DeleteDrink()
        {
            var name = "delete";
            // Init
            var newDrink = CreateDrink(name, 1);
            LocalServiceClient.ShoppingListService.AddDrink(newDrink);

            var response = LocalServiceClient.ShoppingListService.DeleteDrink(name);
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void UpdateDrink()
        {
            //Init 
            var drink1 = LocalServiceClient.ShoppingListService.AddDrink(CreateDrink("drink1", 1)).Model;

            const int newQty = 2;
            var updateRequest = new DrinkRequest() {Quantity = newQty };
            var response = LocalServiceClient.ShoppingListService.UpdateDrink(drink1.Name, updateRequest);
            response.Should().NotBeNull();
            response.HttpStatusCode.Should().Be(HttpStatusCode.OK);
            response.Model.Name.Should().Be(drink1.Name);
            response.Model.Quantity.Should().Be(newQty);
            response.Model.Updated.HasValue.Should().Be(true);

            //Data Cleanup
            LocalServiceClient.ShoppingListService.DeleteDrink(drink1.Name);

        }


        private static DrinkRequest CreateDrink(string name, int quantity)
        {
            return new DrinkRequest() {Name = name, Quantity = quantity};
        }
    }
}