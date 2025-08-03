using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Addresses;
using APIClothesEcommerceShop.Models;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Address
{
    public class AddressRepository : IAddressRepository
    {
        private readonly EcommerceShopContext db;
        public AddressRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<IEnumerable<AddressesResponseDTO>> GetByCustomerAsync(int maKh)
        {
            try
            {
                var data = await db.Diachis
                    .Where(d => d.MaKh == maKh)
                    .Include(d => d.MaKhNavigation)
                    .Select(p => new AddressesResponseDTO
                    {
                        ID = p.ID,
                        Tinh = p.Tinh,
                        QuanHuyen = p.QuanHuyen,
                        XaPhuong = p.XaPhuong,
                        diachichitiet = p.diachichitiet,
                        MacDinh = p.MacDinh,
                        Hoten = p.Hoten,
                        SDT = p.SDT,
                        MaKh = maKh
                    })
                    .ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách địa chỉ của khách hàng có mã {maKh}.", ex);
            }
        }


        public async Task<Diachi> AddAsync(Diachi diachi)
        {
            try
            {
                db.Diachis.Add(diachi);
                await db.SaveChangesAsync();
                return diachi;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm địa chỉ.", ex);
            }
        }
        public async Task<Diachi?> UpdateDefaultAddress(int? id, bool defaultAddress)
        {
            try
            {
                var existing = new Diachi();
                if (id.HasValue)
                {
                    existing = await db.Diachis.FindAsync(id);
                    if (existing == null)
                        return null;
                    if (existing.MacDinh == false && existing.MacDinh != defaultAddress)
                    {
                        var findAddress = await db.Diachis.FirstOrDefaultAsync(p => p.MacDinh == true);
                        findAddress.MacDinh = false;
                        existing.MacDinh = defaultAddress;
                    }
                }
                else
                {
                    var ListAddress = await db.Diachis.Where(p => p.MacDinh == true).ToListAsync();
                    if(ListAddress != null)
                    {
                        foreach (var address in ListAddress)
                        {
                            address.MacDinh = false;
                        }
                        db.Diachis.UpdateRange(ListAddress);
                    }
                }
                await db.SaveChangesAsync();
                return existing;
            }catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật địa chỉ.", ex);
            }
        }
        public async Task<Diachi?> UpdateAsync(Diachi diachi)
        {
            try
            {
                var existing = await db.Diachis.FindAsync(diachi.ID);
                if (existing == null)
                    return null;

                await UpdateDefaultAddress(diachi.ID, existing.MacDinh);
                existing.Tinh = diachi.Tinh;
                existing.QuanHuyen = diachi.QuanHuyen;
                existing.XaPhuong = diachi.XaPhuong;
                if(existing.Tinh != diachi.Tinh || existing.QuanHuyen != diachi.QuanHuyen || existing.XaPhuong != diachi.XaPhuong)
                {
                    existing.diachichitiet = diachi.diachichitiet;
                }
                existing.MacDinh = diachi.MacDinh;
                existing.Hoten = diachi.Hoten;
                existing.SDT = diachi.SDT;
                existing.MaKh = diachi.MaKh;

                await db.SaveChangesAsync();
                return existing;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật địa chỉ.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var diachi = await db.Diachis.FindAsync(id);
                if (diachi == null)
                    return false;

                db.Diachis.Remove(diachi);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa địa chỉ.", ex);
            }
        }
    }
}
