using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly EcommerceShopContext _db;
        public DbInitializer(EcommerceShopContext db)
        {
            _db = db;
        }

        public void InitializeDb()
        {
            // _db.Database.EnsureCreated();
            var isCreateCombo = _db.Combos.Any();
            if (!isCreateCombo)
            {
                InitCombo(3);
            }
        }
        private void InitCombo(int numCreate, int? idOrder = null)
        {
            var sanphams = _db.Sanphams.ToList();
            var chitietsanphams = _db.Chitietsanphams.ToList();
            idOrder = idOrder ?? _db.Hoadons.First().MaHd;

            var random = new Random();

            for (int i = 1; i <= numCreate; i++)
            {
                // Tạo combo mới
                var combo = new Combo
                {
                    TenCombo = $"Combo {i}",
                    Hinh = null,
                    GiaCombo = 0, // Sẽ tính sau
                    SoLuong = random.Next(5, 20),
                    MoTa = $"Mô tả cho combo {i}",
                    IsActive = true
                };

                // Chọn ngẫu nhiên 2-4 chi tiết sản phẩm cho combo này
                var ctspInCombo = chitietsanphams.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();

                int tongGia = 0;
                foreach (var ctsp in ctspInCombo)
                {
                    int soLuong = random.Next(1, 5);
                    int donGia = ctsp.DonGia;

                    combo.Chitietcombohoadons.Add(new Chitietcombohoadon
                    {
                        MaHd = idOrder.Value,
                        MaCtsp = ctsp.MaCtsp,
                        SoLuong = soLuong,
                        DonGia = donGia
                    });

                    tongGia += donGia * soLuong;
                }

                // Gán giá combo là tổng giá các sản phẩm giảm 10%
                combo.GiaCombo = (int)(tongGia * 0.9);

                _db.Combos.Add(combo);
            }

            _db.SaveChanges();
        }
    }
}