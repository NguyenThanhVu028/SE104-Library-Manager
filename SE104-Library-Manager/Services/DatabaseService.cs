﻿using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Data;
using SE104_Library_Manager.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace SE104_Library_Manager.Services;

public class DatabaseService
{
    private DatabaseContext? _dbContext;

    public DatabaseContext DbContext => GetDatabaseContext();

    private DatabaseContext GetDatabaseContext()
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("Database context is not initialized. Call Initialize first.");
        }

        return _dbContext;
    }

    public async Task Initialize(string connectionString)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connectionString)
            .Options;

        _dbContext = new DatabaseContext(options);

        bool isNewlyCreated = await _dbContext.Database.EnsureCreatedAsync();
        if (isNewlyCreated)
        {
            await EnsureDatabaseSeededAsync();
        }
    }
    private async Task EnsureDatabaseSeededAsync()
    {
        DatabaseContext context = GetDatabaseContext();

        await EnsureCreateQuyDinhAsync(context);
        await EnsureCreateVaiTroAsync(context);
        await EnsureCreateBangCapAsync(context);
        await EnsureCreateBoPhanAsync(context);
        await EnsureCreateChucVuAsync(context);
        await EnsureCreateLoaiDocGiaAsync(context);
        await EnsureCreateTheLoaiAsync(context);
        await EnsureCreateTacGiaAsync(context);
        await EnsureCreateNhaXuatBanAsync(context);
        await context.SaveChangesAsync(); // Ensure the above entities are saved before creating employees

        await EnsureCreateNhanVienAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateAdminAccountAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateDocGiaAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreateSachAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreatePhieuMuonAsync(context);
        await context.SaveChangesAsync();

        await EnsureCreatePhieuTraAsync(context);
        await EnsureCreatePhieuPhatAsync(context);
        await context.SaveChangesAsync();
    }

    private async Task EnsureCreateQuyDinhAsync(DatabaseContext context)
    {
        if (!await context.DsQuyDinh.AnyAsync())
        {
            context.DsQuyDinh.Add(new QuyDinh());
        }
    }

    private async Task EnsureCreateVaiTroAsync(DatabaseContext context)
    {
        if (!await context.DsVaiTro.AnyAsync())
        {
            context.DsVaiTro.Add(new VaiTro { TenVaiTro = "Quản trị viên" });
            context.DsVaiTro.Add(new VaiTro { TenVaiTro = "Thủ thư" });
        }
    }

    private async Task EnsureCreateBangCapAsync(DatabaseContext context)
    {
        if (!await context.DsBangCap.AnyAsync())
        {
            context.DsBangCap.Add(new BangCap { TenBangCap = "Tú tài" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Trung cấp" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Cao đẳng" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Đại học" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Thạc sĩ" });
            context.DsBangCap.Add(new BangCap { TenBangCap = "Tiến sĩ" });
        }
    }

    private async Task EnsureCreateBoPhanAsync(DatabaseContext context)
    {
        if (!await context.DsBoPhan.AnyAsync())
        {
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ thư" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ kho" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Thủ quỹ" });
            context.DsBoPhan.Add(new BoPhan { TenBoPhan = "Ban giám đốc" });
        }
    }

    private async Task EnsureCreateChucVuAsync(DatabaseContext context)
    {
        if (!await context.DsChucVu.AnyAsync())
        {
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Nhân viên" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Phó phòng" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Trưởng phòng" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Phó giám đốc" });
            context.DsChucVu.Add(new ChucVu { TenChucVu = "Giám đốc" });
        }
    }

    private async Task EnsureCreateNhanVienAsync(DatabaseContext context)
    {
        if (!await context.DsNhanVien.AnyAsync())
        {
            context.DsNhanVien.AddRange(new NhanVien
            {
                TenNhanVien = "Admin",
                DiaChi = "123 Đường ABC, Quận 1, TP.HCM",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = 5,
                MaBangCap = 6,
                MaBoPhan = 4
            }, new NhanVien
            {
                TenNhanVien = "ThuThu",
                DiaChi = "123 Đường ABC, Quận 1, TP.HCM",
                DienThoai = "0123456789",
                NgaySinh = new DateOnly(1990, 1, 1),
                MaChucVu = 1,
                MaBangCap = 2,
                MaBoPhan = 1
            }
            );
        }
    }

    private async Task EnsureCreateAdminAccountAsync(DatabaseContext context)
    {
        if (!await context.DsTaiKhoan.AnyAsync())
        {
            var adminAccount = new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("admin"),
                MaNhanVien = 1, // Assuming the first employee is the admin
                MaVaiTro = 1 // Assuming the first role is ADMIN
            };
            var thuThuAccount = new TaiKhoan
            {
                TenDangNhap = "thuthu",
                MatKhau = BCrypt.Net.BCrypt.HashPassword("thuthu"),
                MaNhanVien = 2, // Assuming the second employee is the librarian
                MaVaiTro = 2 // Assuming the second role is LIBRARIAN
            };
            context.DsTaiKhoan.AddRange(adminAccount, thuThuAccount);
        }
    }

    private async Task EnsureCreateLoaiDocGiaAsync(DatabaseContext context)
    {
        if (!await context.DsLoaiDocGia.AnyAsync())
        {
            await context.DsLoaiDocGia.AddAsync(new LoaiDocGia { TenLoaiDocGia = "X" });
            await context.DsLoaiDocGia.AddAsync(new LoaiDocGia { TenLoaiDocGia = "Y" });
        }
    }

    private async Task EnsureCreateDocGiaAsync(DatabaseContext context)
    {
        if (!await context.DsDocGia.AnyAsync())
        {
            context.DsDocGia.AddRange(
                new DocGia
                {
                    TenDocGia = "Nguyễn Minh Hùng",
                    DiaChi = "123 Lê Lợi, Q.1, TP.HCM",
                    Email = "hung@example.com",
                    MaLoaiDocGia = 1, // Loại X
                    NgaySinh = new DateOnly(2000, 5, 20),
                    NgayLapThe = new DateOnly(2025, 6, 1),
                    TongNo = 0
                },
                new DocGia
                {
                    TenDocGia = "Trần Thị Mai",
                    DiaChi = "456 Nguyễn Trãi, Q.5, TP.HCM",
                    Email = "mai@example.com",
                    MaLoaiDocGia = 2, // Loại Y
                    NgaySinh = new DateOnly(1998, 9, 10),
                    NgayLapThe = new DateOnly(2025, 6, 2),
                    TongNo = 5000
                },
                new DocGia
                {
                    TenDocGia = "Lê Văn Tuấn",
                    DiaChi = "789 Cách Mạng Tháng 8, Q.10, TP.HCM",
                    Email = "tuan@example.com",
                    MaLoaiDocGia = 1,
                    NgaySinh = new DateOnly(2001, 2, 15),
                    NgayLapThe = new DateOnly(2025, 6, 3),
                    TongNo = 1000000
                }
            );
        }
    }


    private async Task EnsureCreateTheLoaiAsync(DatabaseContext context)
    {
        if (!await context.DsTheLoai.AnyAsync())
        {
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "A" });
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "B" });
            await context.DsTheLoai.AddAsync(new TheLoai { TenTheLoai = "C" });
        }
    }

    private async Task EnsureCreateTacGiaAsync(DatabaseContext context)
    {
        if (!await context.DsTacGia.AnyAsync())
        {
            context.DsTacGia.AddRange(
                new TacGia { TenTacGia = "Nguyễn Văn A" },
                new TacGia { TenTacGia = "Trần Thị B" },
                new TacGia { TenTacGia = "Lê Văn C" }
            );
        }
    }

    private async Task EnsureCreateNhaXuatBanAsync(DatabaseContext context)
    {
        if (!await context.DsNhaXuatBan.AnyAsync())
        {
            context.DsNhaXuatBan.AddRange(
                new NhaXuatBan { TenNhaXuatBan = "NXB Giáo Dục" },
                new NhaXuatBan { TenNhaXuatBan = "NXB Trẻ" },
                new NhaXuatBan { TenNhaXuatBan = "NXB Kim Đồng" }
            );
        }
    }

    private async Task EnsureCreateSachAsync(DatabaseContext context)
    {
        if (!await context.DsSach.AnyAsync())
        {
            var sachList = new List<Sach>
            {
                new Sach { TenSach = "Lập Trình C# Cơ Bản", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = 2020, MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 1), TriGia = 120000, SoLuongHienCo = 5, SoLuongTong = 5, TrangThai = "Còn sách" },
                new Sach { TenSach = "Giải Tích 1", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = 2019, MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 2), TriGia = 95000, SoLuongHienCo = 3, SoLuongTong = 3, TrangThai = "Còn sách" },
                new Sach { TenSach = "Kỹ Năng Sống", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = 2021, MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 3), TriGia = 75000, SoLuongHienCo = 4, SoLuongTong = 4, TrangThai = "Còn sách" },
                new Sach { TenSach = "Toán Cao Cấp", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = 2018, MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 4), TriGia = 110000, SoLuongHienCo = 2, SoLuongTong = 2, TrangThai = "Còn sách" },
                new Sach { TenSach = "Hóa Đại Cương", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = 2022, MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 5), TriGia = 89000, SoLuongHienCo = 6, SoLuongTong = 6, TrangThai = "Còn sách" },
                new Sach { TenSach = "Văn Học Việt Nam", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = 2020, MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 6), TriGia = 78000, SoLuongHienCo = 3, SoLuongTong = 3, TrangThai = "Còn sách" },
                new Sach { TenSach = "Lịch Sử Thế Giới", MaTheLoai = 1, MaTacGia = 2, NamXuatBan = 2017, MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 7), TriGia = 67000, SoLuongHienCo = 4, SoLuongTong = 4, TrangThai = "Còn sách" },
                new Sach { TenSach = "Kinh Tế Vĩ Mô", MaTheLoai = 2, MaTacGia = 1, NamXuatBan = 2023, MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 8), TriGia = 112000, SoLuongHienCo = 5, SoLuongTong = 5, TrangThai = "Còn sách" },
                new Sach { TenSach = "Kỹ Thuật Lập Trình", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = 2021, MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 9), TriGia = 134000, SoLuongHienCo = 2, SoLuongTong = 2, TrangThai = "Còn sách" },
                new Sach { TenSach = "Giáo Dục Công Dân", MaTheLoai = 1, MaTacGia = 2, NamXuatBan = 2019, MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 10), TriGia = 56000, SoLuongHienCo = 7, SoLuongTong = 7, TrangThai = "Còn sách" },
                new Sach { TenSach = "Sinh Học 12", MaTheLoai = 2, MaTacGia = 1, NamXuatBan = 2020, MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 11), TriGia = 87000, SoLuongHienCo = 3, SoLuongTong = 3, TrangThai = "Còn sách" },
                new Sach { TenSach = "Cấu Trúc Dữ Liệu", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = 2022, MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 12), TriGia = 125000, SoLuongHienCo = 4, SoLuongTong = 4, TrangThai = "Còn sách" },
                new Sach { TenSach = "Lập Trình Java", MaTheLoai = 1, MaTacGia = 1, NamXuatBan = 2021, MaNhaXuatBan = 1, NgayNhap = new DateOnly(2024, 6, 13), TriGia = 132000, SoLuongHienCo = 3, SoLuongTong = 3, TrangThai = "Còn sách" },
                new Sach { TenSach = "Kỹ Năng Mềm", MaTheLoai = 2, MaTacGia = 2, NamXuatBan = 2018, MaNhaXuatBan = 2, NgayNhap = new DateOnly(2024, 6, 14), TriGia = 49000, SoLuongHienCo = 6, SoLuongTong = 6, TrangThai = "Còn sách" },
                new Sach { TenSach = "Tâm Lý Học Đại Cương", MaTheLoai = 3, MaTacGia = 3, NamXuatBan = 2020, MaNhaXuatBan = 3, NgayNhap = new DateOnly(2024, 6, 15), TriGia = 99000, SoLuongHienCo = 2, SoLuongTong = 2, TrangThai = "Còn sách" }
            };

            // Set TrangThai for each book based on SoLuongHienCo
            foreach (var sach in sachList)
            {
                SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
            }

            context.DsSach.AddRange(sachList);
            await context.SaveChangesAsync();

            // Create BanSaoSach records for each book
            var banSaoList = new List<BanSaoSach>();
            foreach (var sach in sachList)
            {
                for (int i = 0; i < sach.SoLuongTong; i++)
                {
                    banSaoList.Add(new BanSaoSach
                    {
                        MaSach = sach.MaSach,
                        TinhTrang = "Có sẵn"
                    });
                }
            }
            context.DsBanSaoSach.AddRange(banSaoList);
            await context.SaveChangesAsync();
        }
    }

    private async Task EnsureCreatePhieuMuonAsync(DatabaseContext context)
    {
        if (!await context.DsPhieuMuon.AnyAsync())
        {
            // Get available copies for books 1 and 2
            var banSao1 = await context.DsBanSaoSach.FirstOrDefaultAsync(bs => bs.MaSach == 1 && bs.TinhTrang == "Có sẵn");
            var banSao2 = await context.DsBanSaoSach.FirstOrDefaultAsync(bs => bs.MaSach == 2 && bs.TinhTrang == "Có sẵn");

            if (banSao1 != null && banSao2 != null)
            {
                // Phiếu mượn cho độc giả 1
                var phieuMuon1 = new PhieuMuon
                {
                    NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-40)),
                    MaDocGia = 1,
                    MaNhanVien = 1
                };
                await context.DsPhieuMuon.AddAsync(phieuMuon1);
                await context.SaveChangesAsync(); 

                var dsChiTietMuon1 = new List<ChiTietPhieuMuon>
                {
                    new ChiTietPhieuMuon { MaPhieuMuon = phieuMuon1.MaPhieuMuon, MaBanSao = banSao1.MaBanSao },
                    new ChiTietPhieuMuon { MaPhieuMuon = phieuMuon1.MaPhieuMuon, MaBanSao = banSao2.MaBanSao }
                };

                await context.DsChiTietPhieuMuon.AddRangeAsync(dsChiTietMuon1);

                // Update copy status
                banSao1.TinhTrang = "Đã mượn";
                banSao2.TinhTrang = "Đã mượn";
                context.DsBanSaoSach.UpdateRange(banSao1, banSao2);

                // Update book quantities
                var sach1 = await context.DsSach.FindAsync(1);
                var sach2 = await context.DsSach.FindAsync(2);
                if (sach1 != null)
                {
                    sach1.SoLuongHienCo -= 1;
                    SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach1);
                }
                if (sach2 != null)
                {
                    sach2.SoLuongHienCo -= 1;
                    SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach2);
                }

                // Get available copies for books 3 and 4
                var banSao3 = await context.DsBanSaoSach.FirstOrDefaultAsync(bs => bs.MaSach == 3 && bs.TinhTrang == "Có sẵn");
                var banSao4 = await context.DsBanSaoSach.FirstOrDefaultAsync(bs => bs.MaSach == 4 && bs.TinhTrang == "Có sẵn");

                if (banSao3 != null && banSao4 != null)
                {
                    // Phiếu mượn cho độc giả 2
                    var phieuMuon2 = new PhieuMuon
                    {
                        NgayMuon = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                        MaDocGia = 2,
                        MaNhanVien = 1
                    };
                    await context.DsPhieuMuon.AddAsync(phieuMuon2);
                    await context.SaveChangesAsync();

                    var dsChiTietMuon2 = new List<ChiTietPhieuMuon>
                    {
                        new ChiTietPhieuMuon { MaPhieuMuon = phieuMuon2.MaPhieuMuon, MaBanSao = banSao3.MaBanSao },
                        new ChiTietPhieuMuon { MaPhieuMuon = phieuMuon2.MaPhieuMuon, MaBanSao = banSao4.MaBanSao }
                    };
                    await context.DsChiTietPhieuMuon.AddRangeAsync(dsChiTietMuon2);

                    // Update copy status
                    banSao3.TinhTrang = "Đã mượn";
                    banSao4.TinhTrang = "Đã mượn";
                    context.DsBanSaoSach.UpdateRange(banSao3, banSao4);

                    // Update book quantities
                    var sach3 = await context.DsSach.FindAsync(3);
                    var sach4 = await context.DsSach.FindAsync(4);
                    if (sach3 != null)
                    {
                        sach3.SoLuongHienCo -= 1;
                        SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach3);
                    }
                    if (sach4 != null)
                    {
                        sach4.SoLuongHienCo -= 1;
                        SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach4);
                    }
                }
            }
        }
    }

    private async Task EnsureCreatePhieuTraAsync(DatabaseContext context)
    {
        if (!await context.DsPhieuTra.AnyAsync())
        {
            var phieuMuon1 = await context.DsPhieuMuon
                .Include(pm => pm.DsChiTietPhieuMuon)
                .FirstOrDefaultAsync(pm => pm.MaDocGia == 1);

            if (phieuMuon1 != null && phieuMuon1.DsChiTietPhieuMuon.Any())
            {
                var chiTietMuon = phieuMuon1.DsChiTietPhieuMuon.First();

                var phieuTra = new PhieuTra
                {
                    MaDocGia = phieuMuon1.MaDocGia,
                    NgayTra = DateOnly.FromDateTime(DateTime.Now),
                    MaNhanVien = 1,
                    TienPhatKyNay = 10000,
                    TongNo = 10000
                };

                await context.DsPhieuTra.AddAsync(phieuTra);
                await context.SaveChangesAsync();

                var chiTiet = new ChiTietPhieuTra
                {
                    MaPhieuTra = phieuTra.MaPhieuTra,
                    MaPhieuMuon = phieuMuon1.MaPhieuMuon,
                    MaBanSao = chiTietMuon.MaBanSao,
                    TienPhat = 10000
                };

                await context.DsChiTietPhieuTra.AddAsync(chiTiet);

                // Cập nhật nợ độc giả
                var docGia = await context.DsDocGia.FindAsync(phieuMuon1.MaDocGia);
                if (docGia != null)
                {
                    docGia.TongNo += chiTiet.TienPhat;
                }

                // Cập nhật trạng thái bản sao
                var banSao = await context.DsBanSaoSach
                    .Include(bs => bs.Sach)
                    .FirstOrDefaultAsync(bs => bs.MaBanSao == chiTietMuon.MaBanSao);
                if (banSao != null)
                {
                    banSao.TinhTrang = "Có sẵn";
                    context.DsBanSaoSach.Update(banSao);
                }

                // Cập nhật trạng thái sách
                var sach = await context.DsSach.FindAsync(banSao?.MaSach);
                if (sach != null)
                {
                    sach.SoLuongHienCo += 1; // Increase quantity for returned books
                    SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(sach);
                }
            }
        }
    }

    private async Task EnsureCreatePhieuPhatAsync(DatabaseContext context)
    {
        const int targetDocGiaId = 3;
        var docGia = await context.DsDocGia.FindAsync(targetDocGiaId);

        if (docGia == null)
            throw new InvalidOperationException("Độc giả có mã 3 (Lê Văn Tuấn) chưa tồn tại.");

        if (!await context.DsPhieuPhat.AnyAsync(p => p.MaDocGia == targetDocGiaId))
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            // Giả sử chia ra 3 phiếu phạt lớn
            var phieuPhats = new List<PhieuPhat>
            {
                new PhieuPhat
                {
                    NgayLap = currentDate.AddDays(-10),
                    MaDocGia = targetDocGiaId,
                    TongNo = 50000,
                    TienThu = 10000,
                    ConLai = 40000
                },
                new PhieuPhat
                {
                    NgayLap = currentDate.AddDays(-5),
                    MaDocGia = targetDocGiaId,
                    TongNo = 40000,
                    TienThu = 25000,
                    ConLai = 15000
                },
                new PhieuPhat
                {
                    NgayLap = currentDate.AddDays(-2),
                    MaDocGia = targetDocGiaId,
                    TongNo = 15000,
                    TienThu = 5000,
                    ConLai = 10000
                }
            };

            context.DsPhieuPhat.AddRange(phieuPhats);

            // Cập nhật lại tổng nợ độc giả (chính xác bằng tổng ConLai)
            docGia.TongNo = phieuPhats.Sum(p => p.ConLai);

            await context.SaveChangesAsync();
        }
    }
}
