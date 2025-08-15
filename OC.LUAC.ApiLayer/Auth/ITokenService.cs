using OC.LUAC.ObjectLayer.Accounts;

namespace OC.LUAC.ApiLayer.Auth
{
    public interface ITokenService
    {
        string CreateCustomerToken(Customer customer);
        string CreateAdminToken(string email);
    }
}
