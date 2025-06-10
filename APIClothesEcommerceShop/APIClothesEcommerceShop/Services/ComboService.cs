using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories;
using System.Data.Common;
using System.Drawing.Text;
using static Azure.Core.HttpHeader;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Combos;
using APIClothesEcommerceShop.Repositories.DetailCombo;

namespace APIClothesEcommerceShop.Services
{
    public class ComboService
    {
        private readonly IComboRepository comboRepository;
        private readonly IDetailCombo detailComboRepository;
        private readonly EcommerceShopContext db;
        public ComboService(IComboRepository comboRepository, IDetailCombo detailComboRepository, EcommerceShopContext db)
        {
            this.comboRepository = comboRepository;
            this.detailComboRepository = detailComboRepository;
            this.db = db;
        }
        public async Task AddCombo(ComboRequestDTO combo)
        {
            await db.Database.BeginTransactionAsync();
            try
            {
                if(combo.Hinh != null)
                {
                    var folderPath = Path.Combine("wwwroot/HinhAnh/AnhCombo");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var filePath = Path.Combine(folderPath, combo.Hinh.FileName);
                    using(var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await combo.Hinh.CopyToAsync(stream);
                    }
                }
                var model = new Combo
                {
                    TenCombo = combo.TenCombo,
                    Hinh = combo.Hinh?.FileName,
                    SoTienGiam = combo.SoTienGiam,
                    PhanTramGiam = combo.PhanTramGiam,
                    NgayKetThuc = combo.NgayKetThuc,
                    NgayBatDau = combo.NgayBatDau,
                    SoLuong = combo.SoLuong,
                    MoTa = combo.MoTa,
                    IsActive = true,
                };
                model = await comboRepository.AddCombo(model);
                if(combo.Chitietcombos != null)
                {
                    foreach(var detail in combo.Chitietcombos)
                    {
                        var NewDetailCombo = new Chitietcombo
                        {
                            MaCombo = model.MaCombo,
                            MaSp = detail.MaSp,
                            SoLuongSP = detail.SoLuongSp,
                        };
                        await detailComboRepository.AddDetailCombo(NewDetailCombo);
                    }
                    await db.Database.CommitTransactionAsync();
                }
            }catch(Exception ex)
            {
               await db.Database.RollbackTransactionAsync();
               throw;
            }
        }

        public async Task EditCombo(int id, ComboRequestDTO combo)
        {
            await db.Database.BeginTransactionAsync();
            try
            {
                if (combo.Hinh != null)
                {
                    var folderPath = Path.Combine("wwwroot/HinhAnh/AnhCombo");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var filePath = Path.Combine(folderPath, combo.Hinh.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await combo.Hinh.CopyToAsync(stream);
                    }
                }
                var findCombo = await db.Combos.FindAsync(id);
                if(findCombo == null)
                {
                    throw new Exception("Combo not found");
                }
                findCombo.TenCombo = combo.TenCombo;
                findCombo.PhanTramGiam = combo.PhanTramGiam;
                findCombo.SoTienGiam = combo.SoTienGiam;
                findCombo.MoTa = combo.MoTa;
                findCombo.IsActive = true;
                findCombo.NgayBatDau = combo.NgayBatDau;
                findCombo.NgayKetThuc = combo.NgayKetThuc;
                findCombo.Hinh = combo.Hinh?.FileName ?? findCombo.Hinh;
                findCombo.SoLuong = combo.SoLuong;
                await comboRepository.EditCombo(findCombo);
                await detailComboRepository.DeleteDetailComboByMaCombo(findCombo.MaCombo);
                foreach(var detail in combo.Chitietcombos)
                {
                    var NewCombo = new Chitietcombo
                    {
                        MaCombo = findCombo.MaCombo,
                        MaSp = detail.MaSp,
                        SoLuongSP = detail.SoLuongSp,
                    };
                    await detailComboRepository.AddDetailCombo(NewCombo);
                }   
                await db.Database.CommitTransactionAsync();
            }
            catch(Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
