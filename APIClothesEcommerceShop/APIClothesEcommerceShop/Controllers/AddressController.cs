using APIClothesEcommerceShop.Repositories.Address;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressRepository addressRepository;
        public AddressController(IAddressRepository addressRepositor)
        {
            this.addressRepository = addressRepositor;
        }
        [HttpGet("{Makh}")]
        public async Task<IActionResult> Index(int Makh)
        {
            var addresses = await addressRepository.GetAll(Makh);
            return Ok(addresses);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await addressRepository.Delete(id);
                return Ok(new
                {
                    Message = "Đã xóa địa chỉ"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
            
        }
    }
}
