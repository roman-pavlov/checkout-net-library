using System;
using System.Collections.Generic;

namespace Checkout.ApiServices.Shopping.ResponseModels
{
    public class DrinkList
    {
        public string NextPageLink { get; set; }
        public int? Count { get; set; }
        public List<Drink> Items { get; set; }
    }
}