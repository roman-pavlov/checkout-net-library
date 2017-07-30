using System;

namespace Checkout.ApiServices.Shopping.ResponseModels
{
    public class Drink
    {
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}