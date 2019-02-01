using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace Realmdigital_Interview.Controllers
{
    [RoutePrefix("api")]
    public class ProductController : ApiController
    {

        [Route("product/{productId}")]
        public object GetProductById(string productId)
        {
            string response = "";

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                response = client.UploadString("http://192.168.0.241/eanlist?type=Web", "POST", "{ \"id\": \"" + productId + "\" }");
            }
            var apiResponseProductList = JsonConvert.DeserializeObject<List<ApiResponseProduct>>(response);

            var apiResponseProduct = apiResponseProductList.FirstOrDefault(r => r.BarCode == productId);//Assumed Barcode as productId, otherwise just do FirstOrDefault()

            if (apiResponseProduct != null)
                return new
                {
                    Id = apiResponseProduct.BarCode,
                    Name = apiResponseProduct.ItemName,
                    Prices = GetPrices(apiResponseProduct.PriceRecords)
                };
            else
                return new object();
        }

        private List<object> GetPrices(List<ApiResponsePrice> priceRecords)
        {
            var prices = new List<object>();

            priceRecords.ForEach(prc =>
            {
                if (prc.CurrencyCode == "ZAR")
                {
                    prices.Add(new
                    {
                        Price = prc.SellingPrice,
                        Currency = prc.CurrencyCode
                    });
                }
            });

            return prices;
        }

        [Route("product/search/{productName}")]
        public List<object> GetProductsByName(string productName)
        {
            string response = "";

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                response = client.UploadString("http://192.168.0.241/eanlist?type=Web", "POST", "{ \"names\": \"" + productName + "\" }");
            }

            var apiResponseProductList = JsonConvert.DeserializeObject<List<ApiResponseProduct>>(response);
            
            var result = new List<object>();

            apiResponseProductList.ForEach(apiResponseProduct =>
            {                
                result.Add(new
                {
                    Id = apiResponseProduct.BarCode,
                    Name = apiResponseProduct.ItemName,
                    Prices = GetPrices(apiResponseProduct.PriceRecords)
                });
            });

            return result;
        }
    }



    class ApiResponseProduct
    {
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public List<ApiResponsePrice> PriceRecords { get; set; }
    }

    class ApiResponsePrice
    {
        public string SellingPrice { get; set; }
        public string CurrencyCode { get; set; }
    }
}
