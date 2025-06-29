using APIClothesEcommerceShop.DTO.Order;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
namespace APIClothesEcommerceShop.Services
{
    public class HoaDonDocument : IDocument
    {
        private readonly OrderResponseDTO _order;

        public HoaDonDocument(OrderResponseDTO order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(col =>
                {
                    col.Item().Element(e => e.Column(col =>
                    {
                        col.Item().Text($"HÓA ĐƠN MUA HÀNG - Mã HD: {_order.MaHd}")
                            .Bold().FontSize(16).AlignCenter();

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Người đặt: {_order.TenKh}");
                                c.Item().Text($"Người nhận: {_order.HoTen}");
                                c.Item().Text($"SĐT: {_order.Sdt}");
                                c.Item().Text($"Địa chỉ: {_order.DiaChiNhanHang}");
                                c.Item().Text($"Ngày tạo: {_order.NgayTao}");
                                c.Item().Text($"Ngày nhận: {_order.NgayNhan}");
                                c.Item().Text($"Ngày thanh toán: {_order.NgayThanhToan}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Nhân viên: {_order.TenNv ?? "Chưa xác định"}");
                                c.Item().Text($"Ngày tạo: {_order.NgayTao:dd/MM/yyyy HH:mm}");
                                c.Item().Text($"Thanh toán: {_order.HinhThucTt}");
                                c.Item().Text($"Tình trạng: {_order.TinhTrang}");
                                c.Item().Text($"Mô tả: {_order.MoTa}");
                                c.Item().Text($"Lý do hủy: {(string.IsNullOrEmpty(_order.LyDoHuy) ? "Không có" : _order.LyDoHuy)}");
                            });
                        });

                        col.Item().Text("Chi tiết sản phẩm").Bold().FontSize(14);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // STT
                                columns.RelativeColumn(3); // Tên SP
                                columns.RelativeColumn(2); // Biến thể
                                columns.RelativeColumn(1); // SL
                                columns.RelativeColumn(2); // Giá
                                columns.RelativeColumn(2); // Giảm
                                columns.RelativeColumn(2); // Tổng
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("STT").Bold();
                                header.Cell().Text("Tên SP").Bold();
                                header.Cell().Text("Biến thể").Bold();
                                header.Cell().Text("SL").Bold();
                                header.Cell().Text("Giá").Bold();
                                header.Cell().Text("Giảm").Bold();
                                header.Cell().Text("Thành tiền").Bold();
                            });

                            int stt = 1;
                            foreach (var sp in _order.Cthoadons.Where(x => x.MaCombo == null))
                            {
                                table.Cell().Text((stt++).ToString());
                                table.Cell().Text(sp.TenSanPham ?? "");
                                table.Cell().Text(sp.BienThe ?? "");
                                table.Cell().Text(sp.SoLuong.ToString());
                                table.Cell().Text($"{sp.Gia:N0} đ");
                                table.Cell().Text($"{(sp.GiamGia ?? 0):N0} đ");
                                var tong = sp.Gia * sp.SoLuong - (sp.GiamGia ?? 0);
                                table.Cell().Text($"{tong:N0} đ");
                            }
                        });

                        if (_order.Cthoadons.Any(x => x.MaCombo != null))
                        {
                            col.Item().Text("Combo").Bold().FontSize(14);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(4);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("STT").Bold();
                                    header.Cell().Text("Tên Combo").Bold();
                                    header.Cell().Text("SL").Bold();
                                    header.Cell().Text("Đơn giá").Bold();
                                    header.Cell().Text("Thành tiền").Bold();
                                    header.Cell().Text("Chi tiết sản phẩm").Bold();
                                });

                                int stt = 1;
                                foreach (var combo in _order.Cthoadons.Where(x => x.MaCombo != null))
                                {
                                    var ctsp = _order.Chitietcombohoadons.Where(c => c.MaCombo == combo.MaCombo);
                                    table.Cell().Text((stt++).ToString());
                                    table.Cell().Text(combo.TenCombo ?? "");
                                    table.Cell().Text(combo.SoLuong.ToString());
                                    table.Cell().Text($"{combo.GiaGoc:N0} đ");
                                    table.Cell().Text($"{(combo.GiaGoc * combo.SoLuong):N0} đ");
                                    table.Cell().Text(string.Join("\n", ctsp.Select(c => $"{c.TenSanPham} - {c.KichThuoc} - {c.MauSac} - SL: {c.SoLuong}")));
                                }
                            });
                        }

                        col.Item().AlignRight().Column(t =>
                        {
                            t.Item().Text($"Tạm tính: {_order.TienGoc:N0} đ");
                            t.Item().Text($"Giảm giá: {_order.GiamGiaCoupon:N0} đ");
                            t.Item().Text($"Phí vận chuyển: {_order.PhiVanChuyen:N0} đ");
                            t.Item().Text($"Tổng cộng: {(_order.TienGoc + _order.PhiVanChuyen - _order.GiamGiaCoupon):N0} đ").Bold();
                        });
                    }));

                });
            });
        }
    }
}
