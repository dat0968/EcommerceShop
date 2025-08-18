
using System.Collections.Generic;

namespace APIClothesEcommerceShop.DTO.TryOn
{
    public class TryOnRequest
    {
        public string ModelImage { get; set; }
        public List<string> ProductImages { get; set; }
        public List<ProductData> ProductsData { get; set; }
    }
}
