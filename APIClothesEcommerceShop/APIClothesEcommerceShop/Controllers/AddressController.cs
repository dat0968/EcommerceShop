using APIClothesEcommerceShop.DTO.Addresses;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
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
            var addresses = await addressRepository.GetByCustomerAsync(Makh);
            return Ok(addresses);
        }
        [HttpGet("GetByCustomer_DefaultAddressAsync/{Makh}")]
        public async Task<IActionResult> GetByCustomer_DefaultAddressAsync(int Makh)
        {
            var addresses = await addressRepository.GetByCustomer_DefaultAddressAsync(Makh);
            return Ok(addresses);
        }
        [HttpPost]
        public async Task<IActionResult> Create(AddressesRequestDTO model)
        {
            try
            {
                var NewAddress = new Diachi
                {
                    Hoten = model.Hoten,
                    Tinh = model.Tinh,
                    QuanHuyen = model.QuanHuyen,
                    XaPhuong = model.XaPhuong,
                    diachichitiet = model.diachichitiet,
                    MacDinh = model.MacDinh,
                    SDT = model.SDT,
                    MaKh = model.MaKh,
                };
                var addresses = await addressRepository.AddAsync(NewAddress);
                return Ok(new
                {
                    Success = true,
                    Message = "Đã thêm địa chỉ mới"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AddressesRequestDTO model)
        {
            try
            {
                var UpdateAddress = new Diachi
                {
                    ID = id,
                    Hoten = model.Hoten,
                    Tinh = model.Tinh,
                    QuanHuyen = model.QuanHuyen,
                    XaPhuong = model.XaPhuong,
                    diachichitiet = model.diachichitiet,
                    MacDinh = model.MacDinh,
                    SDT = model.SDT,
                    MaKh = model.MaKh,
                };
                var addresses = await addressRepository.UpdateAsync(UpdateAddress);
                return Ok(new
                {
                    Success = true,
                    Message = "Đã cập nhật địa chỉ"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await addressRepository.DeleteAsync(id);
                return Ok(new
                {
                    Success = true,
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
