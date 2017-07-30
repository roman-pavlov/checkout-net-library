using Checkout.ApiServices.SharedModels;
using Checkout.ApiServices.Shopping.RequestModels;
using Checkout.ApiServices.Shopping.ResponseModels;
using Checkout.Utilities;

namespace Checkout.ApiServices.Shopping
{
    public class ShoppingListService
    {
        public HttpResponse<Drink> AddDrink(DrinkRequest requestModel)
        {
            return new ApiHttpClient().PostRequest<Drink>(ApiUrls.Shopping.Drinks, AppSettings.SecretKey, requestModel);
        }

        public HttpResponse<OkResponse> DeleteDrink(string drinkId)
        {
            var deleteDrinkUri = string.Format(ApiUrls.Shopping.Drink, drinkId);
            return new ApiHttpClient().DeleteRequest<OkResponse>(deleteDrinkUri, AppSettings.SecretKey);
        }

        public HttpResponse<DrinkList> GetDrinkList(DrinkGetList requestModel)
        {
            var drinkListUri = ApiUrls.Shopping.DrinksCountPagesApiUri;

            if (requestModel.Skip.HasValue)
            {
                drinkListUri = UrlHelper.AddParameterToUrl(drinkListUri, "skip", requestModel.Skip.Value.ToString());
            }

            return new ApiHttpClient().GetRequest<DrinkList>(drinkListUri, AppSettings.SecretKey);
        }

        public HttpResponse<Drink> GetDrink(string drinkId)
        {
            var getDrinkUri = string.Format(ApiUrls.Shopping.Drink, drinkId);
            return new ApiHttpClient().GetRequest<Drink>(getDrinkUri, AppSettings.SecretKey);
        }

        public HttpResponse<Drink> UpdateDrink(string drinkId, DrinkRequest requestModel)
        {
            var updateDrinkUri = string.Format(ApiUrls.Shopping.Drink, drinkId);
            return new ApiHttpClient().PutRequest<Drink>(updateDrinkUri, AppSettings.SecretKey, requestModel);
        }
    }
}