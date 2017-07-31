using Checkout.ApiServices.SharedModels;
using Checkout.ApiServices.Shopping.RequestModels;

namespace Checkout.ApiServices.Shopping
{
    public class ShoppingAuthenticationService
    {
        public HttpResponse<OkResponse> GetToken(LoginRequest request)
        {
            return new ApiHttpClient().PostRequest<OkResponse>(ApiUrls.Shopping.AuthApiUri, null, request);
        }
   }
}