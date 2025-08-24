using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IVoucherService
    {
        Task<Voucher?> GetVoucherByCodeAsync(string code);
        Task<List<Voucher>> GetAllVouchersAsync();
        Task<Voucher> CreateVoucherAsync(Voucher voucher);
        Task<Voucher?> UpdateVoucherAsync(Voucher voucher);
        Task<bool> DeleteVoucherAsync(int id);
    }
}
