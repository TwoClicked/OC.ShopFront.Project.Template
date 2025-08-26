using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ServiceLayer.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDbContext _context;

        public VoucherService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Voucher?> GetVoucherByCodeAsync(string code)
        {
            return await _context.Set<Voucher>()
                .FirstOrDefaultAsync(v => v.Code == code && v.IsActive);
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _context.Set<Voucher>().ToListAsync();
        }

        public async Task<Voucher> CreateVoucherAsync(Voucher voucher)
        {
            _context.Set<Voucher>().Add(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<Voucher?> UpdateVoucherAsync(Voucher voucher)
        {
            _context.Set<Voucher>().Update(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await _context.Set<Voucher>().FindAsync(id);
            if (voucher == null) return false;

            _context.Set<Voucher>().Remove(voucher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int id)
        {
           return await _context.Vouchers.FindAsync(id);
        }
    }
}
