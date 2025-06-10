using System.ComponentModel.DataAnnotations.Schema;

namespace APIClothesEcommerceShop.Models
{
    public class Chitietcombo
    {
        public int MaSp { get; set; }
        public int MaCombo { get; set; }
        public int SoLuongSP { get; set; }
        [ForeignKey("MaSp")]    

        public virtual Sanpham MaSpNavigation { get; set; } = null!;
        [ForeignKey("MaCombo")]         
        public virtual Combo MaComboNavigation { get; set; } = null!;

    }
}
